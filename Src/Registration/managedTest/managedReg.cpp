// Dies ist die Haupt-DLL.

#include "stdafx.h"

#include "managedReg.h"

namespace ManagedA
{
	wrapperRegistClass::wrapperRegistClass(void)
	{
		registrationObj = new paperRegistration();
	}

	wrapperRegistClass::~wrapperRegistClass(void)
	{
		delete registrationObj;
	}

	int wrapperRegistClass::detectLocation(Bitmap^ bmp1)
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

		/*System::Drawing::Imaging::BitmapData ^data2 = bmp2->LockBits(
			*(gcnew System::Drawing::Rectangle(0, 0, bmp2->Width, bmp2->Height)),
			System::Drawing::Imaging::ImageLockMode::ReadOnly,
			bmp2->PixelFormat);
					
		if (System::Drawing::Imaging::PixelFormat::Format24bppRgb == bmp2->PixelFormat)
			memcpy(lastImg.data, data2->Scan0.ToPointer(), bmp2->Width * bmp2->Height * 3); 
		else if (System::Drawing::Imaging::PixelFormat::Format32bppArgb == bmp2->PixelFormat) 
		{
			uchar *pm = lastImg.data;
			uchar *pb = (uchar *)data2->Scan0.ToPointer();
			for (int i = 0; i < bmp2->Width * bmp2->Height; i++) 
				memcpy(pm + i * 3, pb + i * 4, 3);
		} else {
			uchar *pm = lastImg.data;
			uchar *pb = (uchar *)data2->Scan0.ToPointer();				
			for (int i = 0; i < bmp2->Width * bmp2->Height; i++) 
				*(pm + i * 3) = *(pm + i * 3 + 1) = *(pm + i * 3 + 2) = *(pb + i);					
		}
		
		bmp2->UnlockBits(data2); */

		return registrationObj->detectLocation(currentImg);
	}
}

