#include "externTrigger.h"

#ifdef __cplusplus_cli
#pragma unmanaged
#endif


//RECIEVER-----------------------------------------------------------------------------------------------------------
void eTrigReceiver::SetFeedbackCallback(externTrigger_Feedback_Msg_handler handler, void* arg)
{
	externTrigger_feedback_cbk = handler;
	externTrigger_feedback_cbk_arg = arg;
}


eTrigReceiver::eTrigReceiver() 
{
	packetNum=0;
	udp_params params = udp_params();
	params.remote_ip = inet_addr(UDP_ETRIGGER_ADDR);
	params.local_port = UDP_ETRIGGER_PORT;
	params.reuse_addr = 1;
	conn = new udp_connection (params);		
	conn->set_callback (MakeDelegate(this,&eTrigReceiver::UDPCallback),conn);
	printf("externTrigger RX Interface Initialized. %s:%d\r\n",UDP_ETRIGGER_ADDR,UDP_ETRIGGER_PORT);
}

eTrigReceiver::~eTrigReceiver ()
{
	delete conn;
	printf("externTrigger Interface Shutdown...\r\n");
}

void eTrigReceiver::UDPCallback(udp_message& msg, udp_connection* conn, void* arg)
{ 		
	if (msg.len < sizeof(externTriggerHeader)) return;
	double ts = this->GetTimestamp (msg.data);
	externTrigger_feedback_cbk (ts,this,externTrigger_feedback_cbk_arg);	
	packetNum++;
}
