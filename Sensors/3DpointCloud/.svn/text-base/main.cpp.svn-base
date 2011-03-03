//amn32 - October 19, 2009
//Broadcasts UDP JPEG compressed images 
//using the memory mapped file camera server

#ifndef _WIN32_WINNT		// Allow use of features specific to Windows XP or later.                   
#define _WIN32_WINNT 0x0501	// Change this to the appropriate value to target other versions of Windows.
#endif			

#define UDPIMGWIDTH 360//480
#define UDPIMGHEIGHT 90//120

#include <stdio.h>
#include <tchar.h>
#include <string>
#include <sstream>



//#include "..\..\Framework\Camera\udpcamerabroadcaster.h"
//#include "..\..\Framework\network\udp_connection.h"
//#include "..\..\Framework\network\net_utility.h"
//#include "..\..\Framework\utility\fastdelegate.h"
//#include "simplejpeg.h"
//#include <fstream>
#include "CameraPose.h"

#include "opencv\cv.h"
#include "opencv\highgui.h"

using namespace std;

int WIDTH = 640;
int HEIGHT = 480;
int CHANNELS = 3;

char camName[]="Global\\CamMappingObject";
HANDLE hMapFile=NULL;
HANDLE ghMutex;
void* pBuf;

//camera configuration
//UDPCameraBroadcaster* broadcastCam;
//UDPCameraBroadcaster* broadcastloggingCam;
CameraPose* cameraPose;
int broadcastmod=0;
int robotID;

bool running=true;
bool hidedisplay = false;
bool displayhidden = false;
bool logcam = false;
int frameNum=0;
double timestamp = 0;
double oldtimestamp = -1;
IplImage* img;
IplImage* resize;
IplImage* resizeBW;
IplImage* colorImg;
unsigned char* rawimg;
char tmpFilename[200];
int imgNum=0;
CvFont font = cvFont(.75,1);
bool logging = false;

using namespace std;

//int GetRobotID()
//{
//	WSAData wsaData;
//	if (WSAStartup(MAKEWORD(1, 1), &wsaData) != 0) {
//		return 255;
//	}
//
//	char ac[80];
//	char ac2[80];
//	if (gethostname(ac, sizeof(ac)) == SOCKET_ERROR) 
//	{
//		cerr << "Error " << WSAGetLastError() <<	" when getting local host name." << endl;
//		return 0;
//	}
//	cout << "Host name is " << ac << "." << endl;
//	int ret=0;
//	sscanf(ac,"spider%s",ac2); 
//	//	sscanf(ac,"spider%d",ret); 
//	ret = atoi(ac2);
//	struct hostent *phe = gethostbyname(ac);
//	if (phe == 0) 
//	{
//		cerr << "Yow! Bad host lookup." << endl; return 0;
//	}
//
//	for (int i = 0; phe->h_addr_list[i] != 0; ++i) 
//	{
//		struct in_addr addr;
//		memcpy(&addr, phe->h_addr_list[i], sizeof(struct in_addr));
//		cout << "Address " << i << ": " << inet_ntoa(addr) << endl;
//	}
//
//	WSACleanup();
//	return ret;
//}

void PoseCallback(RobotPoseMsg pose, CameraPose* rx, void* data)
{
	//printf("I got a pose msg\n");
}

void InitCommon ()
{	
	//broadcastCam = new UDPCameraBroadcaster ();								// Sensor broadcast
	//if(logging)
	//	broadcastloggingCam = new UDPCameraBroadcaster("239.132.1.99", 30099);	// Sensor broadcast logging
	cameraPose = new CameraPose();
	cameraPose->SetCallback(PoseCallback, NULL);
	cvNamedWindow ("output",CV_WINDOW_AUTOSIZE);
	img = cvCreateImage (cvSize(WIDTH,HEIGHT),IPL_DEPTH_8U,CHANNELS);
	resize = cvCreateImage (cvSize(WIDTH,HEIGHT),IPL_DEPTH_8U,3);
	resizeBW = cvCreateImage(cvSize(WIDTH,HEIGHT),IPL_DEPTH_8U,1);
	if (WIDTH > 640)
		colorImg = cvCreateImage(cvSize(UDPIMGWIDTH, UDPIMGHEIGHT), IPL_DEPTH_8U, 3);	//480x120
	else
		colorImg = cvCreateImage(cvSize(320, 240), IPL_DEPTH_8U, 3);


	rawimg = new unsigned char[640*480];
	frameNum=0;
	running=true;		
}



void ConvertToRawImage (IplImage* img, unsigned char* rawimg)
{
	for (int i=0; i<640*480; i++)
		rawimg[i] = *(unsigned char*)(img->imageData + i);
}


double lastTS=0;
void RunCamera(double timestamp)
{			
	cvCvtColor (resize,resizeBW,CV_BGR2GRAY);
	//if (oldtimestamp != timestamp)//
	if((frameNum%10==0)&&(!logcam))
	{
		if (broadcastmod>=3)
		{
			////IplImage* broadcastImg = cvCreateImage (cvSize(320,240),IPL_DEPTH_8U,1);		// Grayscale 320x240 image
			//if (WIDTH > 640)
			//{
			//	cvResize(resize, colorImg);
			//	broadcastCam->SendImage((unsigned char*)colorImg->imageData,colorImg->widthStep, UDPIMGWIDTH, UDPIMGHEIGHT,timestamp,frameNum,robotID);
			//	if(logging)
			//		broadcastloggingCam->SendImage((unsigned char*)colorImg->imageData, colorImg->widthStep, UDPIMGWIDTH, UDPIMGHEIGHT, timestamp, frameNum, robotID);
			//}
			//else
			//{
			//	cvResize(resize, colorImg);
			//	broadcastCam->SendImage((unsigned char*)colorImg->imageData,colorImg->widthStep, 320, 240,timestamp,frameNum,robotID);
			//	if(logging)
			//		broadcastloggingCam->SendImage((unsigned char*)colorImg->imageData, colorImg->widthStep, 320, 240, timestamp, frameNum, robotID);
			//}
			////cvResize (resizeBW,broadcastImg);


			////cvReleaseImage(&broadcastImg);
			//broadcastmod = 0;	
		}
		broadcastmod++;
	}

	if (!hidedisplay)
	{
		char str[30];
		sprintf(str,"ts:%f ind:%d",timestamp,frameNum);
		cvRectangle(resize,cvPoint(10,2),cvPoint(170,10),cvScalar(0,0,0),-1);
		cvPutText (resize,str,cvPoint(10,10),&font,cvScalar(255,255,255));
		cvShowImage("output",resize);
		displayhidden = false;
	}
	else
	{
		if(!displayhidden)
		{
			cvDestroyWindow("output");
			displayhidden = true;
		}
		cvNamedWindow ("output",CV_WINDOW_AUTOSIZE);
		cvResizeWindow("output",0,0);
	}

}


void RunRealtime()
{	 
	while(hMapFile == NULL)
	{
		hMapFile = OpenFileMappingA(FILE_MAP_READ, FALSE, camName);
		if (hMapFile == NULL) 
		{
			printf("Could not access file mapping object (%d).\n",GetLastError());
			Sleep(1000);
		}		
	}	

	char filename[300];
	while(running)
	{	
		//wait for an image
		if (WaitForSingleObject(ghMutex,5000)!=WAIT_OBJECT_0)
		{
			//	running=false;
			printf("Warning: Did not receive global map handle in 5 seconds...\n");
			Sleep(1000);
			continue;
		}
		//EnterCriticalSection(&(rtCam->camgrab_cs));
		pBuf = MapViewOfFile(hMapFile,   // handle to map object
			FILE_MAP_READ, // read/write permission
			0,                   
			0,                   
			0);            //goto max size

		if (pBuf == NULL) 
		{ 
			printf("Could not map view of file (%d).\n", GetLastError()); 
			Sleep(1000);
			continue;
		}
		memcpy(img->imageData,pBuf,WIDTH*HEIGHT*CHANNELS);
		memcpy(&timestamp,(char*)pBuf + (WIDTH*HEIGHT*CHANNELS),sizeof(double));
		UnmapViewOfFile(pBuf);
		ReleaseMutex(ghMutex);

		if(CHANNELS == 1)
			cvCvtColor(img,resize,CV_BayerBG2BGR);
		else
			cvResize(img,resize);

		if((oldtimestamp != timestamp)&&(logcam))	// if((frameNum%30==0)&&(logcam))
		{
			sprintf(filename, "c:\\Camera_Log\\img_ts_%f_ind_%05d.jpg",timestamp, frameNum);
			//cvSaveImage ("C:\\capture.bmp",img);
			cvSaveImage (filename,resize);
			printf("Current image saved to %s\n", filename);
		}

		RunCamera(timestamp);			
		int key = cvWaitKey (1);
		if (key=='q')
			running=false;			
		else if (key == 'c')
		{
			sprintf(filename, "c:\\img_%05d.jpg", frameNum);
			//cvSaveImage ("C:\\capture.bmp",img);
			cvSaveImage (filename,resize);
			printf("Current image saved to %s\n", filename);
		}
		else if (key == 's')
			hidedisplay = !hidedisplay;
		else if (key == 'l')
			logcam = !logcam;

		if(oldtimestamp != timestamp)
		{
			frameNum++;	
			oldtimestamp = timestamp;
		}
	}
	CloseHandle(hMapFile);	
	cvReleaseImage(&colorImg);
}

int main(int argc, char **argv)
{
	if (argc>1)
	{
		if (strcmp(argv[1],"FIREFLY")==0) 
		{
			WIDTH = WIDTH*3;
			HEIGHT = 480;
			CHANNELS = 1;			
			printf ("Using PGR Firefly.\n");			
		}
		else if (strcmp(argv[1],"PLAYLOG")==0) 
		{
			CHANNELS = 1;		
			printf ("Playing logged data.\n");	
		}
		else
		{
			CHANNELS = 3;			
			printf ("Using Unibrain.\n");			
		}

		if(strcmp(argv[2], "LOG") == 0)
		{
			logging = true;
		}

	}
	else
		printf("Warning: No Command Line Arguments Defined. Using Defaults.\n");

	CHANNELS = 1;	
	SetProcessAffinityMask (GetCurrentProcess (),0x01);
	SetErrorMode(0x07);

	Sleep(2000);
	//we use this mutex to know when there are new images...
	ghMutex = CreateMutexA(	NULL,FALSE, "CamMutex");         
	if (ghMutex == NULL) printf("CreateMutex error: %d\n", GetLastError());
	//robotID = GetRobotID() % 10;
	robotID = 3;
	cout<<"CameraBroadcaster"<<endl;
	cout<<"Robot ID: " << robotID << endl;
	cout<<"\n\nPress 's' to toggle the display (off by default)\n";
	cout<<"Press 'l' to toggle the camera logging (off by default)\n";
	cout<<"Logged image is saved under c:\Camera_Log\ \n\n\n";
	InitCommon();	
	Sleep(100);
	RunRealtime();	
	return 0;
}
