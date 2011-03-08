#pragma once
#include "CameraPose.h"
#include "opencv\cv.h"

class CameraParam
{
	struct intrinsics
	{
		double fc0;
		double fc1;
		double cc0;
		double cc1;
		double alpha_c;
	};
	
	struct intrinsics in;
	
public:
	CameraParam(char* calibFilename);
	~CameraParam(void);

	double Get_fc0(void);
	double Get_fc1(void);
	double Get_cc0(void);
	double Get_cc1(void);
	double Get_alpha_c(void);

	void Update_R_T(CameraPose::Pose & pose);
	cv::Mat FindR(double yaw, double pitch, double roll);
	cv::Mat FindProjection();

	cv::Mat K;
	cv::Mat Rwr;
	cv::Mat Twr;
	cv::Mat Rrc;
	cv::Mat Trc;
	cv::Mat T_x;
	cv::Mat RT;
	cv::Mat P;
};

