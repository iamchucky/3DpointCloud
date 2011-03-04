#pragma once
#include "..\..\Framework\Network\udp_connection.h"

#define UDP_POSE_PORT 30198
#define UDP_POSE_ADDR "239.132.1.198"

struct RobotPoseMsg
{
	char msg[395];
};



class CameraPose;

typedef FastDelegate3<RobotPoseMsg, CameraPose*, void*> RobotPoseMsg_handler;

class CameraPose
{
public:
	CameraPose(void);
	~CameraPose(void);
	void SetCallback(RobotPoseMsg_handler handler, void* arg);		
	
	struct Pose
	{
		double x;
		double y;
		double z;
		double yaw;
		double pitch;
		double roll;
		double timestamp;
	};

	int packetCount;
	struct Pose pose;

private:
	udp_connection *conn;
	void UDPCallback(udp_message& msg, udp_connection* conn, void* arg);
	RobotPoseMsg_handler cbk;		
	void* cbk_arg;		

};

