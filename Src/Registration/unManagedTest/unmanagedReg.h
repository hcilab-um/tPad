// unManagedTest.h
#if UNMANAGED_COMPILE_DEFINITION
#define EXPORT_OR_IMPORT __declspec(dllexport)
#else
#define EXPORT_OR_IMPORT __declspec(dllimport)
#endif

#pragma once

#include <string>
#include <opencv2/nonfree/nonfree.hpp>
#include <opencv2\highgui\highgui.hpp>

#include <FlyCapture2.h>

#include "FeatureMatcher.h"

//using namespace System;

class EXPORT_OR_IMPORT paperRegistration
{
public:
	paperRegistration(bool cameraInUse, float imageRatio, FeatureMatcher* matcher);
	~paperRegistration();

	int getPageIdx();
	std::string getPageName();
	cv::Point2f getLocationPxTL();
	cv::Point2f getLocationPxTR();
	cv::Point2f getLocationPxBL();
	cv::Point2f getLocationPxBR();
	cv::Point2f getLocationPxM();
	float getRotationAngle();

	int detectLocation(bool cameraInUse, int previousStatus);
	//int detectLocation(int previousStatus);

	void detectFigures(cv::vector<cv::vector<cv::Point> >& squares, cv::vector<cv::vector<cv::Point> >& triangles,
		float minLength = 25, float maxLength = 80, int tresh_binary = 105);

	//void createIndex(std::string dir_path);

	void imageWarp(float imageRatio);
	void imageWarp(std::string path);

	int connectCamera();
	int disconnectCamera();	
	void setCameraImg();
	void setCameraImg(cv::Mat &camImg);

private:	
	int PageIdx;
	cv::Point2f LocationPxTL, LocationPxTR, LocationPxBL, LocationPxBR, LocationPxM;
	//angle in degree
	float RotationAngle; 

	/*FlyCapture2::Camera cam;
	FlyCapture2::Image rawImage;
	IplImage *frame;*/
	cv::VideoCapture* cap;

	//cv::FlannBasedMatcher* matcher;
	cv::FlannBasedMatcher* fMatcher;
	cv::FREAK* extractor;
	cv::vector<cv::vector<cv::KeyPoint>> dbKeyPoints;
	cv::FastFeatureDetector* fastDetectorCamImg;
	//cv::FastFeatureDetector* fastDetectorPageImg;
	//cv::SurfFeatureDetector* surfDetectorPageImg;
	cv::SurfFeatureDetector* surfDetectorCamImg;

	bool isCameraInUse_;
	bool isCameraConnected;

	float imgRatio_;	

	cv::Mat lastDeviceImage, currentDeviceImg;
	cv::Mat warpMat;

	float computeArea(cv::Point2f pt0, cv::Point2f pt1, cv::Point2f pt2 );
	float computeAngle( cv::Point2f pt1, cv::Point2f pt2, cv::Point2f pt0 );
	float computeLength(cv::Point2f pt0, cv::Point2f pt1);

	cv::Mat computeLocalFeatures(cv::Mat &image);
	
	float compareImages(cv::Mat &lastImg, cv::Mat &currentImg);

	//void getFiles(std::wstring directory, std::vector<std::string> &fileNameList);
	
	void drawMatch(cv::Mat &cameraImage, cv::Mat &homography);
	
	cv::Mat loadCameraImage();
};

