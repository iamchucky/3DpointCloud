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
	sequenceNumber=0;
	packetCount=0;
	dropCount=0;
	printf("UDP Camera RX Interface Initialized. %s:%d\r\n",UDP_POSE_ADDR,UDP_POSE_PORT);
}

CameraPose::~CameraPose (void)
{
	delete conn;
}

void CameraPose::UDPCallback(udp_message& msg, udp_connection* conn, void* arg)
{ 
	//messages come in like this:
	// ID + memory mapped message, ID is 1 byte	
	RobotPoseMsg umsg = *((RobotPoseMsg*)msg.data);
	
	/*int rxSequenceNumber=	umsg.index;
	dropCount += (rxSequenceNumber-(sequenceNumber+1));
	sequenceNumber = rxSequenceNumber;*/
	packetCount++;
	
	//raise event				
	if (!(cbk.empty()))	cbk(umsg, this, cbk_arg);

	Magic::Proto::Pose protoPose;
	protoPose.ParseFromArray(msg.data, msg.len);
	cout << protoPose.x() << "\t" <<  protoPose.y() << "\t" << protoPose.yaw() << "\t" << protoPose.timestamp() <<  endl;

	/*if (packetCount%100==0)	
	{
		#ifdef PRINT_PACKET_COUNT_DEBUG
		printf("UC: Packets: %d Seq: %d Dropped: %d Drop Rate: %f \r\n",packetCount,sequenceNumber,dropCount,((float)dropCount/(float)packetCount)*100.0f);	
		#endif
		packetCount=0; dropCount=0;
	}*/
}
