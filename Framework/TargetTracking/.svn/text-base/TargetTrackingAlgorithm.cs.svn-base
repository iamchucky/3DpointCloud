//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Magic.Common.Mapack;
//using Magic.Common.Messages;

//// Bearing only target tracking algorithm
//// written by Daniel Lee

//namespace Magic.TargetTracking
//{
//    class TargetTrackingAlgorithm
//    {
//        // point of interest (POI) state
//        Matrix targetState;
//        public Matrix TargetState { get { return targetState; } }
//        // POI covaraicne matrix
//        Matrix S_target;
//        public Matrix STarget { get { return S_target; } }
//        MathNet.Numerics.Distributions.NormalDistribution normDist = new MathNet.Numerics.Distributions.NormalDistribution(0, 2);
//        double dT;

//        public TargetTrackingAlgorithm(double dT)
//        {
//            this.dT = dT;
//        }

//        /// <summary>
//        /// Square Root, Sigma Points Information filter function. This estimates the information (of target) at next timestep (k+1) with given input at time k.
//        /// </summary>
//        /// <param name="xPOI">[7 x 1: east north up vel heading turnRate accleration]</param>
//        /// <param name="xx2">[9 x 1 : 3 NAV, 3ATT, 3GIM] = [E N U, roll pitch yaw, pan tilt scan]</param>
//        /// <param name="SPOI">[7 x 7 square root covariance of POI]</param>
//        /// <param name="Sw">[3 x 3 square root covariance of sensor noise: altitude, turnRate, and accleration]</param>
//        /// <param name="SMeasurement">[2 x 1 covariance vector of measurement]</param>
//        /// <param name="Sx2">[9 x 9 square root covariance of x2]</param>
//        /// <param name="zSCR">[2 x 1 Actual measurement of screen (pixels)]</param>
//        /// <param name="sigma_f">scaling factor for the distance of the sigmap points from the mean</param>
//        /// <returns></returns>
//        public void Update(Matrix xPOI, Matrix xx2, Matrix SPOI, Matrix Sw, Matrix SMeasurement, Matrix Sx2, Matrix zSCR, double sigma_f, string screenSize, string cameraType, TargetTypes type)
//        {
//            //////////////////////////////// INITIALIZATION /////////////////////////////////////
//            // SORRY - almost impossible to debug.
//            // variable names are mostly following IEEE Transaction paper written by Dr. Mark Campbell.
//            // also the variable names strictly follow the names I used in MATLAB code.
//            int nPOI = xPOI.Rows;
//            int nw = Sw.Rows;
//            int nLaser = 1;
//            // state matrix
//            Matrix xHat_a0 = new Matrix(nPOI + nw, 1);
//            for (int i = 0; i < nPOI; i++)
//                xHat_a0[i, 0] = xPOI[i, 0];

//            for (int i = 0; i < nw; i++)
//                xHat_a0[i + nPOI, 0] = 0;
//            // covariance matrix
//            Matrix S_a0 = new Matrix(nPOI + nw, nPOI + nw);
//            for (int i = 0; i < nPOI; i++)
//                S_a0[i, i] = SPOI[i, i];
//            for (int i = nPOI; i < nPOI + nw; i++)
//                S_a0[i, i] = Sw[i - nPOI, i - nPOI];

//            // generate 2*(nPOI + nw) + 1 sigma points
//            Matrix Xa0 = new Matrix(nPOI + nw, 2 * (nPOI + nw) + 1);
//            Matrix halfSigmaPoints1 = new Matrix(nPOI + nw, nPOI + nw);
//            Matrix halfSigmaPoints2 = new Matrix(nPOI + nw, nPOI + nw);
//            Matrix ones = new Matrix(1, nPOI + nw);
//            ones.Ones();
//            halfSigmaPoints1 = xHat_a0 * ones + S_a0 * sigma_f;
//            halfSigmaPoints2 = xHat_a0 * ones - S_a0 * sigma_f;
//            Xa0.SetSubMatrix(0, nPOI + nw - 1, 0, 0, xHat_a0);
//            Xa0.SetSubMatrix(0, nPOI + nw - 1, 1, (1 + nPOI + nw) - 1, halfSigmaPoints1);
//            Xa0.SetSubMatrix(0, nPOI + nw - 1, nPOI + nw + 1, 2 * (nPOI + nw), halfSigmaPoints2);

//            // dividing into two parts
//            Matrix XPOI_0 = Xa0.Submatrix(0, nPOI - 1, 0, Xa0.Columns - 1);
//            Matrix Xw_0 = Xa0.Submatrix(nPOI, Xa0.Rows - 1, 0, Xa0.Columns - 1);

//            // weight factors
//            double Wm0 = (sigma_f * sigma_f - (nPOI + nw)) / (sigma_f * sigma_f);
//            double Wc0 = Wm0 + 3 - sigma_f * sigma_f / (nPOI + nw);
//            double W = 1 / (2 * sigma_f * sigma_f);
//            //////////////////////////// SR-SPIF PREDICTION ////////////////////////////////
//            Matrix predPOIs = predPOI(XPOI_0, Xw_0, dT, type); // predicted sigma points
//            Matrix xhatPOIm = predPOIs.Submatrix(0, nPOI - 1, 0, 0) * Wm0;
//            for (int i = 1; i < predPOIs.Columns; i++)
//            {
//                xhatPOIm += predPOIs.Submatrix(0, nPOI - 1, i, i) * W;
//            }
//            // re-arrange sigma points
//            Matrix XPOI_Cm = new Matrix(nPOI, 2 * (nPOI + nw) + 1);
//            for (int i = 0; i < XPOI_Cm.Columns; i++)
//            {
//                for (int j = 0; j < XPOI_Cm.Rows; j++)
//                {
//                    XPOI_Cm[j, i] = predPOIs[j, i] - xhatPOIm[j, 0];
//                }
//            }

//            /////////////////////// CONVERSION FROM PREDICTION TO UPDATE ///////////////////////
//            // define x_x2
//            Matrix x_nav = xx2.Submatrix(0, 2, 0, 0);
//            Matrix x_att = xx2.Submatrix(3, 5, 0, 0);
//            Matrix x_gim = xx2.Submatrix(6, 8, 0, 0);
//            Matrix S_nav = Sx2.Submatrix(0, 2, 0, 2);
//            Matrix S_att = Sx2.Submatrix(3, 5, 3, 5);
//            Matrix S_gim = Sx2.Submatrix(6, 8, 6, 8);

//            int nSCR = 2; int nx2 = xx2.Rows; int n2a = nPOI + nw + nSCR + nx2 + nLaser;
//            Matrix xhat_x2_m = new Matrix(nx2, 1); // creating x_x2 vector
//            for (int i = 0; i < 3; i++)
//            {
//                xhat_x2_m[i, 0] = x_nav[i, 0];
//                xhat_x2_m[i + 3, 0] = x_att[i, 0];
//                xhat_x2_m[i + 6, 0] = x_gim[i, 0];
//            }
//            Matrix S_x2_m = new Matrix(nx2, nx2); // creating augmented covariance diagonal matrix for x2
//            for (int i = 0; i < 3; i++)
//            {
//                S_x2_m[i, i] = S_nav[i, i];
//                S_x2_m[i + 3, i + 3] = S_att[i, i];
//                S_x2_m[i + 6, i + 6] = S_gim[i, i];
//            }

//            // equation 20
//            Matrix X_aug_c0m = new Matrix(nPOI + nSCR + nLaser + nx2, 1);
//            X_aug_c0m.Zero();
//            for (int i = 0; i < nPOI; i++)
//                X_aug_c0m[i, 0] = XPOI_Cm[i, 0];
//            Matrix X_aug_cm = new Matrix(nPOI + nSCR + nLaser + nx2, 2 * n2a); X_aug_cm.Zero();
//            X_aug_cm.SetSubMatrix(0, nPOI - 1, 0, XPOI_Cm.Columns - 2, XPOI_Cm.Submatrix(0, nPOI - 1, 1, XPOI_Cm.Columns - 1));
//            ones = new Matrix(1, 2 * nSCR + nLaser); ones.Ones();
//            X_aug_cm.SetSubMatrix(0, nPOI - 1, XPOI_Cm.Columns - 1, XPOI_Cm.Columns - 1 + (2 * nSCR + nLaser) - 1, XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * ones);
//            ones = new Matrix(1, 2 * nx2); ones.Ones();
//            X_aug_cm.SetSubMatrix(0, nPOI - 1, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser), XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser + nx2) - 1, XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * ones);
//            X_aug_cm.SetSubMatrix(nPOI, nPOI + nSCR + nLaser - 1, XPOI_Cm.Columns - 1, XPOI_Cm.Columns - 1 + nSCR + nLaser - 1, SMeasurement);
//            X_aug_cm.SetSubMatrix(nPOI, nPOI + nSCR + nLaser - 1, XPOI_Cm.Columns - 1 + nSCR + nLaser, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser) - 1, SMeasurement * -1);
//            X_aug_cm.SetSubMatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser), XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser) + nx2 - 1, Sx2);
//            X_aug_cm.SetSubMatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser) + nx2, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser + nx2) - 1, Sx2 * -1);


//            // equation 21
//            Matrix X_aug_m = new Matrix(X_aug_c0m.Rows, X_aug_c0m.Columns + X_aug_cm.Columns);
//            Matrix meanVector = new Matrix(nPOI + nSCR + nLaser + nx2, 1);
//            meanVector.Zero();
//            meanVector.SetSubMatrix(0, nPOI - 1, 0, 0, xhatPOIm);
//            meanVector.SetSubMatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, 0, 0, xx2);
//            ones = new Matrix(1, 2 * n2a); ones.Ones();
//            // create augmented sigma points
//            X_aug_m.SetSubMatrix(0, X_aug_m.Rows - 1, 0, 0, X_aug_c0m + meanVector);
//            X_aug_m.SetSubMatrix(0, X_aug_m.Rows - 1, 1, (2 * n2a) + 1 - 1, X_aug_cm + meanVector * ones);

//            // weight configurationU
//            double Wm0_aug = (sigma_f * sigma_f - n2a) / (sigma_f * sigma_f);
//            double Wc0_aug = Wm0_aug + 3 - (sigma_f * sigma_f / n2a);

//            ////////////////////////////////////// SR-SPIF UPDATE /////////////////////////////////////////////
//            Matrix rangeNoise = new Matrix(1, 1); rangeNoise[0, 0] = SMeasurement[2, 2];
//            Matrix Z_SCR = MeasurementFct(X_aug_m.Submatrix(0, nPOI - 1, 0, X_aug_m.Columns - 1),
//                                                                        X_aug_m.Submatrix(nPOI + nSCR + nLaser, X_aug_m.Rows - 1, 0, X_aug_m.Columns - 1),
//                                                                        X_aug_m.Submatrix(nPOI, nPOI + nSCR + nLaser - 1, 0, X_aug_m.Columns - 1), rangeNoise, cameraType, screenSize);
//            Matrix z_mean = new Matrix(nSCR + nLaser, 1); // measurement mean
//            for (int i = 0; i < Z_SCR.Columns; i++)
//            {
//                if (i == 0)
//                    z_mean += Z_SCR.Submatrix(0, Z_SCR.Rows - 1, 0, 0) * Wm0_aug;
//                else
//                    z_mean += Z_SCR.Submatrix(0, Z_SCR.Rows - 1, i, i) * W;
//            }
//            ones = new Matrix(1, Z_SCR.Columns); ones.Ones();
//            Matrix Z_SCR_c = Z_SCR - z_mean * ones;

//            // equation 27
//            //Matrix P_POIx2SCR = new Matrix(nPOI + nx2, nSCR);
//            Matrix augPOIx2cm = new Matrix(nPOI + nx2, X_aug_cm.Columns);
//            augPOIx2cm.SetSubMatrix(0, nPOI - 1, 0, X_aug_cm.Columns - 1, X_aug_cm.Submatrix(0, nPOI - 1, 0, X_aug_cm.Columns - 1));
//            augPOIx2cm.SetSubMatrix(nPOI, nPOI + nx2 - 1, 0, X_aug_cm.Columns - 1, X_aug_cm.Submatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, 0, X_aug_cm.Columns - 1));
//            Matrix augPOIx2c0m = new Matrix(nPOI + nx2, 1);
//            augPOIx2c0m.SetSubMatrix(0, nPOI - 1, 0, 0, X_aug_c0m.Submatrix(0, nPOI - 1, 0, 0));
//            augPOIx2c0m.SetSubMatrix(nPOI, nPOI + nx2 - 1, 0, 0, X_aug_c0m.Submatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, 0, 0));
//            Matrix P_POIx2SCR = augPOIx2cm * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 1, Z_SCR_c.Columns - 1).Transpose() * W
//                                                        + augPOIx2c0m * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0).Transpose() * Wc0_aug;

//            //// Equation 28 and below chol part is equation 15
//            QrDecomposition qrDecomposition = new QrDecomposition(XPOI_Cm.Submatrix(0, XPOI_Cm.Rows - 1, 1, XPOI_Cm.Columns - 1).Transpose());
//            Matrix R_X_POI_Cm = qrDecomposition.UpperTriangularFactor;
//            CholeskyDecomposition cholUpdate;
//            Matrix S_POI_m;
//            if (Wc0_aug < 0)
//            {
//                cholUpdate = new CholeskyDecomposition(R_X_POI_Cm.Transpose() * R_X_POI_Cm * Math.Sqrt(W) * Math.Sqrt(W) - XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0).Transpose() * Math.Abs(Wc0));
//                S_POI_m = cholUpdate.LeftTriangularFactor.Transpose();
//            }
//            else if (Wc0_aug > 0)
//            {
//                cholUpdate = new CholeskyDecomposition(R_X_POI_Cm.Transpose() * R_X_POI_Cm * Math.Sqrt(W) * Math.Sqrt(W) + XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0).Transpose() * Math.Abs(Wc0));
//                S_POI_m = cholUpdate.LeftTriangularFactor.Transpose();
//            }
//            else
//                throw new Exception("Weight is zero");

//            //Matrix S_POI_mVer2 = new Matrix(nPOI, nPOI);
//            for (int i = 0; i < nPOI; i++)
//            {
//                //S_POI_mVer2[i, i] = Math.Abs(S_POI_m[i, i]);
//                S_POI_m[i, i] = Math.Abs(S_POI_m[i, i]);
//            }
//            QrDecomposition qrSPOImVer2 = new QrDecomposition(S_POI_m.Transpose().Inverse);
//            Matrix R_POIminus = qrSPOImVer2.UpperTriangularFactor;
//            Matrix Y_POIminus = R_POIminus.Transpose() * R_POIminus;

//            //QrDecomposition qrDecomposition_S_x2_m = new QrDecomposition(S_x2_m.Inverse.Transpose());
//            //Matrix R_x2m = qrDecomposition_S_x2_m.UpperTriangularFactor;
//            //QrDecomposition qrDecomposition_S_POI_m = new QrDecomposition(S_POI_m.Inverse.Transpose());
//            //Matrix R_POIm = qrDecomposition_S_POI_m.UpperTriangularFactor;
//            //Matrix P_POIm1 = R_POIm.Transpose() * R_POIm;
//            //Matrix P_x2m1 = R_x2m.Transpose() * R_x2m;
//            //Matrix blkDiag = new Matrix(P_POIm1.Rows + P_x2m1.Rows, P_POIm1.Columns + P_x2m1.Columns);
//            //blkDiag.SetSubMatrix(0, P_POIm1.Rows - 1, 0, P_POIm1.Columns - 1, P_POIm1);
//            //blkDiag.SetSubMatrix(P_POIm1.Rows, P_POIm1.Rows + P_x2m1.Rows - 1, P_POIm1.Columns, P_POIm1.Columns + P_x2m1.Columns - 1, P_x2m1);
//            //Matrix C_SCR = P_POIx2SCR.Transpose() * blkDiag;

//            // Equation 28
//            QrDecomposition qrInvSx2 = new QrDecomposition(Sx2.Inverse);
//            Matrix R_x2minus = qrInvSx2.UpperTriangularFactor;
//            Matrix Y_x2minus = R_x2minus.Transpose() * R_x2minus;
//            Matrix blkDiagYseries = new Matrix(nPOI + nx2, nPOI + nx2);
//            blkDiagYseries.SetSubMatrix(0, nPOI - 1, 0, nPOI - 1, Y_POIminus);
//            blkDiagYseries.SetSubMatrix(nPOI, nPOI + nx2 - 1, nPOI, nPOI + nx2 - 1, Y_x2minus);
//            Matrix C = P_POIx2SCR.Transpose() * blkDiagYseries;

//            // SUPER IMPORTANT STEP !!
//            QrDecomposition anotherQrDecomposition = new QrDecomposition(Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 1, Z_SCR_c.Columns - 1).Transpose());
//            Matrix Rvtilde = anotherQrDecomposition.UpperTriangularFactor;
//            CholeskyDecomposition anotherCholUpdate;
//            Matrix Svtilde;
//            if (Wc0_aug < 0)
//            {
//                anotherCholUpdate = new CholeskyDecomposition(Rvtilde.Transpose() * Rvtilde * Math.Sqrt(W) * Math.Sqrt(W) - Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0) * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0).Transpose() * Math.Abs(Wc0_aug));
//                Svtilde = anotherCholUpdate.LeftTriangularFactor.Transpose();
//            }
//            else if (Wc0_aug > 0)
//            {
//                anotherCholUpdate = new CholeskyDecomposition(Rvtilde.Transpose() * Rvtilde * Math.Sqrt(W) * Math.Sqrt(W) + Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0) * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0).Transpose() * Math.Abs(Wc0_aug));
//                Svtilde = anotherCholUpdate.LeftTriangularFactor.Transpose();
//            }
//            else
//                throw new Exception("Weight is zero");
//            //Matrix SvtildeVer2 = new Matrix(Svtilde.Rows, Svtilde.Columns); SvtildeVer2.Zero();
//            for (int i = 0; i < Svtilde.Columns; i++)
//                Svtilde[i, i] = Math.Abs(Svtilde[i, i]);

//            Matrix P_SCR = Svtilde.Transpose() * Svtilde;
//            Matrix Y_SCR = P_SCR.Inverse;
//            CholeskyDecomposition lastCholDecompose = new CholeskyDecomposition(Y_SCR);
//            Matrix R_SCR = lastCholDecompose.LeftTriangularFactor.Transpose();

//            // Equation 31-33
//            //QrDecomposition qrDecomposition_SSCR = new QrDecomposition(SSCR.Transpose().Inverse);
//            //Matrix R_SCR = qrDecomposition_SSCR.UpperTriangularFactor;
//            Matrix v_kp1 = zSCR - z_mean;
//            Matrix i_kp1 = new Matrix(nx2 + nSCR + nLaser, 1);
//            Matrix xhatPOIm_xhat_x2_m_aug = new Matrix(nPOI + nx2, 1);
//            xhatPOIm_xhat_x2_m_aug.SetSubMatrix(0, nPOI - 1, 0, 0, xhatPOIm);
//            xhatPOIm_xhat_x2_m_aug.SetSubMatrix(nPOI, nPOI + nx2 - 1, 0, 0, xhat_x2_m);
//            i_kp1.SetSubMatrix(0, nx2 - 1, 0, 0, R_x2minus * xhat_x2_m);
//            i_kp1.SetSubMatrix(nx2, nx2 + nSCR + nLaser - 1, 0, 0, R_SCR * (v_kp1 + C * xhatPOIm_xhat_x2_m_aug));
//            Matrix I_kp1 = R_SCR * C;
//            // equation 34
//            // actually [blkDiag; I_kp1];
//            Matrix blkDiag;
//            blkDiag = new Matrix(R_POIminus.Rows + R_x2minus.Rows + I_kp1.Rows, R_POIminus.Columns + R_x2minus.Columns);
//            blkDiag.SetSubMatrix(0, R_POIminus.Rows - 1, 0, R_POIminus.Columns - 1, R_POIminus);
//            blkDiag.SetSubMatrix(R_POIminus.Rows, R_POIminus.Rows + R_x2minus.Rows - 1, R_POIminus.Columns, R_POIminus.Columns + R_x2minus.Columns - 1, R_x2minus);
//            blkDiag.SetSubMatrix(R_POIminus.Rows + R_x2minus.Rows, blkDiag.Rows - 1, 0, blkDiag.Columns - 1, I_kp1);
//            QrDecomposition qrDecomposition_blkDiag = new QrDecomposition(blkDiag);
//            Matrix R_blob = qrDecomposition_blkDiag.UpperTriangularFactor;
//            Matrix T_kp1 = qrDecomposition_blkDiag.OrthogonalFactor;
//            Matrix RPOI = R_blob.Submatrix(0, nPOI - 1, 0, nPOI - 1);
//            Matrix RPOIx2 = R_blob.Submatrix(0, nPOI - 1, nPOI, R_blob.Columns - 1);
//            Matrix Rx2 = R_blob.Submatrix(nPOI, R_blob.Rows - 1, nPOI, R_blob.Columns - 1);

//            // final calculation
//            Matrix ninvRPOI_RPOIx2_invRx2 = (RPOI.Inverse * -1) * RPOIx2 * Rx2.Inverse;
//            Matrix R_POIm_xhatPOIm_i_kp1 = new Matrix(nPOI + i_kp1.Rows, 1);
//            R_POIm_xhatPOIm_i_kp1.SetSubMatrix(0, nPOI - 1, 0, 0, R_POIminus * xhatPOIm);
//            R_POIm_xhatPOIm_i_kp1.SetSubMatrix(nPOI, R_POIm_xhatPOIm_i_kp1.Rows - 1, 0, 0, i_kp1);

//            Matrix invRPOI_ninvRPOI_RPOIx2_invRx2 = new Matrix(RPOI.Rows, RPOI.Columns + ninvRPOI_RPOIx2_invRx2.Columns);
//            invRPOI_ninvRPOI_RPOIx2_invRx2.SetSubMatrix(0, nPOI - 1, 0, nPOI - 1, RPOI.Inverse);
//            invRPOI_ninvRPOI_RPOIx2_invRx2.SetSubMatrix(0, nPOI - 1, nPOI, invRPOI_ninvRPOI_RPOIx2_invRx2.Columns - 1, ninvRPOI_RPOIx2_invRx2);
//            Matrix xhatPOI = invRPOI_ninvRPOI_RPOIx2_invRx2 * T_kp1.Transpose() * R_POIm_xhatPOIm_i_kp1;

//            Matrix invRPOI_ninvRPOI_RPOIx2_invRx2_zeros_invRx2 = new Matrix(invRPOI_ninvRPOI_RPOIx2_invRx2.Rows + nx2 + Rx2.Rows, invRPOI_ninvRPOI_RPOIx2_invRx2.Columns);
//            invRPOI_ninvRPOI_RPOIx2_invRx2_zeros_invRx2.SetSubMatrix(0, invRPOI_ninvRPOI_RPOIx2_invRx2.Rows - 1, 0, invRPOI_ninvRPOI_RPOIx2_invRx2.Columns - 1, invRPOI_ninvRPOI_RPOIx2_invRx2);
//            invRPOI_ninvRPOI_RPOIx2_invRx2_zeros_invRx2.SetSubMatrix(invRPOI_ninvRPOI_RPOIx2_invRx2.Rows, invRPOI_ninvRPOI_RPOIx2_invRx2.Rows + nx2 - 1, nPOI, invRPOI_ninvRPOI_RPOIx2_invRx2.Columns - 1, Rx2.Inverse);
//            QrDecomposition qrDecomposition_invRPOIstuff = new QrDecomposition(invRPOI_ninvRPOI_RPOIx2_invRx2_zeros_invRx2);
//            Matrix S_blob = qrDecomposition_invRPOIstuff.UpperTriangularFactor;
//            Matrix newSPOI = S_blob.Submatrix(0, nPOI - 1, 0, nPOI - 1);

//            targetState = xhatPOI;
//            S_target = newSPOI;

//            //if (Math.Abs(S_target[0, 0]) < Math.Sqrt(0.5))
//            //    S_target[0, 0] = Math.Sqrt(0.5) * Math.Sign(S_target[0, 0]);
//            //if (Math.Abs(S_target[1, 0]) < Math.Sqrt(0.5))
//            //    S_target[1, 0] = Math.Sqrt(0.5) * Math.Sign(S_target[1, 0]);

//            /* if not void
//            List<Matrix> toReturn = new List<Matrix>(2);
//            toReturn.Add(xhatPOI);
//            toReturn.Add(newSPOI);
//            return toReturn;
//            */
//        }

//        private Matrix MeasurementFct(Matrix x_POI, Matrix x_x2, Matrix x_SCR_noise, Matrix rangeNoise, string cameraType, string size)
//        {
//            Matrix Z_SCR = new Matrix(x_SCR_noise.Rows, x_POI.Columns);
//            MathNet.Numerics.Distributions.NormalDistribution normDist = new MathNet.Numerics.Distributions.NormalDistribution(0, 2.0);
//            string newSize = size;
//            if (size.Equals("960x240"))
//                newSize = "320x240";
//            for (int i = 0; i < x_POI.Columns; i++)
//            {
//                Matrix xNAV = x_x2.Submatrix(0, 2, i, i);
//                Matrix xATT = x_x2.Submatrix(3, 5, i, i);
//                Matrix xGIM = x_x2.Submatrix(6, 8, i, i);
//                Z_SCR.SetSubMatrix(0, x_SCR_noise.Rows - 2, i, i, Point2Pixel(x_POI.Submatrix(0, x_POI.Rows - 1, i, i),
//                                                                                        xNAV, xATT, xGIM, x_SCR_noise.Submatrix(0, x_SCR_noise.Rows - 2, i, i), cameraType, newSize));
//                double laser = Math.Sqrt((x_POI[0, i] - xNAV[0, 0]) * (x_POI[0, i] - xNAV[0, 0]) + (x_POI[1, i] - xNAV[1, 0]) * (x_POI[1, i] - xNAV[1, 0]));// + (x_POI[2, i] - xNAV[2, 0]) * (x_POI[2, i] - xNAV[2, 0]));// +normDist.NextDouble() * rangeNoise[0, 0];
//                Z_SCR[2, i] = laser;
//            }
//            return Z_SCR;
//        }


//        /// <summary>
//        /// calculates corresponding pixel point based on the state information. The camera matrix is for Unibrain FireI cam with 320 x 240 resolution
//        /// </summary>
//        /// <param name="xPOI"></param>
//        /// <param name="xNAV"></param>
//        /// <param name="xATT"></param>
//        /// <param name="xGIM"></param>
//        /// <param name="xSCR_noise"></param>
//        /// <returns></returns>
//        public Matrix Point2Pixel(Matrix xPOI, Matrix xNAV, Matrix xATT, Matrix xGIM, Matrix xSCR_noise, string cameraType, string size)
//        {
//            double[] fc = new double[2];
//            double[] cc = new double[2];
//            if (size.Equals("320x240"))
//            {
//                // for 320 x 240 image with Unibrain Fire-i camera
//                if (cameraType.Equals("Fire-i"))
//                {
//                    fc[0] = 384.4507; fc[1] = 384.1266;
//                    cc[0] = 155.1999; cc[1] = 101.5641;
//                }
//                else if (cameraType.Equals("FireFly"))
//                {
//                    fc[0] = 345.26498; fc[1] = 344.99438;
//                    cc[0] = 159.36854; cc[1] = 118.26944;
//                }
//            }
//            else if (size.Equals("640x480"))
//            {
//                // for Unibrain Fire-i
//                if (cameraType.Equals("Fire-i"))
//                {
//                    fc[0] = 763.5805; fc[1] = 763.8337;
//                    cc[0] = 303.0963; cc[1] = 215.9287;
//                }
//                // for Fire-Fly MV (Point Gray)
//                else if (cameraType.Equals("FireFly"))
//                {
//                    fc[0] = 691.09778; fc[1] = 690.70187;
//                    cc[0] = 324.07388; cc[1] = 234.22204;
//                }
//            }
//            double alpha_c = 0;

//            // camera matrix - for Uni-Brain IEEE1394 CCD camera with 320 x 240 resolution
//            Matrix KK = new Matrix(fc[0], alpha_c * fc[0], cc[0], 0, fc[1], cc[1], 0, 0, 1);

//            // extrinsic parameter (transformation)
//            double pan = xGIM[0, 0]; double tilt = xGIM[1, 0]; double scan = xGIM[2, 0];
//            double roll = xATT[0, 0]; double pitch = xATT[1, 0]; double yaw = xATT[2, 0];

//            Matrix vector = xPOI.Submatrix(0, 2, 0, 0) - xNAV;

//            ///// MAKE IT WORK IN 3D ///
//            yaw = yaw - Math.PI / 2;
//            //yaw = Math.PI / 2 - yaw;
//            //Matrix R_ENU2R = new Matrix(Math.Cos(yaw), Math.Sin(yaw), 0, -Math.Sin(yaw), Math.Cos(yaw), 0, 0, 0, 1);
//            Matrix R_ENU2R = new Matrix(Math.Cos(yaw), Math.Sin(yaw), 0, -Math.Sin(yaw), Math.Cos(yaw), 0, 0, 0, 1) *
//                                             new Matrix(1, 0, 0, 0, Math.Cos(pitch), Math.Sin(pitch), 0, -Math.Sin(pitch), Math.Cos(pitch)) *
//                                             new Matrix(Math.Cos(roll), 0, -Math.Sin(roll), 0, 1, 0, Math.Sin(roll), 0, Math.Cos(roll));
//            //Matrix R_R2G = new Matrix(Math.Cos(pan), Math.Sin(pan), 0, -Math.Sin(pan), Math.Cos(pan), 0, 0, 0, 1);
//            Matrix R_R2G = new Matrix(Math.Cos(pan), Math.Sin(pan), 0, -Math.Sin(pan), Math.Cos(pan), 0, 0, 0, 1) *
//                                         new Matrix(1, 0, 0, 0, Math.Cos(tilt), -Math.Sin(tilt), 0, Math.Sin(tilt), Math.Cos(tilt)) *
//                                         new Matrix(Math.Cos(scan), -Math.Sin(scan), 0, Math.Sin(scan), Math.Cos(scan), 0, 0, 0, 1);
//            vector = R_R2G * R_ENU2R * vector; // now in camera coordinate frame

//            Matrix newVector = new Matrix(3, 1);
//            newVector[0, 0] = vector[0, 0] / vector[1, 0];
//            newVector[1, 0] = -vector[2, 0] / vector[1, 0];
//            newVector[2, 0] = vector[1, 0] / vector[1, 0];
//            Matrix xSCR = new Matrix(2, 1);
//            xSCR = KK * newVector;
//            return xSCR.Submatrix(0, 1, 0, 0) + xSCR_noise.Submatrix(0, 1, 0, 0);

//        }
//        /// <summary>
//        /// Predict next sigma points using dynamic model
//        /// </summary>
//        /// <param name="XPOI">Sigma points of POI: [nPOI x (2 * (nPOI+nw) + 1)]</param>
//        /// <param name="Xw">Sigma points of noise: [nw x (2 * (nPOI+nw) + 1)]</param>
//        /// <param name="dT">time interval for integration (discrete system)</param>
//        /// <param name="type">Indicating what kind of target it is</param>
//        /// <returns></returns>
//        private Matrix predPOI(Matrix XPOI, Matrix Xw, double dT, TargetTypes type)
//        {
//            // state: [x y z velocity heading turn-rate accleration]
//            Matrix toReturn = new Matrix(XPOI.Rows, XPOI.Columns);
//            for (int i = 0; i < toReturn.Columns; i++)
//            {
//                for (int j = 0; j < toReturn.Rows; j++)
//                {
//                    if (type == TargetTypes.PotentialSOOI || type == TargetTypes.ConfirmedSOOI)
//                    {
//                        //if (j == 2)
//                        //    toReturn[j, i] = Xw[0, i];
//                        if (j == 5)
//                            toReturn[j, i] = Xw[1, i];
//                        else if (j == 6)
//                            toReturn[j, i] = Xw[2, i];
//                        else
//                            toReturn[j, i] = XPOI[j, i];
//                    }
//                    else if (type == TargetTypes.PotentialMOOI || type == TargetTypes.ConfirmedMOOI)
//                    {
//                        //if (j == 2)
//                        //    toReturn[j, i] = Xw[0, i];
//                        if (j == 5)
//                            toReturn[j, i] = Xw[1, i];
//                        else if (j == 6)
//                            toReturn[j, i] = Xw[2, i];
//                        else
//                            toReturn[j, i] = XPOI[j, i] + normDist.NextDouble() * dT; // random walk????
//                    }
//                }
//            }
//            return toReturn;
//        }

//        internal void UpdateWithoutMeasurement()
//        {
//            S_target += new Matrix(S_target.Rows, S_target.Columns, Math.Sqrt(Math.Abs(normDist.NextDouble() / 1000)));
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Magic.Common.Mapack;
using Magic.Common.Messages;

// Bearing only target tracking algorithm
// written by Daniel Lee

namespace Magic.TargetTracking
{
	class TargetTrackingAlgorithm
	{
		// point of interest (POI) state
		Matrix targetState;
		public Matrix TargetState { get { return targetState; } }
		// POI covaraicne matrix
		Matrix S_target;
		public Matrix STarget { get { return S_target; } }
		MathNet.Numerics.Distributions.NormalDistribution normDist = new MathNet.Numerics.Distributions.NormalDistribution(0, 2);
		double dT;

		public TargetTrackingAlgorithm(double dT)
		{
			this.dT = dT;
		}

		/// <summary>
		/// Square Root, Sigma Points Information filter function. This estimates the information (of target) at next timestep (k+1) with given input at time k.
		/// </summary>
		/// <param name="xPOI">[7 x 1: east north up vel heading turnRate accleration]</param>
		/// <param name="xx2">[9 x 1 : 3 NAV, 3ATT, 3GIM] = [E N U, roll pitch yaw, pan tilt scan]</param>
		/// <param name="SPOI">[7 x 7 square root covariance of POI]</param>
		/// <param name="Sw">[3 x 3 square root covariance of sensor noise: altitude, turnRate, and accleration]</param>
		/// <param name="SMeasurement">[2 x 1 covariance vector of measurement]</param>
		/// <param name="Sx2">[9 x 9 square root covariance of x2]</param>
		/// <param name="zSCR">[2 x 1 Actual measurement of screen (pixels)]</param>
		/// <param name="sigma_f">scaling factor for the distance of the sigmap points from the mean</param>
		/// <returns></returns>
		public void Update(Matrix xPOI, Matrix xx2, Matrix SPOI, Matrix Sw, Matrix SMeasurement, Matrix Sx2, Matrix zSCR, double sigma_f, string screenSize, string cameraType, TargetTypes type)
		{
			//////////////////////////////// INITIALIZATION /////////////////////////////////////
			// SORRY - almost impossible to debug.
			// variable names are mostly following IEEE Transaction paper written by Dr. Mark Campbell.
			// also the variable names strictly follow the names I used in MATLAB code.
			int nPOI = xPOI.Rows;
			int nw = Sw.Rows;
			int nLaser = 1;
			// state matrix
			Matrix xHat_a0 = new Matrix(nPOI + nw, 1);
			for (int i = 0; i < nPOI; i++)
				xHat_a0[i, 0] = xPOI[i, 0];

			for (int i = 0; i < nw; i++)
				xHat_a0[i + nPOI, 0] = 0;
			// covariance matrix
			Matrix S_a0 = new Matrix(nPOI + nw, nPOI + nw);
			for (int i = 0; i < nPOI; i++)
				S_a0[i, i] = SPOI[i, i];
			for (int i = nPOI; i < nPOI + nw; i++)
				S_a0[i, i] = Sw[i - nPOI, i - nPOI];

			// generate 2*(nPOI + nw) + 1 sigma points
			Matrix Xa0 = new Matrix(nPOI + nw, 2 * (nPOI + nw) + 1);
			Matrix halfSigmaPoints1 = new Matrix(nPOI + nw, nPOI + nw);
			Matrix halfSigmaPoints2 = new Matrix(nPOI + nw, nPOI + nw);
			Matrix ones = new Matrix(1, nPOI + nw);
			ones.Ones();
			halfSigmaPoints1 = xHat_a0 * ones + S_a0 * sigma_f;
			halfSigmaPoints2 = xHat_a0 * ones - S_a0 * sigma_f;
			Xa0.SetSubMatrix(0, nPOI + nw - 1, 0, 0, xHat_a0);
			Xa0.SetSubMatrix(0, nPOI + nw - 1, 1, (1 + nPOI + nw) - 1, halfSigmaPoints1);
			Xa0.SetSubMatrix(0, nPOI + nw - 1, nPOI + nw + 1, 2 * (nPOI + nw), halfSigmaPoints2);

			// dividing into two parts
			Matrix XPOI_0 = Xa0.Submatrix(0, nPOI - 1, 0, Xa0.Columns - 1);
			Matrix Xw_0 = Xa0.Submatrix(nPOI, Xa0.Rows - 1, 0, Xa0.Columns - 1);

			// weight factors
			double Wm0 = (sigma_f * sigma_f - (nPOI + nw)) / (sigma_f * sigma_f);
			double Wc0 = Wm0 + 3 - sigma_f * sigma_f / (nPOI + nw);
			double W = 1 / (2 * sigma_f * sigma_f);
			//////////////////////////// SR-SPIF PREDICTION ////////////////////////////////
			Matrix predPOIs = predPOI(XPOI_0, Xw_0, dT, type); // predicted sigma points
			Matrix xhatPOIm = predPOIs.Submatrix(0, nPOI - 1, 0, 0) * Wm0;
			for (int i = 1; i < predPOIs.Columns; i++)
			{
				xhatPOIm += predPOIs.Submatrix(0, nPOI - 1, i, i) * W;
			}
			// re-arrange sigma points
			Matrix XPOI_Cm = new Matrix(nPOI, 2 * (nPOI + nw) + 1);
			for (int i = 0; i < XPOI_Cm.Columns; i++)
			{
				for (int j = 0; j < XPOI_Cm.Rows; j++)
				{
					XPOI_Cm[j, i] = predPOIs[j, i] - xhatPOIm[j, 0];
				}
			}

			/////////////////////// CONVERSION FROM PREDICTION TO UPDATE ///////////////////////
			// define x_x2
			Matrix x_nav = xx2.Submatrix(0, 2, 0, 0);
			Matrix x_att = xx2.Submatrix(3, 5, 0, 0);
			Matrix x_gim = xx2.Submatrix(6, 8, 0, 0);
			Matrix S_nav = Sx2.Submatrix(0, 2, 0, 2);
			Matrix S_att = Sx2.Submatrix(3, 5, 3, 5);
			Matrix S_gim = Sx2.Submatrix(6, 8, 6, 8);

			int nSCR = 2; int nx2 = xx2.Rows; int n2a = nPOI + nw + nSCR + nx2 + nLaser;
			Matrix xhat_x2_m = new Matrix(nx2, 1); // creating x_x2 vector
			for (int i = 0; i < 3; i++)
			{
				xhat_x2_m[i, 0] = x_nav[i, 0];
				xhat_x2_m[i + 3, 0] = x_att[i, 0];
				xhat_x2_m[i + 6, 0] = x_gim[i, 0];
			}
			Matrix S_x2_m = new Matrix(nx2, nx2); // creating augmented covariance diagonal matrix for x2
			for (int i = 0; i < 3; i++)
			{
				S_x2_m[i, i] = S_nav[i, i];
				S_x2_m[i + 3, i + 3] = S_att[i, i];
				S_x2_m[i + 6, i + 6] = S_gim[i, i];
			}

			// equation 20
			Matrix X_aug_c0m = new Matrix(nPOI + nSCR + nLaser + nx2, 1);
			X_aug_c0m.Zero();
			for (int i = 0; i < nPOI; i++)
				X_aug_c0m[i, 0] = XPOI_Cm[i, 0];
			Matrix X_aug_cm = new Matrix(nPOI + nSCR + nLaser + nx2, 2 * n2a); X_aug_cm.Zero();
			X_aug_cm.SetSubMatrix(0, nPOI - 1, 0, XPOI_Cm.Columns - 2, XPOI_Cm.Submatrix(0, nPOI - 1, 1, XPOI_Cm.Columns - 1));
			ones = new Matrix(1, 2 * nSCR + nLaser); ones.Ones();
			X_aug_cm.SetSubMatrix(0, nPOI - 1, XPOI_Cm.Columns - 1, XPOI_Cm.Columns - 1 + (2 * nSCR + nLaser) - 1, XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * ones);
			ones = new Matrix(1, 2 * nx2); ones.Ones();
			X_aug_cm.SetSubMatrix(0, nPOI - 1, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser), XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser + nx2) - 1, XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * ones);
			X_aug_cm.SetSubMatrix(nPOI, nPOI + nSCR + nLaser - 1, XPOI_Cm.Columns - 1, XPOI_Cm.Columns - 1 + nSCR + nLaser - 1, SMeasurement);
			X_aug_cm.SetSubMatrix(nPOI, nPOI + nSCR + nLaser - 1, XPOI_Cm.Columns - 1 + nSCR + nLaser, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser) - 1, SMeasurement * -1);
			X_aug_cm.SetSubMatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser), XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser) + nx2 - 1, Sx2);
			X_aug_cm.SetSubMatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser) + nx2, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser + nx2) - 1, Sx2 * -1);


			// equation 21
			Matrix X_aug_m = new Matrix(X_aug_c0m.Rows, X_aug_c0m.Columns + X_aug_cm.Columns);
			Matrix meanVector = new Matrix(nPOI + nSCR + nLaser + nx2, 1);
			meanVector.Zero();
			meanVector.SetSubMatrix(0, nPOI - 1, 0, 0, xhatPOIm);
			meanVector.SetSubMatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, 0, 0, xx2);
			ones = new Matrix(1, 2 * n2a); ones.Ones();
			// create augmented sigma points
			X_aug_m.SetSubMatrix(0, X_aug_m.Rows - 1, 0, 0, X_aug_c0m + meanVector);
			X_aug_m.SetSubMatrix(0, X_aug_m.Rows - 1, 1, (2 * n2a) + 1 - 1, X_aug_cm + meanVector * ones);

			// weight configurationU
			double Wm0_aug = (sigma_f * sigma_f - n2a) / (sigma_f * sigma_f);
			double Wc0_aug = Wm0_aug + 3 - (sigma_f * sigma_f / n2a);

			////////////////////////////////////// SR-SPIF UPDATE /////////////////////////////////////////////
			Matrix rangeNoise = new Matrix(1, 1); rangeNoise[0, 0] = SMeasurement[2, 2];
			Matrix Z_SCR = MeasurementFct(X_aug_m.Submatrix(0, nPOI - 1, 0, X_aug_m.Columns - 1),
																		X_aug_m.Submatrix(nPOI + nSCR + nLaser, X_aug_m.Rows - 1, 0, X_aug_m.Columns - 1),
																		X_aug_m.Submatrix(nPOI, nPOI + nSCR + nLaser - 1, 0, X_aug_m.Columns - 1), rangeNoise, cameraType, screenSize);
			Matrix z_mean = new Matrix(nSCR + nLaser, 1); // measurement mean
			for (int i = 0; i < Z_SCR.Columns; i++)
			{
				if (i == 0)
					z_mean += Z_SCR.Submatrix(0, Z_SCR.Rows - 1, 0, 0) * Wm0_aug;
				else
					z_mean += Z_SCR.Submatrix(0, Z_SCR.Rows - 1, i, i) * W;
			}
			ones = new Matrix(1, Z_SCR.Columns); ones.Ones();
			Matrix Z_SCR_c = Z_SCR - z_mean * ones;

			// equation 27
			//Matrix P_POIx2SCR = new Matrix(nPOI + nx2, nSCR);
			Matrix augPOIx2cm = new Matrix(nPOI + nx2, X_aug_cm.Columns);
			augPOIx2cm.SetSubMatrix(0, nPOI - 1, 0, X_aug_cm.Columns - 1, X_aug_cm.Submatrix(0, nPOI - 1, 0, X_aug_cm.Columns - 1));
			augPOIx2cm.SetSubMatrix(nPOI, nPOI + nx2 - 1, 0, X_aug_cm.Columns - 1, X_aug_cm.Submatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, 0, X_aug_cm.Columns - 1));
			Matrix augPOIx2c0m = new Matrix(nPOI + nx2, 1);
			augPOIx2c0m.SetSubMatrix(0, nPOI - 1, 0, 0, X_aug_c0m.Submatrix(0, nPOI - 1, 0, 0));
			augPOIx2c0m.SetSubMatrix(nPOI, nPOI + nx2 - 1, 0, 0, X_aug_c0m.Submatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, 0, 0));
			Matrix P_POIx2SCR = augPOIx2cm * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 1, Z_SCR_c.Columns - 1).Transpose() * W
														+ augPOIx2c0m * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0).Transpose() * Wc0_aug;

			//// Equation 28 and below chol part is equation 15
			QrDecomposition qrDecomposition = new QrDecomposition(XPOI_Cm.Submatrix(0, XPOI_Cm.Rows - 1, 1, XPOI_Cm.Columns - 1).Transpose());
			Matrix R_X_POI_Cm = qrDecomposition.UpperTriangularFactor;
			CholeskyDecomposition cholUpdate;
			Matrix S_POI_m;
			if (Wc0_aug < 0)
			{
				cholUpdate = new CholeskyDecomposition(R_X_POI_Cm.Transpose() * R_X_POI_Cm * Math.Sqrt(W) * Math.Sqrt(W) - XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0).Transpose() * Math.Abs(Wc0));
				S_POI_m = cholUpdate.LeftTriangularFactor.Transpose();
			}
			else if (Wc0_aug > 0)
			{
				cholUpdate = new CholeskyDecomposition(R_X_POI_Cm.Transpose() * R_X_POI_Cm * Math.Sqrt(W) * Math.Sqrt(W) + XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0).Transpose() * Math.Abs(Wc0));
				S_POI_m = cholUpdate.LeftTriangularFactor.Transpose();
			}
			else
				throw new Exception("Weight is zero");

			//Matrix S_POI_mVer2 = new Matrix(nPOI, nPOI);
			for (int i = 0; i < nPOI; i++)
			{
				//S_POI_mVer2[i, i] = Math.Abs(S_POI_m[i, i]);
				S_POI_m[i, i] = Math.Abs(S_POI_m[i, i]);
			}
			QrDecomposition qrSPOImVer2 = new QrDecomposition(S_POI_m.Transpose().Inverse);
			Matrix R_POIminus = qrSPOImVer2.UpperTriangularFactor;
			Matrix Y_POIminus = R_POIminus.Transpose() * R_POIminus;

			//QrDecomposition qrDecomposition_S_x2_m = new QrDecomposition(S_x2_m.Inverse.Transpose());
			//Matrix R_x2m = qrDecomposition_S_x2_m.UpperTriangularFactor;
			//QrDecomposition qrDecomposition_S_POI_m = new QrDecomposition(S_POI_m.Inverse.Transpose());
			//Matrix R_POIm = qrDecomposition_S_POI_m.UpperTriangularFactor;
			//Matrix P_POIm1 = R_POIm.Transpose() * R_POIm;
			//Matrix P_x2m1 = R_x2m.Transpose() * R_x2m;
			//Matrix blkDiag = new Matrix(P_POIm1.Rows + P_x2m1.Rows, P_POIm1.Columns + P_x2m1.Columns);
			//blkDiag.SetSubMatrix(0, P_POIm1.Rows - 1, 0, P_POIm1.Columns - 1, P_POIm1);
			//blkDiag.SetSubMatrix(P_POIm1.Rows, P_POIm1.Rows + P_x2m1.Rows - 1, P_POIm1.Columns, P_POIm1.Columns + P_x2m1.Columns - 1, P_x2m1);
			//Matrix C_SCR = P_POIx2SCR.Transpose() * blkDiag;

			// Equation 28
			QrDecomposition qrInvSx2 = new QrDecomposition(Sx2.Inverse);
			Matrix R_x2minus = qrInvSx2.UpperTriangularFactor;
			Matrix Y_x2minus = R_x2minus.Transpose() * R_x2minus;
			Matrix blkDiagYseries = new Matrix(nPOI + nx2, nPOI + nx2);
			blkDiagYseries.SetSubMatrix(0, nPOI - 1, 0, nPOI - 1, Y_POIminus);
			blkDiagYseries.SetSubMatrix(nPOI, nPOI + nx2 - 1, nPOI, nPOI + nx2 - 1, Y_x2minus);
			Matrix C = P_POIx2SCR.Transpose() * blkDiagYseries;

			// SUPER IMPORTANT STEP !!
			QrDecomposition anotherQrDecomposition = new QrDecomposition(Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 1, Z_SCR_c.Columns - 1).Transpose());
			Matrix Rvtilde = anotherQrDecomposition.UpperTriangularFactor;
			CholeskyDecomposition anotherCholUpdate;
			Matrix Svtilde;
			if (Wc0_aug < 0)
			{
				anotherCholUpdate = new CholeskyDecomposition(Rvtilde.Transpose() * Rvtilde * Math.Sqrt(W) * Math.Sqrt(W) - Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0) * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0).Transpose() * Math.Abs(Wc0_aug));
				Svtilde = anotherCholUpdate.LeftTriangularFactor.Transpose();
			}
			else if (Wc0_aug > 0)
			{
				anotherCholUpdate = new CholeskyDecomposition(Rvtilde.Transpose() * Rvtilde * Math.Sqrt(W) * Math.Sqrt(W) + Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0) * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0).Transpose() * Math.Abs(Wc0_aug));
				Svtilde = anotherCholUpdate.LeftTriangularFactor.Transpose();
			}
			else
				throw new Exception("Weight is zero");
			//Matrix SvtildeVer2 = new Matrix(Svtilde.Rows, Svtilde.Columns); SvtildeVer2.Zero();
			for (int i = 0; i < Svtilde.Columns; i++)
				Svtilde[i, i] = Math.Abs(Svtilde[i, i]);

			Matrix P_SCR = Svtilde.Transpose() * Svtilde;
			Matrix Y_SCR = P_SCR.Inverse;
			CholeskyDecomposition lastCholDecompose = new CholeskyDecomposition(Y_SCR);
			Matrix R_SCR = lastCholDecompose.LeftTriangularFactor.Transpose();

			// Equation 31-33
			//QrDecomposition qrDecomposition_SSCR = new QrDecomposition(SSCR.Transpose().Inverse);
			//Matrix R_SCR = qrDecomposition_SSCR.UpperTriangularFactor;
			Matrix v_kp1 = zSCR - z_mean;
			Matrix i_kp1 = new Matrix(nx2 + nSCR + nLaser, 1);
			Matrix xhatPOIm_xhat_x2_m_aug = new Matrix(nPOI + nx2, 1);
			xhatPOIm_xhat_x2_m_aug.SetSubMatrix(0, nPOI - 1, 0, 0, xhatPOIm);
			xhatPOIm_xhat_x2_m_aug.SetSubMatrix(nPOI, nPOI + nx2 - 1, 0, 0, xhat_x2_m);
			i_kp1.SetSubMatrix(0, nx2 - 1, 0, 0, R_x2minus * xhat_x2_m);
			i_kp1.SetSubMatrix(nx2, nx2 + nSCR + nLaser - 1, 0, 0, R_SCR * (v_kp1 + C * xhatPOIm_xhat_x2_m_aug));
			Matrix I_kp1 = R_SCR * C;
			// equation 34
			// actually [blkDiag; I_kp1];
			Matrix blkDiag;
			blkDiag = new Matrix(R_POIminus.Rows + R_x2minus.Rows + I_kp1.Rows, R_POIminus.Columns + R_x2minus.Columns);
			blkDiag.SetSubMatrix(0, R_POIminus.Rows - 1, 0, R_POIminus.Columns - 1, R_POIminus);
			blkDiag.SetSubMatrix(R_POIminus.Rows, R_POIminus.Rows + R_x2minus.Rows - 1, R_POIminus.Columns, R_POIminus.Columns + R_x2minus.Columns - 1, R_x2minus);
			blkDiag.SetSubMatrix(R_POIminus.Rows + R_x2minus.Rows, blkDiag.Rows - 1, 0, blkDiag.Columns - 1, I_kp1);
			QrDecomposition qrDecomposition_blkDiag = new QrDecomposition(blkDiag);
			Matrix R_blob = qrDecomposition_blkDiag.UpperTriangularFactor;
			Matrix T_kp1 = qrDecomposition_blkDiag.OrthogonalFactor;
			Matrix RPOI = R_blob.Submatrix(0, nPOI - 1, 0, nPOI - 1);
			Matrix RPOIx2 = R_blob.Submatrix(0, nPOI - 1, nPOI, R_blob.Columns - 1);
			Matrix Rx2 = R_blob.Submatrix(nPOI, R_blob.Rows - 1, nPOI, R_blob.Columns - 1);

			// final calculation
			Matrix ninvRPOI_RPOIx2_invRx2 = (RPOI.Inverse * -1) * RPOIx2 * Rx2.Inverse;
			Matrix R_POIm_xhatPOIm_i_kp1 = new Matrix(nPOI + i_kp1.Rows, 1);
			R_POIm_xhatPOIm_i_kp1.SetSubMatrix(0, nPOI - 1, 0, 0, R_POIminus * xhatPOIm);
			R_POIm_xhatPOIm_i_kp1.SetSubMatrix(nPOI, R_POIm_xhatPOIm_i_kp1.Rows - 1, 0, 0, i_kp1);

			Matrix invRPOI_ninvRPOI_RPOIx2_invRx2 = new Matrix(RPOI.Rows, RPOI.Columns + ninvRPOI_RPOIx2_invRx2.Columns);
			invRPOI_ninvRPOI_RPOIx2_invRx2.SetSubMatrix(0, nPOI - 1, 0, nPOI - 1, RPOI.Inverse);
			invRPOI_ninvRPOI_RPOIx2_invRx2.SetSubMatrix(0, nPOI - 1, nPOI, invRPOI_ninvRPOI_RPOIx2_invRx2.Columns - 1, ninvRPOI_RPOIx2_invRx2);
			Matrix xhatPOI = invRPOI_ninvRPOI_RPOIx2_invRx2 * T_kp1.Transpose() * R_POIm_xhatPOIm_i_kp1;

			Matrix invRPOI_ninvRPOI_RPOIx2_invRx2_zeros_invRx2 = new Matrix(invRPOI_ninvRPOI_RPOIx2_invRx2.Rows + nx2 + Rx2.Rows, invRPOI_ninvRPOI_RPOIx2_invRx2.Columns);
			invRPOI_ninvRPOI_RPOIx2_invRx2_zeros_invRx2.SetSubMatrix(0, invRPOI_ninvRPOI_RPOIx2_invRx2.Rows - 1, 0, invRPOI_ninvRPOI_RPOIx2_invRx2.Columns - 1, invRPOI_ninvRPOI_RPOIx2_invRx2);
			invRPOI_ninvRPOI_RPOIx2_invRx2_zeros_invRx2.SetSubMatrix(invRPOI_ninvRPOI_RPOIx2_invRx2.Rows, invRPOI_ninvRPOI_RPOIx2_invRx2.Rows + nx2 - 1, nPOI, invRPOI_ninvRPOI_RPOIx2_invRx2.Columns - 1, Rx2.Inverse);
			QrDecomposition qrDecomposition_invRPOIstuff = new QrDecomposition(invRPOI_ninvRPOI_RPOIx2_invRx2_zeros_invRx2);
			Matrix S_blob = qrDecomposition_invRPOIstuff.UpperTriangularFactor;
			Matrix newSPOI = S_blob.Submatrix(0, nPOI - 1, 0, nPOI - 1);

			targetState = xhatPOI;

			if (Math.Abs(newSPOI[0, 0]) < Math.Sqrt(0.05))
				newSPOI[0, 0] = Math.Sqrt(0.05) * Math.Sign(newSPOI[0, 0]);
			if (Math.Abs(newSPOI[1, 1]) < Math.Sqrt(0.06))
				newSPOI[1, 1] = Math.Sqrt(0.05) * Math.Sign(newSPOI[1, 1]);
			S_target = newSPOI;

		}


		private Matrix LaserMeasurementFct(Matrix x_POI, Matrix x_x2, Matrix laserNoise)
		{
			Matrix Z_SCR = new Matrix(laserNoise.Rows, x_POI.Columns);
			for (int i = 0; i < x_POI.Columns; i++)
			{
				Matrix xNAV = x_x2.Submatrix(0, 2, i, i); // E, N, U
				Matrix xATT = x_x2.Submatrix(3, 5, i, i); // roll, pitch, yaw
				double xDiff = x_POI[0, i] - xNAV[0, 0];
				double yDiff = x_POI[1, i] - xNAV[1, 0];
				double range = Math.Sqrt(xDiff * xDiff + yDiff * yDiff) + laserNoise[0, 0] * laserNoise[0, 0];
				double angle = Math.Atan2(yDiff, xDiff) + laserNoise[1, 0] * laserNoise[1, 0];
				//angle = -(Math.PI - angle - xATT[2, 0]);
				angle = angle - xATT[2, 0];
				Matrix subMatrix = new Matrix(laserNoise.Rows, 1);
				subMatrix[0, 0] = range * Math.Cos(angle + xATT[2, 0]) + xNAV[0, 0];
				subMatrix[1, 0] = range * Math.Sin(angle + xATT[2, 0]) + xNAV[1, 0];
				Z_SCR.SetSubMatrix(0, laserNoise.Rows - 1, i, i, subMatrix);
			}
			return Z_SCR;
		}


		private Matrix MeasurementFct(Matrix x_POI, Matrix x_x2, Matrix x_SCR_noise, Matrix rangeNoise, string cameraType, string size)
		{
			Matrix Z_SCR = new Matrix(x_SCR_noise.Rows, x_POI.Columns);
			MathNet.Numerics.Distributions.NormalDistribution normDist = new MathNet.Numerics.Distributions.NormalDistribution(0, 2.0);
			string newSize = "320x240";
			if (size.Equals("960x240"))
				newSize = "320x240";
			else
				newSize = size;
			for (int i = 0; i < x_POI.Columns; i++)
			{
				Matrix xNAV = x_x2.Submatrix(0, 2, i, i);
				Matrix xATT = x_x2.Submatrix(3, 5, i, i);
				Matrix xGIM = x_x2.Submatrix(6, 8, i, i);
				Z_SCR.SetSubMatrix(0, x_SCR_noise.Rows - 2, i, i, Point2Pixel(x_POI.Submatrix(0, x_POI.Rows - 1, i, i),
																						xNAV, xATT, xGIM, x_SCR_noise.Submatrix(0, x_SCR_noise.Rows - 2, i, i), cameraType, newSize));
				double laser = Math.Sqrt((x_POI[0, i] - xNAV[0, 0]) * (x_POI[0, i] - xNAV[0, 0]) + (x_POI[1, i] - xNAV[1, 0]) * (x_POI[1, i] - xNAV[1, 0]) + (x_POI[2, i] - xNAV[2, 0]) * (x_POI[2, i] - xNAV[2, 0]));// +normDist.NextDouble() * rangeNoise[0, 0];
				Z_SCR[2, i] = laser;
			}
			return Z_SCR;
		}


		/// <summary>
		/// calculates corresponding pixel point based on the state information. The camera matrix is for Unibrain FireI cam with 320 x 240 resolution
		/// </summary>
		/// <param name="xPOI"></param>
		/// <param name="xNAV"></param>
		/// <param name="xATT"></param>
		/// <param name="xGIM"></param>
		/// <param name="xSCR_noise"></param>
		/// <returns></returns>
		public Matrix Point2Pixel(Matrix xPOI, Matrix xNAV, Matrix xATT, Matrix xGIM, Matrix xSCR_noise, string cameraType, string size)
		{
			double[] fc = new double[2];
			double[] cc = new double[2];
			if (size.Equals("320x240"))
			{
				// for 320 x 240 image with Unibrain Fire-i camera
				if (cameraType.Equals("Fire-i"))
				{
					fc[0] = 384.4507; fc[1] = 384.1266;
					cc[0] = 155.1999; cc[1] = 101.5641;
				}
				else if (cameraType.Equals("FireFly"))
				{
					fc[0] = 345.26498; fc[1] = 344.99438;
					cc[0] = 159.36854; cc[1] = 118.26944;
				}
			}
			else if (size.Equals("640x480"))
			{
				// for Unibrain Fire-i
				if (cameraType.Equals("Fire-i"))
				{
					fc[0] = 763.5805; fc[1] = 763.8337;
					cc[0] = 303.0963; cc[1] = 215.9287;
				}
				// for Fire-Fly MV (Point Gray)
				else if (cameraType.Equals("FireFly"))
				{
					fc[0] = 691.09778; fc[1] = 690.70187;
					cc[0] = 324.07388; cc[1] = 234.22204;
				}
			}
			double alpha_c = 0;

			// camera matrix - for Uni-Brain IEEE1394 CCD camera with 320 x 240 resolution
			Matrix KK = new Matrix(fc[0], alpha_c * fc[0], cc[0], 0, fc[1], cc[1], 0, 0, 1);

			// extrinsic parameter (transformation)
			double pan = xGIM[0, 0]; double tilt = xGIM[1, 0]; double scan = xGIM[2, 0];
			double roll = xATT[0, 0]; double pitch = xATT[1, 0]; double yaw = xATT[2, 0];

			Matrix vector = xPOI.Submatrix(0, 2, 0, 0) - xNAV;

			///// MAKE IT WORK IN 3D ///
			yaw = yaw - Math.PI / 2;
			//yaw = Math.PI / 2 - yaw;
			//Matrix R_ENU2R = new Matrix(Math.Cos(yaw), Math.Sin(yaw), 0, -Math.Sin(yaw), Math.Cos(yaw), 0, 0, 0, 1);
			Matrix R_ENU2R = new Matrix(Math.Cos(yaw), Math.Sin(yaw), 0, -Math.Sin(yaw), Math.Cos(yaw), 0, 0, 0, 1) *
											 new Matrix(1, 0, 0, 0, Math.Cos(pitch), -Math.Sin(pitch), 0, Math.Sin(pitch), Math.Cos(pitch)) *
											 new Matrix(Math.Cos(roll), 0, Math.Sin(roll), 0, 1, 0, -Math.Sin(roll), 0, Math.Cos(roll));
			//Matrix R_R2G = new Matrix(Math.Cos(pan), Math.Sin(pan), 0, -Math.Sin(pan), Math.Cos(pan), 0, 0, 0, 1);
			Matrix R_R2G = new Matrix(Math.Cos(pan), Math.Sin(pan), 0, -Math.Sin(pan), Math.Cos(pan), 0, 0, 0, 1) *
										 new Matrix(1, 0, 0, 0, Math.Cos(scan), -Math.Sin(tilt), 0, Math.Sin(tilt), Math.Cos(tilt)) *
										 new Matrix(Math.Cos(scan), -Math.Sin(scan), 0, Math.Sin(scan), Math.Cos(scan), 0, 0, 0, 1);
			vector = R_R2G * R_ENU2R * vector; // now in camera coordinate frame

			Matrix newVector = new Matrix(3, 1);
			newVector[0, 0] = vector[0, 0] / vector[1, 0];
			newVector[1, 0] = -vector[2, 0] / vector[1, 0];
			newVector[2, 0] = vector[1, 0] / vector[1, 0];
			Matrix xSCR = new Matrix(2, 1);
			xSCR = KK * newVector;
			return xSCR.Submatrix(0, 1, 0, 0) + xSCR_noise.Submatrix(0, 1, 0, 0);

		}
		/// <summary>
		/// Predict next sigma points using dynamic model
		/// </summary>
		/// <param name="XPOI">Sigma points of POI: [nPOI x (2 * (nPOI+nw) + 1)]</param>
		/// <param name="Xw">Sigma points of noise: [nw x (2 * (nPOI+nw) + 1)]</param>
		/// <param name="dT">time interval for integration (discrete system)</param>
		/// <param name="type">Indicating what kind of target it is</param>
		/// <returns></returns>
		private Matrix predPOI(Matrix XPOI, Matrix Xw, double dT, TargetTypes type)
		{
			// state: [x y z velocity heading turn-rate accleration]
			Matrix toReturn = new Matrix(XPOI.Rows, XPOI.Columns);
			for (int i = 0; i < toReturn.Columns; i++)
			{
				for (int j = 0; j < toReturn.Rows; j++)
				{
					if (type == TargetTypes.PotentialSOOI || type == TargetTypes.ConfirmedSOOI || type == TargetTypes.Junk || type == TargetTypes.Meta)
					{
						if (j == 5)
							toReturn[j, i] = Xw[1, i];
						else if (j == 6)
							toReturn[j, i] = Xw[2, i];
						else
							toReturn[j, i] = XPOI[j, i];
					}
					else if (type == TargetTypes.PotentialMOOI || type == TargetTypes.ConfirmedMOOI)
					{
						if (j == 5)
							toReturn[j, i] = Xw[1, i];
						else if (j == 6)
							toReturn[j, i] = Xw[2, i];
						else
							toReturn[j, i] = XPOI[j, i] + normDist.NextDouble() * dT; // random walk????
					}
				}
			}
			return toReturn;
		}

		internal void UpdateWithoutMeasurement()
		{
			S_target += new Matrix(S_target.Rows, S_target.Columns, Math.Sqrt(Math.Abs(normDist.NextDouble() / 1000)));
		}

		/// <summary>
		/// This function is used to predict next state of existing targets with current pose information - for data association
		/// </summary>
		/// <param name="xPOI"></param>
		/// <param name="xx2"></param>
		/// <param name="SPOI"></param>
		/// <param name="Sw"></param>
		/// <param name="SMeasurement"></param>
		/// <param name="Sx2"></param>
		/// <param name="zSCR"></param>
		/// <param name="sigma_f"></param>
		/// <param name="screenSize"></param>
		/// <param name="cameraType"></param>
		/// <param name="type"></param>
		public void PredictLidarMeasurementWithCurrentPose(Matrix xPOI, Matrix xx2, Matrix SPOI, Matrix Sw, Matrix SMeasurement, Matrix Sx2, TargetTypes type, double sigma_f, out Matrix zMean, out Matrix zCov)
		{
			//////////////////////////////// INITIALIZATION /////////////////////////////////////
			// SORRY - almost impossible to debug.
			// variable names are mostly following IEEE Transaction paper written by Dr. Mark Campbell.
			// also the variable names strictly follow the names I used in MATLAB code.
			int nPOI = xPOI.Rows;
			int nw = Sw.Rows;
			int nLaser = 2;
			// state matrix
			Matrix xHat_a0 = new Matrix(nPOI + nw, 1);
			for (int i = 0; i < nPOI; i++)
				xHat_a0[i, 0] = xPOI[i, 0];

			for (int i = 0; i < nw; i++)
				xHat_a0[i + nPOI, 0] = 0;
			// covariance matrix
			Matrix S_a0 = new Matrix(nPOI + nw, nPOI + nw);
			for (int i = 0; i < nPOI; i++)
				S_a0[i, i] = SPOI[i, i];
			for (int i = nPOI; i < nPOI + nw; i++)
				S_a0[i, i] = Sw[i - nPOI, i - nPOI];

			// generate 2*(nPOI + nw) + 1 sigma points
			Matrix Xa0 = new Matrix(nPOI + nw, 2 * (nPOI + nw) + 1);
			Matrix halfSigmaPoints1 = new Matrix(nPOI + nw, nPOI + nw);
			Matrix halfSigmaPoints2 = new Matrix(nPOI + nw, nPOI + nw);
			Matrix ones = new Matrix(1, nPOI + nw);
			ones.Ones();
			halfSigmaPoints1 = xHat_a0 * ones + S_a0 * sigma_f;
			halfSigmaPoints2 = xHat_a0 * ones - S_a0 * sigma_f;
			Xa0.SetSubMatrix(0, nPOI + nw - 1, 0, 0, xHat_a0);
			Xa0.SetSubMatrix(0, nPOI + nw - 1, 1, (1 + nPOI + nw) - 1, halfSigmaPoints1);
			Xa0.SetSubMatrix(0, nPOI + nw - 1, nPOI + nw + 1, 2 * (nPOI + nw), halfSigmaPoints2);

			// dividing into two parts
			Matrix XPOI_0 = Xa0.Submatrix(0, nPOI - 1, 0, Xa0.Columns - 1);
			Matrix Xw_0 = Xa0.Submatrix(nPOI, Xa0.Rows - 1, 0, Xa0.Columns - 1);

			// weight factors
			double Wm0 = (sigma_f * sigma_f - (nPOI + nw)) / (sigma_f * sigma_f);
			double Wc0 = Wm0 + 3 - sigma_f * sigma_f / (nPOI + nw);
			double W = 1 / (2 * sigma_f * sigma_f);
			//////////////////////////// SR-SPIF PREDICTION ////////////////////////////////
			Matrix predPOIs = predPOI(XPOI_0, Xw_0, dT, type); // predicted sigma points
			Matrix xhatPOIm = predPOIs.Submatrix(0, nPOI - 1, 0, 0) * Wm0;
			for (int i = 1; i < predPOIs.Columns; i++)
			{
				xhatPOIm += predPOIs.Submatrix(0, nPOI - 1, i, i) * W;
			}
			// re-arrange sigma points
			Matrix XPOI_Cm = new Matrix(nPOI, 2 * (nPOI + nw) + 1);
			for (int i = 0; i < XPOI_Cm.Columns; i++)
			{
				for (int j = 0; j < XPOI_Cm.Rows; j++)
				{
					XPOI_Cm[j, i] = predPOIs[j, i] - xhatPOIm[j, 0];
				}
			}

			/////////////////////// CONVERSION FROM PREDICTION TO UPDATE ///////////////////////
			// define x_x2
			Matrix x_nav = xx2.Submatrix(0, 2, 0, 0);
			Matrix x_att = xx2.Submatrix(3, 5, 0, 0);
			Matrix x_gim = xx2.Submatrix(6, 8, 0, 0);
			Matrix S_nav = Sx2.Submatrix(0, 2, 0, 2);
			Matrix S_att = Sx2.Submatrix(3, 5, 3, 5);
			Matrix S_gim = Sx2.Submatrix(6, 8, 6, 8);

			int nSCR = 0; int nx2 = xx2.Rows; int n2a = nPOI + nw + nSCR + nx2 + nLaser;
			Matrix xhat_x2_m = new Matrix(nx2, 1); // creating x_x2 vector
			for (int i = 0; i < 3; i++)
			{
				xhat_x2_m[i, 0] = x_nav[i, 0];
				xhat_x2_m[i + 3, 0] = x_att[i, 0];
				xhat_x2_m[i + 6, 0] = x_gim[i, 0];
			}
			Matrix S_x2_m = new Matrix(nx2, nx2); // creating augmented covariance diagonal matrix for x2
			for (int i = 0; i < 3; i++)
			{
				S_x2_m[i, i] = S_nav[i, i];
				S_x2_m[i + 3, i + 3] = S_att[i, i];
				S_x2_m[i + 6, i + 6] = S_gim[i, i];
			}

			// equation 20
			Matrix X_aug_c0m = new Matrix(nPOI + nSCR + nLaser + nx2, 1);
			X_aug_c0m.Zero();
			for (int i = 0; i < nPOI; i++)
				X_aug_c0m[i, 0] = XPOI_Cm[i, 0];
			Matrix X_aug_cm = new Matrix(nPOI + nSCR + nLaser + nx2, 2 * n2a); X_aug_cm.Zero();
			X_aug_cm.SetSubMatrix(0, nPOI - 1, 0, XPOI_Cm.Columns - 2, XPOI_Cm.Submatrix(0, nPOI - 1, 1, XPOI_Cm.Columns - 1));
			ones = new Matrix(1, 2 * nSCR + nLaser); ones.Ones();
			X_aug_cm.SetSubMatrix(0, nPOI - 1, XPOI_Cm.Columns - 1, XPOI_Cm.Columns - 1 + (2 * nSCR + nLaser) - 1, XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * ones);
			ones = new Matrix(1, 2 * nx2); ones.Ones();
			X_aug_cm.SetSubMatrix(0, nPOI - 1, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser), XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser + nx2) - 1, XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * ones);
			X_aug_cm.SetSubMatrix(nPOI, nPOI + nSCR + nLaser - 1, XPOI_Cm.Columns - 1, XPOI_Cm.Columns - 1 + nSCR + nLaser - 1, SMeasurement);
			X_aug_cm.SetSubMatrix(nPOI, nPOI + nSCR + nLaser - 1, XPOI_Cm.Columns - 1 + nSCR + nLaser, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser) - 1, SMeasurement * -1);
			X_aug_cm.SetSubMatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser), XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser) + nx2 - 1, Sx2);
			X_aug_cm.SetSubMatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser) + nx2, XPOI_Cm.Columns - 1 + 2 * (nSCR + nLaser + nx2) - 1, Sx2 * -1);


			// equation 21
			Matrix X_aug_m = new Matrix(X_aug_c0m.Rows, X_aug_c0m.Columns + X_aug_cm.Columns);
			Matrix meanVector = new Matrix(nPOI + nSCR + nLaser + nx2, 1);
			meanVector.Zero();
			meanVector.SetSubMatrix(0, nPOI - 1, 0, 0, xhatPOIm);
			meanVector.SetSubMatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, 0, 0, xx2);
			ones = new Matrix(1, 2 * n2a); ones.Ones();
			// create augmented sigma points
			X_aug_m.SetSubMatrix(0, X_aug_m.Rows - 1, 0, 0, X_aug_c0m + meanVector);
			X_aug_m.SetSubMatrix(0, X_aug_m.Rows - 1, 1, (2 * n2a) + 1 - 1, X_aug_cm + meanVector * ones);

			// weight configurationU
			double Wm0_aug = (sigma_f * sigma_f - n2a) / (sigma_f * sigma_f);
			double Wc0_aug = Wm0_aug + 3 - (sigma_f * sigma_f / n2a);

			////////////////////////////////////// SR-SPIF UPDATE /////////////////////////////////////////////
			Matrix rangeNoise = new Matrix(1, 1);
			/*
			string cameraType = "FireFly", screenSize = "320x240";
			Matrix Z_SCR = MeasurementFct(X_aug_m.Submatrix(0, nPOI - 1, 0, X_aug_m.Columns - 1),
																		X_aug_m.Submatrix(nPOI + nSCR + nLaser, X_aug_m.Rows - 1, 0, X_aug_m.Columns - 1),
																		X_aug_m.Submatrix(nPOI, nPOI + nSCR + nLaser - 1, 0, X_aug_m.Columns - 1), SMeasurement, cameraType, screenSize);
			 * */
			Matrix Z_SCR = LaserMeasurementFct(X_aug_m.Submatrix(0, nPOI - 1, 0, X_aug_m.Columns - 1), X_aug_m.Submatrix(nPOI + nSCR + nLaser, X_aug_m.Rows - 1, 0, X_aug_m.Columns - 1), SMeasurement);
			Matrix z_mean = new Matrix(nSCR + nLaser, 1); // measurement mean
			for (int i = 0; i < Z_SCR.Columns; i++)
			{
				if (i == 0)
					z_mean += Z_SCR.Submatrix(0, Z_SCR.Rows - 1, 0, 0) * Wm0_aug;
				else
					z_mean += Z_SCR.Submatrix(0, Z_SCR.Rows - 1, i, i) * W;
			}
			ones = new Matrix(1, Z_SCR.Columns); ones.Ones();
			Matrix Z_SCR_c = Z_SCR - z_mean * ones;

			// equation 27
			//Matrix P_POIx2SCR = new Matrix(nPOI + nx2, nSCR);
			Matrix augPOIx2cm = new Matrix(nPOI + nx2, X_aug_cm.Columns);
			augPOIx2cm.SetSubMatrix(0, nPOI - 1, 0, X_aug_cm.Columns - 1, X_aug_cm.Submatrix(0, nPOI - 1, 0, X_aug_cm.Columns - 1));
			augPOIx2cm.SetSubMatrix(nPOI, nPOI + nx2 - 1, 0, X_aug_cm.Columns - 1, X_aug_cm.Submatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, 0, X_aug_cm.Columns - 1));
			Matrix augPOIx2c0m = new Matrix(nPOI + nx2, 1);
			augPOIx2c0m.SetSubMatrix(0, nPOI - 1, 0, 0, X_aug_c0m.Submatrix(0, nPOI - 1, 0, 0));
			augPOIx2c0m.SetSubMatrix(nPOI, nPOI + nx2 - 1, 0, 0, X_aug_c0m.Submatrix(nPOI + nSCR + nLaser, nPOI + nSCR + nLaser + nx2 - 1, 0, 0));
			Matrix P_POIx2SCR = augPOIx2cm * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 1, Z_SCR_c.Columns - 1).Transpose() * W
														+ augPOIx2c0m * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0).Transpose() * Wc0_aug;

			//// Equation 28 and below chol part is equation 15
			QrDecomposition qrDecomposition = new QrDecomposition(XPOI_Cm.Submatrix(0, XPOI_Cm.Rows - 1, 1, XPOI_Cm.Columns - 1).Transpose());
			Matrix R_X_POI_Cm = qrDecomposition.UpperTriangularFactor;
			CholeskyDecomposition cholUpdate;
			Matrix S_POI_m;
			if (Wc0_aug < 0)
			{
				cholUpdate = new CholeskyDecomposition(R_X_POI_Cm.Transpose() * R_X_POI_Cm * Math.Sqrt(W) * Math.Sqrt(W) - XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0).Transpose() * Math.Abs(Wc0));
				S_POI_m = cholUpdate.LeftTriangularFactor.Transpose();
			}
			else if (Wc0_aug > 0)
			{
				cholUpdate = new CholeskyDecomposition(R_X_POI_Cm.Transpose() * R_X_POI_Cm * Math.Sqrt(W) * Math.Sqrt(W) + XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0) * XPOI_Cm.Submatrix(0, nPOI - 1, 0, 0).Transpose() * Math.Abs(Wc0));
				S_POI_m = cholUpdate.LeftTriangularFactor.Transpose();
			}
			else
				throw new Exception("Weight is zero");

			//Matrix S_POI_mVer2 = new Matrix(nPOI, nPOI);
			for (int i = 0; i < nPOI; i++)
			{
				//S_POI_mVer2[i, i] = Math.Abs(S_POI_m[i, i]);
				S_POI_m[i, i] = Math.Abs(S_POI_m[i, i]);
			}
			QrDecomposition qrSPOImVer2 = new QrDecomposition(S_POI_m.Transpose().Inverse);
			Matrix R_POIminus = qrSPOImVer2.UpperTriangularFactor;
			Matrix Y_POIminus = R_POIminus.Transpose() * R_POIminus;

			//QrDecomposition qrDecomposition_S_x2_m = new QrDecomposition(S_x2_m.Inverse.Transpose());
			//Matrix R_x2m = qrDecomposition_S_x2_m.UpperTriangularFactor;
			//QrDecomposition qrDecomposition_S_POI_m = new QrDecomposition(S_POI_m.Inverse.Transpose());
			//Matrix R_POIm = qrDecomposition_S_POI_m.UpperTriangularFactor;
			//Matrix P_POIm1 = R_POIm.Transpose() * R_POIm;
			//Matrix P_x2m1 = R_x2m.Transpose() * R_x2m;
			//Matrix blkDiag = new Matrix(P_POIm1.Rows + P_x2m1.Rows, P_POIm1.Columns + P_x2m1.Columns);
			//blkDiag.SetSubMatrix(0, P_POIm1.Rows - 1, 0, P_POIm1.Columns - 1, P_POIm1);
			//blkDiag.SetSubMatrix(P_POIm1.Rows, P_POIm1.Rows + P_x2m1.Rows - 1, P_POIm1.Columns, P_POIm1.Columns + P_x2m1.Columns - 1, P_x2m1);
			//Matrix C_SCR = P_POIx2SCR.Transpose() * blkDiag;

			// Equation 28
			QrDecomposition qrInvSx2 = new QrDecomposition(Sx2.Inverse);
			Matrix R_x2minus = qrInvSx2.UpperTriangularFactor;
			Matrix Y_x2minus = R_x2minus.Transpose() * R_x2minus;
			Matrix blkDiagYseries = new Matrix(nPOI + nx2, nPOI + nx2);
			blkDiagYseries.SetSubMatrix(0, nPOI - 1, 0, nPOI - 1, Y_POIminus);
			blkDiagYseries.SetSubMatrix(nPOI, nPOI + nx2 - 1, nPOI, nPOI + nx2 - 1, Y_x2minus);
			Matrix C = P_POIx2SCR.Transpose() * blkDiagYseries;

			// SUPER IMPORTANT STEP !!
			QrDecomposition anotherQrDecomposition = new QrDecomposition(Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 1, Z_SCR_c.Columns - 1).Transpose());
			Matrix Rvtilde = anotherQrDecomposition.UpperTriangularFactor;
			CholeskyDecomposition anotherCholUpdate;
			Matrix Svtilde;
			if (Wc0_aug < 0)
			{
				anotherCholUpdate = new CholeskyDecomposition(Rvtilde.Transpose() * Rvtilde * Math.Sqrt(W) * Math.Sqrt(W) - Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0) * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0).Transpose() * Math.Abs(Wc0_aug));
				Svtilde = anotherCholUpdate.LeftTriangularFactor.Transpose();
			}
			else if (Wc0_aug > 0)
			{
				anotherCholUpdate = new CholeskyDecomposition(Rvtilde.Transpose() * Rvtilde * Math.Sqrt(W) * Math.Sqrt(W) + Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0) * Z_SCR_c.Submatrix(0, Z_SCR_c.Rows - 1, 0, 0).Transpose() * Math.Abs(Wc0_aug));
				Svtilde = anotherCholUpdate.LeftTriangularFactor.Transpose();
			}
			else
				throw new Exception("Weight is zero");
			//Matrix SvtildeVer2 = new Matrix(Svtilde.Rows, Svtilde.Columns); SvtildeVer2.Zero();
			for (int i = 0; i < Svtilde.Columns; i++)
				Svtilde[i, i] = Math.Abs(Svtilde[i, i]);

			Matrix P_SCR = Svtilde.Transpose() * Svtilde;
			zCov = P_SCR;
			zMean = z_mean;
		}
	}
}

