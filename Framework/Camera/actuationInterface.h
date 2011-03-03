#pragma once

#include "../../Framework/network/udp_connection.h"
#include "../../Framework/utility/fastdelegate.h"

#define UDP_ACTUATION_PORT 30020
#define UDP_ACTUATION_ADDR "239.132.1.20"

#ifdef __cplusplus_cli
#pragma managed(push,off)
#endif

class ActInterfaceReceiver;

using namespace std;

#pragma pack(1)
struct ActuationHeader
{
	unsigned short seconds;
	unsigned long	 ticks;
	unsigned char	 id;
	unsigned short length;
};
#pragma pack()


typedef FastDelegate3<double, ActInterfaceReceiver*, void*> Actuation_Feedback_Msg_handler;


class ActInterfaceReceiver
{
private:
	udp_connection *conn;		
	void UDPCallback(udp_message& msg, udp_connection* conn, void* arg);	
	Actuation_Feedback_Msg_handler actuation_feedback_cbk;		
	void* actuation_feedback_cbk_arg;					
	int packetNum;

	static double GetTimestamp(void* packet)
	{
		ActuationHeader* header = (ActuationHeader*)packet;
		return ((double)ntohs(header->seconds)) + (((double)ntohl(header->ticks))/10000.0f);
	}

public:
	ActInterfaceReceiver(void);
	~ActInterfaceReceiver(void);	
	void SetFeedbackCallback(Actuation_Feedback_Msg_handler handler, void* arg);				
	
	
};

#ifdef __cplusplus_cli
#pragma managed(pop)
#endif