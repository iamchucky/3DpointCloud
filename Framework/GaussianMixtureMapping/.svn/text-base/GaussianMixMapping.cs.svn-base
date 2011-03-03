using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Mapack;
using Magic.Common;
using Magic.OccupancyGrid;
using Magic.Common.Sensors;
using Magic.Common.DataTypes;
using Magic.SickInterface;
using System.Diagnostics;
using Magic.UnsafeMath;
using Magic.Common.Shapes;

namespace Magic.Sensor.GaussianMixtureMapping
{
    public struct Index
    {
        int row;

        public int Row
        {
            get { return row; }
        }
        int col;

        public int Col
        {
            get { return col; }
        }
        public Index(int col, int row)
        {
            this.row = row;
            this.col = col;
        }
    }

	public struct Position
	{
		float x; public float X { get { return x; } }
		float y; public float Y { get { return y; } }
		public Position(float x, float y)
		{
			this.x = x; this.y = y;
		}
	}

    [Obsolete]
    public class GaussianMixMapping
    {
        UMatrix laserToRobotDCM;
        UMatrix robotToGlocalDCM;
        UMatrix covRobotPose;
        UMatrix covLaserPose;
        UMatrix covLaserScan;
        const double numScanPoint = 181;
        const double MAXRANGE = 30.0;
        const int rangeToApply = 5;

        RobotPose currentRobotPose;
        Dictionary<Index, int> indicesDictionary;
        List<Index> indicesList;
        List<float> heightList;
        List<float> covList;
        List<float> pijSumList;

        Dictionary<int, UMatrix> laser2RobotTransMatrixDictionary;
        Dictionary<int, UMatrix> jacobianLaserPoseDictionary;


        #region accessors
        public OccupancyGrid2D UhatGM
        {
            get { return uhatGM.DeepCopy() as OccupancyGrid2D; }
        }

        public OccupancyGrid2D Pij_sum
        {
            get { return pij_sum.DeepCopy() as OccupancyGrid2D; }
        }
        public OccupancyGrid2D Pu_hat
        {
            get { return pu_hat.DeepCopy() as OccupancyGrid2D; }
        }
        public OccupancyGrid2D Pu_hat_square
        {
            get { return pu_hat_square.DeepCopy() as OccupancyGrid2D; }
        }
        public OccupancyGrid2D Psig_u_hat_square
        {
            get { return psig_u_hat_square.DeepCopy() as OccupancyGrid2D; }
        }
        public OccupancyGrid2D LaserHit
        { get { return LaserHit.DeepCopy() as OccupancyGrid2D; } }
        public OccupancyGrid2D NormalizedPij
        { get { return normalisedPijSum.DeepCopy() as OccupancyGrid2D; } }

        #endregion

        double[] sinLookup = new double[361];
        double[] cosLookup = new double[361];
        OccupancyGrid2D pij_sum;
        OccupancyGrid2D pu_hat;
        OccupancyGrid2D pu_hat_square;
        OccupancyGrid2D psig_u_hat_square;
        OccupancyGrid2D uhatGM;
        OccupancyGrid2D sigSqrGM;
        OccupancyGrid2D laserHit;
        OccupancyGrid2D normalisedPijSum;
        Object locker = new Object();
        UMatrix JTpl;
        public GaussianMixMapping(IOccupancyGrid2D ocToUpdate, SensorPose laserRelativePositionToRover)
        {
            //currentLaserPoseToRobot = laserRelativePositionToRover;
            Matrix4 laser2RobotDCM = Matrix4.FromPose(laserRelativePositionToRover);
            laserToRobotDCM = new UMatrix(4, 4);
            robotToGlocalDCM = new UMatrix(4, 4);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    laserToRobotDCM[i, j] = laser2RobotDCM[i, j];
                }
            }
            covRobotPose = new UMatrix(6, 6);
            covLaserPose = new UMatrix(6, 6);
            covLaserScan = new UMatrix(2, 2);

            pij_sum = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
            pu_hat = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
            pu_hat_square = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
            psig_u_hat_square = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
            uhatGM = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
            sigSqrGM = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
            laserHit = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
            normalisedPijSum = new OccupancyGrid2D((OccupancyGrid2D)ocToUpdate);
            #region Initial setup
            // initial setup
            // these matrices were obtained from Jon using Vicon with Pioneer and HOKUYO laser
            // these matrices need to be redefined
            // these matrices are in row-wise


            //double[] Qp_robot = new double[36]{0.0026,    0.0011,   -0.0008,   -0.0019,    0.0125,   -0.0047,
            //                      0.0011,    0.0082,   -0.0054,    0.0098,    0.0116,   -0.0330,
            //                     -0.0008,   -0.0054,    0.0132,   -0.0173,   -0.0154,    0.0115,
            //                     -0.0019,    0.0098,   -0.0173,    0.0542,    0.0076,   -0.0319,
            //                      0.0125,    0.0116,   -0.0154,    0.0076,    0.0812,   -0.0397,
            //                     -0.0047,   -0.0330,    0.0115,   -0.0319,   -0.0397,    0.1875};

            //double[] Qp_laser = new double[36]{0.0077, -0.0009, -0.0016, -0.0127, -0.0415, -0.0011,
            //                       -0.0009, 0.0051, -0.0013, -0.0035, 0.0045, 0.0197,
            //                       -0.0016, -0.0013, 0.0101, -0.0167, 0.0189, 0.0322,
            //                       -0.0127, -0.0035, -0.0167, 0.2669, -0.0548, -0.2940,
            //                       -0.0415, 0.0045, 0.0189, -0.0548, 0.8129, 0.3627,
            //                       -0.0011, 0.0197, 0.0322, -0.2940, 0.3627, 0.7474};



            double[] Qp_robot = new double[36]{0.1, 0, 0, 0, 0, 0,
																				 0, 0.1, 0, 0, 0, 0,
																				 0, 0, 0.1, 0, 0, 0,
																				 0, 0, 0, 0.01, 0, 0,
																				 0, 0, 0, 0, 0.01, 0,
																				 0, 0, 0, 0, 0, 0.01};


            double[] Qp_laser = new double[36]{0.001, 0, 0, 0, 0, 0,
																				 0, 0.001, 0, 0, 0, 0,
																				 0, 0, 0.001, 0, 0, 0,
																				 0, 0, 0, 0.0001, 0, 0,
																				 0, 0, 0, 0, 0.0001, 0,
																				 0, 0, 0, 0, 0, 0.0001};

            double[] Qr = new double[4] { .01 * .01, 0, 0, (0.1 * Math.PI / 180.0) * (0.1 * Math.PI / 180.0) };

            // assign to our UMatrix
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    covRobotPose[j, i] = Qp_robot[j * 6 + i];// *1e-6;
                    covLaserPose[j, i] = Qp_laser[j * 6 + i];// *1e-6;
                }
            }
            covLaserScan[0, 0] = Qr[0];
            covLaserScan[0, 1] = 0;
            covLaserScan[1, 0] = 0;
            covLaserScan[1, 1] = Qr[3];
            #endregion

            //JTpl = ComputeJacobian(currentLaserPoseToRobot.yaw, currentLaserPoseToRobot.pitch, currentLaserPoseToRobot.roll);


            //	int thetaDegIndex = (int)((p.RThetaPoint.theta * 180.0/Math.PI) * 2) + 180;
            int k = 0;
            for (double i = -90; i <= 90; i += .5)
            {
                sinLookup[k] = (Math.Sin(i * Math.PI / 180.0));
                cosLookup[k] = (Math.Cos(i * Math.PI / 180.0));
                k++;
            }

            // initialize arrays
            //pijToSend = new float[361 * rangeLong * rangeLong];
            //largeUToSend = new float[361 * rangeLong * rangeLong];
            //largeSigToSend = new float[361 * rangeLong * rangeLong];
            //colIdxChange = new int[361 * rangeLong * rangeLong];
            //rowIdxChange = new int[361 * rangeLong * rangeLong];

            indicesList = new List<Index>();
            heightList = new List<float>();
            covList = new List<float>();
            pijSumList = new List<float>();

            laser2RobotTransMatrixDictionary = new Dictionary<int, UMatrix>();
            jacobianLaserPoseDictionary = new Dictionary<int, UMatrix>();

            indicesDictionary = new Dictionary<Index, int>();
        }

        private void UpdateCovariance(Matrix robotCov)
        {
            lock (locker)
            {
                UMatrix abc = new UMatrix(6, 6);
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        abc[i, j] = robotCov[i, j]; // this is a heck !!!!!
                    }

                    if (abc[i, i] < 0)
                        abc[i, i] = Math.Abs(abc[i, i]);
                }
                if (abc[2, 2] == 0) abc[2, 2] = 0.01;
                if (abc[4, 4] == 0) abc[4, 4] = 0.01;
                if (abc[5, 5] == 0) abc[5, 5] = 0.01;
                covRobotPose = abc;
            }
        }


        /// <summary>
        /// Update robot to global coordination DCM with a Pose
        /// </summary>
        /// <param name="robotPosition"></param>
        public void UpdatePose(RobotPose robotPosition)
        {
            this.currentRobotPose = robotPosition;
            Matrix4 robot2GlocalDCM = Matrix4.FromPose(robotPosition);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    robotToGlocalDCM[i, j] = robot2GlocalDCM[i, j];
                }
            }
        }

        public void UpdateSensorPose(SensorPose sensorPose)
        {
            Matrix4 laser2RobotDCM = Matrix4.FromPose(sensorPose);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    laserToRobotDCM[i, j] = laser2RobotDCM[i, j];
                }
            }
        }

        /// <summary>
        /// Update OccupancyGrid based on lidarScan and robotPose received
        /// </summary>
        /// <param name="lidarScan"></param>
        /// <param name="currentRobotPose"></param>
        public void UpdateOccupancyGrid(ILidarScan<ILidar2DPoint> lidarScan, int robotID, RobotPose currentRobotPose, SensorPose lidarPose, List<Polygon> dynamicObstacles)
        {
            if (lidarPose == null)
            {
                lidarPose = new SensorPose(0, 0, 0.5, 0, 0 * Math.PI / 180.0, 0, 0);
            }
            if (laser2RobotTransMatrixDictionary.ContainsKey(robotID))
            {
                JTpl = jacobianLaserPoseDictionary[robotID];
                laserToRobotDCM = laser2RobotTransMatrixDictionary[robotID];
            }
            else
            {
                Matrix4 laser2RobotDCM = Matrix4.FromPose(lidarPose);
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        laserToRobotDCM[i, j] = laser2RobotDCM[i, j];
                    }
                }
                laser2RobotTransMatrixDictionary.Add(robotID, laserToRobotDCM);
                jacobianLaserPoseDictionary.Add(robotID, ComputeJacobian(lidarPose.yaw, lidarPose.pitch, lidarPose.roll));
                JTpl = jacobianLaserPoseDictionary[robotID];
            }

            // calculate robot2global transformation matrix
            Matrix4 robot2GlocalDCM = Matrix4.FromPose(currentRobotPose);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    robotToGlocalDCM[i, j] = robot2GlocalDCM[i, j];
                }
            }

            if (lidarScan == null) return;
            Stopwatch sw1 = new Stopwatch();
            Stopwatch sw2 = new Stopwatch();
            Stopwatch sw3 = new Stopwatch();
            Stopwatch sw4 = new Stopwatch();
            Stopwatch sw5 = new Stopwatch();
            Stopwatch sw6 = new Stopwatch();


            UMatrix JTpr = ComputeJacobian(currentRobotPose.yaw, currentRobotPose.pitch, currentRobotPose.roll);
            List<UMatrix> JfPrCubixLaserToRobotDCM = new List<UMatrix>(6);
            List<UMatrix> RobotToGlocalDCMJfPlCubix = new List<UMatrix>(6);
            for (int i = 0; i < 6; i++)
            {
                //derivative of the robot transform matrtix w.r.t. some element of the robot psoe
                UMatrix j = new UMatrix(4, 4);
                j[0, 0] = JTpr[0, i]; j[1, 0] = JTpr[1, i]; j[2, 0] = JTpr[2, i]; j[3, 0] = JTpr[3, i];
                j[0, 1] = JTpr[4, i]; j[1, 1] = JTpr[5, i]; j[2, 1] = JTpr[6, i]; j[3, 1] = JTpr[7, i];
                j[0, 2] = JTpr[8, i]; j[1, 2] = JTpr[9, i]; j[2, 2] = JTpr[10, i]; j[3, 2] = JTpr[11, i];
                j[0, 3] = JTpr[12, i]; j[1, 3] = JTpr[13, i]; j[2, 3] = JTpr[14, i]; j[3, 3] = JTpr[15, i];
                JfPrCubixLaserToRobotDCM.Add(j * laserToRobotDCM);

                UMatrix tempJacobianPl = new UMatrix(4, 4);
                tempJacobianPl[0, 0] = JTpl[0, i]; tempJacobianPl[1, 0] = JTpl[1, i]; tempJacobianPl[2, 0] = JTpl[2, i]; tempJacobianPl[3, 0] = JTpl[3, i];
                tempJacobianPl[0, 1] = JTpl[4, i]; tempJacobianPl[1, 1] = JTpl[5, i]; tempJacobianPl[2, 1] = JTpl[6, i]; tempJacobianPl[3, 1] = JTpl[7, i];
                tempJacobianPl[0, 2] = JTpl[8, i]; tempJacobianPl[1, 2] = JTpl[9, i]; tempJacobianPl[2, 2] = JTpl[10, i]; tempJacobianPl[3, 2] = JTpl[11, i];
                tempJacobianPl[0, 3] = JTpl[12, i]; tempJacobianPl[1, 3] = JTpl[13, i]; tempJacobianPl[2, 3] = JTpl[14, i]; tempJacobianPl[3, 3] = JTpl[15, i];
                RobotToGlocalDCMJfPlCubix.Add(robotToGlocalDCM * tempJacobianPl);
            }
            UMatrix laserToENU = robotToGlocalDCM * laserToRobotDCM;
            UMatrix pijCell = new UMatrix(rangeToApply * 2 + 1, rangeToApply * 2 + 1);
            // update covariance
            UpdateCovariance(currentRobotPose.covariance);

            //SickPoint p = new SickPoint(new RThetaCoordinate(1.0f, 0.0f));
            for (int laserIndex = 0; laserIndex < lidarScan.Points.Count; laserIndex++)
            {
                lock (locker)
                {

                    ILidar2DPoint p = lidarScan.Points[laserIndex];
                    if (p.RThetaPoint.R >= MAXRANGE) continue;

                    bool hitDynamicObstacles = false;
                    // figure out if this lidar point is hitting other robot

                    // find laser points in 3D space
                    // first define 2D point on the laser plane
                    UMatrix laserPoint = new UMatrix(4, 1);

                    double deg = (p.RThetaPoint.theta * 180.0 / Math.PI);
                    int thetaDegIndex = (int)Math.Round((deg + 90.0) * 2.0) % 360;
                    double cosTheta = cosLookup[thetaDegIndex];
                    double sinTheta = sinLookup[thetaDegIndex];

                    laserPoint[0, 0] = p.RThetaPoint.R * cosTheta;
                    laserPoint[1, 0] = p.RThetaPoint.R * sinTheta;
                    laserPoint[2, 0] = 0;
                    laserPoint[3, 0] = 1;

                    //calculate r_hat_ENU
                    UMatrix r_hat_ENU = laserToENU * laserPoint;

                    foreach (Polygon dp in dynamicObstacles)
                    {
                        if (dp.IsInside(new Vector2(r_hat_ENU[0, 0], r_hat_ENU[1, 0])))
                        {
                            hitDynamicObstacles = true;
                            break;
                        }
                    }
                    if (hitDynamicObstacles) continue;

                    //-------------------------------//
                    // COVARIANCE UMatrix CALCULATION //
                    //-------------------------------//		
                    UMatrix JRr = new UMatrix(4, 2);
                    JRr.Zero();
                    JRr[0, 0] = cosTheta;
                    JRr[0, 1] = -p.RThetaPoint.R * sinTheta;
                    JRr[1, 0] = sinTheta;
                    JRr[1, 1] = p.RThetaPoint.R * cosTheta;

                    UMatrix Jfr = new UMatrix(3, 2); // 3x2 								
                    Jfr = (laserToENU * JRr).Submatrix(0, 2, 0, 1);	 // 4x4 * 4x4 * 4x2			

                    UMatrix Jfpr = new UMatrix(3, 6);
                    UMatrix Jfpl = new UMatrix(3, 6);
                    sw1.Reset();
                    sw1.Start();

                    for (int i = 0; i < 6; i++) //for each state var (i.e. x,y,z,y,p,r)
                    {
                        UMatrix JfprTemp = (JfPrCubixLaserToRobotDCM[i]) * laserPoint; // 4 by 1 UMatrix					
                        Jfpr[0, i] = JfprTemp[0, 0];
                        Jfpr[1, i] = JfprTemp[1, 0];
                        Jfpr[2, i] = JfprTemp[2, 0];

                        //UMatrix JfplTemp = (RobotToGlocalDCMJfPlCubix[i]) * laserPoint; // 4 by 1 UMatrix
                        //Jfpl[0, i] = JfplTemp[0, 0];
                        //Jfpl[1, i] = JfplTemp[1, 0];
                        //Jfpl[2, i] = JfplTemp[2, 0];
                    }
                    sw1.Stop();
                    sw2.Reset();
                    sw2.Start();
                    UMatrix JfprQprJfprT = new UMatrix(3, 3);
                    UMatrix JfplQplJfplT = new UMatrix(3, 3);
                    UMatrix JfrQrJfrT = new UMatrix(3, 3);
                    JfprQprJfprT = (Jfpr * covRobotPose) * Jfpr.Transpose();
                    //JfplQplJfplT = (Jfpl * covLaserPose) * Jfpl.Transpose();
                    JfrQrJfrT = (Jfr * covLaserScan) * Jfr.Transpose();

                    // add above variables together and get the covariance
                    UMatrix covRHatENU = JfprQprJfprT + /*JfplQplJfplT +*/ JfrQrJfrT; // 3x3 UMatrix
                    sw2.Stop();
                    sw3.Reset();
                    sw3.Start();
                    //-----------------------------//
                    // FIND WHICH CELLS TO COMPUTE //
                    //-----------------------------//

                    // find out cells around this laser point
                    int laserPointIndexX = 0;
                    int laserPointIndexY = 0;
                    //this is used just to do the transformation from reaal to grid and visa versa
                    psig_u_hat_square.GetIndicies(r_hat_ENU[0, 0], r_hat_ENU[1, 0], out laserPointIndexX, out laserPointIndexY); // get cell (r_hat_ENU_X, r_hat_ENU_y)
                    sw3.Stop();
                    sw4.Reset();
                    sw4.Start();
                    //-----------------------------------------//
                    // COMPUTE THE DISTRIBUTION OF UNCERTAINTY //
                    //-----------------------------------------//

                    double sigX = Math.Sqrt(covRHatENU[0, 0]);
                    double sigY = Math.Sqrt(covRHatENU[1, 1]);
                    double rho = covRHatENU[1, 0] / (sigX * sigY);
                    double logTerm = Math.Log(2 * Math.PI * sigX * sigY * Math.Sqrt(1 - (rho * rho)));
                    double xTermDenom = (1 - (rho * rho));
                    for (int i = -rangeToApply; i <= rangeToApply; i++) // column
                    {
                        for (int j = -rangeToApply; j <= rangeToApply; j++) // row
                        {
                            // estimate using Bivariate Normal Distribution
                            double posX = 0; double posY = 0;
                            psig_u_hat_square.GetReals(laserPointIndexX + i, laserPointIndexY + j, out posX, out posY);
                            posX += psig_u_hat_square.ResolutionX / 2;
                            posY += psig_u_hat_square.ResolutionY / 2;
                            double x = posX - r_hat_ENU[0, 0];
                            double y = posY - r_hat_ENU[1, 0];
                            double z = (x * x) / (sigX * sigX) -
                                                (2 * rho * x * y / (sigX * sigY)) +
                                                 (y * y) / (sigY * sigY);
                            double xTerm = -0.5 * z / xTermDenom;
                            // chi2 test
                            //if ((2 * (1 - (rho * rho))) * ((x * x) / (sigX * sigX) + (y * y) / (sigY * sigY) - (2 * rho * x * y) / (sigX * sigY)) > 15.2)
                            //  continue;						
                            pijCell[j + rangeToApply, i + rangeToApply] = Math.Exp(xTerm - logTerm) * psig_u_hat_square.ResolutionX * psig_u_hat_square.ResolutionY;
                            laserHit.SetCellByIdx(laserPointIndexX + i, laserPointIndexY + j, laserHit.GetCellByIdx(laserPointIndexX + i, laserPointIndexY + j) + 1);
                        }
                    }
                    sw4.Stop();
                    sw5.Reset();
                    sw5.Start();

                    //---------------------------//
                    // COMPUTE HEIGHT ESTIMATION //
                    //---------------------------//				
                    //				Matrix2 PEN = new Matrix2(covRHatENU[0, 0], covRHatENU[0, 1], covRHatENU[1, 0], covRHatENU[1, 1]);

                    UMatrix PEN = covRHatENU.Submatrix(0, 1, 0, 1);

                    UMatrix PENInv = PEN.Inverse2x2;

                    UMatrix PuEN = new UMatrix(1, 2);
                    UMatrix PENu = new UMatrix(2, 1);
                    UMatrix PuENPENInv = PuEN * PENInv;
                    UMatrix uHatMatrix = new UMatrix(rangeToApply * 2 + 1, rangeToApply * 2 + 1);
                    UMatrix sigUHatMatrix = new UMatrix(rangeToApply * 2 + 1, rangeToApply * 2 + 1);

                    PuEN[0, 0] = covRHatENU[2, 0];
                    PuEN[0, 1] = covRHatENU[2, 1];

                    PENu[0, 0] = covRHatENU[0, 2];
                    PENu[1, 0] = covRHatENU[1, 2];

                    double sig_u_hat_product = (PuENPENInv * PENu)[0, 0]; // output = 1x1 UMatrix

                    for (int i = -rangeToApply; i <= rangeToApply; i++) // column
                    {
                        for (int j = -rangeToApply; j <= rangeToApply; j++) // row
                        {
                            UMatrix ENmr_EN = new UMatrix(2, 1);
                            double posX = 0; double posY = 0;
                            psig_u_hat_square.GetReals(laserPointIndexX + i, laserPointIndexY + j, out posX, out posY);
                            ENmr_EN[0, 0] = posX - r_hat_ENU[0, 0];
                            ENmr_EN[1, 0] = posY - r_hat_ENU[1, 0];
                            double u_hat_product = (PuENPENInv * (ENmr_EN))[0, 0]; // output = 1x1 UMatrix
                            uHatMatrix[j + rangeToApply, i + rangeToApply] = r_hat_ENU[2, 0] + u_hat_product;
                            sigUHatMatrix[j + rangeToApply, i + rangeToApply] = covRHatENU[2, 2] - sig_u_hat_product;
                        }
                    }
                    sw5.Stop();
                    sw6.Reset();
                    sw6.Start();

                    //-------------------------------------------//
                    // ASSIGN FINAL VALUES TO THE OCCUPANCY GRID //
                    //-------------------------------------------//
                    for (int i = -rangeToApply; i <= rangeToApply; i++)
                    {
                        for (int j = -rangeToApply; j <= rangeToApply; j++)
                        {
                            int indexXToUpdate = laserPointIndexX + i;
                            int indexYToUpdate = laserPointIndexY + j;
                            // if the cell to update is out of range, continue
                            if (!psig_u_hat_square.CheckValidIdx(indexXToUpdate, indexYToUpdate))
                            {
                                //Console.WriteLine("Laser points out of the occupancy grid map");
                                continue;
                            }

                            pij_sum.SetCellByIdx(indexXToUpdate, indexYToUpdate,
                                            pijCell[j + rangeToApply, i + rangeToApply] + pij_sum.GetCellByIdx(indexXToUpdate, indexYToUpdate));
                            pu_hat.SetCellByIdx(indexXToUpdate, indexYToUpdate,
                                            pijCell[j + rangeToApply, i + rangeToApply] * uHatMatrix[j + rangeToApply, i + rangeToApply] + pu_hat.GetCellByIdx(indexXToUpdate, indexYToUpdate));
                            pu_hat_square.SetCellByIdx(indexXToUpdate, indexYToUpdate,
                                            pijCell[j + rangeToApply, i + rangeToApply] * uHatMatrix[j + rangeToApply, i + rangeToApply] * uHatMatrix[j + rangeToApply, i + rangeToApply] + pu_hat_square.GetCellByIdx(indexXToUpdate, indexYToUpdate));
                            psig_u_hat_square.SetCellByIdx(indexXToUpdate, indexYToUpdate,
                                            pijCell[j + rangeToApply, i + rangeToApply] * sigUHatMatrix[j + rangeToApply, i + rangeToApply] + psig_u_hat_square.GetCellByIdx(indexXToUpdate, indexYToUpdate));
                            uhatGM.SetCellByIdx(indexXToUpdate, indexYToUpdate,
                                                            (pu_hat.GetCellByIdx(indexXToUpdate, indexYToUpdate) / pij_sum.GetCellByIdx(indexXToUpdate, indexYToUpdate)));
                            normalisedPijSum.SetCellByIdx(indexXToUpdate, indexYToUpdate, pij_sum.GetCellByIdx(indexXToUpdate, indexYToUpdate) / laserHit.GetCellByIdx(indexXToUpdate, indexYToUpdate));
                            double largeU = (pu_hat.GetCellByIdx(indexXToUpdate, indexYToUpdate) / pij_sum.GetCellByIdx(indexXToUpdate, indexYToUpdate));
                            double largeSig = (psig_u_hat_square.GetCellByIdx(indexXToUpdate, indexYToUpdate) + pu_hat_square.GetCellByIdx(indexXToUpdate, indexYToUpdate)) / pij_sum.GetCellByIdx(indexXToUpdate, indexYToUpdate) - largeU * largeU;

                            uhatGM.SetCellByIdx(indexXToUpdate, indexYToUpdate, largeU);
                            sigSqrGM.SetCellByIdx(indexXToUpdate, indexYToUpdate, largeSig);

                            Index index = new Index(indexXToUpdate, indexYToUpdate);
                            if (!indicesDictionary.ContainsKey(index))
                                indicesDictionary.Add(index, indicesDictionary.Count);
                            /*
                            if (indicesDictionary.ContainsKey(index))
                            {
                                int indexOfIndices = indicesDictionary[index];
                                heightList[indexOfIndices] = (float)largeU;
                                covList[indexOfIndices] = (float)largeSig;
                                pijSumList[indexOfIndices] = (float)pij_sum.GetCellByIdx(indexXToUpdate, indexYToUpdate);
                            }
                            else
                            {
                                indicesDictionary.Add(index, indicesDictionary.Count);
                                indicesList.Add(new Index(indexXToUpdate, indexYToUpdate));
                                heightList.Add((float)largeU);
                                covList.Add((float)largeSig);
                                pijSumList.Add((float)pij_sum.GetCellByIdx(indexXToUpdate, indexYToUpdate));
                            }
                             */
                        }
                    }
                    sw6.Stop();

                } // end foreach

                //Console.WriteLine("1: " + sw1.ElapsedMilliseconds +
                //                                    " 2: " + sw2.ElapsedMilliseconds +
                //                                    " 3: " + sw3.ElapsedMilliseconds +
                //                                    " 4: " + sw4.ElapsedMilliseconds +
                //                                    " 5: " + sw5.ElapsedMilliseconds +
                //                                    " 6: " + sw6.ElapsedMilliseconds +
                //                                    " TOTAL: " + (sw1.ElapsedMilliseconds + sw2.ElapsedMilliseconds + sw3.ElapsedMilliseconds + sw4.ElapsedMilliseconds + sw5.ElapsedMilliseconds + sw6.ElapsedMilliseconds).ToString());
            } // end function
        }

        /// <summary>
        /// Returns arrays to send out through messaging
        /// </summary>
        /// <param name="largeUDiff"></param>
        /// <param name="largeSigDiff"></param>
        /// <param name="pijDiff"></param>
        /// <param name="colIdx"></param>
        /// <param name="rowIdx"></param>
        public void GetArraysToSend(out List<Index> indexList, out List<float> heightList, out List<float> covList, out List<float> pijSumList, out List<float> laserHitList)
        {
            int index = 0;
            if (indicesDictionary == null)
            {
                indexList = new List<Index>();
                heightList = new List<float>();
                covList = new List<float>();
                pijSumList = new List<float>();
                laserHitList = new List<float>();
                return;
            }
            else
            {
                indexList = new List<Index>(indicesDictionary.Count);
                heightList = new List<float>(indicesDictionary.Count);
                covList = new List<float>(indicesDictionary.Count);
                pijSumList = new List<float>(indicesDictionary.Count);
                laserHitList = new List<float>(indicesDictionary.Count);
            }

            lock (locker)
            {
                foreach (KeyValuePair<Index, int> pair in indicesDictionary)
                {
                    indexList.Add(pair.Key);
                    heightList.Add((float)uhatGM.GetCellByIdx(pair.Key.Col, pair.Key.Row));
                    covList.Add((float)sigSqrGM.GetCellByIdx(pair.Key.Col, pair.Key.Row));
                    pijSumList.Add((float)pij_sum.GetCellByIdx(pair.Key.Col, pair.Key.Row));
                    laserHitList.Add((float)laserHit.GetCellByIdx(pair.Key.Col, pair.Key.Row));
                }
                indicesDictionary.Clear();
            }

            //indexList = new List<Index>(this.indicesList);
            //heightList = new List<float>(this.heightList);
            //covList = new List<float>(this.covList);
            //pijSumList = new List<float>(this.pijSumList);

            //this.indicesList.Clear();
            //this.heightList.Clear();
            //this.covList.Clear();
            //this.pijSumList.Clear();
            //this.indicesDictionary.Clear();
        }

        //public static void ConvertToOcGrid(float[] incomingMessage, ref OccupancyGrid2D largeU, ref OccupancyGrid2D largeSig, ref OccupancyGrid2D pij)
        //{
        //  int msgCount = (int)incomingMessage[0];
        //  int eachLength = (int)incomingMessage[1];

        //  for (int i = 2; i < eachLength + 2; i++)
        //  {
        //    float largeUVal = incomingMessage[i];
        //    //float largeSigVal = incomingMessage[i + eachLength];
        //    //float pijVal = incomingMessage[i + 2 * eachLength];
        //    int colIdx = (int)incomingMessage[i + 1 * eachLength];
        //    int rowIdx = (int)incomingMessage[i + 2 * eachLength];

        //    //Console.WriteLine("[" + rowIdx + ", " + colIdx + "]");

        //    largeU.SetCellByIdx(colIdx, rowIdx, largeUVal);
        //    //largeSig.SetCellByIdx(colIdx, rowIdx, largeSigVal);
        //    //pij.SetCellByIdx(colIdx, rowIdx, pijVal);
        //  }
        //}


        # region Calculation Helper

        public UMatrix ComputeJacobian(double yaw, double pitch, double roll)
        {
            UMatrix m = new UMatrix(16, 6);
            m.Zero();

            double cp = Math.Cos(pitch);
            double sp = Math.Sin(pitch);
            double cy = Math.Cos(yaw);
            double sy = Math.Sin(yaw);
            double cr = Math.Cos(roll);
            double sr = Math.Sin(roll);

            m[0, 3] = -cp * sy;
            m[0, 4] = -sp * cy;
            m[1, 3] = -sp * sr * sy - cy * cr;
            m[1, 4] = cp * sr * cy;
            m[1, 5] = sp * cr * cy + sy * sr;

            m[2, 3] = -sp * cr * sy + cy * sr;
            m[2, 4] = cp * cr * cy;
            m[2, 5] = -sp * sr * cy + sy * cr;

            m[4, 3] = cp * cy;
            m[4, 4] = -sp * sy;
            m[5, 3] = sp * sr * cy - sy * cr;
            m[5, 4] = cp * sr * sy;
            m[5, 5] = sp * cr * sy - cy * sr;
            m[6, 3] = sp * cr * cy + sy * sr;
            m[6, 4] = cp * cr * sy;
            m[6, 5] = -sp * sr * sy - cy * cr;

            m[8, 4] = -cp;
            m[9, 4] = -sp * sr;
            m[9, 5] = cp * cr;
            m[10, 4] = -sp * cr;
            m[10, 5] = -cp * sr;

            m[12, 0] = 1;
            m[13, 1] = 1;
            m[14, 2] = 1;

            return m;
        }


        private UMatrix ComputeJacobianZYX(double yaw, double pitch, double roll)
        {
            UMatrix m = new UMatrix(16, 6);
            m.Zero();

            m[12, 0] = 1;

            m[13, 1] = 1;

            m[14, 2] = 1;

            m[1, 3] = Math.Cos(roll) * Math.Sin(pitch) * Math.Cos(yaw) + Math.Sin(roll) * Math.Sin(yaw);
            m[2, 3] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Cos(yaw) + Math.Cos(roll) * Math.Sin(yaw);
            m[5, 3] = Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw) - Math.Sin(roll) * Math.Cos(yaw);
            m[6, 3] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Sin(yaw) - Math.Cos(roll) * Math.Cos(yaw);
            m[9, 3] = Math.Cos(roll) * Math.Cos(pitch);
            m[10, 3] = -Math.Sin(roll) * Math.Cos(pitch);

            m[0, 4] = -Math.Sin(pitch) * Math.Cos(yaw);
            m[1, 4] = Math.Sin(roll) * Math.Cos(pitch) * Math.Cos(yaw);
            m[2, 4] = Math.Cos(roll) * Math.Cos(pitch) * Math.Cos(yaw);
            m[4, 4] = -Math.Sin(pitch) * Math.Sin(yaw);
            m[5, 4] = Math.Sin(roll) * Math.Cos(pitch) * Math.Sin(yaw);
            m[6, 4] = Math.Cos(roll) * Math.Cos(pitch) * Math.Sin(yaw);
            m[8, 4] = -Math.Cos(pitch);
            m[9, 4] = -Math.Sin(roll) * Math.Sin(pitch);
            m[10, 4] = -Math.Cos(roll) * Math.Sin(pitch);

            m[0, 5] = -Math.Cos(pitch) * Math.Sin(yaw);
            m[1, 5] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Sin(yaw) - Math.Cos(roll) * Math.Cos(yaw);
            m[2, 5] = -Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw) + Math.Sin(roll) * Math.Cos(yaw);
            m[4, 5] = Math.Cos(pitch) * Math.Cos(yaw);
            m[5, 5] = Math.Sin(roll) * Math.Sin(pitch) * Math.Cos(yaw) - Math.Cos(roll) * Math.Sin(yaw);
            m[6, 5] = Math.Cos(roll) * Math.Sin(pitch) * Math.Cos(yaw) + Math.Sin(roll) * Math.Sin(yaw);

            return m;
        }

        private UMatrix ComputeJacobianXYZ(double roll, double pitch, double yaw)
        {
            UMatrix m = new UMatrix(16, 6);
            m.Zero();

            m[12, 0] = 1;

            m[13, 1] = 1;

            m[14, 2] = 1;

            m[1, 3] = Math.Cos(roll) * Math.Sin(pitch) * Math.Cos(yaw) - Math.Sin(roll) * Math.Sin(yaw);
            m[2, 3] = Math.Sin(roll) * Math.Sin(pitch) * Math.Cos(yaw) + Math.Cos(roll) * Math.Sin(yaw);
            m[5, 3] = -Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw) - Math.Sin(roll) * Math.Cos(yaw);
            m[6, 3] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Sin(yaw) + Math.Cos(roll) * Math.Cos(yaw);
            m[9, 3] = -Math.Cos(roll) * Math.Cos(pitch);
            m[10, 3] = -Math.Sin(roll) * Math.Cos(pitch);

            m[0, 4] = -Math.Sin(pitch) * Math.Cos(yaw);
            m[1, 4] = Math.Sin(roll) * Math.Cos(pitch) * Math.Cos(yaw);
            m[2, 4] = -Math.Cos(roll) * Math.Cos(pitch) * Math.Cos(yaw);
            m[4, 4] = Math.Sin(pitch) * Math.Sin(yaw);
            m[5, 4] = -Math.Sin(roll) * Math.Cos(pitch) * Math.Sin(yaw);
            m[6, 4] = Math.Cos(roll) * Math.Cos(pitch) * Math.Sin(yaw);
            m[8, 4] = Math.Cos(pitch);
            m[9, 4] = Math.Sin(roll) * Math.Sin(pitch);
            m[10, 4] = -Math.Cos(roll) * Math.Sin(pitch);

            m[0, 5] = -Math.Cos(pitch) * Math.Sin(yaw);
            m[1, 5] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Sin(yaw) + Math.Cos(roll) * Math.Cos(yaw);
            m[2, 5] = Math.Cos(roll) * Math.Sin(pitch) * Math.Sin(yaw) + Math.Sin(roll) * Math.Cos(yaw);
            m[4, 5] = -Math.Cos(pitch) * Math.Cos(yaw);
            m[5, 5] = -Math.Sin(roll) * Math.Sin(pitch) * Math.Cos(yaw) - Math.Cos(roll) * Math.Sin(yaw);
            m[6, 5] = Math.Cos(roll) * Math.Sin(pitch) * Math.Cos(yaw) - Math.Sin(roll) * Math.Sin(yaw);

            return m;
        }
        #endregion




        //public static void ConvertToOcGrid(ref OccupancyGrid2D globalHeightMap, ref OccupancyGrid2D globalSigMap, ref OccupancyGrid2D globalPijMap,
        //  List<float> largeU, List<float> largeSig, List<float> pij, List<int> colIdx, List<int> rowIdx)
        //{
        //  for (int i = 0; i < largeU.Count; i++)
        //  {
        //    globalHeightMap.SetCellByIdx(rowIdx[i], colIdx[i], largeU[i]);
        //    globalSigMap.SetCellByIdx(rowIdx[i], colIdx[i], largeSig[i]);
        //    globalPijMap.SetCellByIdx(rowIdx[i], colIdx[i], pij[i]);
        //  }
        //}

        //public static void ConvertToOcGrid(ref OccupancyGrid2D globalHeightMap, ref OccupancyGrid2D globalSigMap, ref OccupancyGrid2D globalPijMap, List<Index> indexList, List<float> heightList, List<float> covList, List<float> pijSumList)
        //{
        //  for (int i = 0; i < indexList.Count; i++)
        //  {
        //    int row = indexList[i].Row;
        //    int col = indexList[i].Col;
        //    globalHeightMap.SetCellByIdx(col, row, heightList[i]);
        //    globalSigMap.SetCellByIdx(col, row, covList[i]);
        //    globalPijMap.SetCellByIdx(col, row, pijSumList[i]);
        //  }
        //}

    }
}
