#pragma once

#include <iostream>

#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2\calib3d\calib3d.hpp>
#include <opencv2/nonfree/nonfree.hpp>
#include <opencv2/flann/flann.hpp>
#include <opencv2/legacy/legacy.hpp>

class PaperRegistration
{	
public:
	PaperRegistration(void);
	~PaperRegistration(void);

	void detectLocation(cv::Mat cameraImage);

private:

	int PageIdx;
	std::string PageName;
	cv::Size LocationPx;
	cv::Size LocationCm;
	//angle in radian
	float RotationAngle; 

	cv::FREAK extractor;
	cv::flann::Index flannIdx;
	cv::Mat warpMat;
	
	cv::Mat imageWarp();
	cv::Mat computeLocalFeatures(cv::Mat &image, cv::vector<cv::KeyPoint> &pageKeyPoints);
	void drawMatch();
	
};

