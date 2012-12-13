#pragma once

#include <iostream>

#include <opencv2/nonfree/nonfree.hpp>

class PaperRegistration
{	
public:
	PaperRegistration(void);
	~PaperRegistration(void);

	void detectLocation(cv::Mat cameraImage);
	void createIndex(cv::vector<cv::Mat> &dbDescriptors, cv::vector<cv::vector<cv::KeyPoint>> &dbKeyPoints);

	int getPageIdx();
	std::string getPageName();
	cv::Size getLocationPx();
	cv::Size getLocationCm();
	float getRotationAngle();

private:

	int PageIdx;
	std::string PageName;
	cv::Size LocationPx;
	cv::Size LocationCm;
	//angle in radian
	float RotationAngle; 

	cv::FREAK extractor;
	cv::FlannBasedMatcher matcher;
	cv::Mat warpMat;
	
	cv::Mat imageWarp();
	cv::Mat computeLocalFeatures(cv::Mat &image, cv::vector<cv::vector<cv::KeyPoint>> &pageKeyPoints);
	void drawMatch(cv::Mat *cameraImage, cv::Mat &homography);
	
};

