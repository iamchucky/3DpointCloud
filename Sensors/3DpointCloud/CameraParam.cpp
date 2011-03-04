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
	
	float kdata[] = {	in.fc0, in.alpha_c*in.fc0,	in.cc0,
						0.0,	in.fc1,				in.cc1,
						0.0,	0.0,				1.0		};
	K = cv::Mat( 3, 3, CV_32F, kdata );

	cout << K.at<float>(0,0) << "\t" << K.at<float>(0,1) << "\t" << K.at<float>(0,2) << endl;
	cout << K.at<float>(1,0) << "\t" << K.at<float>(1,1) << "\t" << K.at<float>(1,2) << endl;
	cout << K.at<float>(2,0) << "\t" << K.at<float>(2,1) << "\t" << K.at<float>(2,2) << endl << endl;
	
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
	float tdata[] = {	pose.x, pose.y, pose.z	};
	T = cv::Mat( 3, 1, CV_32F, tdata );

	float a = pose.yaw;
	float b = pose.pitch;
	float c = pose.roll;
	float rdata[] = {	
		cos(a)*cos(b), cos(a)*sin(b)*sin(c)-sin(a)*cos(c), cos(a)*sin(b)*cos(c)+sin(a)*sin(c),
		sin(a)*cos(b), sin(a)*sin(b)*sin(c)+cos(a)*cos(c), sin(a)*sin(b)*cos(c)-cos(a)*sin(c),
		-sin(b),	   cos(b)*sin(c),					   cos(b)*cos(c)					};

	R = cv::Mat( 3, 3, CV_32F, rdata );

#ifdef DEBUG_LEVEL1
	cout << endl << "Translation Matrix:" << endl;
	cout << T.at<float>(0,0) << endl;
	cout << T.at<float>(1,0) << endl;
	cout << T.at<float>(2,0) << endl;

	cout << endl << "Rotation Matrix:" << endl;
	cout << R.at<float>(0,0) << "\t" << R.at<float>(0,1) << "\t" << R.at<float>(0,2) << endl;
	cout << R.at<float>(1,0) << "\t" << R.at<float>(1,1) << "\t" << R.at<float>(1,2) << endl;
	cout << R.at<float>(2,0) << "\t" << R.at<float>(2,1) << "\t" << R.at<float>(2,2) << endl;
#endif
}