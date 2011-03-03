using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common;
using Magic.Common.Mapack;

namespace Magic.Rendering.Renderables
{
	public class PoseTrackRenderer : IRender
	{
		string name;
		List<RobotPose> poses = new List<RobotPose>();
		GLPen pen;
		GLPen ellipsePen;
		int maxPoints = 100000;
		public PoseTrackRenderer(Color c, string name)
		{
			this.name = name;
			pen = new GLPen(c, 1.0f);
			ellipsePen = new GLPen(Color.LightBlue, 1.0f);
		}

		public void AddPose(RobotPose p)
		{
			lock (poses)
			{
				poses.Add(p);
				if (poses.Count > maxPoints)
					poses.RemoveAt(0);
			}
		}

		#region IRender Members

		public string GetName()
		{
			return name;
		}

        public Vector2 LastPose
        {
            get {
                if (poses.Count > 0)
                    return poses[0].ToVector2();
                else
                    return new Vector2 (0,0);
            } 
        }
		public void Draw(Renderer cam)
		{
			if (poses.Count == 0) return;
			lock (poses)
			{
				RobotPose lastPose = poses[0];
				foreach (RobotPose p in poses)
				{
					GLUtility.DrawCross(pen, p.ToVector2 (), .2f);
					//GLUtility.DrawLine(pen, lastPose.ToVector2(), p.ToVector2());
					lastPose = p;
				}

				if (poses[poses.Count - 1].covariance[0, 0] != 0)
				{
					//calculate the error ellipse
					Matrix cov = new Matrix(2, 2);
					cov[0, 0] = poses[poses.Count - 1].covariance[0, 0];
					cov[0, 1] = poses[poses.Count - 1].covariance[0, 1];
					cov[1, 0] = poses[poses.Count - 1].covariance[1, 0];
					cov[1, 1] = poses[poses.Count - 1].covariance[1, 1];

					double theta = .5 * Math.Atan2((-2 * cov[0, 1]) , (cov[0, 0] - cov[1, 1]));
					if (Double.IsNaN(theta)) theta = 0;
					double sigu2 = (cov[0, 0] * Math.Sin(theta) * Math.Sin(theta)) + 
													(2 * cov[0, 1] * Math.Sin(theta) * Math.Cos(theta)) + 
													(cov[1, 1] * Math.Cos(theta) * Math.Cos(theta));

					double sigv2 = (cov[0, 0] * Math.Cos(theta) * Math.Cos(theta)) +
													(2 * cov[0, 1] * Math.Sin(theta) * Math.Cos(theta)) +
													(cov[1, 1] * Math.Sin(theta) * Math.Sin(theta));

					GLUtility.GoToTransformXYZ((float)poses[poses.Count - 1].x,(float) poses[poses.Count - 1].y, 0);
					GLUtility.GoToTransformYPR((float)theta, 0, 0);

					GLUtility.DrawEllipse(ellipsePen, new RectangleF((float)-sigu2 / 2, (float)-sigv2 / 2, (float)sigu2, (float)sigv2));
					GLUtility.ComeBackFromTransform();
					GLUtility.ComeBackFromTransform();
				}
			}
		}

		public void ClearBuffer()
		{
			poses = new List<RobotPose>();
		}

		public bool VehicleRelative
		{
			get { return false; }
		}

		public int? VehicleRelativeID
		{
			get { return null; }
		}

		#endregion
	}
}
