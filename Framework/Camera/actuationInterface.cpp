#include "actuationInterface.h"

#ifdef __cplusplus_cli
#pragma unmanaged
#endif


//RECIEVER-----------------------------------------------------------------------------------------------------------
void ActInterfaceReceiver::SetFeedbackCallback(Actuation_Feedback_Msg_handler handler, void* arg)
{
	actuation_feedback_cbk = handler;
	actuation_feedback_cbk_arg = arg;
}


ActInterfaceReceiver::ActInterfaceReceiver() 
{
	packetNum=0;
	udp_params params = udp_params();
	params.remote_ip = inet_addr(UDP_ACTUATION_ADDR);
	params.local_port = UDP_ACTUATION_PORT;
	params.reuse_addr = 1;
	conn = new udp_connection (params);		
	conn->set_callback (MakeDelegate(this,&ActInterfaceReceiver::UDPCallback),conn);
	printf("Actuation RX Interface Initialized. %s:%d\r\n",UDP_ACTUATION_ADDR,UDP_ACTUATION_PORT);
}

ActInterfaceReceiver::~ActInterfaceReceiver ()
{
	delete conn;
	printf("Actuation Interface Shutdown...\r\n");
}

void ActInterfaceReceiver::UDPCallback(udp_message& msg, udp_connection* conn, void* arg)
{ 		
	if (msg.len < sizeof(ActuationHeader)) return;
	double ts = this->GetTimestamp (msg.data);
	actuation_feedback_cbk (ts,this,actuation_feedback_cbk_arg);	
	packetNum++;
}
