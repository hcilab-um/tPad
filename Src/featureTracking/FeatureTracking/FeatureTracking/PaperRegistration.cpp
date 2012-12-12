#include "stdafx.h"
#include "PaperRegistration.h"

PaperRegistration::PaperRegistration(void)
{
	PageIdx = -1;
	PageName = "default";

	LocationPx = cv::Size(-1,-1);
	LocationCm = cv::Size(-1,-1);

	RotationAngle = 0;		
}

PaperRegistration::~PaperRegistration(void)
{
}

cv::Mat PaperRegistration::imageWarp()
{
	cv::Point2f srcPoint[4] = {cv::Point2f(240, 242),cv::Point2f(132, 730), cv::Point2f(998, 241), cv::Point2f(1128,653)};
	cv::Point2f destPoint[4] = {cv::Point2f(476,577), cv::Point2f(465,720), cv::Point2f(594,577), cv::Point2f(600,701)};	

	cv::Mat homography = cv::getPerspectiveTransform(srcPoint, destPoint);
		
	return homography;
}

cv::Mat PaperRegistration::computeLocalFeatures(cv::Mat &cameraImage, cv::vector<cv::KeyPoint> &pageKeyPoints)
{   
	std::vector<cv::KeyPoint> deviceKeypoints;
	cv::FAST(cameraImage, deviceKeypoints, 30, true);

	cv::Mat deviceImageDescriptors;	
	extractor.compute(cameraImage, deviceKeypoints, deviceImageDescriptors);
	
	if (deviceImageDescriptors.rows > 4)
	{
		cv::Mat indices, dist;
		flannIdx.knnSearch(deviceImageDescriptors, indices, dist, 2, cv::flann::SearchParams(32));
			
		std::vector<cv::Point2f> mpts_1, mpts_2;
		for (unsigned int i=0; i<(unsigned int)deviceImageDescriptors.rows; ++i)
		{
			if(dist.at<float>(i,0) < 0.7*(dist.at<float>(i,1)))
			{
				mpts_1.push_back(deviceKeypoints.at(i).pt);		
				mpts_2.push_back(pageKeyPoints.at(indices.at<int>(i,0)).pt);
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

void PaperRegistration::drawMatch()
{
}

void PaperRegistration::detectLocation(cv::Mat cameraImage)
{	
	cv::Mat pageImage = cv::imread("images/Usability Engineering for Augmented Reality.png", CV_LOAD_IMAGE_GRAYSCALE);
	
	cv::vector<cv::KeyPoint> pageKeyPoints;
	cv::Mat pageImageDescriptors;
	//first page
	cv::FAST(pageImage, pageKeyPoints, 100, true);
	extractor.compute(pageImage, pageKeyPoints, pageImageDescriptors);
	//second image

	//compute flann index
	cv::flann::IndexParams idx = *new cv::flann::LshIndexParams(10, 30, 1);	
	flannIdx = *new cv::flann::Index(pageImageDescriptors, idx, cvflann::FLANN_DIST_HAMMING);
	
	cv::warpPerspective(cameraImage, cameraImage, warpMat, cameraImage.size());
	cv::Mat locationHM = computeLocalFeatures(cameraImage, pageKeyPoints);

	drawMatch();
}