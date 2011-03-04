//ty244 - March 4, 2011
//Generate 3D point cloud from camera input
//using the memory mapped file camera server

#ifndef _WIN32_WINNT		// Allow use of features specific to Windows XP or later.                   
#define _WIN32_WINNT 0x0501	// Change this to the appropriate value to target other versions of Windows.
#endif			

#include <stdio.h>
#include <tchar.h>
#include <string>
#include <sstream>

#include "CameraPose.h"
#include "CameraParam.h"

#include "opencv\cv.h"
#include "opencv\highgui.h"

using namespace std;

#define WIDTH 640
#define HEIGHT 480
#define CHANNELS 1

char camName[]="Global\\CamMappingObject";
HANDLE hMapFile=NULL;
HANDLE ghMutex;
void* pBuf;

char calibFilename[]="calib\\PGR_FireFlyMV.cal";

CameraPose* cameraPose;
CameraParam * cameraParam;

int robotID;
bool running=true;
bool hidedisplay = false;
bool displayhidden = false;
int frameNum=0;
double timestamp = 0;
double oldtimestamp = -1;
IplImage* img;
IplImage* resize;
IplImage* resizeBW;

CONSOLE_SCREEN_BUFFER_INFO startConsoleInfo;

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

void ClearScreen(bool startFromBeginning)
{
	HANDLE                     hStdOut;
	CONSOLE_SCREEN_BUFFER_INFO csbi;
	DWORD                      count;
	DWORD                      cellCount;
	COORD                      homeCoords = { 0, 0 };

	hStdOut = GetStdHandle( STD_OUTPUT_HANDLE );
	if (hStdOut == INVALID_HANDLE_VALUE) return;

	/* Get the number of cells in the current buffer */
	if (!GetConsoleScreenBufferInfo( hStdOut, &csbi )) return;
	cellCount = csbi.dwSize.X *csbi.dwSize.Y;

	if (!startFromBeginning)
	{
		homeCoords = startConsoleInfo.dwCursorPosition;
		cellCount = startConsoleInfo.dwSize.X *startConsoleInfo.dwSize.Y;
	}

	/* Fill the entire buffer with spaces */
	if (!FillConsoleOutputCharacter(
	hStdOut,
	(TCHAR) ' ',
	cellCount,
	homeCoords,
	&count
	)) return;

	/* Fill the entire buffer with the current colors and attributes */
	if (!FillConsoleOutputAttribute(
	hStdOut,
	csbi.wAttributes,
	cellCount,
	homeCoords,
	&count
	)) return;

	/* Move the cursor home */
	SetConsoleCursorPosition( hStdOut, homeCoords );
}

void GetConsoleCursorPosition()
{
	HANDLE                     hStdOut;

	hStdOut = GetStdHandle( STD_OUTPUT_HANDLE );
	if (hStdOut == INVALID_HANDLE_VALUE) return;

	/* Get the number of cells in the current buffer */
	if (!GetConsoleScreenBufferInfo( hStdOut, &startConsoleInfo )) return;
}

void PoseCallback(RobotPoseMsg pmsg, CameraPose* cameraPose, void* data)
{
	ClearScreen(false);
	cameraParam->Update_R_T(cameraPose->pose);
}

void InitCommon ()
{	
	cout << endl << "Loading camera intrinsic parameters: " << endl;
	cameraParam = new CameraParam(calibFilename);	// Load camera intrinsic parameters (loaded K)

	cameraPose = new CameraPose();
	cameraPose->SetCallback(PoseCallback, NULL);

	GetConsoleCursorPosition();

	img = cvCreateImage (cvSize(WIDTH,HEIGHT),IPL_DEPTH_8U,CHANNELS);
	resize = cvCreateImage (cvSize(WIDTH,HEIGHT),IPL_DEPTH_8U,3);
	resizeBW = cvCreateImage(cvSize(WIDTH,HEIGHT),IPL_DEPTH_8U,1);

	frameNum=0;
	running=true;		
}

double lastTS=0;
void RunCamera()
{			
	cvCvtColor (resize,resizeBW,CV_BGR2GRAY);

	if (!hidedisplay)
	{
		char str[30];
		sprintf(str,"ts:%f ind:%d",timestamp,frameNum);
		cvRectangle(resize,cvPoint(10,2),cvPoint(170,10),cvScalar(0,0,0),-1);
		cvPutText (resize,str,cvPoint(10,10),&cvFont(.75,1),cvScalar(255,255,255));
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

bool GetCameraInput()
{
	//wait for an image
	if (WaitForSingleObject(ghMutex,5000)!=WAIT_OBJECT_0)
	{
		//	running=false;
		printf("Warning: Did not receive global map handle in 5 seconds...\n");
		Sleep(1000);
		return false;
	}

	pBuf = MapViewOfFile(hMapFile,   // handle to map object
		FILE_MAP_READ, // read/write permission
		0,                   
		0,                   
		0);            //goto max size

	if (pBuf == NULL) 
	{ 
		printf("Could not map view of file (%d).\n", GetLastError()); 
		Sleep(1000);
		return false;
	}
	memcpy(img->imageData,pBuf,WIDTH*HEIGHT*CHANNELS);
	memcpy(&timestamp,(char*)pBuf + (WIDTH*HEIGHT*CHANNELS),sizeof(double));
	UnmapViewOfFile(pBuf);
	ReleaseMutex(ghMutex);

	if(CHANNELS == 1)
		cvCvtColor(img,resize,CV_BayerBG2BGR);
	else
		cvResize(img,resize);
}

void RunRealtime()
{	 
	// Make we can get camera input
	bool warnNoMappingObject = true;
	while(hMapFile == NULL)
	{
		hMapFile = OpenFileMappingA(FILE_MAP_READ, FALSE, camName);
		if (hMapFile == NULL) 
		{
			if (warnNoMappingObject)
			{
				cout << endl << "Waiting for CameraServer ... " << endl;
				warnNoMappingObject = false;
			}
			Sleep(1000);
		}		
	}	

	char filename[300];
	while(running)
	{	
		if (!GetCameraInput())	// Get image saved to IplImage "resize"
		{
			cout << "[RunRealtime()]: Can't get camera input from memory mapped file." << endl;
			continue;
		}

		// Start of my stuff


		// End of my stuff

		// Display current camera view
		RunCamera();			

		int key = cvWaitKey (1);
		if (key=='q')
			running=false;			
		else if (key == 'c')
		{
		}
		else if (key == 's')
			hidedisplay = !hidedisplay;

		if(oldtimestamp != timestamp)
		{
			frameNum++;	
			oldtimestamp = timestamp;
		}
	}
}

void CleanUp()
{
	cout << "Closing the program ..." << endl;
	CloseHandle(hMapFile);	
}

int main(int argc, char **argv)
{
	SetProcessAffinityMask (GetCurrentProcess (),0x01);
	SetErrorMode(0x07);

	//we use this mutex to know when there are new images...
	ghMutex = CreateMutexA(	NULL,FALSE, "CamMutex");         
	if (ghMutex == NULL) printf("CreateMutex error: %d\n", GetLastError());
	//robotID = GetRobotID() % 10;
	robotID = 3;
	cout<<"3DpointCloud"<<endl;
	cout<<"Robot ID: " << robotID << endl;

	InitCommon();	
	Sleep(100);
	RunRealtime();	

	CleanUp();

	return 0;
}
