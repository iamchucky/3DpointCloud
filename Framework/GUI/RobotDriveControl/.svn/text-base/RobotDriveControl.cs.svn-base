using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Magic.Common.Robots;

namespace RobotDriveControl
{
	public partial class RobotDriveControl : UserControl
	{


		double velCmd = 0;

		public double VelCmd
		{
			get { return velCmd; }			
		}
		double turnCmd = 0;

		public double TurnCmd
		{
			get { return turnCmd; }			
		}
		double gain = .5;

		public double Gain
		{
			get { return gain; }
			set { gain = value; }
		}
		bool lowpass = false;

		public bool Lowpass
		{
			get { return lowpass; }
			set { lowpass = value; }
		}
		
		public RobotDriveControl()
		{
			InitializeComponent();
		}
		

		private void pbGUI_MouseDown(object sender, MouseEventArgs e)
		{
			pbGUI_MouseMove(sender, e);
		}

		private void pbGUI_MouseUp(object sender, MouseEventArgs e)
		{
			Console.Write("Reset");
			//this need to be made less "violent"
			velCmd = 0;
			turnCmd = 0;
		}

		private void pbGUI_MouseMove(object sender, MouseEventArgs e)
		{
			int widthDiv2 = pbGUI.Width / 2;
			int heightDiv2 = pbGUI.Height / 2;
			if (e.Button == MouseButtons.Left)
			{
				int recenterX = e.X - widthDiv2;
				int recenterY = e.Y - heightDiv2;
				if (recenterX > 160) recenterX = widthDiv2;
				if (recenterX < -160) recenterX = -widthDiv2;
				if (recenterY > 160) recenterY = heightDiv2;
				if (recenterY < -160) recenterY = -heightDiv2;
				recenterY *= -1;
				recenterX *= -1;

				if (lowpass)
				{
					velCmd = (gain) * velCmd + (1 - gain) * (recenterY / 20.0);
					turnCmd = (gain) * turnCmd + (1 - gain) * recenterX * 5;
				}
				else
				{
					velCmd = recenterY / 20.0;
					turnCmd = recenterX * 5;
				}

			}
		}

		private void RobotDriveControl_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Left)
				turnCmd = e.Control ? 200 : 100;
			if (e.KeyCode == Keys.Right)
				turnCmd = e.Control ? -200 : -100;
			if (e.KeyCode == Keys.Up)
				velCmd = e.Control ? 2 : 1;
			if (e.KeyCode == Keys.Down)
				velCmd = e.Control ? -2 : -1;
		}

		private void RobotDriveControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Right)
				turnCmd = 0;
			if (e.KeyCode == Keys.Left)
				turnCmd = 0;
			if (e.KeyCode == Keys.Up)
				velCmd = 0;
			if (e.KeyCode == Keys.Down)
				velCmd = 0;
		}

		public RobotTwoWheelCommand GetCommand()
		{
			return new RobotTwoWheelCommand(velCmd, turnCmd);
		}
	}
}
