﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Magic.Network;
using Magic.Common;
using Magic.Common.Robots;
using Magic.Common.Mapack;
using Magic.Common.Sim;
using System.Threading;

namespace Magic.Pioneer
{
	public class Pioneer3 : IRobotTwoWheel
	{
		private IPAddress ipMulticast;
		private IPAddress ipUnicast;
		private Int32 port;
		private Int32 controlPort = 20;
		private byte[] buf;
		private Socket sock;
		private UdpClient udpClient;
		private GenericMulticastServer<SimMessage<RobotTwoWheelCommand>> simRobotCommandServer;
		private GenericMulticastClient<SimMessage<IRobotTwoWheelStatus>> simSegwayFeedback;
		private SegwayStatus status;
        private TimerCallback timerDelegate = new TimerCallback(sendHeartBeat);
        private byte syncIndex, syncRecieved;
        
		private int robotID;
		private bool simMode;
		private bool isBackwards = false;
		public event EventHandler StatusUpdated;
		public event EventHandler Shutdown;
		public event EventHandler<TimestampedEventArgs<IRobotTwoWheelStatus>> WheelSpeedUpdate;
		public event EventHandler<TimestampedEventArgs<IRobotTwoWheelStatus>> WheelPositionUpdate;
		public event EventHandler<TimestampedEventArgs<int>> Msg1;

		double wheelPosTs = 0;
		double wheelSpeedTs = 0;
		double wheelTorqueTs = 0;

        //state of sending
        byte sendState = 0;

		public IRobotTwoWheelStatus Status
		{
			get { return new SegwayStatus(status); }
		}

		public double TrackWidth
		{
			get { return 0.4625; }
		}

		public double ForwardVelocity { get { return (status.rightWheelSpeed + status.leftWheelSpeed) / 2; } }
		public double RotationRate { get { return (status.rightWheelSpeed - status.leftWheelSpeed) / TrackWidth; } }

		public Pioneer3(NetworkAddress feedback, NetworkAddress control, bool simmode, bool isBackwards, int id)
			: this(feedback, control, isBackwards, id)
		{
			if (simmode)
			{
				INetworkAddressProvider addrProvider = new HardcodedAddressProvider();
				simRobotCommandServer = new GenericMulticastServer<SimMessage<RobotTwoWheelCommand>>(addrProvider.GetAddressByName("RobotSimCommands"), new CSharpMulticastSerializer<SimMessage<RobotTwoWheelCommand>>(true));
				simRobotCommandServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));

				//Fake Segway feedback (for Odom)
				simSegwayFeedback = new GenericMulticastClient<SimMessage<IRobotTwoWheelStatus>>(addrProvider.GetAddressByName("SimSegwayFeedback"), new CSharpMulticastSerializer<SimMessage<IRobotTwoWheelStatus>>(true));
				simSegwayFeedback.MsgReceived += new EventHandler<MsgReceivedEventArgs<SimMessage<IRobotTwoWheelStatus>>>(simSegwayFeedback_MsgReceived);
				simSegwayFeedback.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));
			}
			this.simMode = simmode;
		}

		public Pioneer3(NetworkAddress feedback, NetworkAddress control, bool isBackwards, int id)
		{
			this.isBackwards = isBackwards;
			this.ipMulticast = feedback.Address;
			this.port = feedback.Port;
			this.ipUnicast = control.Address;
			this.controlPort = control.Port;
			status = new SegwayStatus();
			udpClient = new UdpClient();
			simMode = false;
			robotID = id;
		}
		void simSegwayFeedback_MsgReceived(object sender, MsgReceivedEventArgs<SimMessage<IRobotTwoWheelStatus>> e)
		{
			if (e.message.RobotID == robotID && WheelPositionUpdate != null)
				WheelPositionUpdate(this, new TimestampedEventArgs<IRobotTwoWheelStatus>(e.message.Timestamp, e.message.Message));
		}
		public void Start()
		{
			BuildSocket(IPAddress.Any);
            if (!simMode)
                SetupCommunication();
		}

		public void Start(IPAddress bindIP)
		{
			BuildSocket(bindIP);
            if (!simMode)
                SetupCommunication();
		}
		public void Stop()
		{
			if (sock == null) return; //the socket is already killed...ignore this stop command
			this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(this.ipMulticast));
			this.sock = null;
		}

        short chksum;
        int checkSum(byte[] packetM)
        {
            int i;
            //char *buffer = &packetM[3];
            int c = 0;
            byte n;

            //bridge offset
            byte offset = 4;

            i = 3;
            n = (byte)(packetM[2+offset] - 2);
            while (n > 1)
            {
                c += ((char)packetM[i+offset] << 8) | (char)packetM[i + 1+offset];
                c = c & 0xffff;
                n -= 2;
                i += 2;
                //buffer+=2;
            }
            if (n > 0) c = c ^ (int)((char)packetM[i+offset]);
            return c;
        }

		public void SetVelocityTurn(RobotTwoWheelCommand command)
		{
			double velocity = command.velocity;
			double turn = command.turn;
            byte direction = 0x3b;//moving forward

			///////////////////////////////////////////////////////////////////////////////////
			//DO NOT CHANGE THESE VALUES WITHOUT ASKING MARK FIRST, THIS IS FOR SAFETY PURPOSES
			///////////////////////////////////////////////////////////////////////////////////
			if (turn > 600)
				turn = 600;
			if (turn < -600)
				turn = -600;

			if (velocity > 3)
				velocity = 3;
			if (velocity < -1)
				velocity = -1;
			///////////////////////////////////////////////////////////////////////////////////

			if (isBackwards)
			{
				velocity *= -1;
                direction = 0x1b;
			}
            double rotational_term = Math.PI/180*turn/7.8;

            double leftvel = (command.velocity - rotational_term);
            double rightvel = (command.velocity + rotational_term);

            if (leftvel > 0x50)
                leftvel = 0x50;

            if (rightvel > 0x50)
                rightvel = 0x50;

            int pioneerVelocity = (int)(command.velocity*3.04108844*15);//convert counts to mm/sec

			RMPControlMsg ctrl = new RMPControlMsg(velocity, turn);

			byte[] send = new byte[14];

            send[0] = 0x01; //send to pioneer
            send[1] = 0x00; //blank
            send[2] = 0x00; //len (hi)
            send[3] = 0x3; //len (lo) for bridge
            send[4] = 0xfa; // Pioneer start byte 1
            send[5] = 0xfb; // Pioneer start byte 2
            send[6] = 0x6; // pioneer packet length
            send[7] = 32; // pioneer command
            send[8] = 0x3b; // args type
            send[9] = (byte)rightvel; // high byte of vel
            send[10] = (byte)leftvel;
            send[11] = (byte)((checkSum(send)) >> 8);
            send[12] = (byte)((checkSum(send)) & 0xff);// checksum (excluding startbytes+command)

            SendData(send, 13);

            System.Threading.Thread.Sleep(1000);

		/*	send[0] = 0x00; //send to bridge
			send[1] = 0x17; //command
			send[2] = 0x00; //len (hi)
			send[3] = 0x0a; //len (lo)
			send[4] = 0x04; // can msg hi
			send[5] = 0x13; // can msg lo
			send[6] = (byte)(ctrl.velocityCommand >> 8);
			send[7] = (byte)(ctrl.velocityCommand & 0xFF);
			send[8] = (byte)(ctrl.turningCommand >> 8);
			send[9] = (byte)(ctrl.turningCommand & 0xFF);
			send[10] = (byte)((short)ctrl.configurationCommand >> 8);
			send[11] = (byte)((short)ctrl.configurationCommand & 0xFF);
			send[12] = (byte)(ctrl.configurationParameter >> 8);
			send[13] = (byte)(ctrl.configurationParameter & 0xFF);
            
            send[0] = 0x01; //send to pioneer
            send[1] = 0x00; //command
            send[2] = 0x00; //len (hi)
            send[3] = 0x3; //len (lo)
            send[4] = 0xfa; // can msg hi
            send[5] = 0xfb; // can msg lo
            send[6] = 0x5; // can msg lo
            send[7] = 0xb; // can msg lo
            send[8] = 0xa; // can msg lo
            send[9] = 0xa; // can msg lo
            send[10] = (byte)((checkSum(send)) >> 8);
            send[11] = (byte)((checkSum(send)) & 0xff);// checksum (excluding startbytes+command)*/

           /* MoveMotors(send);
            
			if (simMode)
				simRobotCommandServer.SendUnreliably(new SimMessage<RobotTwoWheelCommand>(NetworkAddress.GetSimRobotID("behavioral"), command, wheelPosTs));

			else
				udpClient.Send(send, 12, new IPEndPoint(ipUnicast, controlPort));*/
		}

		private void BuildSocket(IPAddress bindAddress)
		{
			lock (this)
			{
				if (this.sock == null)
				{
					if (this.buf == null)
						this.buf = new byte[65536];
					this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					this.sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
					this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 10);
					this.sock.Bind(new IPEndPoint(bindAddress, this.port));
					this.sock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(this.ipMulticast, bindAddress));
					this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
				}
			}
		}

		private void ReceiveCallback(IAsyncResult ar)
		{
			if (this.sock == null) return;
			//try
			//{
			int bytesRead = this.sock.EndReceive(ar);
			if (bytesRead > 0)
			{
				MemoryStream ms = new MemoryStream(buf, 0, bytesRead, false);
				BinaryReader br = new BinaryReader(ms);
				ParseSegwayPacket(br);
			}
			//}
			//catch (Exception ex)
			//{
			//  Console.WriteLine("Socket exception! " + ex.Message);

			//}
			//finally
			//{
			this.sock.BeginReceive(this.buf, 0, this.buf.Length, SocketFlags.None, ReceiveCallback, null);
			//}
		}

		private void ParseSegwayPacket(BinaryReader br)
		{
			//figure out what it is
			UInt16 secs = Endian.BigToLittle(br.ReadUInt16());
			UInt32 ticks = Endian.BigToLittle(br.ReadUInt32());
			double ts = secs + ticks / 10000.0;

			byte id = br.ReadByte();
			if (id != 0xFF&&syncIndex>4) return;
            UInt16 len = Endian.BigToLittle(br.ReadUInt16()); //ushort msgID = Endian.BigToLittle(br.ReadUInt16());
           // byte junk = br.ReadByte(); 
		//	byte msgID = br.ReadByte();
       /*     if (syncIndex < 3)
            {
                syncIndex++;
            }
*/        //    else if (syncIndex == 5)
         //   {

         //   }
      //      else// if(syncIndex>8)
       //     {
                byte msgID = br.ReadByte();



                switch (msgID)
                {
                    case MotorsStoppedMsg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 1)); HandleMotorsStoppedMsg(br, ts);
                        break;
                    case MotorsMovingMsg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 1)); HandleMotorsMovingMsg(br, ts);
                        break;
          //          case Sync3Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 1)); HandleSync3(br, ts, br.ReadByte());
           //             break;
               //     case CommandZeroMsg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 1)); HandleCommandZero(br, ts, br.ReadByte());
               //         break;
                    case Sync0Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 1)); HandleSync0(br, ts);
                        break;
                    case Sync1Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 1)); HandleSync1(br, ts);
                        break;
                    case Sync2Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 1)); HandleSync2(br, ts);
                        break;
                    case Sync3Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 1)); HandleSync3(br, ts);
                        break;

                    
                /*    case RMPHeartbeatMsg.id: HandleHeartbeat(br, ts);
                        break;
                    case RMPMonitor1Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 1)); HandleMonitor1(br, ts);
                        break;
                    case RMPMonitor2Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 2)); HandleMonitor2(br, ts);
                        break;
                    case RMPMonitor3Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 3)); HandleMonitor3(br, ts);
                        break;
                    case RMPMonitor4Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 4)); HandleMonitor4(br, ts);
                        break;
                    case RMPMonitor5Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 5)); HandleMonitor5(br, ts);
                        break;
                    case RMPMonitor6Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 6)); HandleMonitor6(br, ts);
                        break;
                    case RMPMonitor7Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 7)); HandleMonitor7(br, ts);
                        break;
                    case RMPMonitor8Msg.id: if (Msg1 != null) Msg1(this, new TimestampedEventArgs<int>(ts, 8)); HandleMonitor8(br, ts);
                        break;
                    case RMPControlMsg.id: HandleControl(br);
                        break;
                    case RMPShutdownMsg.id: HandleShutdown(br);
                        break;
                    case RMPStatusMsg.id: HandleStatus(br);
                        break;*/
                    default:
                        Console.WriteLine("Unknown msg id: " + msgID); break;
                }
            }
		//}

        public void SetupCommunication()
        {
            byte[] send = new byte[14];
            closeConnection();
            syncIndex = 0;
            syncRecieved = 8;
        //    byte index=0;
            System.Threading.Thread.Sleep(10);
       //     for (index = 0; index < 3; index++)
            byte failed = 0;
            while(syncIndex<3)
            {
                if (failed == 15)
                {
                    closeConnection();
                    failed = 0; BuildSocket(IPAddress.Any);
                    System.Threading.Thread.Sleep(1000);
                    continue;
                }
                send[0] = 0x01; //send to pioneer
                send[1] = 0x00; //blank
                send[2] = 0x00; //len (hi)
                send[3] = 0x3; //len (lo) for bridge
                send[4] = 0xfa; // Pioneer start byte 1
                send[5] = 0xfb; // Pioneer start byte 2
                send[6] = 0x3; // pioneer packet length
                send[7] = (byte)syncIndex; // pioneer command
                send[8] = 0x0; // args
                send[9] = (byte)syncIndex; // checksum (excluding startbytes)

                
                SendData(send,10);
                Console.WriteLine("sending sync {0,1:d}", (int)syncIndex);
                failed++;
                
                System.Threading.Thread.Sleep(100);
            }
            syncIndex = 3;
         //   while (syncIndex < 4)
          //  {
                //open command
                send[0] = 0x01; //send to pioneer
                send[1] = 0x00; //blank
                send[2] = 0x00; //len (hi)
                send[3] = 0x3; //len (lo) for bridge
                send[4] = 0xfa; // Pioneer start byte 1
                send[5] = 0xfb; // Pioneer start byte 2
                send[6] = 0x3; // pioneer packet length
                send[7] = 1; // pioneer command
                send[8] = 0x0; // args
                send[9] = 1; // checksum (excluding startbytes and command)

                SendData(send, 10);
                Console.WriteLine("sending open: \n");
                System.Threading.Thread.Sleep(10);
        //    }
        //    syncIndex = 4;
       //     while (syncIndex < 5)
       //     {

                Console.WriteLine("sending pulse after open: \n");
                SendPulse();
        //    }
         //   while(syncIndex<6)

            Console.WriteLine("disabling sonar: \n");
            send[0] = 0x01; //send to pioneer
            send[1] = 0x00; //blank
            send[2] = 0x00; //len (hi)
            send[3] = 0x3; //len (lo) for bridge
            send[4] = 0xfa;
            send[5] = 0xfb;
            send[6] = 6;
            send[7] = 28;
            send[8] = 0x3b;
            send[9] = 0x00;
            send[10] = 0x00;
            send[11] = (byte)((checkSum(send)) >> 8);
            send[12] = (byte)((checkSum(send)) & 0xff);

            SendData(send, 13);
            System.Threading.Thread.Sleep(1000);

            EnableMotors(send);

           // MoveMotors(send);
/*
            send[0] = 0x01; //send to pioneer
            send[1] = 0x00; //command
            send[2] = 0x00; //len (hi)
            send[3] = 0x3; //len (lo)
            send[4] = 0xfa; // can msg hi
            send[5] = 0xfb; // can msg lo
            send[6] = 0x5; // can msg lo
            send[7] = 0xb; // can msg lo
            send[8] = 0xa; // can msg lo
            send[9] = 0xa; // can msg lo
            send[10] = (byte)((checkSum(send)) >> 8);
            send[11] = (byte)((checkSum(send)) & 0xff);// checksum (excluding startbytes+command)

            SendData(send, 12);
            System.Threading.Thread.Sleep(1000);

            send[0] = 0x01; //send to pioneer
            send[1] = 0x00; //command
            send[2] = 0x00; //len (hi)
            send[3] = 0x3; //len (lo)
            send[4] = 0xfa; // can msg hi
            send[5] = 0xfb; // can msg lo
            send[6] = 0x5; // can msg lo
            send[7] = 0xb; // can msg lo
            send[8] = 0xa; // can msg lo
            send[9] = 0xa; // can msg lo
            send[10] = (byte)((checkSum(send)) >> 8);
            send[11] = (byte)((checkSum(send)) & 0xff);// checksum (excluding startbytes+command)

            SendData(send, 12);
            System.Threading.Thread.Sleep(1000);
            */
        }

        private void SendData(byte[] data,int length)
        {
            udpClient.Send(data, length, new IPEndPoint(ipUnicast, controlPort));
        }

        private void EnableMotors(byte[] send)
        {
         //   SendPulse();

            send[0] = 0x01; //send to pioneer
            send[1] = 0x00; //blank
            send[2] = 0x00; //len (hi)
            send[3] = 0x3; //len (lo) for bridge
            send[4] = 0xfa; // Pioneer start byte 1
            send[5] = 0xfb; // Pioneer start byte 2
            send[6] = 0x6; // pioneer packet length
            send[7] = 0x4; // pioneer command
            send[8] = 0x3b; // args
            send[9] = 0x01; // 
            send[10] = 0x00; // 
            send[11] = (byte)((checkSum(send)) >> 8);
            send[12] = (byte)((checkSum(send)) & 0xff);// checksum (excluding startbytes+command)

            SendData(send,13);

            System.Threading.Thread.Sleep(1000);
        }

        private void closeConnection()
        {

            syncIndex = 0;
            syncRecieved = 8;
            
            byte[] send = new byte[10];
            send[0] = 0x01; //send to pioneer
            send[1] = 0x00; //blank
            send[2] = 0x00; //len (hi)
            send[3] = 0x3; //len (lo) for bridge
            send[4] = 0xfa; // Pioneer start byte 1
            send[5] = 0xfb; // Pioneer start byte 2
            send[6] = 0x3; // pioneer packet length
            send[7] = 2; // pioneer command
            send[8] = 0x0; // args
            send[9] = 2; // checksum (excluding startbytes)

            SendData(send, 10);
            Console.WriteLine("reseting at sync: {0,1:d}", (int)syncIndex);

            System.Threading.Thread.Sleep(10);
        }

        private void MoveMotors(byte[] send)
        {
            //   SendPulse();

            send[0] = 0x01; //send to pioneer
            send[1] = 0x00; //blank
            send[2] = 0x00; //len (hi)
            send[3] = 0x3; //len (lo) for bridge
            send[4] = 0xfa; // Pioneer start byte 1
            send[5] = 0xfb; // Pioneer start byte 2
            send[6] = 0x6; // pioneer packet length
            send[7] = 0x8; // pioneer command
            send[8] = 0x3b; // args
            send[9] = 0x00; // 
            send[10] = 0x01; // 
            send[11] = (byte)((checkSum(send)) >> 8);
            send[12] = (byte)((checkSum(send)) & 0xff);// checksum (excluding startbytes+command)

            SendData(send, 13);

            System.Threading.Thread.Sleep(1000);
        }

        private void SendPulse()
        {
            byte[] send = new byte[10];
            //pulse
            send[0] = 0x01; //send to pioneer
            send[1] = 0x00; //blank
            send[2] = 0x00; //len (hi)
            send[3] = 0x3; //len (lo) for bridge
            send[4] = 0xfa; // Pioneer start byte 1
            send[5] = 0xfb; // Pioneer start byte 2
            send[6] = 0x3; // pioneer packet length
            send[7] = 0; // pioneer command
            send[8] = 0x0; // args
            send[9] = 0; // checksum (excluding startbytes+command)

            SendData(send, 10);
            
            System.Threading.Thread.Sleep(1000);
        }

		private void HandleStatus(BinaryReader br)
		{
			RMPStatusMsg msg = new RMPStatusMsg(br);
			Console.WriteLine("Got Status");
			//nothing to do
		}

		private void HandleShutdown(BinaryReader br)
		{
			RMPShutdownMsg msg = new RMPShutdownMsg(br);
			if (Shutdown != null) Shutdown(this, null);
		}

		private void HandleControl(BinaryReader br)
		{
			RMPControlMsg msg = new RMPControlMsg(br);
			//this should not happen
		}

		private void HandleMonitor8(BinaryReader br, double ts)
		{
			RMPMonitor8Msg msg = new RMPMonitor8Msg(br);

		}

		private void HandleMonitor7(BinaryReader br, double ts)
		{
			RMPMonitor7Msg msg = new RMPMonitor7Msg(br);
			status.powerbaseVoltage = msg.baseBatteryVoltage / 4.0;
			status.gainSchedule = msg.gainSchedule;
			status.operationalMode = msg.operationalMode;
			status.userInterfaceVoltage = 1.4 + (msg.uiBatteryVoltage * 0.0125);
		}

		private void HandleMonitor6(BinaryReader br, double ts)
		{
			RMPMonitor6Msg msg = new RMPMonitor6Msg(br);
			if (isBackwards)
			{
				status.motorTorqueLeft = msg.rightMotorTorque / 1463.0;
				status.motorTorqueRight =  msg.leftMotorTorque / 1463.0;
			}
			else 
			{
				status.motorTorqueLeft = msg.leftMotorTorque / 1463.0;
				status.motorTorqueRight = msg.rightMotorTorque / 1463.0;
			}
			status.wheelTorqueTs = ts;
			wheelTorqueTs = ts;
		}

		private void HandleMonitor5(BinaryReader br, double ts)
		{
			RMPMonitor5Msg msg = new RMPMonitor5Msg(br);
			if (isBackwards)
			{
				status.integratedForeAftPosition = -msg.integratedForeAftDisp * 2.0 / 40181.0;
				status.integratedTurnPosition = msg.integratedYawDisp * 2.0 / 11703.0;
			}
			else
			{
				status.integratedForeAftPosition = msg.integratedForeAftDisp * 2.0 / 40181.0;
				status.integratedTurnPosition = msg.integratedYawDisp * 2.0 / 11703.0;
			}
		}

		private void HandleMonitor4(BinaryReader br, double ts)
		{
			RMPMonitor4Msg msg = new RMPMonitor4Msg(br);
			if (isBackwards)
			{
				status.integratedLeftWheelPosition = -msg.integratedRightWheelDisp * 2.0 / 40181.0;
				status.integratedRightWheelPosition = -msg.integratedLeftWheelDisp * 2.0 / 40181.0;
			}
			else
			{
				status.integratedLeftWheelPosition = msg.integratedLeftWheelDisp * 2.0 / 40181.0;
				status.integratedRightWheelPosition = msg.integratedRightWheelDisp * 2.0 / 40181.0;
			}
			status.wheelPosTs = ts;
			wheelPosTs = ts;
			if (WheelPositionUpdate != null) WheelPositionUpdate(this, new TimestampedEventArgs<IRobotTwoWheelStatus>(ts, new SegwayStatus(status)));

		}

		private void HandleMonitor3(BinaryReader br, double ts)
		{
			RMPMonitor3Msg msg = new RMPMonitor3Msg(br);
			if (isBackwards)
			{
				status.leftWheelSpeed = -msg.rightWheelVel / 401.0;
				status.rightWheelSpeed = -msg.leftWheelVel / 401.0;
				status.yawRate = msg.yawRate / 7.8;
			}
			else
			{
				status.leftWheelSpeed = msg.leftWheelVel / 401.0;
				status.rightWheelSpeed = msg.rightWheelVel / 401.0;
				status.yawRate = msg.yawRate / 7.8;
			}
			status.servoFrameCounter = msg.servoFrames * .01;
			status.wheelSpeedTs = ts;
			wheelSpeedTs = ts;
			if (StatusUpdated != null) StatusUpdated(this, null);
			if (WheelSpeedUpdate != null) WheelSpeedUpdate(this, new TimestampedEventArgs<IRobotTwoWheelStatus>(ts, new SegwayStatus(status)));
		}

		private void HandleMonitor2(BinaryReader br, double ts)
		{
			RMPMonitor2Msg msg = new RMPMonitor2Msg(br);
			status.pitch = msg.pitchAngle / 7.8;
			status.pitchRate = msg.pitchRate / 7.8;
			status.roll = msg.rollAngle / 7.8;
			status.rollRate = msg.rollRate / 7.8;
		}

		private void HandleMonitor1(BinaryReader br, double ts)
		{
			RMPMonitor1Msg msg = new RMPMonitor1Msg(br);
			//nothing to do
		}

        private void HandleMotorsStoppedMsg(BinaryReader br, double ts)
		{
            MotorsStoppedMsg msg = new MotorsStoppedMsg(br);
			//nothing to do
		}

        private void HandleMotorsMovingMsg(BinaryReader br, double ts)
        {
            MotorsMovingMsg msg = new MotorsMovingMsg(br);
            if (isBackwards)
            {
                status.integratedLeftWheelPosition = -msg.integratedRightWheelDisp * 2.0 / 40181.0/1000;
                status.integratedRightWheelPosition = -msg.integratedLeftWheelDisp * 2.0 / 40181.0/1000;
            }
            else
            {
                status.integratedLeftWheelPosition = msg.integratedLeftWheelDisp * 2.0 / 40181.0/1000;
                status.integratedRightWheelPosition = msg.integratedRightWheelDisp * 2.0 / 40181.0/1000;
            }
            status.wheelPosTs = ts;
            wheelPosTs = ts;

            if (isBackwards)
            {
                status.leftWheelSpeed = -msg.rightWheelVel / 401.0;
                status.rightWheelSpeed = -msg.leftWheelVel / 401.0;
     //           status.yawRate = msg.yawRate / 7.8;
            }
            else
            {
                status.leftWheelSpeed = msg.leftWheelVel / 401.0;
                status.rightWheelSpeed = msg.rightWheelVel / 401.0;
    //            status.yawRate = msg.yawRate / 7.8;
            }
    //        status.servoFrameCounter = msg.servoFrames * .01;
            status.wheelSpeedTs = ts;
            wheelSpeedTs = ts;
            if (StatusUpdated != null) StatusUpdated(this, null);
            if (WheelSpeedUpdate != null) WheelSpeedUpdate(this, new TimestampedEventArgs<IRobotTwoWheelStatus>(ts, new SegwayStatus(status)));

        }

        private void HandleCommandZeroMsg(BinaryReader br, double ts, byte syncNumber)
		{
            CommandZeroMsg msg = new CommandZeroMsg(br);
            if (syncNumber == 4)
                syncIndex = 5;
		}

        private void HandleSync3(BinaryReader br, double ts)
        {
            Sync3Msg msg = new Sync3Msg(br);
            Console.WriteLine("0xff: {0,1:d}", (int)syncIndex);
            //if (syncIndex < 1)
     //           syncIndex = 0;
      //          syncRecieved = 8;
       //     closeConnection();
         //   closeConnection();
            //else
              //  syncIndex++;

      /*      if (syncIndex == 0 && syncNumber != 0) return;
            switch(syncNumber)
            {
                case 0:
                    syncIndex = 1;
                    break;
                case 1:
                    syncIndex = 2;
                    break;
                case 2:
                    syncIndex = 3;
                    break;
                case 3:
                    syncIndex = 4;
                    break;
                case 4:
                    syncIndex = 5;
                    break;
                case 5:
                    syncIndex = 6;
                    break;
                case 6:
                    syncIndex = 7;
                    break;
                default:
                    break;
            }*/
            
            
        }

        private void HandleSync0(BinaryReader br, double ts)
        {
            Sync0Msg msg = new Sync0Msg(br);
            Console.WriteLine("recieved sync: {0,1:d}", (int)syncIndex);
            if(syncIndex<1)
                syncIndex = 1;
            syncRecieved = 0;
            
        }

        private void HandleSync1(BinaryReader br, double ts)
        {
            Sync1Msg msg = new Sync1Msg(br);
            Console.WriteLine("recieved sync: {0,1:d}", (int)syncIndex);
            if (syncIndex < 2)
                syncIndex = 2;
            syncRecieved = 1;

        }

        private void HandleSync2(BinaryReader br, double ts)
        {
            Sync2Msg msg = new Sync2Msg(br);
            Console.WriteLine("recieved sync: {0,1:d}", (int)syncIndex);
            if (syncIndex < 3)
            syncIndex = 3;
            syncRecieved = 2;
            
        }

		private void HandleHeartbeat(BinaryReader br, double ts)
		{
			RMPHeartbeatMsg msg = new RMPHeartbeatMsg(br);
			Console.WriteLine("Got Heartbeat!");
		}



		public void GetWheelPositions(out double leftPosition, out double rightPosition, out double timestamp)
		{
			leftPosition = status.integratedLeftWheelPosition;
			rightPosition = status.integratedRightWheelPosition;
			timestamp = wheelPosTs;
		}

		public void GetWheelSpeeds(out double leftSpeed, out double rightSpeed, out double timestamp)
		{
			leftSpeed = status.leftWheelSpeed;
			rightSpeed = status.rightWheelSpeed;
			timestamp = wheelSpeedTs;
		}
		public void GetWheelTorque(out double leftTorque, out double rightTorque, out double timestamp)
		{
			leftTorque = status.motorTorqueLeft;
			rightTorque = status.motorTorqueRight;
			timestamp = wheelTorqueTs;
		}

        public static void sendHeartBeat(object state)
        {
            ((Pioneer3)state).SendPulse();
        }

		public class SegwayStatus : IRobotTwoWheelStatus
		{
			public SegwayStatus() { }
			public SegwayStatus(SegwayStatus toCopy)
			{
				this.pitch = toCopy.pitch;
				this.pitchRate = toCopy.pitchRate;
				this.roll = toCopy.roll;
				this.rollRate = toCopy.rollRate;
				this.leftWheelSpeed = toCopy.leftWheelSpeed;
				this.rightWheelSpeed = toCopy.rightWheelSpeed;
				this.yawRate = toCopy.yawRate;
				this.servoFrameCounter = toCopy.servoFrameCounter;
				this.integratedLeftWheelPosition = toCopy.integratedLeftWheelPosition;
				this.integratedRightWheelPosition = toCopy.integratedRightWheelPosition;
				this.integratedForeAftPosition = toCopy.integratedForeAftPosition;
				this.integratedTurnPosition = toCopy.integratedTurnPosition;
				this.powerbaseVoltage = toCopy.powerbaseVoltage;
				this.userInterfaceVoltage = toCopy.userInterfaceVoltage;
				this.motorTorqueLeft = toCopy.motorTorqueLeft;
				this.motorTorqueRight = toCopy.motorTorqueRight;
				this.gainSchedule = toCopy.gainSchedule;
				this.operationalMode = toCopy.operationalMode;
				this.wheelPosTs = toCopy.wheelPosTs;
				this.wheelTorqueTs = toCopy.wheelTorqueTs;
				this.wheelSpeedTs = toCopy.wheelSpeedTs;
			}

			//everythig here is in SI units and RADIANS!!!!
			public double pitch;
			public double pitchRate;
			public double roll;
			public double rollRate;
			public double leftWheelSpeed;
			public double rightWheelSpeed;
			public double yawRate;
			public double servoFrameCounter;
			public double integratedLeftWheelPosition;
			public double integratedRightWheelPosition;
			public double integratedForeAftPosition;
			public double integratedTurnPosition;
			public double powerbaseVoltage;
			public double userInterfaceVoltage;
			public double motorTorqueLeft;
			public double motorTorqueRight;
			public short gainSchedule;
			public short operationalMode;
			public double wheelPosTs;
			public double wheelTorqueTs;
			public double wheelSpeedTs;
			public override string ToString()
			{
				StringBuilder s = new StringBuilder();
				s.AppendLine("pitch: " + pitch + " deg");
				s.AppendLine("pitchRate: " + pitchRate + " deg/sec");
				s.AppendLine("roll: " + roll + " deg");
				s.AppendLine("rollRate: " + rollRate + " deg/sec");
				s.AppendLine("leftWheelSpeed: " + leftWheelSpeed + " m/s");
				s.AppendLine("rightWheelSpeed: " + rightWheelSpeed + " m/s");
				s.AppendLine("yawRate: " + yawRate + " deg/sec");
				s.AppendLine("servoFrameCounter: " + servoFrameCounter + " sec");
				s.AppendLine("integratedLeftWheelPosition: " + integratedLeftWheelPosition + " m");
				s.AppendLine("integratedRightWheelPosition: " + integratedRightWheelPosition + " m");
				s.AppendLine("integratedForeAftPosition: " + integratedForeAftPosition + " m");
				s.AppendLine("integratedTurnPosition: " + integratedTurnPosition + " revolutions");
				s.AppendLine("powerbaseVoltage: " + powerbaseVoltage + " VDC");
				s.AppendLine("userInterfaceVoltage: " + userInterfaceVoltage + " VDC");
				s.AppendLine("motorTorqueLeft: " + motorTorqueLeft + " Nm");
				s.AppendLine("motorTorqueRight: " + motorTorqueRight + " Nm");
				s.AppendLine("gainSchedule: " + gainSchedule + "");
				s.AppendLine("operationalMode: " + operationalMode + "");
				s.AppendLine("wheelPosTs: " + wheelPosTs + "");
				s.AppendLine("wheelTorqueTs: " + wheelTorqueTs + "");
				s.AppendLine("wheelSpeedTs: " + wheelSpeedTs + "");
				return s.ToString();
			}

			#region IRobotTwoWheelStatus Members

			public double LeftWheelSpeed
			{
				get { return leftWheelSpeed; }
			}

			public double RightWheelSpeed
			{
				get { return rightWheelSpeed; }
			}

			public double IntegratedLeftWheelPosition
			{
				get { return integratedLeftWheelPosition; }
			}

			public double IntegratedRightWheelPosition
			{
				get { return integratedRightWheelPosition; }
			}

			public string StatusString { get { return this.ToString(); } }
			#endregion
		}

		public SensorPose ToBody
		{
			get { return new SensorPose(); }
		}

		#region IRobot
		public double Height { get { return .43; } }
		public double Width { get { return .71; } }
		public double Length { get { return .56; } }
		public IRobotShape Shape { get { return IRobotShape.Circular; } }
		#endregion

		#region raw segway messages
		//these are the RAW messages sent to the RMP mcu. this is not really what 
		//users shoudl be populating if possible.
		internal struct RMPHeartbeatMsg
		{
			public byte canChannel;
			public short batteryStatus;
			public short batteryVoltage;
			public const ushort id = 0x0688;
			public RMPHeartbeatMsg(BinaryReader br)
			{
				br.ReadByte();
				canChannel = br.ReadByte();
				batteryStatus = Endian.BigToLittle(br.ReadInt16());
				batteryVoltage = Endian.BigToLittle(br.ReadInt16());
			}
		}

		internal struct RMPShutdownMsg
		{
			public const ushort id = 0x0412;
			//this message has no contents
			public RMPShutdownMsg(BinaryReader br) { }
		}

		internal struct RMPControlMsg
		{
			public const ushort id = 0x0413;
			public short velocityCommand;
			public short turningCommand;
			public RMPConfigurationCommand configurationCommand;
			public Int16 configurationParameter;
			public RMPControlMsg(double velocity, double turn)
			{
				velocityCommand = (short)(velocity * 147.0);
				turningCommand = (short)turn;
				configurationCommand = RMPConfigurationCommand.RMPNone;
				configurationParameter = 0;
			}
			public RMPControlMsg(BinaryReader br)
			{
				velocityCommand = 0;
				turningCommand = 0;
				configurationCommand = 0;
				configurationParameter = 0;
			}
		}

		internal enum RMPConfigurationCommand : short
		{
			RMPNone = 0,
			RMPSetMaxVelocity = 10,
			RMPSetMaxAccelScaleFactor = 11,
			RMPSetMaxTurnRateScaleFactor = 12,
			RMPSetGainSchedule = 13,
			RMPSetCurrentLimitScaleFactor = 14,
			RMPSetBalanceModeLockout = 15,
			RMPSetOperationalMode = 16,
			RMPResetIntegrators = 50
		}

		internal enum RMPBitfieldResetIntegrators : byte
		{
			RMPRightWheelDisplacement = 1,
			RMPLeftWheelDisplacement = 2,
			RMPYawDisplacement = 4,
			RMPForeAftDisplacement = 8
		}

		internal struct RMPStatusMsg
		{
			//bits 0,1 indicate the controller mode
			//bit 2 indicates wake control
			public byte status;
			public const ushort id = 0x680;
			public RMPStatusMsg(BinaryReader br)
			{ status = 0x0; }
		}

		internal enum RMPBitfieldStatus
		{
			RMPDisabled = 0,
			RMPTractor = 1,
			RMPBalance = 2,
			RMPUnknown = 3
		}

		internal struct RMPMonitor1Msg
		{
			public const ushort id = 0x0400;
			//undefined all slots	
			public RMPMonitor1Msg(BinaryReader br)
			{ }
		}

        internal struct MotorsStoppedMsg
        {
            public const byte id = 0x32;
            //undefined all slots	
            public MotorsStoppedMsg(BinaryReader br)
            { }
        }

        internal struct MotorsMovingMsg
        {
            public const byte id = 0x33;

            public int integratedLeftWheelDisp;
            public int integratedRightWheelDisp;

   //         public int integratedForeAftDisp;
   //         public int integratedYawDisp;

			public short leftWheelVel;
			public short rightWheelVel;
		//	public short yawRate;
		//	public ushort servoFrames;
            public MotorsMovingMsg(BinaryReader br)
			{
                byte[] buffer = new byte[2];
               // buffer[1] = br.ReadByte();
                //buffer[0] = br.ReadByte();
                BinaryReader br2 = new BinaryReader(new MemoryStream(buffer));
                integratedLeftWheelDisp = br.ReadInt16();



                integratedRightWheelDisp = br.ReadInt16();

                //read heading
                br.ReadInt16();

				leftWheelVel = br.ReadInt16();
				rightWheelVel = br.ReadInt16();
			//	yawRate = Endian.BigToLittle(br.ReadInt16());
			//	servoFrames = Endian.BigToLittle(br.ReadUInt16());
			}
        }

        internal struct CommandZeroMsg
        {
            public const byte id = 0x00;
            //undefined all slots	
            public CommandZeroMsg(BinaryReader br)
            { }
        }

        internal struct Sync0Msg
        {
            public const byte id = 0x00;
            //undefined all slots	
            public Sync0Msg(BinaryReader br)
            {}
        }

        internal struct Sync1Msg
        {
            public const byte id = 0x01;
            //undefined all slots	
            public Sync1Msg(BinaryReader br)
            { }
        }

        internal struct Sync2Msg
        {
            public const byte id = 0x02;
            //undefined all slots	
            public Sync2Msg(BinaryReader br)
            {}
        }

        internal struct Sync3Msg
        {
            public const byte id = 0xff;
            //undefined all slots	
            public Sync3Msg(BinaryReader br)
            {}
        }

		internal struct RMPMonitor2Msg
		{
			public const ushort id = 0x0401;
			public short pitchAngle;
			public short pitchRate;
			public short rollAngle;
			public short rollRate;
			public RMPMonitor2Msg(BinaryReader br)
			{
				pitchAngle = Endian.BigToLittle(br.ReadInt16());
				pitchRate = Endian.BigToLittle(br.ReadInt16());
				rollAngle = Endian.BigToLittle(br.ReadInt16());
				rollRate = Endian.BigToLittle(br.ReadInt16());
			}
		}
		internal struct RMPMonitor3Msg
		{
			public const ushort id = 0x0402;
			public short leftWheelVel;
			public short rightWheelVel;
			public short yawRate;
			public ushort servoFrames;
			public RMPMonitor3Msg(BinaryReader br)
			{
				leftWheelVel = Endian.BigToLittle(br.ReadInt16());
				rightWheelVel = Endian.BigToLittle(br.ReadInt16());
				yawRate = Endian.BigToLittle(br.ReadInt16());
				servoFrames = Endian.BigToLittle(br.ReadUInt16());
			}
		}
		internal struct RMPMonitor4Msg
		{
			public const ushort id = 0x0403;
			public int integratedLeftWheelDisp;
			public int integratedRightWheelDisp;
			public RMPMonitor4Msg(BinaryReader br)
			{
				byte[] buffer = new byte[4];
				buffer[1] = br.ReadByte();
				buffer[0] = br.ReadByte();
				buffer[3] = br.ReadByte();
				buffer[2] = br.ReadByte();
				BinaryReader br2 = new BinaryReader(new MemoryStream(buffer));
				integratedLeftWheelDisp = br2.ReadInt32();

				buffer[1] = br.ReadByte();
				buffer[0] = br.ReadByte();
				buffer[3] = br.ReadByte();
				buffer[2] = br.ReadByte();
				br2 = new BinaryReader(new MemoryStream(buffer));

				integratedRightWheelDisp = br2.ReadInt32();
			}
		}
		internal struct RMPMonitor5Msg
		{
			public const ushort id = 0x0404;
			public int integratedForeAftDisp;
			public int integratedYawDisp;

			public RMPMonitor5Msg(BinaryReader br)
			{
				byte[] buffer = new byte[4];
				buffer[1] = br.ReadByte();
				buffer[0] = br.ReadByte();
				buffer[3] = br.ReadByte();
				buffer[2] = br.ReadByte();
				BinaryReader br2 = new BinaryReader(new MemoryStream(buffer));
				integratedForeAftDisp = br2.ReadInt32();

				buffer[1] = br.ReadByte();
				buffer[0] = br.ReadByte();
				buffer[3] = br.ReadByte();
				buffer[2] = br.ReadByte();
				br2 = new BinaryReader(new MemoryStream(buffer));

				integratedYawDisp = br2.ReadInt32();
			}
		}
		internal struct RMPMonitor6Msg
		{
			public const ushort id = 0x0405;
			public short leftMotorTorque;
			public short rightMotorTorque;

			public RMPMonitor6Msg(BinaryReader br)
			{
				leftMotorTorque = Endian.BigToLittle(br.ReadInt16());
				rightMotorTorque = Endian.BigToLittle(br.ReadInt16());
			}
		}
		internal struct RMPMonitor7Msg
		{
			public const ushort id = 0x0406;
			public short operationalMode;
			public short gainSchedule;
			public short uiBatteryVoltage;
			public short baseBatteryVoltage;
			public RMPMonitor7Msg(BinaryReader br)
			{
				operationalMode = Endian.BigToLittle(br.ReadInt16());
				gainSchedule = Endian.BigToLittle(br.ReadInt16());
				uiBatteryVoltage = Endian.BigToLittle(br.ReadInt16());
				baseBatteryVoltage = Endian.BigToLittle(br.ReadInt16());
			}
		}
		internal struct RMPMonitor8Msg
		{
			public const ushort id = 0x0407;
			public short velocityCommand;
			public short turnCommand;
			public RMPMonitor8Msg(BinaryReader br)
			{
				velocityCommand = Endian.BigToLittle(br.ReadInt16());
				turnCommand = Endian.BigToLittle(br.ReadInt16());
			}
		}
		#endregion


	}//end pioneer3
}
