// FeatureTracking.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"

#include <ctime>

#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2\calib3d\calib3d.hpp>
#include <opencv2/nonfree/nonfree.hpp>
#include <opencv2/flann/flann.hpp>
#include <opencv2/legacy/legacy.hpp>

#include <FlyCapture2.h>

cv::Point2f device[4] = {cv::Point2f(311, 347),cv::Point2f(725, 342), cv::Point2f(289, 591), cv::Point2f(753,536)};
cv::Point2f page[4] = {cv::Point2f(422,669), cv::Point2f(698,656), cv::Point2f(422, 878), cv::Point2f(672, 833)};	  

cv::FREAK extractor;

cv::vector<cv::KeyPoint> pageKeyPoints;
cv::Mat pageImageDescriptors;

cv::flann::Index flannIdx;
cv::Mat pageImage, deviceImage;

static inline float quadDist(cv::Point2f &pt1, cv::Point2f &pt2)
{
	return ((pt1.x-pt2.x)*(pt1.x-pt2.x))+((pt1.y-pt2.y)*(pt1.y-pt2.y));
}

cv::Mat imageWarp(cv::Mat &src, cv::Mat &dest)
{
	cv::Mat homography = cv::getPerspectiveTransform(device, page);
	cv::warpPerspective(src, dest, homography, dest.size());
	
	return homography;
}

std::vector<cv::KeyPoint> reduceKeyPoints(std::vector<cv::KeyPoint> &imagePoints)
{
	std::vector<cv::KeyPoint> reducedKeyPoints;
	float radius = 49;
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

cv::Mat featureMatching(cv::Mat &deviceImage, cv::Mat &pageImage, cv::Mat &warp_hG)
{	
	int begin_detect = clock();
    //extract features from the device image
	std::vector<cv::KeyPoint> deviceKeypoints;
	cv::FAST(deviceImage, deviceKeypoints, 25, true);
	int end_detect = clock();
	int begin_reduce = clock();
	deviceKeypoints = reduceKeyPoints(deviceKeypoints);
	int end_reduce = clock();

	int begin_extract = clock();
	cv::Mat deviceImageDescriptors;	
	extractor.compute(deviceImage, deviceKeypoints, deviceImageDescriptors);
	int end_extract = clock();

	int begin_search = clock();
	//compute matching keypoints
	cv::Mat indices, dist;
	flannIdx.knnSearch(deviceImageDescriptors, indices, dist, 2);//, cv::flann::SearchParams(64));
	int end_search = clock();

	int begin_match = clock();
	// Find correspondences by NNDR (Nearest Neighbor Distance Ratio)
	//std::vector<cv::DMatch > good_matches;
	std::vector<cv::Point2f> mpts_1, mpts_2; // Used for homography	
	for(unsigned int i=0; i<deviceImageDescriptors.rows; ++i)
	{
		// Check if this descriptor matches with those of the objects
		//if(matches[i][0].distance <= 2*min_dist)//0.3 * dist.at<float>(i,1))
		if(dist.at<float>(i,0) < 0.7*(dist.at<float>(i,1)))
		{
			mpts_1.push_back(deviceKeypoints.at(i).pt);		
			mpts_2.push_back(pageKeyPoints.at(indices.at<int>(i,0)).pt);

			/*cv::DMatch match;
			match.distance = dist.at<float>(i,0);
			match.queryIdx = i;
			match.trainIdx = indices.at<int>(i,0);
			good_matches.push_back(match);*/
        }
	}

	int end_match = clock();
	
	 //Draw only "good" matches
	/*cv::Mat image_matches;
	cv::drawMatches(deviceImage, deviceKeypoints, pageImage, pageKeyPoints, good_matches, image_matches, cv::Scalar::all(-1), cv::Scalar::all(-1), std::vector<char>(), 
		cv::DrawMatchesFlags::DRAW_RICH_KEYPOINTS);
	imshow("m", image_matches);*/


	/*printf ("detect (%f seconds).\n",((float)(end_detect - begin_detect))/CLOCKS_PER_SEC);
	printf ("reduce (%f seconds).\n",((float)(end_reduce - begin_reduce))/CLOCKS_PER_SEC);
	printf ("extract (%f seconds).\n",((float)(end_extract - begin_extract))/CLOCKS_PER_SEC);
	printf ("search (%f seconds).\n",((float)(end_search - begin_search))/CLOCKS_PER_SEC);
	printf ("match (%f seconds).\n",((float)(end_match - begin_match))/CLOCKS_PER_SEC);*/
	
	if (mpts_1.size() >= 4)
    {
		return findHomography(mpts_1, mpts_2, cv::RANSAC);		
	}
	return cv::Mat();

}

int _tmain(int argc, _TCHAR* argv[])
{
	//load page image
	pageImage = cv::imread("images/paper_page.png", CV_LOAD_IMAGE_GRAYSCALE);
	//compute surf features for pageImage	
	cv::FAST(pageImage, pageKeyPoints, 100, true);
	extractor.compute(pageImage, pageKeyPoints, pageImageDescriptors);
	//imagePageKeyPoints.FilterByKeypointSize(minSize, maxSize);
    pageKeyPoints = reduceKeyPoints(pageKeyPoints);
	extractor.compute(pageImage, pageKeyPoints, pageImageDescriptors);

	//flann
	cv::flann::IndexParams idx = *new cv::flann::LshIndexParams(10, 30, 1);	
	flannIdx = *new cv::flann::Index(pageImageDescriptors, idx, cvflann::FLANN_DIST_HAMMING);
	
	////read camera picture
	//cv::Mat *frame = NULL;
 //   
 //   FlyCapture2::Error error;
 //   FlyCapture2::PGRGuid guid;
 //   FlyCapture2::BusManager busMgr;
 //   
 //  // Getting the GUID of the cam
 //   error = busMgr.GetCameraFromIndex(0, &guid);
 //   if (error != FlyCapture2::PGRERROR_OK)
 //   {
 //       error.PrintErrorTrace();
 //       return -1;
 //   }
 //   
 //   FlyCapture2::Camera cam;
 //  //  Connect to a camera
 //   error = cam.Connect(&guid);
 //   if (error != FlyCapture2::PGRERROR_OK)
 //   {
 //       error.PrintErrorTrace();
 //       return -1;
 //   }
 //   
 //  // Starting the capture
 //   error = cam.StartCapture();
 //   if (error != FlyCapture2::PGRERROR_OK)
 //   {
 //       error.PrintErrorTrace();
 //       return -1;
 //   }
 //   
 //  // Get one raw image to be able to calculate the OpenCV window size
 //   FlyCapture2::Image rawImage;
 //   cam.RetrieveBuffer(&rawImage);
 //       
 //  // Setting the window size in OpenCV
	//frame = new cv::Mat(cv::Size(rawImage.GetCols(), rawImage.GetRows()), CV_8UC3);

	//int key;
 //   while(key != 'q') 
 //   {    
 //       // Start capturing images
 //       cam.RetrieveBuffer(&rawImage);

 //       // Get the raw image dimensions
 //       FlyCapture2::PixelFormat pixFormat;
 //       unsigned int rows, cols, stride;
 //       rawImage.GetDimensions( &rows, &cols, &stride, &pixFormat );

 //       // Create a converted image
 //       FlyCapture2::Image convertedImage;

 //        //Convert the raw image
	//	error = rawImage.Convert( FlyCapture2::PIXEL_FORMAT_BGR, &convertedImage );
 //       if (error != FlyCapture2::PGRERROR_OK)
 //       {
 //           error.PrintErrorTrace();
 //           return -1;
 //       }

 //      // Copy the image into the IplImage of OpenCV
	//	memcpy(frame->data, convertedImage.GetData(), convertedImage.GetDataSize());

 //       /* always check */
 //       if( !frame ) break;

 //       //Display the original image
	//	cv::imshow( "Original", *frame );

 //       //exit if user press 'q' 
 //       key = cvWaitKey( 1 );
 //   }
 //
 //   /* free memory */
 //   cvDestroyWindow( "Original" );
    
	// Stop capturing images
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

	int begin = clock();
	//load device image
	deviceImage = cv::imread("images/photo 1.jpg", CV_LOAD_IMAGE_GRAYSCALE);  
	int begin_withoutImg = clock();
	//warp image        	
	cv::Mat warpHM = imageWarp(deviceImage, deviceImage);

	//SURF computation
	int begin_feature = clock();
	cv::Mat locationHM = featureMatching(deviceImage, pageImage, warpHM);

	int end = clock();
	printf ("feature(%f seconds).\n",((float)(end - begin_feature))/CLOCKS_PER_SEC);
	printf ("It took me %f seconds.\n",((float)(end - begin))/CLOCKS_PER_SEC);
	printf ("time without loading device image %f seconds.\n",((float)(end - begin_withoutImg))/CLOCKS_PER_SEC);
	
	//draw detected region
	std::vector<cv::Point2f> device_corners(4);
	device_corners[0] = cvPoint(0,0);
	device_corners[1] = cvPoint(deviceImage.cols, 0 );
	device_corners[2] = cvPoint(deviceImage.cols, deviceImage.rows ); 
	device_corners[3] = cvPoint(0, deviceImage.rows );

	if (!locationHM.empty())
	{
		cv::perspectiveTransform(device_corners, device_corners, locationHM*warpHM);		
		
		//-- Draw lines between the corners (the mapped object in the scene - image_2 )
		cv::line( pageImage, device_corners[0], device_corners[1] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[1], device_corners[2] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[2], device_corners[3] , cv::Scalar( 0, 255, 0), 4 );
		cv::line( pageImage, device_corners[3] , device_corners[0] , cv::Scalar( 0, 255, 0), 4 );
	}
	else cv::perspectiveTransform(device_corners, device_corners, warpHM);

	//-- Show detected matches
	imshow( "Match", pageImage );
	//imshow("warp image", deviceImage);
	
	cvWaitKey(0);	
	cv::destroyAllWindows();

	flannIdx.release();

	return 0;
}
