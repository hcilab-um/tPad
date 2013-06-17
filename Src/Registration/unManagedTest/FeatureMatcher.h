// unManagedTest.h
#if UNMANAGED_COMPILE_DEFINITION
#define EXPORT_OR_IMPORT __declspec(dllexport)
#else
#define EXPORT_OR_IMPORT __declspec(dllimport)
#endif

#pragma once

#include <string>
#include <opencv2\nonfree\nonfree.hpp>

class EXPORT_OR_IMPORT FeatureMatcher
{
public:
	FeatureMatcher();
	FeatureMatcher(bool isCameraInUse, std::string pathPdfImg);
	~FeatureMatcher();

	cv::vector<cv::vector<cv::KeyPoint>> getDBKeyPoints();
	cv::FlannBasedMatcher* getFeatureMatcher();
private:
	cv::vector<cv::vector<cv::KeyPoint>> dbKeyPoints;

	cv::FlannBasedMatcher* matcher;
	cv::FREAK* extractor;	
	cv::FastFeatureDetector* fastDetectorPageImg;
	cv::SurfFeatureDetector* surfDetectorPageImg;

	bool _isCameraInUse; 

	void getFiles(std::wstring directory, std::vector<std::string> &fileNameList);
	void createIndex(std::string dir_path);
};

