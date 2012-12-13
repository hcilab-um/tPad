// FeatureTracking.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"

#include <ctime>
#include <numeric>

#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2\calib3d\calib3d.hpp>
#include <opencv2/nonfree/nonfree.hpp>
#include <opencv2/flann/flann.hpp>
#include <opencv2/legacy/legacy.hpp>
#include <opencv2\ml\ml.hpp>

#include "PaperRegistration.h"

int _tmain(int argc, _TCHAR* argv[])
{
	//load page image
	cv::Mat deviceImage = cv::imread("images/test.JPG", CV_LOAD_IMAGE_GRAYSCALE);  
	
	PaperRegistration registrationObject;
	registrationObject.detectLocation(deviceImage);

	
	return 0;
}
