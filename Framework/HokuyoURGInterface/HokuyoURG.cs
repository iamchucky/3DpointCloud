using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common;
using System.IO.Ports;
using Magic.Common.Sensors;
using System.Threading;
using System.Diagnostics;
using Magic.Common.Messages;
using Magic.Network;
using Magic.Segway;
using Magic.SickInterface;

namespace Magic.HokuyoURGInterface
{
    public class HokuyoURG : ILidar2D
    {

        SerialPort sp;
        ASCIIEncoding conv = new ASCIIEncoding();
        int scannerId;
        string port;
        bool isRunning = true;
        bool useHokuyoTimestamp = false;
        double timeStamp = 0;
        SensorPose sensorPose;
        SegwayRMP50 segway;

        public HokuyoURG(string port, int scannerId, bool useHokuyoTimestamp)
        {
            this.port = port;
            this.scannerId = scannerId;
            this.sensorPose = new SensorPose(0, 0, 0, 0, 0, 0, 0);
            this.useHokuyoTimestamp = useHokuyoTimestamp;
            if (!useHokuyoTimestamp)
            {
                INetworkAddressProvider addressProvider = new HardcodedAddressProvider();
                segway = new SegwayRMP50(addressProvider.GetAddressByName("SegwayFeedback"), addressProvider.GetAddressByName("SegwayControl"), false, 1);
                segway.Start();
                segway.WheelPositionUpdate += new EventHandler<TimestampedEventArgs<Magic.Common.Robots.IRobotTwoWheelStatus>>(segway_WheelPositionUpdate);
            }
        }

        void segway_WheelPositionUpdate(object sender, TimestampedEventArgs<Magic.Common.Robots.IRobotTwoWheelStatus> e)
        {
            timeStamp = e.TimeStamp;
        }

        public HokuyoURG(string port, int scannerId, SensorPose sensorPose, bool useHokuyoTimestamp)
        {
            this.port = port;
            this.scannerId = scannerId;
            this.sensorPose = sensorPose;
            this.useHokuyoTimestamp = useHokuyoTimestamp;
			if (!useHokuyoTimestamp)
			{
				INetworkAddressProvider addressProvider = new HardcodedAddressProvider();
				//segway = new SegwayRMP50(addressProvider.GetAddressByName("SegwayFeedback"), addressProvider.GetAddressByName("SegwayControl"), false, 1);
				//segway.Start();
				//segway.WheelPositionUpdate += new EventHandler<TimestampedEventArgs<Magic.Common.Robots.IRobotTwoWheelStatus>>(segway_WheelPositionUpdate);
				ILidar2D lidar = new SickLMS(addressProvider.GetAddressByName("Sick"), new SensorPose(), true);
				lidar.ScanReceived += new EventHandler<ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>>(lidar_ScanReceived);
				lidar.Start();
			}
        }

		void lidar_ScanReceived(object sender, ILidarScanEventArgs<ILidarScan<ILidar2DPoint>> e)
		{
			timeStamp = e.Scan.Timestamp;
		}

        void SetContinuousMode()
        {
            byte[] msg = conv.GetBytes("MD0044072501000;\n");
            sp.Write(msg, 0, msg.Length);
        }

		void SetBaudrate(int rate)
		{
			byte[] msg = new byte[1];
			if (rate == 19200)
				msg = conv.GetBytes("SS019200;\n");
			else if(rate == 57600)
				msg = conv.GetBytes("SS057600;\n");
			else if(rate == 115200)
				msg = conv.GetBytes("SS115200;\n");
			else if (rate == 250000)
				msg = conv.GetBytes("SS250000;\n");
			else if (rate == 500000)
				msg = conv.GetBytes("SS500000;\n");
			else if (rate == 750000)
				msg = conv.GetBytes("SS750000;\n");
			sp.Write(msg, 0, msg.Length);
		}

        private void SetSCIPv2()
        {
            byte[] msg = conv.GetBytes("SCIP2.0\n");
            sp.Write(msg, 0, msg.Length);
        }

        void StartRead(object o)
        {
            Stopwatch sw = new Stopwatch();
            while (isRunning)
            {
                if (sp.BytesToRead != 0)
                {
                    //Console.WriteLine(sp.BytesToRead);
                    string s = sp.ReadExisting();
                    if (s.Length == 2138) // specific for 240 degree reading
                    {
                        //sw.Reset();
                        //sw.Start();
                        ConvertToRange(s);
                        //Console.WriteLine(sw.ElapsedMilliseconds + " ms");
                    }
                }
                Thread.Sleep(10);
            }
        }

        private void ConvertToRange(string s)
        {
            HokuyoURGScan scan = new HokuyoURGScan();
            scan.points = new List<ILidar2DPoint>();

            string msg = s;
            string timeStampStr = msg.Substring(21, 4);
            double timeStamp = ((timeStampStr[0] - 0x30) * 64 * 64 * 64) + ((timeStampStr[1] - 0x30) * 64 * 64) +
                               ((timeStampStr[2] - 0x30) * 64) + (timeStampStr[3] - 0x30);

            msg = msg.Remove(0, 27);
            int dataCount = msg.Length / 66;
            string onlyData = "";
            for (int i = 0; i < dataCount; i++)
            {
                // remove all the line feed (LF)
                onlyData += msg.Substring(0, 64);
                msg = msg.Remove(0, 66);
            }
            onlyData += msg.Substring(0, msg.Length - 3);

            double angleResolution = 240.0 / 682.0;
            int first, second, third;
            float range;
            for (int j = 0; j < onlyData.Length; j += 3)
            {
                if (onlyData[j] == 10 || onlyData[j + 1] == 10 || onlyData[j + 2] == 10)
                    continue;
                first = onlyData[j] - 0x30;
                second = onlyData[j + 1] - 0x30;
                third = onlyData[j + 2] - 0x30;
                range = (float)((first * 64 * 64 + second * 64 + third) / 1000.0);
                scan.points.Add(new HokuyoURGPoint(new RThetaCoordinate(range, (float)((angleResolution * j / 3 - 120) * Math.PI / 180.0), (float)(angleResolution * j / 3 - 120))));
                //Console.WriteLine(range);
            }

            scan.scannerID = this.scannerId;
            if (useHokuyoTimestamp)
                scan.timestamp = timeStamp;
            else
                scan.timestamp = this.timeStamp;

            if (ScanReceived != null)
                ScanReceived(this, new ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>(scan));
        }


        #region ILidar2D Members

        public event EventHandler<ILidarScanEventArgs<ILidarScan<ILidar2DPoint>>> ScanReceived;

        #endregion

        #region ISensor Members

        public void Start()
        {
            sp = new SerialPort(port);
			sp.BaudRate = 115200;
            sp.Open();
			//Status();
			SetSCIPv2();
			SetContinuousMode();
			SetBaudrate(115200);

            Thread t = new Thread(new ParameterizedThreadStart(StartRead));
            t.Start();
        }

		void Reset()
		{
			byte[] msg = conv.GetBytes("RS;\n");
			sp.Write(msg, 0, msg.Length);
		}

        public void Start(System.Net.IPAddress localBind)
        {
            Start();
        }

		void Status()
		{
			byte[] msg = conv.GetBytes("II;\n");
			sp.Write(msg, 0, msg.Length);
		}

        public void Stop()
        {
            byte[] msg = conv.GetBytes("QT;\n");
            sp.Write(msg, 0, msg.Length);
            isRunning = false;
        }

        public Magic.Common.SensorPose ToBody
        {
            get { return sensorPose; }
        }

        #endregion
    }
}
