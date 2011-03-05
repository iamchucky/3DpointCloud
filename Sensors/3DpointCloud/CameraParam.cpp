#include "CameraParam.h"
#include <iostream>
using std::cerr;
using std::cout;
using std::endl;
#include <fstream>
using std::ifstream;
#include <cstdlib> // for exit function

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
	
	K = (cv::Mat_<double>(3,3) <<	in.fc0, in.alpha_c*in.fc0,	in.cc0,
									0.0,	in.fc1,				in.cc1,
									0.0,	0.0,				1.0		);

	cout << K.at<double>(0,0) << "\t" << K.at<double>(0,1) << "\t" << K.at<double>(0,2) << endl;
	cout << K.at<double>(1,0) << "\t" << K.at<double>(1,1) << "\t" << K.at<double>(1,2) << endl;
	cout << K.at<double>(2,0) << "\t" << K.at<double>(2,1) << "\t" << K.at<double>(2,2) << endl << endl;
	
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
	T = (cv::Mat_<double>(3,1) << pose.x, pose.y, pose.z);
	T_x = (cv::Mat_<double>(3,3) << 0.f, -pose.z, pose.y,
									pose.z, 0.f, -pose.x,
									-pose.y, pose.x, 0.f	);
	double a = pose.yaw;
	double b = pose.pitch;
	double c = pose.roll;
	
	R = (cv::Mat_<double>(3,3) <<	cos(a)*cos(b), cos(a)*sin(b)*sin(c)-sin(a)*cos(c), cos(a)*sin(b)*cos(c)+sin(a)*sin(c),
									sin(a)*cos(b), sin(a)*sin(b)*sin(c)+cos(a)*cos(c), sin(a)*sin(b)*cos(c)-cos(a)*sin(c),
									-sin(b),	   cos(b)*sin(c),					   cos(b)*cos(c)					);

	RT = (cv::Mat_<double>(4,4) <<	cos(a)*cos(b), cos(a)*sin(b)*sin(c)-sin(a)*cos(c), cos(a)*sin(b)*cos(c)+sin(a)*sin(c), pose.x,
									sin(a)*cos(b), sin(a)*sin(b)*sin(c)+cos(a)*cos(c), sin(a)*sin(b)*cos(c)-cos(a)*sin(c), pose.y,
									-sin(b),	   cos(b)*sin(c),					   cos(b)*cos(c),					   pose.z,
									0.0,		   0.0,								   0.0,								   1.0	);

	cv::Mat I = (cv::Mat_<double>(3,4) << 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0);
		
	// K*[ eye(3) zeros(3,1) ]*RT;
	P = K*I*RT;

#ifdef DEBUG_LEVEL1
	cout.fixed;
	cout.precision(9);
	cout << endl << "Translation Matrix:" << endl;
	cout << T.at<double>(0,0) << endl;
	cout << T.at<double>(1,0) << endl;
	cout << T.at<double>(2,0) << endl;

	cout << endl << "Rotation Matrix:" << endl;
	cout << R.at<double>(0,0) << "\t" << R.at<double>(0,1) << "\t" << R.at<double>(0,2) << endl;
	cout << R.at<double>(1,0) << "\t" << R.at<double>(1,1) << "\t" << R.at<double>(1,2) << endl;
	cout << R.at<double>(2,0) << "\t" << R.at<double>(2,1) << "\t" << R.at<double>(2,2) << endl;

	cout << endl << "RT Matrix:" << endl;
	cout << RT.at<double>(0,0) << "\t" << RT.at<double>(0,1) << "\t" << RT.at<double>(0,2) << "\t" << RT.at<double>(0,3) << endl;
	cout << RT.at<double>(1,0) << "\t" << RT.at<double>(1,1) << "\t" << RT.at<double>(1,2) << "\t" << RT.at<double>(1,3) << endl;
	cout << RT.at<double>(2,0) << "\t" << RT.at<double>(2,1) << "\t" << RT.at<double>(2,2) << "\t" << RT.at<double>(2,3) << endl;
	cout << RT.at<double>(3,0) << "\t\t" << RT.at<double>(3,1) << "\t\t" << RT.at<double>(3,2) << "\t\t" << RT.at<double>(3,3) << endl;

	cout << endl << "P Matrix:" << endl;
	cout << P.at<double>(0,0) << "\t" << P.at<double>(0,1) << "\t" << P.at<double>(0,2) << "\t" << P.at<double>(0,3) << endl;
	cout << P.at<double>(1,0) << "\t" << P.at<double>(1,1) << "\t" << P.at<double>(1,2) << "\t" << P.at<double>(1,3) << endl;
	cout << P.at<double>(2,0) << "\t" << P.at<double>(2,1) << "\t" << P.at<double>(2,2) << "\t" << P.at<double>(2,3) << endl;
#endif
}

cv::Mat CameraParam::FindProjection(void)
{
	return P;
}