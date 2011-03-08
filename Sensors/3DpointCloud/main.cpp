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
#include <iostream>
#include <fstream>

#include "CameraPose.h"
#include "CameraParam.h"
#include "Features.h"

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
char outputFilename[]="3D\\output.txt";

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
IplImage* prevImg;
vector <cv::Point2f> prevPts;
vector <cv::Point2f> nextPts;
cv::Mat prevP;
cv::Mat nextP;
cv::Mat threeDcoord;
ofstream output;

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
#ifdef DEBUG_LEVEL1
	ClearScreen(false);
#endif
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
	prevImg = cvCreateImage(cvGetSize(resize),IPL_DEPTH_8U,1);

	threeDcoord = cv::Mat(4,1,CV_64F);
	/*prevP = cv::Mat(3,4,CV_64F);
	nextP = cv::Mat(3,4,CV_64F);*/

	output.open (outputFilename, ios::out);

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
	{
		cvCvtColor(img,resize,CV_BayerBG2BGR);
		cvCvtColor(resize,resizeBW,CV_BGR2GRAY);
	}
	else
	{
		cvResize(img,resize);
		cvCvtColor(resize,resizeBW,CV_BGR2GRAY);
	}
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
		if (timestamp != oldtimestamp && !cameraParam->K.empty() && !cameraParam->RT.empty() && oldtimestamp > 0)
		{
			/*FeatureSet features1;
			computeFeatures ( resize, features1, 1 );*/
			std::vector<cv::KeyPoint> keypoints;
			cv::Mat descriptors;
			nextP = cameraParam->P.clone();
			cv::FAST(resizeBW,keypoints,50);

			vector <uchar> status;
			vector <float> err;
			
			cv::Mat origin =  (cv::Mat_<double>(4,1) << 0.0, 0.0, 0.0, 1.0);
			cv::Mat origin2d = cv::Mat(3,1,CV_64F);
			origin2d = nextP*origin;
			int orix = origin2d.at<double>(0,0)/origin2d.at<double>(2,0);
			int oriy = origin2d.at<double>(1,0)/origin2d.at<double>(2,0);
			if (abs(orix) < 640 && abs(oriy) < 480 && orix > 0)
			{
				cvRectangle( resize, cvPoint(orix-10,oriy-10), cvPoint(orix+10,oriy+10),cvScalar(255,0,0) );

				cv::Mat onex =  (cv::Mat_<double>(4,1) << 0.5, 0.0, 0.0, 1.0);
				cv::Mat oney =  (cv::Mat_<double>(4,1) << 0.0, 0.5, 0.0, 1.0);
				cv::Mat onez =  (cv::Mat_<double>(4,1) << 0.0, 0.0, 0.5, 1.0);
				cv::Mat onex2d = cv::Mat(3,1,CV_64F);
				cv::Mat oney2d = cv::Mat(3,1,CV_64F);
				cv::Mat onez2d = cv::Mat(3,1,CV_64F);
				onex2d = nextP*onex;
				oney2d = nextP*oney;
				onez2d = nextP*onez;
				int onexpx = onex2d.at<double>(0,0)/onex2d.at<double>(2,0);
				int onexpy = onex2d.at<double>(1,0)/onex2d.at<double>(2,0);
				int oneypx = oney2d.at<double>(0,0)/oney2d.at<double>(2,0);
				int oneypy = oney2d.at<double>(1,0)/oney2d.at<double>(2,0);
				int onezpx = onez2d.at<double>(0,0)/onez2d.at<double>(2,0);
				int onezpy = onez2d.at<double>(1,0)/onez2d.at<double>(2,0);
				cvLine( resize, cvPoint(orix,oriy), cvPoint(onexpx,onexpy), cvScalar(255,0,0));
				cvLine( resize, cvPoint(orix,oriy), cvPoint(oneypx,oneypy), cvScalar(0,255,0));
				cvLine( resize, cvPoint(orix,oriy), cvPoint(onezpx,onezpy), cvScalar(0,0,255));
			}
			
			if (!prevP.empty())
			{
				cv::calcOpticalFlowPyrLK( prevImg, resizeBW, prevPts, nextPts, status, err);
				/*cv::SIFT sift;
				sift ( resizeBW, cv::Mat (), keypoints, descriptors, true);*/
				int kx;
				int ky;
				for ( int i = 0; i < keypoints.size (); ++i )
				{
					kx = keypoints[i].pt.x;
					ky = keypoints[i].pt.y;
					cvRectangle( resize, cvPoint(kx-2,ky-2), cvPoint(kx+2,ky+2),cvScalar(0,0,255) );
				}
				for ( int i = 0; i < prevPts.size (); ++i )
				{
					if (status[i] && err[i] < 50.f)
					{
						int px = prevPts[i].x;
						int py = prevPts[i].y;
						int nx = nextPts[i].x;
						int ny = nextPts[i].y;
						int d = (px-nx)*(px-nx)+(py-ny)*(py-ny);
						/*if (d < 1000)
						{*/
							cvLine( resize, cvPoint(px,py), cvPoint(nx,ny), cvScalar(0,0,0));

							// Triangulation using P and feature correspondence
							// prevP
							// cameraParam->P
							// prevPts
							// nextPts
							cv::Mat A = (cv::Mat_<double>(4,4) <<	
								prevP.at<double>(2,0)*px-prevP.at<double>(0,0), prevP.at<double>(2,1)*px-prevP.at<double>(0,1), prevP.at<double>(2,2)*px-prevP.at<double>(0,2), prevP.at<double>(2,3)*px-prevP.at<double>(0,3),
								prevP.at<double>(2,0)*py-prevP.at<double>(1,0), prevP.at<double>(2,1)*py-prevP.at<double>(1,1), prevP.at<double>(2,2)*py-prevP.at<double>(1,2), prevP.at<double>(2,3)*py-prevP.at<double>(1,3),
								nextP.at<double>(2,0)*nx-nextP.at<double>(0,0), nextP.at<double>(2,1)*nx-nextP.at<double>(0,1), nextP.at<double>(2,2)*nx-nextP.at<double>(0,2), nextP.at<double>(2,3)*nx-nextP.at<double>(0,3),
								nextP.at<double>(2,0)*ny-nextP.at<double>(1,0), nextP.at<double>(2,1)*ny-nextP.at<double>(1,1), nextP.at<double>(2,2)*ny-nextP.at<double>(1,2), nextP.at<double>(2,3)*ny-nextP.at<double>(1,3)	);
							//cv::Mat B = cv::Mat::zeros(4,1,CV_64F);
							//cv::solve(A,B,threeDcoord,cv::DECOMP_SVD);
							cv::SVD::solveZ(A, threeDcoord);

							/*ClearScreen(false);
							cout << endl << "3D coord:" << endl;
							cout << threeDcoord.at<double>(0,0)/threeDcoord.at<double>(3,0) << endl;
							cout << threeDcoord.at<double>(1,0)/threeDcoord.at<double>(3,0) << endl;
							cout << threeDcoord.at<double>(2,0)/threeDcoord.at<double>(3,0) << endl;*/
							//cout << threeDcoord.at<double>(3,0) << endl;
							double ox = threeDcoord.at<double>(0,0)/threeDcoord.at<double>(3,0);
							double oy = threeDcoord.at<double>(1,0)/threeDcoord.at<double>(3,0);
							double oz = threeDcoord.at<double>(2,0)/threeDcoord.at<double>(3,0);
							if ( abs(ox) < 10 && abs(oy) < 10 && abs(oz) < 10)
							{
								output << cameraPose->pose.x << "\t" << cameraPose->pose.y << "\t" << cameraPose->pose.z << "\t" << cameraPose->pose.yaw << "\t" << cameraPose->pose.pitch << "\t" << cameraPose->pose.roll << "\t" ;
								output << ox << "\t" << oy << "\t" << oz << endl;
							}
						//}
					}
				}
				
			}
			prevP = nextP.clone();
			cvCopy(resizeBW,prevImg);
			prevPts.resize(keypoints.size());
			for (int n = 0; n < prevPts.size(); ++n)
			{
				prevPts[n].x = keypoints[n].pt.x;
				prevPts[n].y = keypoints[n].pt.y;
			}
			

			
			
			// End of my stuff

			// Display current camera view
			RunCamera();			
		}
		

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
	cvReleaseImage(&img);
	cvReleaseImage(&resize);
	cvReleaseImage(&resizeBW);
	cvReleaseImage(&prevImg);
	output.close();
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
