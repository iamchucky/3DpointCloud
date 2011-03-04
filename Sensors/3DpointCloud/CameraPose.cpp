#include "CameraPose.h"
#include "pose.pb.h"

//RECIEVER-----------------------------------------------------------------------------------------------------------
void CameraPose::SetCallback(RobotPoseMsg_handler handler, void* arg)
{
	cbk = handler;
	cbk_arg = arg;
}

CameraPose::CameraPose(void) 
{
	udp_params params = udp_params();
	params.remote_ip = inet_addr(UDP_POSE_ADDR);
	params.local_port = UDP_POSE_PORT;
	params.reuse_addr = 1;
	conn = new udp_connection (params);	
	conn->set_callback (MakeDelegate(this,&CameraPose::UDPCallback),conn);	
	packetCount=0;
	printf("UDP Camera RX Interface Initialized. %s:%d\r\n",UDP_POSE_ADDR,UDP_POSE_PORT);
}

CameraPose::~CameraPose (void)
{
	delete conn;
}

void CameraPose::UDPCallback(udp_message& msg, udp_connection* conn, void* arg)
{ 
	// messages come from the protobuf:
	RobotPoseMsg umsg = *((RobotPoseMsg*)msg.data);
	packetCount++;

	// save protobuf message into struct
	Magic::Proto::Pose protoPose;
	protoPose.ParseFromArray(msg.data, msg.len);
	pose.x = protoPose.x();
	pose.y = protoPose.y();
	pose.z = protoPose.z();
	pose.yaw = protoPose.yaw();
	pose.pitch = protoPose.pitch();
	pose.roll = protoPose.roll();
	pose.timestamp = protoPose.timestamp();
	
	//raise event				
	if (!(cbk.empty()))	cbk(umsg, this, cbk_arg);

}
