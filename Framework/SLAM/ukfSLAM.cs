using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Sensors;
using Magic.Common;
using System.Threading;
using Magic.Common.Mapack;
using lpsolve55;

namespace Magic.SLAM
{
	public class SLAMEKF : IPoseProvider, IDisposable
	{
		#region C# setup

		#region datalock and running
		object dataLock = new object();
		bool running = true;
		#endregion

		#region slam list setup
		List<Vector2> slamFeatures = new List<Vector2>();
		List<Vector2> slamLandmarks = new List<Vector2>();
		List<Matrix> slamLandmarksCovariance = new List<Matrix>();
		List<Vector2> slamCorrespondences = new List<Vector2>();
		public List<bool> landmarkObsv = new List<bool>();
		public List<Vector2> landmarkList = new List<Vector2>();
		public List<Vector2> fz = new List<Vector2>();
		public List<Vector2> fzHat = new List<Vector2>();

		public List<Vector2> SlamCorrespondences
		{
			get
			{
				lock (dataLock)
				{
					List<Vector2> toReturn = new List<Vector2>(slamCorrespondences);
					return toReturn;
				}
			}
		}
		public List<Vector2> SlamLandmarks
		{
			get
			{
				lock (dataLock)
				{
					List<Vector2> slamLandmarksCopy = new List<Vector2>(slamLandmarks);
					return slamLandmarksCopy;
				}
			}
		}
		public List<Vector2> SlamFeatures
		{
			get
			{
				lock (dataLock)
				{
					List<Vector2> slamFeaturesCopy = new List<Vector2>(slamFeatures);
					return slamFeaturesCopy;
				}
			}
		}
		public List<Matrix> SlamLandmarksCov
		{
			get
			{
				lock (dataLock)
				{
					List<Matrix> toReturn = new List<Matrix>(slamLandmarksCovariance);
					return toReturn;
				}
			}
		}
		#endregion

		#region state information
		//-------------------------------------------------------- vicon

		RobotPose viconPose, viconLastPose;
		public RobotPose CurrentPose
		{
			get { return viconPose; }
			set { viconPose = value; }
		}

		//-------------------------------------------------------- odometry

		RobotPose odomPose;
		public RobotPose OdomPose
		{
			set { odomPose = value; }
			//get { return odomPose; }
		}

		#region estimator

		RobotPose filteredPose;
		public RobotPose FilterPose
		{
			get { return filteredPose; }
		}

		RobotPose transformedOdomPose;
		public RobotPose TransformedOdomPose
		{
			get { return transformedOdomPose; }
		}

		public RobotPose initialViconPose;
		public bool initialized = false;

		public RobotPose initialOdomPose;
		#endregion
		#endregion

		#region lidar scans
		ILidarScan<ILidar2DPoint> currentScan = null;
		public ILidarScan<ILidar2DPoint> CurrentScan
		{
			get { return currentScan; }
			set
			{
				lock (dataLock) { currentScan = value; }
			}
		}
		public ILidarScan<ILidar2DPoint> lastScan;
		#endregion

		#region slam variable setup

		Matrix covMatrix;
		public Matrix CovMatrix
		{ get { return covMatrix; } }
		#endregion

		#region basic SLAM variables:
		public int k; // discrete time-step
		public double sigw = 0.05;
		public double sigv = 0.01;
		public double maxRange = 30.0;

		public Matrix Pv;
		public Matrix Paug;
		public Matrix Qv;

		public List<int> ibook;
		public double bAngle = 120.0 * (Math.PI / 180.0);
		public int numLandmarks;
		public Matrix W;
		public Matrix S;
		public Matrix zCart;
		public Matrix yBarCart;
		public Matrix mhat;
		public Matrix wm;
		public Matrix wc;
		public Matrix chi0;
		public Matrix chi1;
		public Matrix chi2;
		public Matrix chi;
		public Matrix chiUpdate;
		public Matrix chiUpdateObsv;
		public int numObsv;
		public int currentlandmark;
		public RobotPose odomLastPose;
		public RobotPose xhatvPose;
		public RobotPose xStep;
		public RobotPose uvk = new RobotPose(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 1);
		#endregion

		#endregion

		public SLAMEKF()
		{//-------------------------------------------------------- SLAM thread ---------------------------------------------------
			Thread t = new Thread(new ParameterizedThreadStart(SLAMEKFMainLoop));
			t.Start();
		}
		void SLAMEKFMainLoop(object o)
		{
			k = 0;
			while (running)
			{
				lock (dataLock)
				{
					if (currentScan == null || viconPose == null || odomPose == null || viconLastPose == viconPose)
						continue; // no data, no update
					else
					{
						#region discrete-time increment
						k = k + 1;
						#endregion

						if (k == 1)
						{
							#region state initialization
							initialOdomPose = odomPose;
							transformedOdomPose = new RobotPose(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, odomPose.timestamp);
							transformedOdomPose.x = (Math.Cos(initialViconPose.yaw - initialOdomPose.yaw)) * odomPose.x - (Math.Sin(initialViconPose.yaw - initialOdomPose.yaw)) * odomPose.y;
							transformedOdomPose.y = (Math.Sin(initialViconPose.yaw - initialOdomPose.yaw)) * odomPose.x + (Math.Cos(initialViconPose.yaw - initialOdomPose.yaw)) * odomPose.y;
							transformedOdomPose.yaw = odomPose.yaw;
							transformedOdomPose = transformedOdomPose - initialOdomPose + initialViconPose;
							odomLastPose = transformedOdomPose;
							xhatvPose = transformedOdomPose;
							#endregion

							#region map initialization and management
							#region extract features -> fz
							List<Vector2> currentScanXY = new List<Vector2>();
							fz.Clear();
							ibook = fExtract(currentScan, bAngle, maxRange);
							if (ibook != null)
							{
								for (int i = 0; i < currentScan.Points.Count; i++)
									currentScanXY.Add(currentScan.Points[i].RThetaPoint.ToVector2());

								for (int i = 0; i < ibook.Count; i++)
									fz.Add(rotateTranslate(currentScanXY[ibook[i]], xhatvPose));
							}
							#endregion

							#region populate the map -> landmarkList
							landmarkList.Clear();
							for (int i = 0; i < fz.Count; i++)
								landmarkList.Add(fz[i]);
							#endregion

							#region populate the map -> mhat
							mhat = new Matrix(2 * landmarkList.Count, 1);
							for (int i = 0; i < landmarkList.Count; i++)
							{
								mhat[2 * i, 0] = landmarkList[i].X;
								mhat[2 * i + 1, 0] = landmarkList[i].Y;
							}
							#endregion

							#region covariance management -> Paug
							Qv = new Matrix(3, 3, Math.Pow(sigw, 2));
							Pv = Qv;
							Paug = Pv;
							for (int i = 0; i < fz.Count; i++)
								Paug = blkdiag(Paug, new Matrix(2, 2, Math.Pow(sigw, 2)));
							#endregion

							lastScan = currentScan;
							#endregion
						}
						else
						{
							#region odometry processing
							transformedOdomPose = new RobotPose(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, odomPose.timestamp);
							transformedOdomPose.x = (Math.Cos(initialViconPose.yaw - initialOdomPose.yaw)) * odomPose.x - (Math.Sin(initialViconPose.yaw - initialOdomPose.yaw)) * odomPose.y;
							transformedOdomPose.y = (Math.Sin(initialViconPose.yaw - initialOdomPose.yaw)) * odomPose.x + (Math.Cos(initialViconPose.yaw - initialOdomPose.yaw)) * odomPose.y;
							transformedOdomPose.yaw = odomPose.yaw;
							transformedOdomPose = transformedOdomPose - initialOdomPose + initialViconPose;

							xStep = transformedOdomPose - odomLastPose;
							double omega = odomLastPose.yaw + xStep.yaw;
							if (xStep.x == 0)
								uvk.x = 0;
							else
							{
								double vx = xStep.x / Math.Cos(omega);
								uvk.x = vx * Math.Cos(omega);
							}
							if (xStep.y == 0)
								uvk.y = 0;
							else
							{
								double vy = xStep.y / Math.Sin(omega);
								uvk.y = vy * Math.Sin(omega);
							}
							uvk.yaw = xStep.yaw;
							uvk.z = 0;
							uvk.roll = 0;
							uvk.pitch = 0;
							uvk.timestamp = odomPose.timestamp;
							odomLastPose = transformedOdomPose;
							#endregion

							#region time-update
							xhatvPose = xhatvPose + uvk;
							Matrix Uvk = new Matrix(3, 1);
							Uvk[0, 0] = uvk.x;
							Uvk[1, 0] = uvk.y;
							Uvk[2, 0] = uvk.yaw;
							Pv = Pv + ScalarMultiply(Math.Pow(0.5,2),Uvk * Uvk.Transpose()) + Qv;
							
							Paug.SetSubMatrix(0, 2, 0, 2, Pv);
							Matrix Qmv = new Matrix(0, 0, 1.0);
							for (int i = 0; i < landmarkList.Count; i++)
								Qmv = blkdiag(Qmv, ScalarMultiply(Math.Pow(sigw / 5, 2), Pv.Submatrix(0, 1, 0, 1)));

							Matrix Qm = new Matrix(Paug.Rows - 3, Paug.Columns - 3, Math.Pow(sigw / 5, 2));
							Paug.SetSubMatrix(3, Paug.Rows - 1, 3, Paug.Columns - 1, Paug.Submatrix(3, Paug.Rows - 1, 3, Paug.Columns - 1) + Qmv + Qm);
							#endregion

							#region feature exraction
							numLandmarks = landmarkList.Count;
							List<Vector2> currentScanXY = new List<Vector2>();
							fz.Clear();
							ibook = fExtract(currentScan, bAngle, maxRange);
							if (ibook != null)
							{
								for (int i = 0; i < currentScan.Points.Count; i++)
									currentScanXY.Add(currentScan.Points[i].RThetaPoint.ToVector2());

								for (int i = 0; i < ibook.Count; i++)
									fz.Add(rotateTranslate(currentScanXY[ibook[i]], xhatvPose));
							}
							#endregion

							#region data association

							#region variables
							fzHat = new List<Vector2>(landmarkList);
							List<Vector3> jObsv = new List<Vector3>();
							List<Vector2> iObsv = new List<Vector2>();
							List<Vector2> obsvFeatureList = new List<Vector2>();
							Matrix cLP = new Matrix(fz.Count, fzHat.Count);
							Matrix dapGate = new Matrix(fz.Count, fzHat.Count);
							Matrix fzM = new Matrix(2,1);
							Matrix fzHatM = new Matrix(2,1);
							#endregion

							#region initialization
							// "lpsolver55.dll" directory:
							// (note: Alt + F7 -> Build -> enable "Allow UnSafe Code")
							lpsolve.Init("C:\\users\\labuser\\desktop\\MAGIC\\Framework\\SLAM");
							double[] xOpt;
							double ignoreThisEntry = 0;
							int nDecision = fz.Count * fzHat.Count;
							int nConstraints = fz.Count + fzHat.Count;
							int lp = lpsolve.make_lp((int)ignoreThisEntry, nDecision);
							//lpsolve.set_sense(lp, true);
							#endregion

							#region c-vector (objective function)
							double[] cObj = new double[nDecision + 1];
							cObj[0] = ignoreThisEntry;
							int ic = 0;
							for (int i = 0; i < fz.Count; i++)
							{
								for (int j = 0; j < fzHat.Count; j++)
								{
									ic = ic + 1;
									cObj[ic] = -1000000.0 + ciGate(fz[i], Paug.Submatrix(0, 1, 0, 1), fzHat[j], Paug.Submatrix(2 * j + 3, 2 * j + 4, 2 * j + 3, 2 * j + 4), 1.0);
									cLP[i, j] = cObj[ic];
									fzM[0,0] = fz[i].X;
									fzM[1,0] = fz[i].Y;
									fzHatM[0,0] = fzHat[j].X;
									fzHatM[1,0] = fzHat[j].Y;
									dapGate[i, j] = ((fzM - fzHatM).Transpose() * Paug.Submatrix(2 * j + 3, 2 * j + 4, 2 * j + 3, 2 * j + 4).Inverse * (fzM - fzHatM))[0, 0];
								}
							}
							lpsolve.set_obj_fn(lp, cObj);
							//lpsolve.set_timeout(lp, 0);
							#endregion

							#region b-vector (RHS of LE)
							double[] bVec = new double[nConstraints];
							for (int i = 0; i < bVec.Length; i++)
								bVec[i] = 1.0;
							#endregion

							#region A-matrix (constraints setup)
							double[,] A = new double[nConstraints, nDecision];
							int jc = 0;
							for (int i = 0; i < fz.Count; i++)
							{
								int jr = 1;
								for (int j = 0; j < fzHat.Count; j++)
								{
									A[i, j + jc] = 1;
									A[fz.Count - 1 + jr, j + jc] = 1;
									jr = jr + 1;
								}
								jc = jc + fzHat.Count;
							}
							List<double[]> lpConstraints = new List<double[]>();
							lpConstraints.Clear();
							for (int i = 0; i < nConstraints; i++)
							{
								double[] Aline = new double[nDecision + 1];
								Aline[0] = ignoreThisEntry;
								for (int j = 1; j < nDecision + 1; j++)
								{
									Aline[j] = A[i, j - 1];
								}
								lpConstraints.Add(Aline);
							}
							for (int i = 0; i < nConstraints; i++)
								lpsolve.add_constraint(lp, lpConstraints[i], lpsolve.lpsolve_constr_types.LE, bVec[i]);
							#endregion

							#region optimization and results
							lpsolve.solve(lp);
							xOpt = new double[lpsolve.get_Ncolumns(lp)];
							lpsolve.get_variables(lp, xOpt);
							lpsolve.delete_lp(lp);

							ic = 0;
							double tau = 6.63;
							for (int i = 0; i < fz.Count; i++)
							{
								for (int j = 0; j < fzHat.Count; j++)
								{
									if ((xOpt[ic] > 0.98) && (xOpt[ic] < 1.02) && (dapGate[i, j] < tau))
										jObsv.Add(new Vector3(i, j, cLP[i, j]));
									ic = ic + 1;
								}
							}
							if (jObsv.Count > 0)
							{
								for (int j = 0; j < jObsv.Count; j++)
								{
									iObsv.Add(new Vector2(jObsv[j].X, jObsv[j].Y));
									obsvFeatureList.Add(rotateTranslate(currentScanXY[ibook[(int)(jObsv[j].X)]], xhatvPose)); // dev-only
								}
							}
							numObsv = iObsv.Count;
							#endregion

							#endregion

							#region measurement-update
							if (numObsv > 0)
							{
								#region spf parameters
								double L = 3 + 2 * landmarkList.Count;
								double alpha = 1.0;
								double kappa = 0.0;
								double beta = 2.0;
								double lambda = Math.Pow(alpha, 2) * (L + kappa) - L;
								double gam = Math.Sqrt(L + lambda);
								double wm0 = lambda / (L + lambda);
								double wc0 = lambda / (L + lambda) + (1 - Math.Pow(alpha, 2) + beta);
								wm = new Matrix((int)(2 * L) + 1, 1);
								for (int j = 0; j < (int)(2 * L) + 1; j++)
								{
									if (j == 0)
										wm[j, 0] = wm0;
									else
										wm[j, 0] = 1 / (2.0 * (L + lambda));
								}
								wc = new Matrix((int)(2 * L) + 1, 1);
								for (int j = 0; j < (int)(2.0 * L) + 1; j++)
								{
									if (j == 0)
										wc[j, 0] = wc0;
									else
										wc[j, 0] = 1 / (2.0 * (L + lambda));
								}
								#endregion

								#region spf sampler
								CholeskyDecomposition PCholContainer = new CholeskyDecomposition(ScalarMultiply(L + lambda, Paug));
								Matrix PChol = PCholContainer.LeftTriangularFactor;

								chi0 = new Matrix((int)L, 1);
								chi0[0, 0] = xhatvPose.x - uvk.x;
								chi0[1, 0] = xhatvPose.y - uvk.y;
								chi0[2, 0] = xhatvPose.yaw - uvk.yaw;
								for (int i = 0; i < mhat.Rows; i++)
									chi0[3 + i, 0] = mhat[i, 0];

								chi1 = new Matrix((int)L, (int)L);
								for (int i = 0; i < (int)L; i++)
									chi1.SetSubMatrix(0, chi1.Rows - 1, i, i, chi0 + PChol.Submatrix(0, chi0.Rows - 1, i, i));

								chi2 = new Matrix((int)L, (int)L);
								for (int i = 0; i < (int)L; i++)
									chi2.SetSubMatrix(0, chi2.Rows - 1, i, i, chi0 - PChol.Submatrix(0, chi0.Rows - 1, i, i));

								chi = chi0;
								chi = chi.Concat(2, chi1);
								chi = chi.Concat(2, chi2);
								#endregion

								#region spf time-update

								Matrix chiBar = new Matrix((int)L, (int)(2 * L) + 1);
								Matrix uvkSPF = new Matrix((int)L, 1);
								uvkSPF.Zero();
								uvkSPF[0, 0] = uvk.x;
								uvkSPF[1, 0] = uvk.y;
								uvkSPF[2, 0] = uvk.yaw;

								for (int i = 0; i < (int)(2 * L) + 1; i++)
									chiBar.SetSubMatrix(0, chiBar.Rows - 1, i, i, chi.Submatrix(0, chi.Rows - 1, i, i) + uvkSPF);

								Matrix xBar = chiBar * wm;

								Matrix PBar = new Matrix((int)L, (int)L);
								PBar.Zero();

								for (int i = 0; i < (int)(2 * L) + 1; i++)
									PBar = PBar + ScalarMultiply(wc[i, 0], (chiBar.Submatrix(0, chiBar.Rows - 1, i, i) - xBar) * (chiBar.Submatrix(0, chiBar.Rows - 1, i, i) - xBar).Transpose());
								#endregion

								#region sigma-point update
								CholeskyDecomposition PBarCholContainer = new CholeskyDecomposition(PBar);
								Matrix PBarChol = PBarCholContainer.LeftTriangularFactor;

								chi0 = xBar;
								chi1 = new Matrix((int)L, (int)L);
								for (int i = 0; i < (int)L; i++)
									chi1.SetSubMatrix(0, chi1.Rows - 1, i, i, xBar + ScalarMultiply(gam, PBarChol.Submatrix(0, chi0.Rows - 1, i, i)));

								chi2 = new Matrix((int)L, (int)L);
								for (int i = 0; i < (int)L; i++)
									chi2.SetSubMatrix(0, chi2.Rows - 1, i, i, xBar - ScalarMultiply(gam, PBarChol.Submatrix(0, chi0.Rows - 1, i, i)));

								chiUpdate = chi0;
								chiUpdate = chiUpdate.Concat(2, chi1);
								chiUpdate = chiUpdate.Concat(2, chi2);
								#endregion

								#region spf measurement-update setup

								#region predicted measurement
								Matrix YBar = new Matrix(2 * numObsv, (int)(2 * L) + 1);
								YBar.Zero();
								for (int i = 0; i < (int)(2 * L) + 1; i++)
								{
									chiUpdateObsv = new Matrix((int)(2 * numObsv), 1);
									for (int j = 0; j < numObsv; j++)
									{
										chiUpdateObsv[2 * j, 0] = chiUpdate[(int)(2 * iObsv[j].Y + 3), i];
										chiUpdateObsv[2 * j + 1, 0] = chiUpdate[(int)(2 * iObsv[j].Y + 1 + 3), i];
									}
									YBar.SetSubMatrix(0, YBar.Rows - 1, i, i, hFunction(chiUpdate.Submatrix(0, 2, i, i), chiUpdateObsv));
								}
								Matrix yBar = YBar * wm;
								#endregion

								#region actual measurement
								Matrix z = new Matrix(2 * numObsv, 1);
								for (int i = 0; i < numObsv; i++)
								{
									z[2 * i, 0] = currentScan.Points[ibook[(int)(iObsv[i].X)]].RThetaPoint.R;
									z[2 * i + 1, 0] = currentScan.Points[ibook[(int)(iObsv[i].X)]].RThetaPoint.theta;
								}
								#endregion

								#region innovation covariance
								S = new Matrix(2 * numObsv, 2 * numObsv);
								S.Zero();
								for (int i = 0; i < (int)(2 * L) + 1; i++)
									S = S + ScalarMultiply(wc[i, 0], (YBar.Submatrix(0, YBar.Rows - 1, i, i) - yBar) *
																	 (YBar.Submatrix(0, YBar.Rows - 1, i, i) - yBar).Transpose());

								S = S + new Matrix(2 * numObsv, 2 * numObsv, Math.Pow(sigv, 2));
								#endregion

								#region Kalman gain matrix
								W = new Matrix((int)L, 2 * numObsv);
								W.Zero();
								for (int i = 0; i < (int)(2 * L) + 1; i++)
									W = W + ScalarMultiply(wc[i, 0], (chiUpdate.Submatrix(0, chiUpdate.Rows - 1, i, i) - xBar) *
																	 (YBar.Submatrix(0, YBar.Rows - 1, i, i) - yBar).Transpose());
								W = W * S.Inverse;
								#endregion

								#region measurement wrapper
								zCart = new Matrix(2 * numObsv, 1);
								for (int i = 0; i < numObsv; i++)
								{
									zCart[2 * i, 0] = z[2 * i, 0] * Math.Cos(z[2 * i + 1, 0]);
									zCart[2 * i + 1, 0] = z[2 * i, 0] * Math.Sin(z[2 * i + 1, 0]);
								}
								for (int i = 0; i < numObsv; i++)
								{
									z[2 * i, 0] = Math.Sqrt(Math.Pow(zCart[2 * i, 0], 2) + Math.Pow(zCart[2 * i + 1, 0], 2));
									z[2 * i + 1, 0] = Math.Atan2(zCart[2 * i + 1, 0], zCart[2 * i, 0]);
								}

								yBarCart = new Matrix(2 * numObsv, 1);
								for (int i = 0; i < numObsv; i++)
								{
									yBarCart[2 * i, 0] = yBar[2 * i, 0] * Math.Cos(yBar[2 * i + 1, 0]);
									yBarCart[2 * i + 1, 0] = yBar[2 * i, 0] * Math.Sin(yBar[2 * i + 1, 0]);
								}
								for (int i = 0; i < numObsv; i++)
								{
									yBar[2 * i, 0] = Math.Sqrt(Math.Pow(yBarCart[2 * i, 0], 2) + Math.Pow(yBarCart[2 * i + 1, 0], 2));
									yBar[2 * i + 1, 0] = Math.Atan2(yBarCart[2 * i + 1, 0], yBarCart[2 * i, 0]);
								}
								#endregion

								#endregion

								#region spf measurement-update

								//(((z - yBar).Transpose() * S * (z - yBar))[0, 0] < 6.63)

								xBar = xBar + W * (z - yBar);
								Paug = PBar - W * S * W.Transpose();

								#region SLAM-context of measurement update

								#region localization
								xhatvPose.x = xBar[0, 0];
								xhatvPose.y = xBar[1, 0];
								xhatvPose.yaw = xBar[2, 0];
								Pv = Paug.Submatrix(0, 2, 0, 2);
								#endregion

								#region mapping
								mhat = xBar.Submatrix(3, (int)L - 1, 0, 0);
								int landmarkListCount = landmarkList.Count;
								landmarkList.Clear();
								for (int i = 0; i < landmarkListCount; i++)
									landmarkList.Add(new Vector2(mhat[2 * i, 0], mhat[2 * i + 1, 0]));
								#endregion

								#endregion

								#endregion
							}
							#endregion

							#region show features
							slamFeatures.Clear();
							if (ibook != null)
							{
								for (int i = 0; i < ibook.Count; i++)
									slamFeatures.Add(fz[i]);
							}
							#endregion

							#region map management
							double minDist = 0.5;
							double sigMapFreeze = 0.5;
							int landmarkMaxCount = 20;

							for (int i = 0; i < iObsv.Count; i++)
								fz.RemoveAt((int)(iObsv[i].X - i));

							for (int i = 0; i < fz.Count; i++)
							{
								int nFlag = 0;
								for (int j = 0; j < landmarkList.Count; j++)
								{
									if ((fz[i] - landmarkList[j]).Length > minDist)
										nFlag = nFlag + 1;
								}
								if ((nFlag == landmarkList.Count) && (Pv.Trace < sigMapFreeze) && (landmarkList.Count - 1 <= landmarkMaxCount))
								{
									landmarkList.Add(fz[i]);
									Paug = blkdiag(Paug, new Matrix(2, 2, (Pv.Submatrix(0, 1, 0, 1)).Trace));
									Matrix mHatNew = new Matrix(2,1);
									mHatNew[0, 0] = fz[i].X;
									mHatNew[1, 0] = fz[i].Y;
									mhat = mhat.Concat(1, mHatNew);
									break; // add one feature at a time
								}
							}
							#endregion

							#region show landmarks
							slamLandmarks.Clear();
							slamLandmarksCovariance.Clear();
							for (int i = 0; i < landmarkList.Count; i++)
							{
								slamLandmarks.Add(landmarkList[i]);
								slamLandmarksCovariance.Add(Paug.Submatrix(2 * i + 3, 2 * i + 4, 2 * i + 3, 2 * i + 4));
							}
							#endregion

							#region show correspondences
							slamCorrespondences.Clear();
							for (int i = 0; i < obsvFeatureList.Count; i++)
								slamCorrespondences.Add(obsvFeatureList[i]);
							#endregion

							#region broadcast the results
							viconLastPose = viconPose;
							//filteredPose = transformedOdomPose;
							filteredPose = xhatvPose;
							lastScan = currentScan;

							//covMatrix = Pv; // only vehicle xy-submatrix is used in the form
							covMatrix = Pv; // only vehicle xy-submatrix is used in the form
							//transformedOdomPose = viconPose;
							#endregion
						}
					}
				}
				Thread.Sleep(100);
			}
		}
		#region utilities
		#region interfaces
		#region IPoseProvider Members

		public event EventHandler<NewPoseAvailableEventArgs> NewPoseAvailable;

		public RobotPose Pose
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
		#region IDisposable Members

		public void Dispose()
		{
			running = false;
		}

		#endregion
		#endregion

		#region functions
		public Matrix ScalarMultiply(double c, Matrix M)
		{
			// get the matrix dimensions:
			// --------------------------
			//int rowsM = M.GetLength(0);
			//int colsM = M.GetLength(1);
			int rowsM = M.Rows;
			int colsM = M.Columns;

			// initialization:
			// ---------------
			Matrix Mout = new Matrix(rowsM, colsM);

			// multiplication:
			// ---------------
			for (int i = 0; i < rowsM; i++)
			{
				for (int j = 0; j < colsM; j++)
				{
					Mout[i, j] = c * M[i, j];
				}
			}
			return Mout;
		}
		public void MatrixConsole(Matrix M)
		{
			Console.WriteLine("***********************************************");
			for (int i = 0; i < M.Rows; i++)
			{
				String t = "";
				for (int j = 0; j < M.Columns; j++)
				{
					t = t + M[i, j] + ", ";
				}
				Console.WriteLine(t);
			}
		}
		public Matrix hFunction(Matrix x, Matrix m)
		{
			Matrix h = new Matrix((int)(m.Rows), 1);
			for (int i = 0; i < (m.Rows) / 2; i++)
			{
				h[2 * i, 0] = Math.Sqrt(Math.Pow((m[2 * i, 0] - x[0, 0]), 2) + Math.Pow((m[2 * i + 1, 0] - x[1, 0]), 2));
				h[2 * i + 1, 0] = Math.Atan2(m[2 * i + 1, 0] - x[1, 0], m[2 * i, 0] - x[0, 0]) - x[2, 0];
			}
			return h;
		}
		public List<int> dotProductExtract(List<Vector2> xCart, double bAngle)
		{
			List<double> bAngles1 = new List<double>();
			List<double> bAngles2 = new List<double>();
			List<int> iCorners = new List<int>();
			for (int i = 2; i < ibook.Count - 2; i++)
			{
				Vector2 a = new Vector2(xCart[i + 1].Y - xCart[i].Y, xCart[i + 1].X - xCart[i].X);
				Vector2 b = new Vector2(xCart[i - 1].Y - xCart[i].Y, xCart[i - 1].X - xCart[i].X);
				Vector2 c = new Vector2(xCart[i + 2].Y - xCart[i].Y, xCart[i + 2].X - xCart[i].X);
				Vector2 d = new Vector2(xCart[i - 2].Y - xCart[i].Y, xCart[i - 2].X - xCart[i].X);
				bAngles1.Add(Math.Acos(a.Dot(b) / (a.Length * b.Length)));
				bAngles2.Add(Math.Acos(c.Dot(d) / (c.Length * d.Length)));
				if ((bAngles1[i - 2] < bAngle) && (bAngles2[i - 2] < bAngle))
					iCorners.Add(i);
			}
			return iCorners;
		}
		public List<int> fExtract(ILidarScan<ILidar2DPoint> currentScan, double bAngle, double maxRange)
		{
			#region cluster basis variables
			ibook = new List<int>();
			List<Vector2> xCart = new List<Vector2>();
			List<Vector2> currentScanXY = new List<Vector2>();
			List<double> iNonzero = new List<double>();
			List<double> tPulse = new List<double>();
			List<double> rCart = new List<double>();
			List<double> rCartXY = new List<double>();
			List<double> rMap = new List<double>();
			List<int> iDotCorners = new List<int>();
			#endregion

			#region cluster basis
			for (int i = 0; i < currentScan.Points.Count; i++)
			{
				currentScanXY.Add(currentScan.Points[i].RThetaPoint.ToVector2());
				rCartXY.Add(currentScan.Points[i].RThetaPoint.R);
				if (currentScan.Points[i].RThetaPoint.R < maxRange)
				{
					xCart.Add(currentScanXY[i]);
					ibook.Add(i);
					tPulse.Add(currentScan.Points[i].RThetaPoint.theta);
					rCart.Add(currentScan.Points[i].RThetaPoint.R);
				}
			}
			iDotCorners = dotProductExtract(xCart, bAngle);
			List<int> ibookTemp = new List<int>();
			List<double> tPulseTemp = new List<double>();
			List<double> rCartTemp = new List<double>();
			for (int i = 0; i < iDotCorners.Count; i++)
			{
				ibookTemp.Add(ibook[iDotCorners[i]]);
				tPulseTemp.Add(tPulse[iDotCorners[i]]);
				rCartTemp.Add(rCart[iDotCorners[i]]);
			}
			ibook = new List<int>(ibookTemp);
			tPulse = new List<double>(tPulseTemp);
			rMap = new List<double>(rCartTemp);
			#endregion

			#region cluster train variables
			List<bool> rPulse = new List<bool>();
			List<bool> rState = new List<bool>();
			List<int> iCluster = new List<int>();
			List<Vector2> iClusterVec = new List<Vector2>();
			List<Vector2> iClusterVecTemp = new List<Vector2>();
			List<double> cMap = new List<double>();
			List<int> stableClusters = new List<int>();

			rPulse.Add(true);
			rState.Add(true);
			iCluster.Add(0);
			#endregion

			#region cluster train
			for (int i = 1; i < rMap.Count - 1; i++)
			{
				rPulse.Add((Math.Abs(rMap[i] - rMap[i - 1]) < 1.0) && (Math.Abs(tPulse[i] - tPulse[i - 1]) < 5.0 * (Math.PI / 180.0)));
				if (rPulse[i] == true)
					rState.Add(rState[i - 1]);
				else
				{
					rState.Add(!(rState[i - 1]));
					iCluster.Add(i - 1);
					iCluster.Add(i);
				}
			}
			#endregion

			#region stable (single-element) clusters
			if (iCluster.Count > 1)
			{
				#region instantiate clusters
				iCluster.Add(iDotCorners.Count - 1);
				for (int i = 0; i < iCluster.Count / 2; i++)
					iClusterVec.Add(new Vector2(iCluster[2 * i], iCluster[2 * i + 1]));

				for (int i = 0; i < iClusterVec.Count; i++)
				{
					double nC = 0.0;
					Vector2 qTemp = new Vector2();
					qTemp.X = 0;
					qTemp.Y = 0;
					for (int j = (int)iClusterVec[i].X; j < (int)iClusterVec[i].Y + 1; j++)
					{
						qTemp = qTemp + currentScanXY[ibook[j]];
						nC = nC + 1.0;
					}
					qTemp = qTemp / nC;
					cMap.Add(Math.Sqrt(Math.Pow(qTemp.X, 2) + Math.Pow(qTemp.Y, 2)));
				}
				#endregion

				#region propose stable clusters
				iClusterVecTemp.Clear();
				for (int i = 1; i < cMap.Count - 1; i++)
				{
					if ((cMap[i] > cMap[i - 1]) && (cMap[i] > cMap[i + 1]))
					{ }
					else
					{
						if (cMap[i] > 1.0)
							iClusterVecTemp.Add(iClusterVec[i]);
					}
				}
				iClusterVec = new List<Vector2>(iClusterVecTemp);
				#endregion

				#region cluster stability assessment
				stableClusters.Clear();
				for (int i = 0; i < iClusterVec.Count; i++)
				{
					#region shadow-test-1
					double st1 = rCart[iDotCorners[(int)iClusterVec[i].X] - 1];
					double nC = 0.0;
					double rTemp = 0.0;
					for (int j = (int)iClusterVec[i].X; j < (int)iClusterVec[i].Y + 1; j++)
					{
						rTemp = rTemp + rCart[iDotCorners[j]];
						nC = nC + 1.0;
					}
					double st2 = rTemp / nC;
					double st3 = rCart[iDotCorners[(int)iClusterVec[i].X] + 1];
					Vector2 O = new Vector2(2.0, st2);
					Vector2 u = new Vector2(1.0, st1);
					Vector2 v = new Vector2(3.0, st3);
					Vector2 stX = u + v - 2 * O;
					#endregion

					#region shadow-test-2
					st1 = rCart[iDotCorners[(int)iClusterVec[i].Y] - 1];
					nC = 0.0;
					rTemp = 0.0;
					for (int j = (int)iClusterVec[i].X; j < (int)iClusterVec[i].Y + 1; j++)
					{
						rTemp = rTemp + rCart[iDotCorners[j]];
						nC = nC + 1.0;
					}
					st2 = rTemp / nC;
					st3 = rCart[iDotCorners[(int)iClusterVec[i].Y] + 1];
					O = new Vector2(2.0, st2);
					u = new Vector2(1.0, st1);
					v = new Vector2(3.0, st3);
					Vector2 stY = u + v - 2 * O;
					#endregion

					if (Math.Atan2(stX.Y, stX.X) > 0 && Math.Atan2(stY.Y, stY.X) > 0)
						stableClusters.Add(i);
				}
				iClusterVecTemp.Clear();
				for (int i = 0; i < stableClusters.Count; i++)
					iClusterVecTemp.Add(iClusterVec[stableClusters[i]]);

				iClusterVec = new List<Vector2>(iClusterVecTemp);
				#endregion

				#region reject large clusters
				iClusterVecTemp.Clear();
				for (int i = 0; i < iClusterVec.Count; i++)
				{
					if (Math.Abs((int)iClusterVec[i].X - (int)iClusterVec[i].Y) <= 1)
						iClusterVecTemp.Add(iClusterVec[i]);
				}
				iClusterVec = new List<Vector2>(iClusterVecTemp);
				ibookTemp.Clear();
				if (ibook != null)
				{
					for (int i = 0; i < iClusterVec.Count; i++)
					{
						ibookTemp.Add(ibook[(int)iClusterVec[i].X]);
					}
					ibook = new List<int>(ibookTemp);
				}
				#endregion
			}
			else
			{
				ibook = null;
			}
			#endregion

			return ibook;
		}
		public Vector2 rotateTranslate(Vector2 X, RobotPose xParms)
		{
			Vector2 XT = new Vector2(X.X * Math.Cos(xParms.yaw) - X.Y * Math.Sin(xParms.yaw) + xParms.x,
									 X.X * Math.Sin(xParms.yaw) + X.Y * Math.Cos(xParms.yaw) + xParms.y);
			return XT;
		}
		public Matrix blkdiag(Matrix A, Matrix B)
		{
			int rowsA = A.Rows;
			int colsA = A.Columns;
			int rowsB = B.Rows;
			int colsB = B.Columns;
			Matrix C = new Matrix(rowsA + rowsB, colsA + colsB, 1.0);
			for (int i = 0; i < rowsA; i++)
			{
				for (int j = 0; j < colsA; j++)
					C[i, j] = A[i, j];
			}
			for (int i = 0; i < rowsB; i++)
			{
				for (int j = 0; j < colsB; j++)
					C[i + rowsA, j + colsA] = B[i, j];
			}
			return C;
		}
		public double ciGate(Vector2 fz, Matrix Pz, Vector2 xL, Matrix PL, double w)
		{
			Matrix fzM = new Matrix(2, 1);
			fzM[0, 0] = fz.X;
			fzM[1, 0] = fz.Y;

			Matrix xLM = new Matrix(2, 1);
			xLM[0, 0] = xL.X;
			xLM[1, 0] = xL.Y;

			Matrix C = new Matrix(2, 2);
			C = (ScalarMultiply(w, PL.Inverse) + ScalarMultiply(1 - w, Pz.Inverse)).Inverse;

			Matrix c = new Matrix(2, 1);
			c = C * (ScalarMultiply(w, PL.Inverse) * fzM + ScalarMultiply(1 - w, Pz.Inverse) * xLM);

			Matrix Jc = new Matrix(1, 1);
			Jc = (c - xLM).Transpose() * PL.Inverse * (c - xLM);

			return Jc[0, 0];
		}
		#endregion
		#endregion
	}
}