// unManagedTest.h
#if UNMANAGED_COMPILE_DEFINITION
#define EXPORT_OR_IMPORT __declspec(dllexport)
#else
#define EXPORT_OR_IMPORT __declspec(dllimport)
#endif

#pragma once

#include <string>
#include <opencv2/nonfree/nonfree.hpp>

//using namespace System;

class EXPORT_OR_IMPORT paperRegistration
{
public:
	paperRegistration();
	~paperRegistration();

	int getPageIdx();
	std::string getPageName();
	cv::Point2f getLocationPxTL();
	cv::Point2f getLocationPxTR();
	cv::Point2f getLocationPxBL();
	cv::Point2f getLocationPxBR();
	cv::Point2f getLocationPxM();
	float getRotationAngle();

	int detectLocation(cv::Mat &currentImg, cv::Mat &lastImg);
	void createIndex(std::string dir_path);

private:	
	int PageIdx;
	std::string PageName;
	cv::Point2f LocationPxTL, LocationPxTR, LocationPxBL, LocationPxBR, LocationPxM;
	//angle in degree
	float RotationAngle; 

	cv::FlannBasedMatcher matcher;	
	cv::Mat warpMat;
	cv::vector<cv::vector<cv::KeyPoint>> dbKeyPoints;
	
	cv::Mat imageWarp();
	cv::Mat computeLocalFeatures(cv::Mat &image, cv::vector<cv::vector<cv::KeyPoint>> &pageKeyPoints);
	float compareImages(cv::Mat &lastImg, cv::Mat &currentImg);

	void getFiles(std::wstring directory, std::vector<std::string> &fileNameList);
	void drawMatch(cv::Mat *cameraImage, cv::Mat &homography);
};

