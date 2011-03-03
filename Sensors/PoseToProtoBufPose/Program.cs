﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Network;
using Magic.Common.Messages;
using Magic.Common;
using System.Net;
using System.Threading;

namespace PoseToProtoBufPose
{
    class Program
    {
        static INetworkAddressProvider addressProvider;
        static GenericMulticastClient<RobotPoseMessage> poseClient;
        static GenericMulticastServer<Magic.Proto.Pose> protoPoseServer;

        static RobotPose pose;
        static Magic.Proto.Pose protoPose;

        static void Main(string[] args)
        {
            protoPose = new Magic.Proto.Pose();
            StartNetwork();
            while (true)
            {
                Thread.Sleep(10);
            }
        }

        static void StartNetwork()
        {
            addressProvider = new HardcodedAddressProvider();

            poseClient = new GenericMulticastClient<RobotPoseMessage>(addressProvider.GetAddressByName("RobotPoseTRUTH"), new CSharpMulticastSerializer<RobotPoseMessage>(true));
            poseClient.Start();
            poseClient.MsgReceived += new EventHandler<MsgReceivedEventArgs<RobotPoseMessage>>(poseClient_MsgReceived);

            protoPoseServer = new GenericMulticastServer<Magic.Proto.Pose>(new NetworkAddress("RobotPosePROTO", IPAddress.Parse("239.132.1.198"), 30198, NetworkAddressProtocol.UDP_MULTI),
                new ProtoBuffSerializer<Magic.Proto.Pose>());
            protoPoseServer.Start(NetworkAddress.GetBindingAddressByType(NetworkAddress.BindingType.Wired));
        }

        static void poseClient_MsgReceived(object sender, MsgReceivedEventArgs<RobotPoseMessage> e)
        {
            if (e.message == null) return;
            pose = e.message.Pose;
            Console.WriteLine(pose.x + "\t" + pose.y + "\t" + pose.yaw + "\t" + pose.timestamp);
            protoPose.x = pose.x;
            protoPose.y = pose.y;
            protoPose.z = pose.z;
            protoPose.yaw = pose.yaw;
            protoPose.pitch = pose.pitch;
            protoPose.roll = pose.roll;
            protoPose.timeStamp = pose.TimeStamp;

            protoPoseServer.SendUnreliably(protoPose);
            Console.WriteLine("Seccessfully sent out ProtoPose @ " + protoPose.timeStamp);
        }
    }
}
