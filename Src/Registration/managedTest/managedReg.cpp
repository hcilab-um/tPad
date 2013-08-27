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

	Bitmap^ wrapperRegistClass::GetCameraImg(bool warped)
	{
		cv::Mat cameraImg = registrationObj->getCameraImg(warped);
		IplImage *info = &(IplImage)cameraImg;

		int width = info->width;
		int height = info->height;

		if(returnImg == nullptr)
			returnImg = gcnew Bitmap(width, height, System::Drawing::Imaging::PixelFormat::Format24bppRgb); //Format16bppGrayScale
		else if(returnImg->Width != width || returnImg->Height != height)
			returnImg = gcnew Bitmap(width, height, System::Drawing::Imaging::PixelFormat::Format24bppRgb); //Format16bppGrayScale

		System::Drawing::Imaging::BitmapData ^data = returnImg->LockBits(
				*(gcnew System::Drawing::Rectangle(0, 0, returnImg->Width, returnImg->Height)),
				System::Drawing::Imaging::ImageLockMode::ReadWrite,
				returnImg->PixelFormat);

		if(!warped)
		{
			uchar *source = cameraImg.data;
			uchar *dest = (uchar *)data->Scan0.ToPointer();
			int sourcePixelIndex = 0;
			int destPixelIndex = 0;
			for(int row = 0 ; row < height; row++)
			{
				for(int col = 0 ; col < width ; col++)
				{
					sourcePixelIndex = (row * info->widthStep + col);
					destPixelIndex = (row * width + col);

					*(dest + destPixelIndex * 3 + 0) = *(source + sourcePixelIndex);
					*(dest + destPixelIndex * 3 + 1) = *(source + sourcePixelIndex);
					*(dest + destPixelIndex * 3 + 2) = *(source + sourcePixelIndex);
				}
			}
		}
		else
		{
			cv::Mat cleanCameraImg;
			cameraImg.copyTo(cleanCameraImg);
			uchar *source = cleanCameraImg.data;
			uchar *dest = (uchar *)data->Scan0.ToPointer();
			int sourcePixelIndex = 0;
			int destPixelIndex = 0;
			for(int row = 0 ; row < height; row++)
			{
				for(int col = 0 ; col < width ; col++)
				{
					sourcePixelIndex = (row * width + col) - row;
					destPixelIndex = (row * width + col);

					*(dest + destPixelIndex * 3 + 0) = *(source + sourcePixelIndex);
					*(dest + destPixelIndex * 3 + 1) = *(source + sourcePixelIndex);
					*(dest + destPixelIndex * 3 + 2) = *(source + sourcePixelIndex);
				}
			}
		}
		returnImg->UnlockBits(data);

		return returnImg;
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

