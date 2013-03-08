// Dies ist die Haupt-DLL.

#include "stdafx.h"

#include "unmanagedReg.h"

#include <Windows.h>
#include <iostream>

#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2\calib3d\calib3d.hpp>

paperRegistration::paperRegistration(bool isCameraInUse, float imageRatio)
{
	PageIdx = -1;
	
	LocationPxTL = cv::Point2f(-1,-1);
	LocationPxTR = cv::Point2f(-1,-1);
	LocationPxBL = cv::Point2f(-1,-1);
	LocationPxBR = cv::Point2f(-1,-1);
	LocationPxM = cv::Point2f(-1,-1);
	RotationAngle = 0;	

	isCameraInUse_ = isCameraInUse;
	isCameraConnected = false;
	imgRatio_ = imageRatio;

	if (isCameraInUse_)
	{
		fastDetectorPageImg = new cv::FastFeatureDetector(145, true);
		fastDetectorCamImg = new cv::FastFeatureDetector(30, true);
		matcher = new cv::FlannBasedMatcher(new cv::flann::LshIndexParams(4, 21, 0));
		extractor = new cv::FREAK(true, false, 20.0F, 2);
	}	
	else 
	{
		surfDetectorPageImg = new cv::SurfFeatureDetector(2000, 4, 1, false);
		surfDetectorCamImg = new cv::SurfFeatureDetector(600,4, 1, false);
		matcher = new cv::FlannBasedMatcher(new cv::flann::LshIndexParams(4, 25, 0));
		extractor = new cv::FREAK(true, false, 10.0F, 3);		
	}

	lastDeviceImage = cv::Mat();
	currentDeviceImg = cv::Mat();
}

paperRegistration::~paperRegistration()
{
}

int paperRegistration::getPageIdx()
{
	return PageIdx;
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

void paperRegistration::setCameraImg()
{
	currentDeviceImg = loadCameraImage();
}

void paperRegistration::setCameraImg(cv::Mat &camImg)
{
	cvtColor(camImg, camImg, CV_BGR2GRAY);
	currentDeviceImg = camImg;
}

void paperRegistration::imageWarp(float imageRatio)
{
	imgRatio_ = imageRatio;
}

void paperRegistration::imageWarp(std::string path)
{
	warpMat = cv::Mat( 3, 3, CV_32FC1 );
	cv::FileStorage fw(path, cv::FileStorage::READ );
	fw["homography"] >> warpMat; 
	fw.release();
}

float paperRegistration::computeAngle( cv::Point2f pt1, cv::Point2f pt2, cv::Point2f pt0 )
{
	float slope1 = (pt0.y - pt2.y)/(pt0.x - pt2.x);
	float slope2 = (pt2.y - pt1.y)/(pt2.x - pt1.x);

	if (slope2*slope1 == -1)
		return 90.0f;
	else return (atan(fabs((slope2-slope1)/(1.0f+slope2*slope1))))*180/CV_PI;
}

float paperRegistration::computeLength(cv::Point2f pt0, cv::Point2f pt1)
{
	return sqrt((pt0.x-pt1.x)*(pt0.x-pt1.x)+(pt0.y-pt1.y)*(pt0.y-pt1.y));
}

float paperRegistration::computeArea(cv::Point2f pt0, cv::Point2f pt1, cv::Point2f pt2 )
{
	float a = sqrt((pt0.x-pt1.x)*(pt0.x-pt1.x)+(pt0.y-pt1.y)*(pt0.y-pt1.y));
	float b = sqrt((pt1.x-pt2.x)*(pt1.x-pt2.x)+(pt1.y-pt2.y)*(pt1.y-pt2.y));
	float c = sqrt((pt0.x-pt2.x)*(pt0.x-pt2.x)+(pt0.y-pt2.y)*(pt0.y-pt2.y));

	float perimeter = a+b+c;

	return sqrt(0.5*perimeter*(0.5*perimeter-a)*(0.5*perimeter-b)*(0.5*perimeter-c));
}

void paperRegistration::detectFigures(cv::vector<cv::vector<cv::Point>>& squares, cv::vector<cv::vector<cv::Point>>& triangles,
	float minLength, float maxLength, int tresh_binary)
{
	if (currentDeviceImg.empty())
		return;

	cv::Mat image = currentDeviceImg;
	//cv::Mat image = cv::imread("C:/Users/sophie/Desktop/meinz.png", CV_LOAD_IMAGE_GRAYSCALE);// cv::imread(path, CV_LOAD_IMAGE_GRAYSCALE);  
	//resize(image, image, cv::Size(500,700));

	squares.clear();  
	triangles.clear();	
	
	cv::Mat gray;
	cv::Mat element = getStructuringElement(cv::MORPH_ELLIPSE, cv::Size(7,7));
	cv::vector<cv::vector<cv::Point> > contours;

	//compute binary image
	//use dilatation and erosion to improve edges
	threshold(image, gray, tresh_binary, 255, cv::THRESH_BINARY_INV);	
	dilate(gray, gray, element, cv::Point(-1,-1));
	erode(gray, gray, element, cv::Point(-1,-1));
	
	// find contours and store them all as a list
	cv::findContours(gray, contours, CV_RETR_LIST, CV_CHAIN_APPROX_SIMPLE);
	
	//test each contour
	cv::vector<cv::Point> approx;
	cv::vector<cv::vector<cv::Point> >::iterator iterEnd = contours.end();
	for(cv::vector<cv::vector<cv::Point> >::iterator iter = contours.begin(); iter != iterEnd; ++iter)
	{
		// approximate contour with accuracy proportional
		// to the contour perimeter
		cv::approxPolyDP(*iter, approx, arcLength(*iter, true)*0.02, true);
	     
		//contours should be convex
		if (isContourConvex(approx))
		{
			// square contours should have 4 vertices after approximation and 
			// relatively large length (to filter out noisy contours)
			if( approx.size() == 4)
			{
				bool rectangular = true;	 
				for( int j = 3; j < 6; j++ )
				{
					// if cosines of all angles are small
					// (all angles are ~90 degree) then write
					// vertices to result
					 if (fabs(90 - fabs(computeAngle(approx[j%4], approx[j-3], approx[j-2]))) > 7)
					 {
						rectangular = false;
						break;
					 }
				}
				
				if (!rectangular)
					continue;
				
				float side1 = computeLength(approx[0], approx[1]);
				float side2 = computeLength(approx[1], approx[2]);
					
				if (side1 > minLength && side1 < maxLength && 
					side2 > minLength && side2 < maxLength)
					squares.push_back(approx);
			}		
			// triangle contours should have 3 vertices after approximation and 
			// relatively large length (to filter out noisy contours)
			else if ( approx.size() == 3)
			{
				float side1 = computeLength(approx[0], approx[1]);
				float side2 = computeLength(approx[1], approx[2]);
				float side3 = computeLength(approx[2], approx[0]);
				
				if (side1 > minLength && side1 < maxLength && 
					side2 > minLength && side2 < maxLength &&
					side3 > minLength && side3 < maxLength)
					triangles.push_back(approx);
			}
		}
	}
}

cv::Mat paperRegistration::computeLocalFeatures(cv::Mat &deviceImage)
{   
	std::vector<cv::KeyPoint> deviceKeypoints;
	cv::Mat deviceImageDescriptors;	

	if (isCameraInUse_)
		fastDetectorCamImg->detect(deviceImage, deviceKeypoints);
	else surfDetectorCamImg->detect(deviceImage, deviceKeypoints);
	extractor->compute(deviceImage, deviceKeypoints, deviceImageDescriptors);
	
	if (deviceImageDescriptors.rows > 4)
	{
		std::vector<std::vector<cv::DMatch>> dmatches;
		matcher->knnMatch(deviceImageDescriptors, dmatches, 2);

		//set votingPageIndices to null
		for (int i =0; i < votingPageIndices.size(); i++)
			votingPageIndices[i] = 0;

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
		PageIdx = -1;
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
		if (isCameraInUse_)
			fastDetectorPageImg->detect(pageImage, pageKeyPoints);
		else surfDetectorPageImg->detect(pageImage, pageKeyPoints);

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
	
	int count = dbKeyPoints.size();
	for (int i = 0; i < count; i++)
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
	
	//set video mode
	error = cam.SetVideoModeAndFrameRate(FlyCapture2::VIDEOMODE_640x480Y8, FlyCapture2::FRAMERATE_30);
	if (error != FlyCapture2::PGRERROR_OK)
	{
		error.PrintErrorTrace();
		return -1;
	}

	//set brightness
	FlyCapture2::Property prop;
	prop.type = FlyCapture2::BRIGHTNESS;
	prop.valueA = 480;
	error = cam.SetProperty(&prop);
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

	isCameraConnected = true;
	return 1;
}

int paperRegistration::disconnectCamera() 
{
	if (!isCameraConnected)
		return 1;
	isCameraConnected = false;

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
	if (!isCameraConnected)
		return cv::Mat();

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

int paperRegistration::detectLocation(bool cameraInUse, int previousStatus)
{
	cv::Mat cameraImage = currentDeviceImg;
	if (cameraImage.empty())
		return -1;

	if (previousStatus != 1 || compareImages(cameraImage, lastDeviceImage) > 1.5)
	{
		lastDeviceImage = cameraImage.clone();
				
		if (cameraInUse)
		{
			std::vector<cv::Point2f> point(2);
			point[0] = cvPoint(0,0);
			point[1] = cvPoint(cameraImage.cols,cameraImage.rows);
		
			if (!warpMat.empty())
			{
				cv::warpPerspective(cameraImage, cameraImage, warpMat, cv::Size(1000,2000));			
				cv::perspectiveTransform(point, point, warpMat);
			}
			cameraImage = cv::Mat(cameraImage, cv::Rect(point[0], point[1]));
		
			cv::Mat blurrImg;
			cv::GaussianBlur(cameraImage, blurrImg, cv::Size(5,5), 3);		
			cv::addWeighted(cameraImage, 2.3, blurrImg, -0.5, 0, cameraImage);
			cv::addWeighted(cameraImage, 1.5, blurrImg, -0.5, 0, cameraImage);
		}
		else
		{			
			warpMat = getRotationMatrix2D(cv::Point2f(cameraImage.cols/2.0, cameraImage.rows/2.0), 180, 1);
			cv::warpAffine(cameraImage, cameraImage, warpMat, cameraImage.size());
			cv::resize(cameraImage, cameraImage, cv::Size(cameraImage.cols*imgRatio_, cameraImage.rows*imgRatio_), 0, 0 ,cv::INTER_LINEAR);
		}

		cv::Mat locationHM = computeLocalFeatures(cameraImage);
		
		//compute rotation angle (in degree)
		if (!locationHM.empty())
		{
			//compute location
			std::vector<cv::Point2f> device_point(5);
			device_point[0] = cvPoint(0,0);
			device_point[1] = cvPoint(cameraImage.cols,0);
			device_point[2] = cvPoint(cameraImage.cols,cameraImage.rows);
			device_point[3] = cvPoint(0,cameraImage.rows);
			device_point[4] = cvPoint(cameraImage.cols/2,cameraImage.rows/2);

			float areaCamImg = computeArea(device_point[0], device_point[1], device_point[3]) * computeArea(device_point[1], device_point[2], device_point[3]);
			
			cv::perspectiveTransform(device_point, device_point, locationHM);
			
			//proof validity of result
			//3 angles must be around 90 degree
			for( int j = 3; j < 6; j++ )
			{
				float angleCorner = computeAngle(device_point[j%4], device_point[j-3], device_point[j-2]);
				if (fabs(90-angleCorner) > 7)
					return -1;
			}
			
			float areaDetectedImg = computeArea(device_point[0], device_point[1], device_point[3]) * computeArea(device_point[1], device_point[2], device_point[3]);
			//proof size of detected area
			if (fabs(areaDetectedImg-areaCamImg) >= areaCamImg * 0.1)
				return -1;

			cv::Mat rotationMat, orthMat;
			cv::Vec3d eulerAngles;
			eulerAngles = cv::RQDecomp3x3(locationHM, rotationMat, orthMat);
			RotationAngle = eulerAngles[2];

			LocationPxTL = device_point[0];
			LocationPxTR = device_point[1];
			LocationPxBR = device_point[2];
			LocationPxBL = device_point[3];
			LocationPxM = device_point[4];
									
			//drawMatch(&cameraImage, locationHM);
						
			return 1;
		}
		else return -1;
	}
	else return 0;	//new image is similiar to previous one
}

//int paperRegistration::detectLocation(cv::Mat &cameraImg, int previousStatus)
//{
//	if (previousStatus != 1 || compareImages(cameraImg, lastDeviceImage) > 1.5)
//	{
//		lastDeviceImage = cameraImg.clone();
//		
//		//toDo: load matcher	
//		/*std::string sceneImageData = "sceneImagedatamodel.xml";
//		cv::FileStorage fs(sceneImageData, cv::FileStorage::READ);
//		cv::FileNode fn = fs.getFirstTopLevelNode(); 
//		matcher.read(fn);*/
//		
//		cvtColor(cameraImg, cameraImg, CV_BGR2GRAY);
//		warpMat = getRotationMatrix2D(cv::Point2f(cameraImg.cols/2.0, cameraImg.rows/2.0), 180, 1);
//		cv::warpAffine(cameraImg, cameraImg, warpMat, cameraImg.size());
//		cv::resize(cameraImg, cameraImg, cv::Size(cameraImg.cols*imgRatio_, cameraImg.rows*imgRatio_), 0, 0 ,cv::INTER_LINEAR);
//		
//		cv::Mat locationHM = computeLocalFeatures(cameraImg);
//		
//		//compute rotation angle (in degree)
//		if (!locationHM.empty())
//		{
//			//compute location
//			std::vector<cv::Point2f> device_point(5);
//			device_point[0] = cvPoint(0,0);
//			device_point[1] = cvPoint(cameraImg.cols,0);			
//			device_point[2] = cvPoint(cameraImg.cols,cameraImg.rows);
//			device_point[3] = cvPoint(0,cameraImg.rows);
//			device_point[4] = cvPoint(cameraImg.cols/2,cameraImg.rows/2);
//
//			float areaCamImg = computeArea(device_point[0], device_point[1], device_point[3]) * computeArea(device_point[1], device_point[2], device_point[3]);
//			
//			cv::perspectiveTransform(device_point, device_point, locationHM);
//			cv::imwrite("test.png", cameraImg);
//			//proof validity of result
//			//3 angles must be around 90 degree
//			for( int j = 3; j < 6; j++ )
//			{
//				float angleCorner = computeAngle(device_point[j%4], device_point[j-3], device_point[j-2]);
//				if (fabs(90-angleCorner) > 7)
//					return -1;
//			}
//			
//			float areaDetectedImg = computeArea(device_point[0], device_point[1], device_point[3]) * computeArea(device_point[1], device_point[2], device_point[3]);
//			//proof size of detected area
//			if (fabs(areaDetectedImg-areaCamImg) > areaCamImg * 0.1)
//				return -1;
//
//			cv::Mat rotationMat, orthMat;
//			cv::Vec3d eulerAngles;
//			eulerAngles = cv::RQDecomp3x3(locationHM, rotationMat, orthMat);
//			RotationAngle = eulerAngles[2];
//
//			LocationPxTL = device_point[0];
//			LocationPxTR = device_point[1];
//			LocationPxBR = device_point[2];
//			LocationPxBL = device_point[3];
//			LocationPxM = device_point[4];
//									
//			//drawMatch(&cameraImage, locationHM);
//						
//			return 1;
//		}
//		else return -1;
//	}
//	else return 0;	//new image is similiar to previous one
//}