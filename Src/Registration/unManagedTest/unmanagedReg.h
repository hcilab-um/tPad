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
	cv::Point2f getLocationPx();
	float getRotationAngle();

	void detectLocation(cv::Mat cameraImage);
	void createIndex(std::string dir_path, cv::vector<cv::Mat> &dbDescriptors, cv::vector<cv::vector<cv::KeyPoint>> &dbKeyPoints);

private:	
	int PageIdx;
	std::string PageName;
	cv::Point2f LocationPx;
	//angle in degree
	float RotationAngle; 

	cv::FlannBasedMatcher matcher;	
	cv::Mat warpMat;
	
	cv::Mat imageWarp();
	cv::Mat computeLocalFeatures(cv::Mat &image, cv::vector<cv::vector<cv::KeyPoint>> &pageKeyPoints);
	
	void getFiles(std::wstring directory, std::vector<std::string> &fileNameList);
	void drawMatch(cv::Mat *cameraImage, cv::Mat &homography);
};

