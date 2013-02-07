// Dies ist die Haupt-DLL.

#include "stdafx.h"

#include "unmanagedReg.h"

#include <Windows.h>
#include <iostream>

#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2\calib3d\calib3d.hpp>

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

	isSimulation_ = true;
	imgRatio_ = 1.0;

	detectorCamImg = new cv::SurfFeatureDetector(600,4,1, false);
	matcher = new cv::FlannBasedMatcher(new cv::flann::LshIndexParams(4, 25, 0));
	extractor = new cv::FREAK(true, false, 10.0F, 3);

	frame = new IplImage();
	lastDeviceImage = cv::Mat();
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

void paperRegistration::imageWarp(float imageRatio, bool isSim)
{
	isSimulation_ = isSim;
	imgRatio_ = imageRatio;

	if (!isSimulation_)
	{
		cv::Point2f srcPoint[4] = {cv::Point2f(10,10),cv::Point2f(155,10), cv::Point2f(10,292), cv::Point2f(166,292)};
		cv::Point2f destPoint[4] = {cv::Point2f(446,666), cv::Point2f(267,666), cv::Point2f(446,316), cv::Point2f(253,316)};		
		
		//compute homography matrix
		warpMat = cv::getPerspectiveTransform(srcPoint, destPoint);
	}
}

cv::Mat paperRegistration::computeLocalFeatures(cv::Mat &deviceImage)
{   
	std::vector<cv::KeyPoint> deviceKeypoints;
	cv::Mat deviceImageDescriptors;	

	detectorCamImg->detect(deviceImage, deviceKeypoints);
	extractor->compute(deviceImage, deviceKeypoints, deviceImageDescriptors);
	
	if (deviceImageDescriptors.rows > 4)
	{
		std::vector<std::vector<cv::DMatch>> dmatches;
		matcher->knnMatch(deviceImageDescriptors, dmatches, 2);

		std::vector<cv::Point2f> mpts_1, mpts_2; // Used for homography	
		std::vector<std::vector<cv::DMatch>>::iterator endIterator = dmatches.end();
		for (std::vector<std::vector<cv::DMatch>>::iterator iter = dmatches.begin(); iter != endIterator; ++iter)
		{
			std::vector<cv::DMatch>::iterator firstMatch = iter->begin();
			if(!iter->empty()) 
			{
				if (firstMatch->distance < 0.7*(++(iter->begin()))->distance)
				{
					mpts_1.push_back(deviceKeypoints[firstMatch->queryIdx].pt);		
					mpts_2.push_back(dbKeyPoints[firstMatch->imgIdx][firstMatch->trainIdx].pt);
					votingPageIndices[firstMatch->imgIdx]++;
				}								
			}
		}

		int max = 0;
		int PageIdx = -1;
		for (unsigned int i = 0; i < votingPageIndices.size(); ++i)
		{			
			if (votingPageIndices[i] > max)
			{
				max = votingPageIndices[i];
				PageIdx = i;
			}
		}

		if (mpts_1.size() >= 4)
			return findHomography(mpts_1, mpts_2, cv::RANSAC);
	}

	return cv::Mat();
}

void paperRegistration::drawMatch(cv::Mat &cameraImage, cv::Mat &homography, cv::Mat &pageImage)
{
	//cv::Mat pageImage = cv::imread("C:/Users/sophie/Desktop/Registration/unManagedTest/images/New folder/paper_page.png", CV_LOAD_IMAGE_GRAYSCALE);

	//draw detected region
	std::vector<cv::Point2f> device_corners(5);
	device_corners[0] = cvPoint(0,0);
	device_corners[1] = cvPoint(cameraImage.cols, 0 );
	device_corners[2] = cvPoint(cameraImage.cols, cameraImage.rows ); 
	device_corners[3] = cvPoint(0, cameraImage.rows );
	device_corners[4] = cvPoint(cameraImage.cols/2.0f, cameraImage.rows/2.0f );

	if (!homography.empty())
	{
		cv::perspectiveTransform(device_corners, device_corners, homography);		
		
		LocationPxTL = device_corners[0];
			LocationPxTR = device_corners[1];
			LocationPxBL = device_corners[2];
			LocationPxBR = device_corners[3];
			LocationPxM = device_corners[4];

		//-- Draw lines between the corners (the mapped object in the scene - image_2 )
		cv::line( pageImage, device_corners[0], device_corners[1] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[1], device_corners[2] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[2], device_corners[3] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[3] , device_corners[0] , cv::Scalar( 0, 255, 0), 4 );
	}

	cv::imshow( "Original", pageImage );
	cv::imshow( "frame", cameraImage );

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
	dbKeyPoints.clear();
	votingPageIndices.clear();

	cv::vector<cv::Mat> dbDescriptors;

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
		cv::SurfFeatureDetector detectorPageImg(2000, 4, 1, false);
		detectorPageImg.detect(pageImage, pageKeyPoints);

		cv::Mat pageImageDescriptors;
		extractor->compute(pageImage, pageKeyPoints, pageImageDescriptors);
	
		dbDescriptors.push_back(pageImageDescriptors);
		dbKeyPoints.push_back(pageKeyPoints);
	}

	//ToDo save FlannIDX
	/*cv::FlannBasedMatcher flannMatcher = new cv::flann::LshIndexParams(10, 30, 1);
	std::string sceneImageData = "sceneImagedatamodel.xml";
	cv::FileStorage fs(sceneImageData, cv::FileStorage::WRITE);
	flannMatcher.write(fs);*/
	
	for (int i=0; i < fileNameList.size(); i++)
		votingPageIndices.push_back(0);

	matcher = new cv::FlannBasedMatcher(new cv::flann::LshIndexParams(4,25, 0));
	matcher->add(dbDescriptors);
	matcher->train();
	
}

float paperRegistration::compareImages(cv::Mat &lastImg, cv::Mat &currentImg)
{
	cv::Scalar mean;
	cv::Mat diff;
	if (lastImg.size != currentImg.size)
		mean = 10;
	else {
		cv::subtract(lastImg, currentImg, diff);
		mean = cv::mean(diff);
	}

	return mean[0];
}

int paperRegistration::connectCamera()
{
	FlyCapture2::Error error;
	FlyCapture2::PGRGuid guid;
	FlyCapture2::BusManager busMgr;

	// Getting the GUID of the cam
	error = busMgr.GetCameraFromIndex(0, &guid);
	if (error != FlyCapture2::PGRERROR_OK)
	{
		error.PrintErrorTrace();
		return -1;
	}
	
	// Connect to a camera
	error = cam.Connect(&guid);
	if (error != FlyCapture2::PGRERROR_OK)
	{
		error.PrintErrorTrace();
		return -1;
	}
	
	// Starting the capture
	error = cam.StartCapture();
	if (error != FlyCapture2::PGRERROR_OK)
	{
		error.PrintErrorTrace();
		return -1;
	}
	
	// Get one raw image to be able to calculate the OpenCV window size
	cam.RetrieveBuffer(&rawImage);
	// Setting the window size in OpenCV
	frame = cvCreateImage(cv::Size(rawImage.GetCols(), rawImage.GetRows()), 8, 1);

	return 1;
}

int paperRegistration::disconnectCamera() 
{
	FlyCapture2::Error error;
	// Stop capturing images
    error = cam.StopCapture();
    if (error != FlyCapture2::PGRERROR_OK)
	{
		error.PrintErrorTrace();
		return -1;
	}
	
	//Disconnect the camera
    error = cam.Disconnect();
    if (error != FlyCapture2::PGRERROR_OK)
	{
		error.PrintErrorTrace();
		return -1;
	}

	return 1;
}

cv::Mat paperRegistration::loadCameraImage()
{
	FlyCapture2::Error error;

	// Start capturing images
	cam.RetrieveBuffer(&rawImage);
		
	// Get the raw image dimensions
	FlyCapture2::PixelFormat pixFormat;
	unsigned int rows, cols, stride;
	rawImage.GetDimensions( &rows, &cols, &stride, &pixFormat );
		
	// Create a converted image
	FlyCapture2::Image convertedImage;
		
	//Convert the raw image
	error = rawImage.Convert( FlyCapture2::PIXEL_FORMAT_MONO8, &convertedImage );
	if (error != FlyCapture2::PGRERROR_OK)
	{
		error.PrintErrorTrace();
		return cv::Mat();
	}
		
	// Copy the image into the Mat of OpenCV
	memcpy(frame->imageData, convertedImage.GetData(), convertedImage.GetDataSize());
		
	return frame;
}

int paperRegistration::detectLocation(cv::Mat &cameraImage)
{	
	return -1;
	//cv::resize(cameraImage, cameraImage, cv::Size(299,428));
	//cv::resize(lastImg, lastImg, cv::Size(299,428));
	//return 0;

	if (compareImages(cameraImage, lastDeviceImage) > 1.5)
	{
		lastDeviceImage = cameraImage;
		
		//toDo: load matcher	
		/*std::string sceneImageData = "sceneImagedatamodel.xml";
		cv::FileStorage fs(sceneImageData, cv::FileStorage::READ);
		cv::FileNode fn = fs.getFirstTopLevelNode(); 
		matcher.read(fn);*/
		
		//ToDo size in warp image
		if (isSimulation_)
		{
			cvtColor(cameraImage, cameraImage, CV_BGR2GRAY);
			//toDo: don't do rotation here
			warpMat = getRotationMatrix2D(cv::Point2f(cameraImage.cols/2.0, cameraImage.rows/2.0), 180, 1);
			cv::warpAffine(cameraImage, cameraImage, warpMat, cameraImage.size());
			cv::resize(cameraImage, cameraImage, cv::Size(cameraImage.cols*imgRatio_, cameraImage.rows*imgRatio_), 0, 0 ,cv::INTER_LINEAR);
			cv::imwrite("result.png", cameraImage);
		}
		else 
		{
			cameraImage = loadCameraImage();

			std::vector<cv::Point2f> point(2);
			point[0] = cvPoint(0,0);
			point[1] = cvPoint(cameraImage.cols,cameraImage.rows);
			
			cv::warpPerspective(cameraImage, cameraImage, warpMat, cv::Size(2500,3300));			
			cv::perspectiveTransform(point, point, warpMat);

			cameraImage = cv::Mat(cameraImage, cv::Rect(point[0], point[1]));
		}
		
		cv::Mat locationHM = computeLocalFeatures(cameraImage);
		
		//compute rotation angle (in degree)
		if (!locationHM.empty())
		{
			//locationHM = locationHM * warpMat;
			cv::Mat rotationMat, orthMat;
			cv::Vec3d eulerAngles;
			eulerAngles = cv::RQDecomp3x3(locationHM, rotationMat, orthMat);
			RotationAngle = eulerAngles[2];
			
			//compute location
			std::vector<cv::Point2f> device_point(5);
			device_point[0] = cvPoint(0,0);
			device_point[1] = cvPoint(cameraImage.cols,0);
			device_point[2] = cvPoint(0,cameraImage.rows);
			device_point[3] = cvPoint(cameraImage.cols,cameraImage.rows);
			device_point[4] = cvPoint(cameraImage.cols/2,cameraImage.rows/2);
			
			cv::perspectiveTransform(device_point, device_point, locationHM);

			LocationPxTL = device_point[0];
			LocationPxTR = device_point[1];
			LocationPxBL = device_point[2];
			LocationPxBR = device_point[3];
			LocationPxM = device_point[4];
			
			/*cv::Mat pageImage = cv::imread("C:/Users/Sophie/Documents/GitHub/tPad/Src/tPad/ActiveReader/bin/x64/Release/Document/17030000002000000.png", CV_LOAD_IMAGE_GRAYSCALE);

			if (LocationPxTL.x < 0)
				LocationPxTL.x = 0;
			else if (LocationPxTL.x > pageImage.cols)
				LocationPxTL.x = pageImage.cols;
			if (LocationPxTL.y < 0)
				LocationPxTL.y = 0;
			else if (LocationPxTL.y > pageImage.rows)
				LocationPxTL.y = pageImage.rows;
			if (LocationPxBR.x < 0)
				LocationPxBR.x = 0;
			else if (LocationPxBR.x > pageImage.cols)
				LocationPxBR.x = pageImage.cols;
			if (LocationPxBR.y < 0)
				LocationPxBR.y = 0;
			else if (LocationPxBR.y > pageImage.rows)
				LocationPxBR.y = pageImage.rows;
			cv::Mat result = cv::Mat(pageImage, cv::Rect(LocationPxTL, LocationPxBR));
			cv::imwrite("result.png", result);*/
			
			//drawMatch(&cameraImage, locationHM);
						
			return 1;
		}
		else return -1;
	}
	else return 0;
	
}