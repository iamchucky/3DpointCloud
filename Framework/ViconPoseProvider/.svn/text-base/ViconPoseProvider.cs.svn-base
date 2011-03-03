using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Magic.Common;
using Magic.Common.Sensors;
using Magic.ViconInterface;
using Magic.Common.Mapack;

namespace Magic.ViconPoseProvider
{
	public class ViconPoseProvider : IPoseProvider, IDisposable
	{
		private RobotPose pose;
		private ViconTarsus vicon;
		private String subject;
		private String segment;
		private IPAddress ip;
		private Int32 port;

		private List<String> wantedChannelNames;
		private List<int> wantedChannelNumbers;

		public ViconPoseProvider(String ip, Int32 port, String subject, String segment)
		{
			this.ip = IPAddress.Parse(ip);
			this.port = port;
			this.subject = subject;
			this.segment = segment;

			wantedChannelNames = new List<string>(6);
			pose = new RobotPose();
			vicon = new ViconTarsus(ip, port);
			vicon.Start();

			vicon.DataReceived += new EventHandler<IViconFrameEventArgs>(vicon_DataReceived);

			wantedChannelNames.Add(subject + ":" + segment + " <t-X>");
			wantedChannelNames.Add(subject + ":" + segment + " <t-Y>");
			wantedChannelNames.Add(subject + ":" + segment + " <t-Z>");
			wantedChannelNames.Add(subject + ":" + segment + " <a-X>");
			wantedChannelNames.Add(subject + ":" + segment + " <a-Y>");
			wantedChannelNames.Add(subject + ":" + segment + " <a-Z>");

			wantedChannelNumbers = vicon.GetChannelNumbers(wantedChannelNames);
			vicon.SetStreamingChannels(wantedChannelNumbers);
			vicon.SetStreaming(true);
		}

		void vicon_DataReceived(object sender, IViconFrameEventArgs e)
		{
			if (e.Frame != null && e.Frame.Capacity > 0)
			{
				double ax, ay, az, magnitude, tempX, tempY, tempZ, cosine, sine, temp;
				Matrix3 matrix = new Matrix3();

				pose.x = (e.Frame.ElementAt(0))/1000;
				pose.y = (e.Frame.ElementAt(1))/1000;
				pose.z = (e.Frame.ElementAt(2))/1000;
                pose.timestamp = TimeSync.CurrentTime;

				// Changing from axis-angle that Vicon gives to rotation matrix
				ax = e.Frame.ElementAt(3);
				ay = e.Frame.ElementAt(4);
				az = e.Frame.ElementAt(5);

				magnitude = Math.Sqrt(ax * ax + ay * ay + az * az);

				tempX = ax / magnitude;
				tempY = ay / magnitude;
				tempZ = az / magnitude;

				cosine = Math.Cos(magnitude);
				sine = Math.Sin(magnitude);

				temp = 1 - cosine;

				matrix[0, 0] = cosine + temp * tempX * tempX;
				matrix[0, 1] = temp * tempX * tempY + sine * tempZ;
				matrix[0, 2] = temp * tempX * tempZ - sine * tempY;
				matrix[1, 0] = temp * tempY * tempX - sine * tempZ;
				matrix[1, 1] = cosine + temp * tempY * tempY;
				matrix[1, 2] = temp * tempY * tempZ + sine * tempX;
				matrix[2, 0] = temp * tempZ * tempX + sine * tempY;
				matrix[2, 1] = temp * tempZ * tempY - sine * tempX;
				matrix[2, 2] = cosine + temp * tempZ * tempZ;

				// Changing from rotation matrix to pitch, yaw, roll
				pose.pitch = matrix.ExtractYPR().Y;
				pose.yaw = -matrix.ExtractYPR().X;
				pose.roll = -matrix.ExtractYPR().Z;
				
				if (NewPoseAvailable != null)
					NewPoseAvailable(this, new NewPoseAvailableEventArgs(pose));
			}
		}

		#region IPoseProvider Members

		public event EventHandler<NewPoseAvailableEventArgs> NewPoseAvailable;

		public RobotPose Pose
		{
			get { return pose; }
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			vicon.Dispose();
		}

		#endregion
	}
}
