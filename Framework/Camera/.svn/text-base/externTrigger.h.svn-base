#pragma once

#include "../../Framework/network/udp_connection.h"
#include "../../Framework/utility/fastdelegate.h"

#define UDP_ETRIGGER_PORT 30021
#define UDP_ETRIGGER_ADDR "239.132.1.21"

#ifdef __cplusplus_cli
#pragma managed(push,off)
#endif

class eTrigReceiver;

using namespace std;

#pragma pack(1)
struct externTriggerHeader
{
	unsigned short seconds;
	unsigned long	 ticks;
	unsigned char	 id;
	unsigned short length;
};
#pragma pack()


typedef FastDelegate3<double, eTrigReceiver*, void*> externTrigger_Feedback_Msg_handler;


class eTrigReceiver
{
private:
	udp_connection *conn;		
	void UDPCallback(udp_message& msg, udp_connection* conn, void* arg);	
	externTrigger_Feedback_Msg_handler externTrigger_feedback_cbk;		
	void* externTrigger_feedback_cbk_arg;					
	int packetNum;

	static double GetTimestamp(void* packet)
	{
		externTriggerHeader* header = (externTriggerHeader*)packet;
		return ((double)ntohs(header->seconds)) + (((double)ntohl(header->ticks))/10000.0f);
	}

public:
	eTrigReceiver(void);
	~eTrigReceiver(void);	
	void SetFeedbackCallback(externTrigger_Feedback_Msg_handler handler, void* arg);				
	
	
};

#ifdef __cplusplus_cli
#pragma managed(pop)
#endif