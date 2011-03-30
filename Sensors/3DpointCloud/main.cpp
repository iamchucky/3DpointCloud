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

//define this to get dll import definition for win32
#define SIFTGPU_DLL
//#ifdef _DEBUG 
//    #pragma comment(lib, "lib/siftgpu_d.lib")
//#else
    #pragma comment(lib, "lib/siftgpu.lib")
//#endif

#include "SiftGPU/SiftGPU.h"

using namespace std;

#define WIDTH 640
#define HEIGHT 480
#define CHANNELS 1
#define USE_F_GUIDED_SIFT true
#define USE_H_GUIDED_SIFT true
#define ANGLE_SECTION 24

char camName[]="Global\\CamMappingObject";
HANDLE hMapFile=NULL;
HANDLE ghMutex;
void* pBuf;

char calibFilename[]="calib\\PGR_FireFlyMV.cal";
char outputFilename[]="3D\\output.txt";

CameraPose* cameraPose;
CameraPose::Pose prevPose;
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

bool gotDisparity = false;
bool goneOnce = false;
SiftGPU  *sift;
SiftMatchGPU *matcher;
vector<float > descriptors1(1), descriptors2(1);
vector<SiftGPU::SiftKeypoint> keys1(1), keys2(1);    
int num1 = 0, num2 = 0;

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

struct siftmatch
{
	int goneOnce;
	bool update;
	int num;
	vector<float> descriptors;
	vector<SiftGPU::SiftKeypoint> keys;
	cv::Mat prevP;
	double posex;
	double posey;
	double poseyaw;
};

struct siftmatch siftm[ANGLE_SECTION];

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
//#ifdef DEBUG_LEVEL1
//	ClearScreen(false);
//#endif
	/*double t = cameraPose->pose.timestamp;
	t = t*1000.0;
	int roundt = (int)t%100;
	if (roundt < 20)*/
		cameraParam->Update_R_T(cameraPose->pose);
	/*if (!goneOnce)
	{
		prevPose.x = cameraPose->pose.x;
		prevPose.y = cameraPose->pose.y;
		prevPose.yaw = (cameraPose->pose.yaw < 0)? cameraPose->pose.yaw+2.0*3.1415:cameraPose->pose.yaw;
		goneOnce = true;
	}
	else
	{
		double d = pow(cameraPose->pose.x-prevPose.x,2.0)+pow(cameraPose->pose.y-prevPose.y,2.0);
		double dyaw = prevPose.yaw-((cameraPose->pose.yaw < 0)? (cameraPose->pose.yaw+2.0*3.1415):cameraPose->pose.yaw);
		if (d > 0.25 && abs(dyaw) < 0.1)
		{
			prevPose.x = cameraPose->pose.x;
			prevPose.y = cameraPose->pose.y;
			prevPose.yaw = cameraPose->pose.yaw;
			gotDisparity = true;
		}
	}*/
}

void InitCommon ()
{	
	cout << endl << "Loading camera intrinsic parameters: " << endl;
	cameraParam = new CameraParam(calibFilename);	// Load camera intrinsic parameters (loaded K)

	GetConsoleCursorPosition();

	cameraPose = new CameraPose();
	cameraPose->SetCallback(PoseCallback, NULL);

	img = cvCreateImage (cvSize(WIDTH,HEIGHT),IPL_DEPTH_8U,CHANNELS);
	resize = cvCreateImage (cvSize(WIDTH,HEIGHT),IPL_DEPTH_8U,3);
	resizeBW = cvCreateImage(cvSize(WIDTH,HEIGHT),IPL_DEPTH_8U,1);
	prevImg = cvCreateImage(cvGetSize(resize),IPL_DEPTH_8U,1);

	threeDcoord = cv::Mat(4,1,CV_64F);
	/*prevP = cv::Mat(3,4,CV_64F);
	nextP = cv::Mat(3,4,CV_64F);*/

	prevPose.x = 0.0;
	prevPose.y = 0.0;
	prevPose.yaw = 0.0;

	// SIFTGPU stuff
	sift = new SiftGPU;
	matcher = new SiftMatchGPU(4096);
	//Create a context for computation, and SiftGPU will be initialized automatically 
    //The same context can be used by SiftMatchGPU
    if(sift->CreateContextGL() != SiftGPU::SIFTGPU_FULL_SUPPORTED) cout << "SIFTGPU init failed\n";

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
void PlotOrigin(cv::Mat & P)
{
	cv::Mat origin =  (cv::Mat_<double>(4,1) << 0.0, 0.0, 0.0, 1.0);
	cv::Mat origin2d = cv::Mat(3,1,CV_64F);
	origin2d = P*origin;
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
}

void PlotEpiline(cv::Mat & F, std::vector<cv::Point2f> points)
{
	for ( int i = 0; i < points.size (); ++i )
	{
		int px = points[i].x;
		int py = points[i].y;
		cv::Mat p = (cv::Mat_<double>(3,1) << px, py, 1);
		for ( int qy = 0; qy < resize->height; ++qy)
		{
			for ( int qx = 0; qx < resize->width; ++qx)
			{
				cv::Mat qT = (cv::Mat_<double>(1,3) << qx, qy, 1);
				cv::Mat result = qT*F*p;
				if (abs(result.at<double> (0,0)) < 0.5)
				{
					CV_IMAGE_ELEM(resize, char, qy, qx) = 255;
				}
			}
		}
	}
}



void MatchSIFT(int id)
{
	if (siftm[id].num > 0 && siftm[id].goneOnce)
	{
		//Verify current OpenGL Context and initialize the Matcher;
		//If you don't have an OpenGL Context, call matcher->CreateContextGL instead;
		matcher->VerifyContextGL(); //must call once

		//Set descriptors to match, the first argument must be either 0 or 1
		//if you want to use more than 4096 or less than 4096
		//call matcher->SetMaxSift() to change the limit before calling setdescriptor
		matcher->SetDescriptors(0, siftm[id].num, &siftm[id].descriptors[0]); //image 1
		matcher->SetDescriptors(1, num2, &descriptors2[0]); //image 2

		//match and get result.    
		int (*match_buf)[2] = new int[siftm[id].num][2];
		int num_match = 0;
		//use the default thresholds. Check the declaration in SiftGPU.h
		if (!USE_F_GUIDED_SIFT)
		{
			num_match = matcher->GetSiftMatch(siftm[id].num, match_buf);
			std::cout << num_match << " sift matches were found;\n";
		}
		else
		{
			//*****************GPU Guided SIFT MATCHING***************
			//example: define a homography, and use default threshold 32 to search in a 64x64 window
			//float h[3][3] = {{0.8f, 0, 0}, {0, 0.8f, 0}, {0, 0, 1.0f}};
						
			cv::Mat F = cameraParam->GetFfromP(siftm[id].prevP,nextP);
			float f[3][3] = {{F.at<double>(0,0), F.at<double>(0,1), F.at<double>(0,2)}, 
								{F.at<double>(1,0), F.at<double>(1,1), F.at<double>(1,2)},
								{F.at<double>(2,0), F.at<double>(2,1), F.at<double>(2,2)}};
			matcher->SetFeatureLocation(0, &siftm[id].keys[0]); //SetFeatureLocaiton after SetDescriptors
			matcher->SetFeatureLocation(1, &keys2[0]);
			num_match = matcher->GetGuidedSiftMatch(siftm[id].num, match_buf, NULL, f);
			cv::Mat H;
			if (USE_H_GUIDED_SIFT)
			{
				cv::Mat srcPt(num_match, 2, CV_64F);
				cv::Mat dstPt(num_match, 2, CV_64F);
				for(int i  = 0; i < num_match; ++i)
				{
					//How to get the feature matches: 
					SiftGPU::SiftKeypoint & key1 = siftm[id].keys[match_buf[i][0]];
					SiftGPU::SiftKeypoint & key2 = keys2[match_buf[i][1]];
					srcPt.at<double>(i,0) = key1.x;
					srcPt.at<double>(i,1) = key1.y;
					dstPt.at<double>(i,0) = key2.x;
					dstPt.at<double>(i,1) = key2.y;
				}
							
				H = cv::findHomography(srcPt, dstPt);
				float h[3][3] = {{H.at<double>(0,0), H.at<double>(0,1), H.at<double>(0,2)}, 
									{H.at<double>(1,0), H.at<double>(1,1), H.at<double>(1,2)},
									{H.at<double>(2,0), H.at<double>(2,1), H.at<double>(2,2)}};
				num_match = matcher->GetGuidedSiftMatch(num_match, match_buf, h, f);
			}
			std::cout << num_match << " guided sift matches were found;\n";
			//if you can want to use a Fundamental matrix, check the function definition
		}
    
		//enumerate all the feature matches
		for(int i  = 0; i < num_match; ++i)
		{
			//How to get the feature matches: 
			SiftGPU::SiftKeypoint & key1 = siftm[id].keys[match_buf[i][0]];
			SiftGPU::SiftKeypoint & key2 = keys2[match_buf[i][1]];
			//key1 in the first image matches with key2 in the second image

			if (!siftm[id].prevP.empty())
			{
				//cv::Mat F = cameraParam->GetFfromP(prevP,nextP);
				/*cv::Mat p = (cv::Mat_<double>(3,1) << key1.x, key1.y, 1);
				cv::Mat qT = (cv::Mat_<double>(1,3) << key2.x, key2.y, 1);
				cv::Mat result = qT*F*p;
				double resultd = abs(result.at<double> (0,0));
				if (resultd > 100)
				{
					continue;
				}*/

				cvRectangle( resize, cvPoint(key2.x-2,key2.y-2), cvPoint(key2.x+2,key2.y+2),cvScalar(0,0,255) );

				int px = key1.x;
				int py = key1.y;
				int nx = key2.x;
				int ny = key2.y;
				cvLine( resize, cvPoint(px,py), cvPoint(nx,ny), cvScalar(0,0,0));

				// Triangulation using P and feature correspondence
				// prevP
				// cameraParam->P
				// prevPts
				// nextPts
				cv::Mat prevP = siftm[id].prevP;
				cv::Mat A = (cv::Mat_<double>(4,4) <<	
					prevP.at<double>(2,0)*px-prevP.at<double>(0,0), prevP.at<double>(2,1)*px-prevP.at<double>(0,1), prevP.at<double>(2,2)*px-prevP.at<double>(0,2), prevP.at<double>(2,3)*px-prevP.at<double>(0,3),
					prevP.at<double>(2,0)*py-prevP.at<double>(1,0), prevP.at<double>(2,1)*py-prevP.at<double>(1,1), prevP.at<double>(2,2)*py-prevP.at<double>(1,2), prevP.at<double>(2,3)*py-prevP.at<double>(1,3),
					nextP.at<double>(2,0)*nx-nextP.at<double>(0,0), nextP.at<double>(2,1)*nx-nextP.at<double>(0,1), nextP.at<double>(2,2)*nx-nextP.at<double>(0,2), nextP.at<double>(2,3)*nx-nextP.at<double>(0,3),
					nextP.at<double>(2,0)*ny-nextP.at<double>(1,0), nextP.at<double>(2,1)*ny-nextP.at<double>(1,1), nextP.at<double>(2,2)*ny-nextP.at<double>(1,2), nextP.at<double>(2,3)*ny-nextP.at<double>(1,3)	);
				//cv::Mat B = cv::Mat::zeros(4,1,CV_64F);
				//cv::solve(A,B,threeDcoord,cv::DECOMP_SVD);
				cv::SVD::solveZ(A, threeDcoord);

				double ox = threeDcoord.at<double>(0,0)/threeDcoord.at<double>(3,0);
				double oy = threeDcoord.at<double>(1,0)/threeDcoord.at<double>(3,0);
				double oz = threeDcoord.at<double>(2,0)/threeDcoord.at<double>(3,0);
				if ( abs(ox) < 10 && abs(oy) < 10 && abs(oz) < 10)
				{
					output << cameraPose->pose.x << "\t" << cameraPose->pose.y << "\t" << cameraPose->pose.z << "\t" << cameraPose->pose.yaw << "\t" << cameraPose->pose.pitch << "\t" << cameraPose->pose.roll << "\t" ;
					output << px << "\t" << py << "\t" << nx << "\t" << ny << "\t";
					output << prevP.at<double>(0,0) << "\t" << prevP.at<double>(0,1) << "\t" << prevP.at<double>(0,2) << "\t" << prevP.at<double>(0,3) << "\t";
					output << prevP.at<double>(1,0) << "\t" << prevP.at<double>(1,1) << "\t" << prevP.at<double>(1,2) << "\t" << prevP.at<double>(1,3) << "\t";
					output << prevP.at<double>(2,0) << "\t" << prevP.at<double>(2,1) << "\t" << prevP.at<double>(2,2) << "\t" << prevP.at<double>(2,3) << "\t";

					output << nextP.at<double>(0,0) << "\t" << nextP.at<double>(0,1) << "\t" << nextP.at<double>(0,2) << "\t" << nextP.at<double>(0,3) << "\t";
					output << nextP.at<double>(1,0) << "\t" << nextP.at<double>(1,1) << "\t" << nextP.at<double>(1,2) << "\t" << nextP.at<double>(1,3) << "\t";
					output << nextP.at<double>(2,0) << "\t" << nextP.at<double>(2,1) << "\t" << nextP.at<double>(2,2) << "\t" << nextP.at<double>(2,3) << "\t";
					output << A.at<double>(0,0) << "\t" << A.at<double>(0,1) << "\t" << A.at<double>(0,2) << "\t" << A.at<double>(0,3) << "\t";
					output << A.at<double>(1,0) << "\t" << A.at<double>(1,1) << "\t" << A.at<double>(1,2) << "\t" << A.at<double>(1,3) << "\t";
					output << A.at<double>(2,0) << "\t" << A.at<double>(2,1) << "\t" << A.at<double>(2,2) << "\t" << A.at<double>(2,3) << "\t";
					output << A.at<double>(3,0) << "\t" << A.at<double>(3,1) << "\t" << A.at<double>(3,2) << "\t" << A.at<double>(3,3) << "\t";
					output << ox << "\t" << oy << "\t" << oz << endl;
				}
			}
		}
			
		delete[] match_buf;
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
		if (timestamp != oldtimestamp && !cameraParam->K.empty() && !cameraParam->RT.empty() && oldtimestamp > 0 )//&& gotDisparity)
		{
			ClearScreen(false);
			double dt = timestamp-cameraPose->pose.timestamp;
			cout << "timestamp diff: " << dt << endl;
			//gotDisparity = false;
			nextP = cameraParam->P.clone();
			double posex = cameraPose->pose.x;
			double posey = cameraPose->pose.y;
			double poseyaw = (cameraPose->pose.yaw < 0)? cameraPose->pose.yaw+2.0*3.141592653589793:cameraPose->pose.yaw;
			int section = floor(poseyaw/(3.141592653589793*2.0/ANGLE_SECTION));
			// Start of SIFTGPU stuff
			if (1)
			{
				gotDisparity = false;
				if (sift->RunSIFT(resize->width, resize->height, resize->imageData, 0x80E0, 0x1401))
				{
					//get feature count
					num2 = sift->GetFeatureNum();
					//allocate memory
					keys2.resize(num2);    descriptors2.resize(128*num2);
					//reading back feature vectors is faster than writing files
					//if you dont need keys or descriptors, just put NULLs here
					sift->GetFeatureVector(&keys2[0], &descriptors2[0]);
				}

				double d = pow(posex-siftm[section].posex,2.0)+pow(posey-siftm[section].posey,2.0);
				double tempyaw1 = ((siftm[section].poseyaw > 3.141592653589793)?siftm[section].poseyaw - 3.141592653589793*2:siftm[section].poseyaw)+3.141592653589793;
				double tempyaw2 = ((poseyaw > 3.141592653589793)?poseyaw - 3.141592653589793*2:poseyaw)+3.141592653589793;
				double dyaw = tempyaw1-tempyaw2;
				if (d > 0.5 && abs(dyaw) < 0.05)
				{
					MatchSIFT(section);
					siftm[section].update = true;
				}
				// End of SIFTGPU stuff

			
				if (num2 > 0)
				{
					if (!siftm[section].goneOnce || siftm[section].update)
					{
						siftm[section].posex = posex;
						siftm[section].posey = posey;
						siftm[section].poseyaw = poseyaw;
						siftm[section].num = sift->GetFeatureNum();
						siftm[section].keys.resize(siftm[section].num);    siftm[section].descriptors.resize(128*siftm[section].num);
						sift->GetFeatureVector(&siftm[section].keys[0], &siftm[section].descriptors[0]);
						siftm[section].prevP = nextP.clone();
						siftm[section].update = false;
						siftm[section].goneOnce = true;
					}
				}
			}

			PlotOrigin(nextP);
			
			prevP = nextP.clone();
			//cvCopy(resizeBW,prevImg);
			// End of my stuff

			// Display current camera view
			RunCamera();			
		}
		

		int key = cvWaitKey (1);
		if (key=='q')
			running=false;			
		else if (key == 'c')
		{
			gotDisparity = true;
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

	// clean up..
    
    delete sift;
    delete matcher;
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
