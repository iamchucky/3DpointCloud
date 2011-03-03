using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Magic.Common;
using Magic.Common.Mapack;

namespace Magic.Rendering.Renderables
{
	public class MeanCovarianceRenderer : IRender
	{
		Color meanColor, covColor;
		float width, penThickness;
		List<List<Vector2>> covariance;
		List<List<Vector2>> mean;
		int numCovPt;
		int sigma;
		bool drawMean, drawCov;
		object dataLock;
		public MeanCovarianceRenderer(Color meanColor, Color covColor, float penThickness, float width, int numPtforCovariance, int sigma, bool drawMean, bool drawCov)
		{
			this.meanColor = meanColor;
			this.covColor = covColor;
			this.width = width;
			this.penThickness = penThickness;
			this.numCovPt = numPtforCovariance;
			this.sigma = sigma;
			this.drawCov = drawCov;
			this.drawMean = drawMean;

			mean = new List<List<Vector2>>();
			covariance = new List<List<Vector2>>();
		}

		public void UpdateMeanCovariance(List<Vector2> pt, List<Matrix> cov, RobotPose currentPose)
		{
			mean.Clear(); covariance.Clear();
			if (pt.Count != cov.Count)
				throw new ArgumentException("Length of List<Vector2> and List<Matrix> do not match");
			else
			{
				for (int i = 0; i < pt.Count; i++)
				{
					double x = pt[i].X;// pt[i].X * Math.Cos(currentPose.yaw) - pt[i].Y * Math.Sin(currentPose.yaw) + currentPose.x;
					double y = pt[i].Y;// pt[i].X * Math.Sin(currentPose.yaw) + pt[i].Y * Math.Cos(currentPose.yaw) + currentPose.y;
					mean.Add(new List<Vector2>());
					covariance.Add(new List<Vector2>());
					// mean
					//mean[i].Add(new Vector2(x - width / 2, y - width / 2));
					//mean[i].Add(new Vector2(x + width / 2, y - width / 2));
					//mean[i].Add(new Vector2(x + width / 2, y + width / 2));
					//mean[i].Add(new Vector2(x - width / 2, y + width / 2));
					mean[i].Add(new Vector2(x, y));

					// covariance ellipse
					Matrix ellipse = GenerateEllipse(numCovPt, x, y, cov[i], sigma);
					for (int j = 0; j < ellipse.Rows; j++)
					{
						covariance[i].Add(new Vector2(ellipse[j, 0], ellipse[j, 1]));
					}
				}
			}
		}

		public void UpdateMeanCovariance(Vector2 pt, Matrix cov)
		{
			List<Vector2> ptList = new List<Vector2>();
			List<Matrix> covList = new List<Matrix>();
			ptList.Add(pt); covList.Add(cov);
			UpdateMeanCovariance(ptList, covList, new RobotPose());
		}

		private Matrix GenerateEllipse(int numPoint, double centerX, double centerY, Matrix sMatrix, int numSigma)
		{
			Matrix ep = new Matrix(numPoint, 2); // ellipse points
			// generate points in a circle
			for (int i = 0; i < numPoint; i++)
			{
				ep[i, 0] = numSigma * Math.Cos(2 * Math.PI / numPoint * i);
				ep[i, 1] = numSigma * Math.Sin(2 * Math.PI / numPoint * i);
			}

			// skew the ellipse
			CholeskyDecomposition chol = new CholeskyDecomposition(sMatrix);
			ep = chol.LeftTriangularFactor.Transpose() * ep.Transpose();
			ep = ep.Transpose();

			// re-center the ellipse
			Matrix ones = new Matrix(numPoint, 2);
			for (int i = 0; i < numPoint; i++)
			{
				ep[i, 0] = ep[i, 0] + centerX;
				ep[i, 1] = ep[i, 1] + centerY;
			}
			return ep;
		}
		#region IRender Members

		public string GetName()
		{
			return "MeanCovarianceRenderer";
		}

		public void Draw(Renderer cam)
		{
			if (mean != null && covariance != null)
			{

				List<List<Vector2>> meanCpy = new List<List<Vector2>>(mean);
				foreach (List<Vector2> list in meanCpy)
				{
					if (drawMean)
					{
						for (float i = width; i >= 0; i -= 0.1f)
							GLUtility.DrawDiamond(new GLPen(meanColor, penThickness), list[0], i);
					}
					if (drawCov)
					{
						try
						{
							GLUtility.DrawLineLoop(new GLPen(covColor, penThickness), covariance[meanCpy.IndexOf(list)].ToArray());
						}
						catch
						{ }
					}
				}
			}
		}

		public void ClearBuffer()
		{
			mean.Clear();
			covariance.Clear();
		}

		public bool VehicleRelative
		{
			get { return false; }
		}

		public int? VehicleRelativeID
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
