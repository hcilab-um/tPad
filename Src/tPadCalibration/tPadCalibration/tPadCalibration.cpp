// tPadCalibration.cpp : Defines the entry point for the console application.

#include "stdafx.h"

#include <iostream>

#include "opencv2/opencv.hpp"
#include <opencv2\calib3d\calib3d.hpp>
#include <opencv2\contrib\contrib.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2\calib3d\calib3d.hpp>
//#include <FlyCapture2.h>
//
//FlyCapture2::Camera cam;
//FlyCapture2::Image rawImage;
//IplImage *frame;
cv::VideoCapture* cap;
bool isCameraConnected = false;

int connectCamera()
{
	//FlyCapture2::Error error;
	//FlyCapture2::PGRGuid guid;
	//FlyCapture2::BusManager busMgr;

	//// Getting the GUID of the cam
	//error = busMgr.GetCameraFromIndex(0, &guid);
	//if (error != FlyCapture2::PGRERROR_OK)
	//{
	//	error.PrintErrorTrace();
	//	return -1;
	//}
	//
	//// Connect to a camera
	//error = cam.Connect(&guid);
	//if (error != FlyCapture2::PGRERROR_OK)
	//{
	//	error.PrintErrorTrace();
	//	return -1;
	//}
	//
	////set video mode
	//error = cam.SetVideoModeAndFrameRate(FlyCapture2::VIDEOMODE_640x480Y8, FlyCapture2::FRAMERATE_30);
	//if (error != FlyCapture2::PGRERROR_OK)
	//{
	//	error.PrintErrorTrace();
	//	return -1;
	//}

	////set brightness
	//FlyCapture2::Property prop;
	//prop.type = FlyCapture2::BRIGHTNESS;
	//prop.valueA = 480;
	//error = cam.SetProperty(&prop);
	//if (error != FlyCapture2::PGRERROR_OK)
	//{
	//	error.PrintErrorTrace();
	//	return -1;
	//}

	//// Starting the capture
	//error = cam.StartCapture();
	//if (error != FlyCapture2::PGRERROR_OK)
	//{
	//	error.PrintErrorTrace();
	//	return -1;
	//}
	//

	//// Get one raw image to be able to calculate the OpenCV window size
	//cam.RetrieveBuffer(&rawImage);
	//// Setting the window size in OpenCV
	//frame = cvCreateImage(cv::Size(rawImage.GetCols(), rawImage.GetRows()), 8, 1);

	//isCameraConnected = true;
	cap = new cv::VideoCapture(CV_CAP_ANY);

	cap->set(CV_CAP_PROP_FRAME_HEIGHT, 360);
	cap->set(CV_CAP_PROP_FRAME_WIDTH, 640);
	cap->set(CV_CAP_PROP_BRIGHTNESS, 180);
	cap->set(CV_CAP_PROP_CONTRAST, 3);
	cap->set(CV_CAP_PROP_FOCUS, 14);
	cap->set(CV_CAP_PROP_SATURATION, 0);
	
	if (!cap->isOpened())
			return -1;

	return 1;
}

int disconnectCamera() 
{
	//if (!isCameraConnected)
	//	return 1;
	//isCameraConnected = false;

	//FlyCapture2::Error error;
	//// Stop capturing images
 //   error = cam.StopCapture();
 //   if (error != FlyCapture2::PGRERROR_OK)
	//{
	//	error.PrintErrorTrace();
	//	return -1;
	//}
	//
	////Disconnect the camera
 //   error = cam.Disconnect();
 //   if (error != FlyCapture2::PGRERROR_OK)
	//{
	//	error.PrintErrorTrace();
	//	return -1;
	//}
	cap->release();

	return 1;
}

cv::Mat loadCameraImage()
{
	//if (!isCameraConnected)
	//	return cv::Mat();

	//FlyCapture2::Error error;

	//// Start capturing images
	//cam.RetrieveBuffer(&rawImage);
	//	
	//// Get the raw image dimensions
	//FlyCapture2::PixelFormat pixFormat;
	//unsigned int rows, cols, stride;
	//rawImage.GetDimensions( &rows, &cols, &stride, &pixFormat );
	//	
	//// Create a converted image
	//FlyCapture2::Image convertedImage;
	//	
	////Convert the raw image
	//error = rawImage.Convert( FlyCapture2::PIXEL_FORMAT_MONO8, &convertedImage );
	//if (error != FlyCapture2::PGRERROR_OK)
	//{
	//	error.PrintErrorTrace();
	//	return cv::Mat();
	//}
	//	
	//// Copy the image into the Mat of OpenCV
	//memcpy(frame->imageData, convertedImage.GetData(), convertedImage.GetDataSize());
	
	cv::Mat cameraImage;
	if (cap->isOpened())
	{
		*cap >> cameraImage;
		cv::cvtColor(cameraImage, cameraImage, CV_RGB2GRAY);	
		return cameraImage;
	}

	return cv::Mat();
}

int _tmain(int argc, _TCHAR* argv[])
{
	//load page image
	cv::Mat pageImage = cv::imread("CalibrationPattern.png", CV_LOAD_IMAGE_GRAYSCALE);
	cv::vector<cv::Point2f> cornersPage;
	cv::findChessboardCorners(pageImage, cv::Size(8,6), cornersPage);
	
	char key;
	std::cout << "=== Camera Calibration === \n" << std::endl;
	std::cout << "\nPlease connect the camera to the tPad, print the calibration pattern and place it below the tPad. \nStart camera now? [y/n]" << std::endl;
	std::cin >> key;
	
	if (key != 'y')
		return 0;
	
	for (;;)
	{
		std::cout << "Please place the calibration pattern below the tPad and press enter when the camera captures the whole calibration pattern. \nThe pattern's edges should be aligned parallel to the device's edges." << std::endl;
		if (connectCamera() == -1)
			return -1;
		cv::Mat cameraImage;
		for (;;)
		{
			cameraImage = loadCameraImage();// cv::imread("cameraImg.png", CV_LOAD_IMAGE_GRAYSCALE); /
			cv::imshow( "Camera", cameraImage );

			if ( cvWaitKey(1) == 13 )
			{
				cv::destroyAllWindows();
				break;
			}

			if ( cvWaitKey(1) == 27 )
			{
				disconnectCamera();
				return 0;
			}
		}
		if (disconnectCamera() == -1)
			return -1;

		std::cout << "Warping Matrix is computed ..." << std::endl;
		cv::Mat imgBinary;
		cv::threshold(cameraImage, imgBinary, 130,255, CV_THRESH_BINARY);
		//cv::imshow("thresh", imgBinary);

		cv::vector<cv::Point2f> cornersCamera;
		cv::findChessboardCorners(imgBinary, cv::Size(8,6), cornersCamera);
		//cv::drawChessboardCorners(imgBinary, cv::Size(8,6), cornersCamera, true);
	
		if (cornersCamera.size() == 8*6)
		{
			cv::Mat homography = cv::Mat( 3, 3, CV_32FC1 );
			homography = cv::findHomography(cornersCamera, cornersPage);

			cv::FileStorage fs("homography.xml", cv::FileStorage::WRITE );
			fs << "homography" << homography; 
			fs.release();

			std::cout << "Warping Matrix is saved." << std::endl;
			std::cout << "Please check whether the calibration was succesful or not. \n[Close the images and enter 'y' or 'n'.]" << std::endl;

			cv::warpPerspective(cameraImage, cameraImage, homography, pageImage.size());

			cv::resize(cameraImage, imgBinary, cv::Size(cameraImage.cols * 0.5, cameraImage.rows * 0.5));			
			imshow( "Result", imgBinary);
			cv::resize(pageImage, pageImage, cv::Size(pageImage.cols * 0.5, pageImage.rows * 0.5));
			imshow( "Original", pageImage);	
			cvWaitKey(0);
			cvDestroyAllWindows();

			std::cout << "Is the calibration pattern aligned correctly? [y/n]." << std::endl;
			std::cin >> key;

			if (key == 'y')
				break;
			else 
			{
				std::cout << "Press Escape to close the program. \n\n" <<std::endl;
				cvDestroyAllWindows();
				continue;
			}
		}
		else 
		{
			std::cout << "\nComputation of warpingwhich squarecsdcdsvc matrix failed! Please try again.\n" <<std::endl;
			std::cout << "Press Escape to close the program. \n\n" <<std::endl;
			continue;
		}		
	}
	return 0;
}
