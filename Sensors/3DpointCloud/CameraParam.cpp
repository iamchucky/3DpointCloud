#include "CameraParam.h"
#include <iostream>
using std::cerr;
using std::cout;
using std::endl;
#include <fstream>
using std::ifstream;
#include <cstdlib> // for exit function
#define PI 3.141592653589793

CameraParam::CameraParam(char* calibFilename)
{
	ifstream calib;

	calib.open(calibFilename);
	if (!calib)		// file couldn't be opened
	{
		cerr << "[CameraParam]: Cannot load calibration file!" << endl;
		exit(1);
	}

	calib >> in.fc0 >> in.fc1 >> in.cc0 >> in.cc1 >> in.alpha_c;
	calib.close();
	
	K = (cv::Mat_<double>(3,3) <<	-in.fc0, in.alpha_c*in.fc0,	in.cc0,
									0.0,	in.fc1,				in.cc1,
									0.0,	0.0,				1.0		);

	/*T = cv::Mat(3,1,CV_64F);
	T_x = cv::Mat(3,3,CV_64F);*/
	Rwr = cv::Mat(3,3,CV_64F);
	Rrc = cv::Mat(3,3,CV_64F);
	RT = cv::Mat::zeros(4,4,CV_64F);

	cout << K.at<double>(0,0) << "\t" << K.at<double>(0,1) << "\t" << K.at<double>(0,2) << endl;
	cout << K.at<double>(1,0) << "\t" << K.at<double>(1,1) << "\t" << K.at<double>(1,2) << endl;
	cout << K.at<double>(2,0) << "\t" << K.at<double>(2,1) << "\t" << K.at<double>(2,2) << endl;
	
}


CameraParam::~CameraParam(void)
{
}


double CameraParam::Get_fc0(void)
{
	return in.fc0;
}
double CameraParam::Get_fc1(void)
{
	return in.fc1;
}
double CameraParam::Get_cc0(void)
{
	return in.cc0;
}
double CameraParam::Get_cc1(void)
{
	return in.cc1;
}
double CameraParam::Get_alpha_c(void)
{
	return in.alpha_c;
}

void CameraParam::Update_R_T(CameraPose::Pose & pose)
{
	double camera_z = 0.4;
	cv::Mat Trc = (cv::Mat_<double>(3,1) << 0.0, 0.0, camera_z);
	Twr = (cv::Mat_<double>(3,1) << pose.x, pose.y, pose.z);
	T_x = (cv::Mat_<double>(3,3) << 0.f, -pose.z, pose.y,
									pose.z, 0.f, -pose.x,
									-pose.y, pose.x, 0.f	);

	FindR(pose.yaw, pose.pitch, pose.roll).copyTo(Rwr);
	FindR(-PI/2, 0.0, PI/2).copyTo(Rrc);

	cv::Mat RwrT = cv::Mat(3,3,CV_64F);
	cv::Mat RrcT = cv::Mat(3,3,CV_64F);
	cv::transpose(Rwr,RwrT);
	cv::transpose(Rrc,RrcT);
	cv::Mat R = RrcT*RwrT;
	cv::Mat T = RrcT*(-RwrT*Twr-Trc);
	R.copyTo(RT(cv::Rect(0,0,3,3)));
	T.copyTo(RT(cv::Rect(3,0,1,3)));
	RT.at<double> (3,3) = 1.0;
	cv::Mat I = (cv::Mat_<double>(3,4) << 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);
				
	// K*[ eye(3) zeros(3,1) ]*RT;
	P = K*I*RT;

//#ifdef DEBUG_LEVEL1
//	cout.fixed;
//	cout.precision(9);
//	cout << endl << "Translation Matrix:" << endl;
//	cout << Twr.at<double>(0,0) << endl;
//	cout << Twr.at<double>(1,0) << endl;
//	cout << Twr.at<double>(2,0) << endl;
//
//	cout << endl << "Rotation Matrix:" << endl;
//	cout << Rwr.at<double>(0,0) << "\t" << Rwr.at<double>(0,1) << "\t" << Rwr.at<double>(0,2) << endl;
//	cout << Rwr.at<double>(1,0) << "\t" << Rwr.at<double>(1,1) << "\t" << Rwr.at<double>(1,2) << endl;
//	cout << Rwr.at<double>(2,0) << "\t" << Rwr.at<double>(2,1) << "\t" << Rwr.at<double>(2,2) << endl;
//
//	cout << endl << "RT Matrix:" << endl;
//	cout << RT.at<double>(0,0) << "\t" << RT.at<double>(0,1) << "\t" << RT.at<double>(0,2) << "\t" << RT.at<double>(0,3) << endl;
//	cout << RT.at<double>(1,0) << "\t" << RT.at<double>(1,1) << "\t" << RT.at<double>(1,2) << "\t" << RT.at<double>(1,3) << endl;
//	cout << RT.at<double>(2,0) << "\t" << RT.at<double>(2,1) << "\t" << RT.at<double>(2,2) << "\t" << RT.at<double>(2,3) << endl;
//	cout << RT.at<double>(3,0) << "\t\t" << RT.at<double>(3,1) << "\t\t" << RT.at<double>(3,2) << "\t\t" << RT.at<double>(3,3) << endl;
//
//	cout << endl << "P Matrix:" << endl;
//	cout << P.at<double>(0,0) << "\t" << P.at<double>(0,1) << "\t" << P.at<double>(0,2) << "\t" << P.at<double>(0,3) << endl;
//	cout << P.at<double>(1,0) << "\t" << P.at<double>(1,1) << "\t" << P.at<double>(1,2) << "\t" << P.at<double>(1,3) << endl;
//	cout << P.at<double>(2,0) << "\t" << P.at<double>(2,1) << "\t" << P.at<double>(2,2) << "\t" << P.at<double>(2,3) << endl;
//#endif
}

cv::Mat CameraParam::FindR(double yaw, double pitch, double roll)
{
	cv::Mat rotationM = (cv::Mat_<double>(3,3) <<	
		cos(yaw)*cos(pitch), cos(yaw)*sin(pitch)*sin(roll)-sin(yaw)*cos(roll), cos(yaw)*sin(pitch)*cos(roll)+sin(yaw)*sin(roll),
		sin(yaw)*cos(pitch), sin(yaw)*sin(pitch)*sin(roll)+cos(yaw)*cos(roll), sin(yaw)*sin(pitch)*cos(roll)-cos(yaw)*sin(roll),
		-sin(pitch),		 cos(pitch)*sin(roll),							   cos(pitch)*cos(roll)					);

	return rotationM;
}

cv::Mat CameraParam::GetFfromP(cv::Mat & P1, cv::Mat & P2)
{
	cv::Mat F = cv::Mat(3,3,CV_64F);
	cv::Mat X1 = cv::Mat(2,4,CV_64F);
	cv::Mat X2 = cv::Mat(2,4,CV_64F);
	cv::Mat X3 = cv::Mat(2,4,CV_64F);
	cv::Mat Y1 = cv::Mat(2,4,CV_64F);
	cv::Mat Y2 = cv::Mat(2,4,CV_64F);
	cv::Mat Y3 = cv::Mat(2,4,CV_64F);

	P1(cv::Rect(0,1,4,2)).copyTo(X1);		// X1 = P1([2 3],:);
	P1(cv::Rect(0,2,4,1)).copyTo(X2(cv::Rect(0,0,4,1)));	// X2 = P1([3 1],:);
	P1(cv::Rect(0,0,4,1)).copyTo(X2(cv::Rect(0,1,4,1)));
	P1(cv::Rect(0,0,4,2)).copyTo(X3);		// X3 = P1([1 2],:);

	P2(cv::Rect(0,1,4,2)).copyTo(Y1);		// Y1 = P2([2 3],:);
	P2(cv::Rect(0,2,4,1)).copyTo(Y2(cv::Rect(0,0,4,1)));	// Y2 = P2([3 1],:);
	P2(cv::Rect(0,0,4,1)).copyTo(Y2(cv::Rect(0,1,4,1)));
	P2(cv::Rect(0,0,4,2)).copyTo(Y3);		// Y3 = P2([1 2],:);

	cv::Mat fDet = cv::Mat(4,4,CV_64F);
	X1.copyTo(fDet(cv::Rect(0,0,4,2)));
	Y1.copyTo(fDet(cv::Rect(0,2,4,2)));
	F.at<double> (0,0) = cv::determinant(fDet);	// det([X1; Y1])

	X2.copyTo(fDet(cv::Rect(0,0,4,2)));
	Y1.copyTo(fDet(cv::Rect(0,2,4,2)));
	F.at<double> (0,1) = cv::determinant(fDet);	// det([X2; Y1])

	X3.copyTo(fDet(cv::Rect(0,0,4,2)));
	Y1.copyTo(fDet(cv::Rect(0,2,4,2)));
	F.at<double> (0,2) = cv::determinant(fDet);	// det([X3; Y1])

	X1.copyTo(fDet(cv::Rect(0,0,4,2)));
	Y2.copyTo(fDet(cv::Rect(0,2,4,2)));
	F.at<double> (1,0) = cv::determinant(fDet);	// det([X1; Y2])

	X2.copyTo(fDet(cv::Rect(0,0,4,2)));
	Y2.copyTo(fDet(cv::Rect(0,2,4,2)));
	F.at<double> (1,1) = cv::determinant(fDet);	// det([X2; Y2])

	X3.copyTo(fDet(cv::Rect(0,0,4,2)));
	Y2.copyTo(fDet(cv::Rect(0,2,4,2)));
	F.at<double> (1,2) = cv::determinant(fDet);	// det([X3; Y2])

	X1.copyTo(fDet(cv::Rect(0,0,4,2)));
	Y3.copyTo(fDet(cv::Rect(0,2,4,2)));
	F.at<double> (2,0) = cv::determinant(fDet);	// det([X1; Y3])

	X2.copyTo(fDet(cv::Rect(0,0,4,2)));
	Y3.copyTo(fDet(cv::Rect(0,2,4,2)));
	F.at<double> (2,1) = cv::determinant(fDet);	// det([X2; Y3])

	X3.copyTo(fDet(cv::Rect(0,0,4,2)));
	Y3.copyTo(fDet(cv::Rect(0,2,4,2)));
	F.at<double> (2,2) = cv::determinant(fDet);	// det([X3; Y3])

	/*cout << endl << "F Matrix:" << endl;
	cout << F.at<double>(0,0) << "\t" << F.at<double>(0,1) << "\t" << F.at<double>(0,2) << endl;
	cout << F.at<double>(1,0) << "\t" << F.at<double>(1,1) << "\t" << F.at<double>(1,2) << endl;
	cout << F.at<double>(2,0) << "\t" << F.at<double>(2,1) << "\t" << F.at<double>(2,2) << endl;*/

	return F;
}

cv::Mat CameraParam::FindProjection(void)
{
	return P;
}