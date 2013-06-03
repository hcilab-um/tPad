// Dies ist die Haupt-DLL.

#include "stdafx.h"

#include "managedReg.h"

namespace ManagedA
{
	wrapperFeatureMatcher::wrapperFeatureMatcher(bool IsCameraInUse, String^ pathPDFImg)
	{		
		char* str = (char*)(void*)Marshal::StringToHGlobalAnsi(pathPDFImg);
		matcherObj = new FeatureMatcher(IsCameraInUse, str);
	}

	wrapperFeatureMatcher::~wrapperFeatureMatcher(void)
	{
		delete matcherObj;
	}

	wrapperRegistClass::wrapperRegistClass(bool IsCameraInUse, float imageRatio, wrapperFeatureMatcher^ fMatcher)
	{		
		FeatureMatcher* matcher = new FeatureMatcher();
		*matcher = fMatcher->GetFeatureMatcher();
		
		registrationObj = new paperRegistration(IsCameraInUse, imageRatio, matcher);
	}

	wrapperRegistClass::~wrapperRegistClass(void)
	{
		delete registrationObj;
	}
		
	void wrapperRegistClass::SetCameraImg(Bitmap^ bmp1)
	{
		cv::Mat currentImg(bmp1->Height, bmp1->Width, CV_8UC3);

		System::Drawing::Imaging::BitmapData ^data1 = bmp1->LockBits(
			*(gcnew System::Drawing::Rectangle(0, 0, bmp1->Width, bmp1->Height)),
			System::Drawing::Imaging::ImageLockMode::ReadOnly,
			bmp1->PixelFormat);
					
		if (System::Drawing::Imaging::PixelFormat::Format24bppRgb == bmp1->PixelFormat)
			memcpy(currentImg.data, data1->Scan0.ToPointer(), bmp1->Width * bmp1->Height * 3); 
		else if (System::Drawing::Imaging::PixelFormat::Format32bppArgb == bmp1->PixelFormat) 
		{
			uchar *pm = currentImg.data;
			uchar *pb = (uchar *)data1->Scan0.ToPointer();
			for (int i = 0; i < bmp1->Width * bmp1->Height; i++) 
				memcpy(pm + i * 3, pb + i * 4, 3);
		} else {
			uchar *pm = currentImg.data;
			uchar *pb = (uchar *)data1->Scan0.ToPointer();				
			for (int i = 0; i < bmp1->Width * bmp1->Height; i++) 
				*(pm + i * 3) = *(pm + i * 3 + 1) = *(pm + i * 3 + 2) = *(pb + i);					
		}
		
		bmp1->UnlockBits(data1);  	

		registrationObj->setCameraImg(currentImg);
	}

	Glyphs wrapperRegistClass::DetectFigures(float minLength, float maxLength, int tresh_binary)
	{
		cv::vector<cv::vector<cv::Point>> squares, triangles;
		registrationObj->detectFigures(squares, triangles, minLength, maxLength, tresh_binary);

		Glyphs ^ fig = gcnew Glyphs;
		fig -> numberSquares = squares.size();
		fig -> numberTriangles = triangles.size();
		
		return *fig;
	}
}

