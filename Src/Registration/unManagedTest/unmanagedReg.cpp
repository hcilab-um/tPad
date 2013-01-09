// Dies ist die Haupt-DLL.

#include "stdafx.h"

#include "unmanagedReg.h"

#include <Windows.h>
#include <iostream>

#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2\calib3d\calib3d.hpp>


//#include <ctime>

paperRegistration::paperRegistration()
{
	/*cv::Point2f point;
	point.x = 2;
	randomValue = 7.0 + (double) RotationAngle;
	changeRandomVariable();*/
	PageIdx = -1;
	PageName = "default";

	LocationPxTL = cv::Point2f(-1,-1);
	LocationPxTR = cv::Point2f(-1,-1);
	LocationPxBL = cv::Point2f(-1,-1);
	LocationPxBR = cv::Point2f(-1,-1);
	LocationPxM = cv::Point2f(-1,-1);
	RotationAngle = 0;	
}

paperRegistration::~paperRegistration()
{
}

int paperRegistration::getPageIdx()
{
	return PageIdx;
}

std::string paperRegistration::getPageName()
{
	return PageName;
}

cv::Point2f paperRegistration::getLocationPxTL()
{
	return LocationPxTL;
}

cv::Point2f paperRegistration::getLocationPxM()
{
	return LocationPxM;
}

cv::Point2f paperRegistration::getLocationPxTR()
{
	return LocationPxTR;
}

cv::Point2f paperRegistration::getLocationPxBL()
{
	return LocationPxBL;
}

cv::Point2f paperRegistration::getLocationPxBR()
{
	return LocationPxBR;
}

float paperRegistration::getRotationAngle()
{
	return RotationAngle;
}

cv::Mat paperRegistration::imageWarp()
{
	cv::Point2f srcPoint[4] = {cv::Point2f(10,91),cv::Point2f(184,77), cv::Point2f(10,365), cv::Point2f(161,364)};
	cv::Point2f destPoint[4] = {cv::Point2f(386,664), cv::Point2f(163,681), cv::Point2f(387,312), cv::Point2f(193,313)};	

	cv::Mat homography = cv::getPerspectiveTransform(srcPoint, destPoint);
		
	return homography;
}

cv::Mat paperRegistration::computeLocalFeatures(cv::Mat &cameraImage, cv::vector<cv::vector<cv::KeyPoint>> &dbKeyPoints)
{   
	std::vector<cv::KeyPoint> deviceKeypoints;
	cv::FAST(cameraImage, deviceKeypoints, 30, true);

	cv::Mat deviceImageDescriptors;	
	cv::FREAK extractor;
	extractor.compute(cameraImage, deviceKeypoints, deviceImageDescriptors);
	
	if (deviceImageDescriptors.rows > 4)
	{
		std::vector<std::vector<cv::DMatch>> dmatches;
		matcher.knnMatch(deviceImageDescriptors, dmatches, 2);
			
		std::vector<cv::Point2f> mpts_1, mpts_2; // Used for homography	
		bool PageIdxIsSet = false;
		for(unsigned int i=0; i<(unsigned int)dmatches.size(); ++i)
		{
			if(!dmatches[i].empty()) 
			{
				if (dmatches[i][0].distance < 0.7*dmatches[i][1].distance)
				{
					mpts_1.push_back(deviceKeypoints[dmatches[i][0].queryIdx].pt);		
					mpts_2.push_back(dbKeyPoints[dmatches[i][0].imgIdx][dmatches[i][0].trainIdx].pt);
				}	

				if (PageIdxIsSet != true)
				{
					PageIdx = dmatches[0][0].imgIdx;
					PageIdxIsSet = true;
				}
			}
		}

		std::vector<uchar> mask;
		if (mpts_1.size() >= 4)
		{
			cv::Mat loc = findHomography(mpts_1, mpts_2, CV_RANSAC, 3, mask);
			return loc;
		}
	}
	return cv::Mat();
}

void paperRegistration::drawMatch(cv::Mat *cameraImage, cv::Mat &homography)
{
	cv::Mat pageImage = cv::imread("C:/Users/sophie/Documents/GitHub/tPad/Src/Registration/unManagedTest/images/New folder/paper_page.png", CV_LOAD_IMAGE_GRAYSCALE);

	//draw detected region
	std::vector<cv::Point2f> device_corners(4);
	device_corners[0] = cvPoint(0,0);
	device_corners[1] = cvPoint(cameraImage->cols, 0 );
	device_corners[2] = cvPoint(cameraImage->cols, cameraImage->rows ); 
	device_corners[3] = cvPoint(0, cameraImage->rows );
				
	if (!homography.empty())
	{
		cv::perspectiveTransform(device_corners, device_corners, homography);		
		
		//-- Draw lines between the corners (the mapped object in the scene - image_2 )
		cv::line( pageImage, device_corners[0], device_corners[1] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[1], device_corners[2] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[2], device_corners[3] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[3] , device_corners[0] , cv::Scalar( 0, 255, 0), 4 );
	}

	cv::imshow( "Original", pageImage );
	cv::imshow( "frame", *cameraImage );

	cv::waitKey(0);
}


void paperRegistration::getFiles(std::wstring directory, std::vector<std::string> &fileNameList)
{
	HANDLE handle;
	WIN32_FIND_DATA finddata; 

	handle = FindFirstFile(directory.c_str(),&finddata);
	FindNextFile(handle,&finddata);
	while (FindNextFile(handle,&finddata))
	{
		std::wstring fileName = finddata.cFileName;
		std::string str( fileName.begin(), fileName.end() );
		fileNameList.push_back(str);
	}
	FindClose(handle);
}

void paperRegistration::createIndex(std::string dir_path)
{
	cv::vector<cv::Mat> dbDescriptors;
	cv::FREAK extractor;

	//load pages
	std::string path = dir_path + "\\*";
	std::wstring wsTmp(path.begin(), path.end());
	std::wstring ws = wsTmp;
	std::vector<std::string> fileNameList;
	getFiles(ws, fileNameList);

	for (unsigned int i = 0; i < fileNameList.size(); i++)
	{		
		std::string imagePath = dir_path + "/" + fileNameList[i];
		cv::Mat pageImage = cv::imread(imagePath, CV_LOAD_IMAGE_GRAYSCALE);

		cv::vector<cv::KeyPoint> pageKeyPoints;
		cv::Mat pageImageDescriptors;
		cv::FAST(pageImage, pageKeyPoints, 100, true);
		extractor.compute(pageImage, pageKeyPoints, pageImageDescriptors);
	
		dbDescriptors.push_back(pageImageDescriptors);
		dbKeyPoints.push_back(pageKeyPoints);
	}
	//ToDo save FlannIDX
	/*cv::FlannBasedMatcher flannMatcher = new cv::flann::LshIndexParams(10, 30, 1);
	std::string sceneImageData = "sceneImagedatamodel.xml";
	cv::FileStorage fs(sceneImageData, cv::FileStorage::WRITE);

	flannMatcher.write(fs);*/

	matcher = cv::FlannBasedMatcher(new cv::flann::LshIndexParams(10, 30, 1));
	matcher.add(dbDescriptors);
	matcher.train();
}


float paperRegistration::compareImages(cv::Mat &lastImg, cv::Mat &currentImg)
{
	/*int histSize = 1;
	float range[] = {0, 256};
	const float* histRange = {range};
	bool uniform = true;
	bool accumulate = false;
	cv::Mat a1_hist, a2_hist;
	
	cv::calcHist(&lastImg, 1, 0, cv::Mat(), a1_hist, 1, &histSize, &histRange, uniform, accumulate );
	cv::calcHist(&currentImg, 1, 0, cv::Mat(), a2_hist, 1, &histSize, &histRange, uniform, accumulate );
	
	float compar_c = cv::compareHist(a1_hist, a2_hist, CV_COMP_CORREL);*/
	cv::Scalar mean;
	cv::Mat diff;
	if (lastImg.size != currentImg.size)
		mean = 1;
	else {
		cv::subtract(lastImg, currentImg, diff);
		mean = cv::mean(diff);
	}

	return mean[0];
}

int paperRegistration::detectLocation(cv::Mat &cameraImage, cv::Mat &lastImg)
{
	//conert to grey scale image
	cvtColor(cameraImage, cameraImage, CV_BGR2GRAY);
	cvtColor(lastImg, lastImg, CV_BGR2GRAY);
	
	if (compareImages(cameraImage, lastImg) > 1.5)
	{
		//int begin = clock();	
		//cv::vector<cv::Mat> dbDescriptors;
		//createIndex("C:/Users/sophie/Documents/GitHub/tPad/Src/Registration/unManagedTest/images/New folder", dbDescriptors, dbKeyPoints);
	
		//toDo: load matcher	
		/*std::string sceneImageData = "sceneImagedatamodel.xml";
		cv::FileStorage fs(sceneImageData, cv::FileStorage::READ);
		cv::FileNode fn = fs.getFirstTopLevelNode(); 
		matcher.read(fn);*/
	
		//int end = clock();
	
		//ToDo load warpImage
		cv::Mat warpedImage;
		cv::warpPerspective(cameraImage, warpedImage, warpMat, cv::Size(815,1204));
		std::vector<cv::Point2f> point(2);
		point[0] = cvPoint(0,0);
		point[1] = cvPoint(cameraImage.cols,cameraImage.rows);
		cv::perspectiveTransform(point, point, warpMat);	
		cameraImage = cv::Mat(warpedImage, cv::Rect(point[0], point[1]));
		warpedImage.release();
		cv::Mat locationHM = computeLocalFeatures(cameraImage, dbKeyPoints);
	
		//compute rotation angle (in degree)
		if (!locationHM.empty())
		{
			cv::Mat rotationMat, orthMat;
			cv::Vec3d eulerAngles;
			eulerAngles = cv::RQDecomp3x3(locationHM, rotationMat, orthMat);
			RotationAngle = eulerAngles[2];

			//compute location
			//ToDo: use Center of device instead of top left corner
			std::vector<cv::Point2f> device_point(5);
			device_point[0] = cvPoint(0,0);
			device_point[1] = cvPoint(cameraImage.cols,0);
			device_point[2] = cvPoint(0,cameraImage.rows);
			device_point[3] = cvPoint(cameraImage.cols,cameraImage.rows);
			device_point[4] = cvPoint(cameraImage.cols/2.0f,cameraImage.rows/2.0f);
			cv::perspectiveTransform(device_point, device_point, locationHM);	
			LocationPxTL = device_point[0];
			LocationPxTR = device_point[1];
			LocationPxBL = device_point[2];
			LocationPxBR = device_point[3];
			LocationPxM = device_point[4];

			return 1;
		}
		else return 0;
	}
	//drawMatch(&cameraImage, locationHM);
}