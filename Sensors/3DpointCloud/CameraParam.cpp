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
	double camera_z = 0.4;
	cv::Mat Trc = (cv::Mat_<double>(3,1) << 0.0, 0.0, camera_z);
	Twr = (cv::Mat_<double>(3,1) << pose.x, pose.y, pose.z);
	T_x = (cv::Mat_<double>(3,3) << 0.f, -pose.z, pose.y,
									pose.z, 0.f, -pose.x,
									-pose.y, pose.x, 0.f	);

	FindR(pose.yaw, pose.pitch, pose.roll).copyTo(Rwr);
	FindR(-PI/2, 0.0, PI/2).copyTo(Rrc);

	/*RT = (cv::Mat_<double>(4,4) <<	cos(a)*cos(b), cos(a)*sin(b)*sin(c)-sin(a)*cos(c), cos(a)*sin(b)*cos(c)+sin(a)*sin(c), pose.x,
									sin(a)*cos(b), sin(a)*sin(b)*sin(c)+cos(a)*cos(c), sin(a)*sin(b)*cos(c)-cos(a)*sin(c), pose.y,
									-sin(b),	   cos(b)*sin(c),					   cos(b)*cos(c),					   pose.z,
									0.0,		   0.0,								   0.0,								   1.0	);*/
	/*cv::Mat RTwr = cv::Mat::zeros(4,4,CV_64F);
	Rwr.copyTo(RTwr(cv::Rect(0,0,3,3)));
	Twr.copyTo(RTwr(cv::Rect(3,0,1,3)));
	RTwr.at<double> (3,3) = 1.0;

	cv::Mat RTrc = cv::Mat::zeros(4,4,CV_64F);
	Rrc.copyTo(RTrc(cv::Rect(0,0,3,3)));
	Trc.copyTo(RTrc(cv::Rect(3,0,1,3)));
	RTrc.at<double> (3,3) = 1.0;*/

	/*cv::Mat eyeTwr = (cv::Mat_<double>(3,4) << 1, 0, 0, pose.x, 0, 1, 0, pose.y, 0, 0, 1, pose.z);
	cv::Mat eyeTrc = (cv::Mat_<double>(3,4) << 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, camera_z);*/
	
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

#ifdef DEBUG_LEVEL1
	cout.fixed;
	cout.precision(9);
	cout << endl << "Translation Matrix:" << endl;
	cout << Twr.at<double>(0,0) << endl;
	cout << Twr.at<double>(1,0) << endl;
	cout << Twr.at<double>(2,0) << endl;

	cout << endl << "Rotation Matrix:" << endl;
	cout << Rwr.at<double>(0,0) << "\t" << Rwr.at<double>(0,1) << "\t" << Rwr.at<double>(0,2) << endl;
	cout << Rwr.at<double>(1,0) << "\t" << Rwr.at<double>(1,1) << "\t" << Rwr.at<double>(1,2) << endl;
	cout << Rwr.at<double>(2,0) << "\t" << Rwr.at<double>(2,1) << "\t" << Rwr.at<double>(2,2) << endl;

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

cv::Mat CameraParam::FindR(double yaw, double pitch, double roll)
{
	cv::Mat rotationM = (cv::Mat_<double>(3,3) <<	
		cos(yaw)*cos(pitch), cos(yaw)*sin(pitch)*sin(roll)-sin(yaw)*cos(roll), cos(yaw)*sin(pitch)*cos(roll)+sin(yaw)*sin(roll),
		sin(yaw)*cos(pitch), sin(yaw)*sin(pitch)*sin(roll)+cos(yaw)*cos(roll), sin(yaw)*sin(pitch)*cos(roll)-cos(yaw)*sin(roll),
		-sin(pitch),		 cos(pitch)*sin(roll),							   cos(pitch)*cos(roll)					);

	return rotationM;
}

cv::Mat CameraParam::FindProjection(void)
{
	return P;
}