namespace RobotDriveControl
{
	partial class RobotDriveControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RobotDriveControl));
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.pbGUI = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pbGUI)).BeginInit();
			this.SuspendLayout();
			// 
			// label4
			// 
			this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label4.BackColor = System.Drawing.Color.Transparent;
			this.label4.Location = new System.Drawing.Point(3, 98);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(10, 13);
			this.label4.TabIndex = 14;
			this.label4.Text = "L";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.label3.Location = new System.Drawing.Point(237, 99);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(13, 12);
			this.label3.TabIndex = 13;
			this.label3.Text = "R";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.label2.Location = new System.Drawing.Point(118, 188);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(17, 12);
			this.label2.TabIndex = 12;
			this.label2.Text = "B";
			this.label2.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.Location = new System.Drawing.Point(105, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(45, 10);
			this.label1.TabIndex = 11;
			this.label1.Text = "F";
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// pbGUI
			// 
			this.pbGUI.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.pbGUI.BackColor = System.Drawing.Color.White;
			this.pbGUI.ErrorImage = ((System.Drawing.Image)(resources.GetObject("pbGUI.ErrorImage")));
			this.pbGUI.Image = global::RobotDriveControl.Properties.Resources.cross;
			this.pbGUI.InitialImage = ((System.Drawing.Image)(resources.GetObject("pbGUI.InitialImage")));
			this.pbGUI.Location = new System.Drawing.Point(15, 15);
			this.pbGUI.Name = "pbGUI";
			this.pbGUI.Size = new System.Drawing.Size(220, 170);
			this.pbGUI.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.pbGUI.TabIndex = 10;
			this.pbGUI.TabStop = false;
			this.pbGUI.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pbGUI_MouseMove);
			this.pbGUI.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pbGUI_MouseDown);
			this.pbGUI.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pbGUI_MouseUp);
			// 
			// RobotDriveControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.pbGUI);
			this.Name = "RobotDriveControl";
			this.Size = new System.Drawing.Size(250, 200);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RobotDriveControl_KeyUp);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RobotDriveControl_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.pbGUI)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.PictureBox pbGUI;
	}
}
