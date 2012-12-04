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

#include <FlyCapture2.h>

/*cv::Point2f device[4] = {cv::Point2f(311, 347),cv::Point2f(725, 342), cv::Point2f(289, 591), cv::Point2f(753,536)};
cv::Point2f page[4] = {cv::Point2f(422,669), cv::Point2f(698,656), cv::Point2f(422, 878), cv::Point2f(672, 833)};	 */ 
cv::Point2f device[4] = {cv::Point2f(240, 242),cv::Point2f(132, 730), cv::Point2f(998, 241), cv::Point2f(1128,653)};
cv::Point2f page[4] = {cv::Point2f(476,577), cv::Point2f(465,720), cv::Point2f(594,577), cv::Point2f(600,701)};	 

cv::FREAK extractor;

cv::vector<cv::KeyPoint> pageKeyPoints;
cv::Mat pageImageDescriptors;

cv::flann::Index flannIdx;
cv::Mat pageImage, deviceImage;

FlyCapture2::Error error;
FlyCapture2::Camera cam;

static inline float quadDist(cv::Point2f &pt1, cv::Point2f &pt2)
{
	return ((pt1.x-pt2.x)*(pt1.x-pt2.x))+((pt1.y-pt2.y)*(pt1.y-pt2.y));
}

cv::Mat imageWarp(cv::Mat* src, cv::Mat* dest)
{
	cv::Mat homography = cv::getPerspectiveTransform(device, page);
		
	return homography;
}

std::vector<cv::KeyPoint> reduceKeyPoints(std::vector<cv::KeyPoint> &imagePoints, const float radius)
{
	std::vector<cv::KeyPoint> reducedKeyPoints;
	//float radius = 5;
	bool* processed = new bool[imagePoints.size()];
	//std::vector<bool> processed(imagePoints.size());

    for (int i = 0; i < imagePoints.size(); i++)
    {
		if (processed[i] == true)
			continue;

        processed[i] = true;

        for (unsigned int k = (i + 1); k < imagePoints.size(); k++)
        {
          if (processed[k] == true)
            continue;

		  if (quadDist(imagePoints[i].pt,imagePoints[k].pt) < radius)
            processed[k] = true;
        }
		reducedKeyPoints.push_back(imagePoints[i]);    
	}
	return reducedKeyPoints;
}

cv::Mat featureMatching(cv::Mat* deviceImage, cv::Mat* pageImage)
{	
    //extract features from the device image
	std::vector<cv::KeyPoint> deviceKeypoints;
	cv::FAST(*deviceImage, deviceKeypoints, 30, true);
	//deviceKeypoints = reduceKeyPoints(deviceKeypoints, 100);

	cv::Mat deviceImageDescriptors;	
	extractor.compute(*deviceImage, deviceKeypoints, deviceImageDescriptors);
	
	if (deviceImageDescriptors.rows > 4)
	{
		//compute matching keypoints
		cv::Mat indices, dist;
		flannIdx.knnSearch(deviceImageDescriptors, indices, dist, 2, cv::flann::SearchParams(32));
			
		// Find correspondences by NNDR (Nearest Neighbor Distance Ratio)
		//std::vector<cv::DMatch > good_matches;
		std::vector<cv::Point2f> mpts_1, mpts_2; // Used for homography	
		CvMat mat1;
		for(unsigned int i=0; i<(unsigned int)deviceImageDescriptors.rows; ++i)
		{
			if(dist.at<float>(i,0) < 0.7*(dist.at<float>(i,1)))
			{
				mpts_1.push_back(deviceKeypoints.at(i).pt);		
				mpts_2.push_back(pageKeyPoints.at(indices.at<int>(i,0)).pt);
			}			
		}

	
		 //Draw only "good" matches
		/*cv::Mat image_matches;
		cv::drawMatches(deviceImage, deviceKeypoints, pageImage, pageKeyPoints, good_matches, image_matches, cv::Scalar::all(-1), cv::Scalar::all(-1), std::vector<char>(), 
			cv::DrawMatchesFlags::DRAW_RICH_KEYPOINTS);
		imshow("m", image_matches);*/
		/*cv::Mat image_matches = deviceImage->clone();
		drawKeypoints(*deviceImage, deviceKeypoints, image_matches);
		imshow("m", image_matches);*/

		//printf ("#keyPoints: %i \n",deviceKeypoints.size());
		//printf ("#matches: %i \n",mpts_1.size());
		std::vector <uchar> mask;

		if (mpts_1.size() >= 4)
		{
			cv::Mat loc = findHomography(mpts_1, mpts_2, CV_RANSAC, 3, mask);
			//if (std::accumulate(mask.begin(), mask.end(), 0) >=5)
				return loc;
		}		 
	}
	return cv::Mat();
}

bool enableCamera()
{
    FlyCapture2::PGRGuid guid;
    FlyCapture2::BusManager busMgr;
    
   // Getting the GUID of the cam
    error = busMgr.GetCameraFromIndex(0, &guid);
    if (error != FlyCapture2::PGRERROR_OK)
    {
        error.PrintErrorTrace();
        return false;
    }    
    
   //  Connect to a camera
    error = cam.Connect(&guid);
    if (error != FlyCapture2::PGRERROR_OK)
    {
        error.PrintErrorTrace();
        return false;
    }
    
   // Starting the capture
    error = cam.StartCapture();
    if (error != FlyCapture2::PGRERROR_OK)
    {
        error.PrintErrorTrace();
        return false;
    }    

	return true;
}

int _tmain(int argc, _TCHAR* argv[])
{
	//load page image
	pageImage = cv::imread("images/Usability Engineering for Augmented Reality.png", CV_LOAD_IMAGE_GRAYSCALE);
	//compute surf features for pageImage	
	cv::FAST(pageImage, pageKeyPoints, 100, true);
	extractor.compute(pageImage, pageKeyPoints, pageImageDescriptors);
	//imagePageKeyPoints.FilterByKeypointSize(minSize, maxSize);
    //pageKeyPoints = reduceKeyPoints(pageKeyPoints, 49);
	extractor.compute(pageImage, pageKeyPoints, pageImageDescriptors);

	//flann
	cv::flann::IndexParams idx = *new cv::flann::LshIndexParams(10, 30, 1);	
	flannIdx = *new cv::flann::Index(pageImageDescriptors, idx, cvflann::FLANN_DIST_HAMMING);
	
	//int begin = clock();
	////load device image	
	deviceImage = cv::imread("images/LCD.JPG", CV_LOAD_IMAGE_GRAYSCALE);  
	cv::Mat* frame_new = new cv::Mat(cv::Size(deviceImage.cols, deviceImage.rows), CV_8UC1);
	*frame_new = deviceImage;
	////imshow("warp image", deviceImage);
	cv::Mat warpHM = imageWarp(&deviceImage, &deviceImage);
		
	//read camera picture
	cv::Mat *frame_last = NULL;
	//cv::Mat *frame_new = NULL;
    
	if (!enableCamera())
		return -1;
    
   // Get one raw image to be able to calculate the OpenCV window size
    FlyCapture2::Image rawImage;
    cam.RetrieveBuffer(&rawImage);
        
   // Setting the window size in OpenCV
	frame_last = new cv::Mat(cv::Size(rawImage.GetCols(), rawImage.GetRows()), CV_8UC1);
	frame_new = new cv::Mat(cv::Size(rawImage.GetCols(), rawImage.GetRows()), CV_8UC1);

	int key = 0;
	int last_diff = 0;
	
	cv::Mat imageMatch = pageImage;
	float meanValue = 0;
    while(key != 'q') 
    {    
		
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
			printf("here");
            return -1;
        }
		
       // Copy the image into the Mat of OpenCV
		*frame_last = frame_new->clone();
		memcpy(frame_new->data, convertedImage.GetData(), convertedImage.GetDataSize());
		
        /* always check */
        if( !frame_new ) return -1;
		
		//warp image	
		cv::warpPerspective(*frame_new, *frame_new, warpHM, frame_new->size());

		cv::Mat diff;
		cv::absdiff(*frame_new, *frame_last, diff);
		// Equal if no elements disagree
		meanValue = (cv::mean(diff)[0]);
		
		if (meanValue > 0.09)
		{
			//SURF computation
			cv::Mat locationHM = featureMatching(frame_new, frame_new);
		
			//draw detected region
			std::vector<cv::Point2f> device_corners(4);
			device_corners[0] = cvPoint(0,0);
			device_corners[1] = cvPoint(frame_new->cols, 0 );
			device_corners[2] = cvPoint(frame_new->cols, frame_new->rows ); 
			device_corners[3] = cvPoint(0, frame_new->rows );
				
			if (!locationHM.empty())
			{
				imageMatch = pageImage.clone();
				cv::perspectiveTransform(device_corners, device_corners, locationHM);		
		
				//-- Draw lines between the corners (the mapped object in the scene - image_2 )
				cv::line( imageMatch, device_corners[0], device_corners[1] , cv::Scalar( 0, 255, 0), 4 );
				cv::line( imageMatch, device_corners[1], device_corners[2] , cv::Scalar( 0, 255, 0), 4 );
				cv::line( imageMatch, device_corners[2], device_corners[3] , cv::Scalar( 0, 255, 0), 4 );
				cv::line( imageMatch, device_corners[3] , device_corners[0] , cv::Scalar( 0, 255, 0), 4 );
			}
		}
		////else cv::perspectiveTransform(device_corners, device_corners, warpHM);
		////Display the original image
		cv::imshow( "Original", imageMatch );
		cv::imshow( "frame", *frame_new );
		//imageMatch.release();
		
        //exit if user press 'q' 				
        key = cvWaitKey( 1 );
    }
     
	 //Stop capturing images
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
	

	//-- Show detected matches
	//imshow( "Match", pageImage );
	
	
	cvWaitKey(0);	
	cv::destroyAllWindows();

	flannIdx.release();

	return 0;
}
