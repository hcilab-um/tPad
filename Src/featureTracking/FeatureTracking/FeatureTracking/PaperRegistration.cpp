#include "stdafx.h"
#include "PaperRegistration.h"


#include <opencv2/highgui/highgui.hpp>
#include <opencv2/legacy/legacy.hpp>
//#include <opencv2/core/core.hpp>
//#include <opencv2/imgproc/imgproc.hpp>
//#include <opencv2\calib3d\calib3d.hpp>
//#include <opencv2/flann/flann.hpp>



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

cv::Mat PaperRegistration::computeLocalFeatures(cv::Mat &cameraImage, cv::vector<cv::vector<cv::KeyPoint>> &dbKeyPoints)
{   
	std::vector<cv::KeyPoint> deviceKeypoints;
	cv::FAST(cameraImage, deviceKeypoints, 30, true);

	cv::Mat deviceImageDescriptors;	
	extractor.compute(cameraImage, deviceKeypoints, deviceImageDescriptors);
	
	if (deviceImageDescriptors.rows > 4)
	{
		std::vector<std::vector<cv::DMatch>> dmatches;
		matcher.knnMatch(deviceImageDescriptors, dmatches, 2);
			
		std::vector<cv::Point2f> mpts_1, mpts_2; // Used for homography	
		bool PageIdxIsSet = false;
		for(unsigned int i=0; i<(unsigned int)dmatches.size(); ++i)
		{
			if(!dmatches[i].empty()) 
			{
				if (dmatches[i][0].distance < 0.7*dmatches[i][1].distance)
				{
					mpts_1.push_back(deviceKeypoints[dmatches[i][0].queryIdx].pt);		
					mpts_2.push_back(dbKeyPoints[dmatches[i][0].imgIdx][dmatches[i][0].trainIdx].pt);
				}	

				if (PageIdxIsSet != true)
				{
					PageIdx = dmatches[0][0].imgIdx;
					PageIdxIsSet = true;
				}
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

void PaperRegistration::drawMatch(cv::Mat *cameraImage, cv::Mat &homography)
{
	cv::Mat pageImage = cv::imread("images/paper_page.png", CV_LOAD_IMAGE_GRAYSCALE);

	//draw detected region
	std::vector<cv::Point2f> device_corners(4);
	device_corners[0] = cvPoint(0,0);
	device_corners[1] = cvPoint(cameraImage->cols, 0 );
	device_corners[2] = cvPoint(cameraImage->cols, cameraImage->rows ); 
	device_corners[3] = cvPoint(0, cameraImage->rows );
				
	if (!homography.empty())
	{
		cv::perspectiveTransform(device_corners, device_corners, homography);		
		
		//-- Draw lines between the corners (the mapped object in the scene - image_2 )
		cv::line( pageImage, device_corners[0], device_corners[1] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[1], device_corners[2] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[2], device_corners[3] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[3] , device_corners[0] , cv::Scalar( 0, 255, 0), 4 );
	}

	cv::imshow( "Original", pageImage );
	cv::imshow( "frame", *cameraImage );

	cv::waitKey(0);
}

void PaperRegistration::createIndex(cv::vector<cv::Mat> &dbDescriptors, cv::vector<cv::vector<cv::KeyPoint>> &dbKeyPoints)
{
	//first page
	cv::Mat pageImage = cv::imread("images/paper_page.png", CV_LOAD_IMAGE_GRAYSCALE);
	
	cv::vector<cv::KeyPoint> pageKeyPoints;
	cv::Mat pageImageDescriptors;
	cv::FAST(pageImage, pageKeyPoints, 100, true);
	extractor.compute(pageImage, pageKeyPoints, pageImageDescriptors);
	
	//second image
	pageImage = cv::imread("images/Usability Engineering for Augmented Reality.png", CV_LOAD_IMAGE_GRAYSCALE);
	cv::vector<cv::KeyPoint> pageKeyPoints1;
	cv::Mat pageImageDescriptors1;
	cv::FAST(pageImage, pageKeyPoints1, 100, true);
	extractor.compute(pageImage, pageKeyPoints1, pageImageDescriptors1);

	dbDescriptors.push_back(pageImageDescriptors);
	dbKeyPoints.push_back(pageKeyPoints);
	dbDescriptors.push_back(pageImageDescriptors1);
	dbKeyPoints.push_back(pageKeyPoints1);	

	//ToDo save FlannIDX
	/*cv::FlannBasedMatcher flannMatcher = new cv::flann::LshIndexParams(10, 30, 1);
	std::string sceneImageData = "sceneImagedatamodel.xml";
	cv::FileStorage fs(sceneImageData, cv::FileStorage::WRITE);

	flannMatcher.write(fs);*/
}

void PaperRegistration::detectLocation(cv::Mat cameraImage)
{	
	cv::vector<cv::vector<cv::KeyPoint>> dbKeyPoints;
	cv::vector<cv::Mat> dbDescriptors;
	createIndex(dbDescriptors, dbKeyPoints);
	
	//toDo: load matcher	
	/*std::string sceneImageData = "sceneImagedatamodel.xml";
	cv::FileStorage fs(sceneImageData, cv::FileStorage::READ);
	cv::FileNode fn = fs.getFirstTopLevelNode(); 
	matcher.read(fn);*/
	matcher = cv::FlannBasedMatcher(new cv::flann::LshIndexParams(10, 30, 1));
	matcher.add(dbDescriptors);
	matcher.train();
	
	//ToDo load warpImage
	//cv::warpPerspective(cameraImage, cameraImage, warpMat, cameraImage.size());
	cv::Mat locationHM = computeLocalFeatures(cameraImage, dbKeyPoints);
	
	//compute rotation angle (in degree)
	cv::Mat rotationMat, orthMat;
	cv::Vec3d eulerAngles;
	eulerAngles = cv::RQDecomp3x3(locationHM, rotationMat, orthMat);
	RotationAngle = eulerAngles[2];

	//compute location
	//ToDo: use Center of device instead of top left corner
	std::vector<cv::Point2f> device_point(1);
	device_point[0] = cvPoint(0,cameraImage.cols);
	cv::perspectiveTransform(device_point, device_point, locationHM);	
	LocationPx.width = device_point[0].x;
	LocationPx.height = device_point[0].y;

	drawMatch(&cameraImage, locationHM);
}