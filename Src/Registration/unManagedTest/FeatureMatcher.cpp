#include "StdAfx.h"
#include "FeatureMatcher.h"

#include <Windows.h>
#include <iostream>

#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc/imgproc.hpp>

FeatureMatcher::FeatureMatcher(bool isCameraInUse, std::string pathPdfImg)
{
	_isCameraInUse = isCameraInUse;

	if (_isCameraInUse)
	{
		fastDetectorPageImg = new cv::FastFeatureDetector(10, true);
		matcher = new cv::FlannBasedMatcher(new cv::flann::LshIndexParams(4, 22, 0));
		extractor = new cv::FREAK(true, false, 13.0F, 2);
	}	
	else 
	{
		surfDetectorPageImg = new cv::SurfFeatureDetector(2000, 4, 1, false);
		matcher = new cv::FlannBasedMatcher(new cv::flann::LshIndexParams(4, 25, 0));
		extractor = new cv::FREAK(true, false, 10.0F, 3);		
	}

	createIndex(pathPdfImg);
}

FeatureMatcher::~FeatureMatcher()
{
}

cv::vector<cv::vector<cv::KeyPoint>> FeatureMatcher::getDBKeyPoints()
{
	return dbKeyPoints;
}

cv::FlannBasedMatcher* FeatureMatcher::getFeatureMatcher()
{
	return matcher;
}

void FeatureMatcher::getFiles(std::wstring directory, std::vector<std::string> &fileNameList)
{
	HANDLE handle;
	WIN32_FIND_DATA finddata; 

	handle = FindFirstFile(directory.c_str(),&finddata);
	FindNextFile(handle,&finddata);
	while (FindNextFile(handle,&finddata))
	{
		std::wstring fileName = finddata.cFileName;
		std::string str( fileName.begin(), fileName.end() );
		fileNameList.push_back(str);
	}
	FindClose(handle);
}

void FeatureMatcher::createIndex(std::string dir_path)
{
	dbKeyPoints.clear();
	cv::vector<cv::Mat> dbDescriptors;

	//load pages
	std::string path = dir_path + "\\*";
	std::wstring wsTmp(path.begin(), path.end());
	std::wstring ws = wsTmp;
	std::vector<std::string> fileNameList;
	getFiles(ws, fileNameList);

	for (unsigned int i = 0; i < fileNameList.size(); i++)
	{		
		std::string imagePath = dir_path + "/" + fileNameList[i];
		cv::Mat pageImage = cv::imread(imagePath, CV_LOAD_IMAGE_GRAYSCALE);

		cv::vector<cv::KeyPoint> pageKeyPoints;
		if (_isCameraInUse)
			fastDetectorPageImg->detect(pageImage, pageKeyPoints);
		else surfDetectorPageImg->detect(pageImage, pageKeyPoints);

		cv::Mat pageImageDescriptors;
		extractor->compute(pageImage, pageKeyPoints, pageImageDescriptors);
	
		dbDescriptors.push_back(pageImageDescriptors);
		dbKeyPoints.push_back(pageKeyPoints);
	}

	//ToDo save FlannIDX
	/*cv::FlannBasedMatcher flannMatcher = new cv::flann::LshIndexParams(10, 30, 1);
	std::string sceneImageData = "sceneImagedatamodel.xml";
	cv::FileStorage fs(sceneImageData, cv::FileStorage::WRITE);
	flannMatcher.write(fs);*/
		
	matcher->add(dbDescriptors);
	matcher->train();	
}
