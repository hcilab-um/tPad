// managedTest.h

#pragma once

using namespace System;
using namespace System::Drawing;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;


#include "unmanagedReg.h" //Unmanaged class INeedThisClass
//#pragma managed(push, off)
//#include <opencv2/highgui/highgui.hpp>
//#pragma managed(pop)

namespace ManagedA 
{	
	public ref class wrapperRegistClass : IDisposable
	{
	public:
		wrapperRegistClass(void)
		{
			registrationObj = new paperRegistration();
		}

		~wrapperRegistClass(void)
		{
			delete registrationObj;
		}
		
		property float RotationAngle
		{
			float get()
			{
				return registrationObj->getRotationAngle();
			}
		}
		
		property int PageIdx
		{
			int get()
			{
				return registrationObj->getPageIdx();
			}
		}
		
		property String^ PageName
		{
			String^ get()
			{
				return gcnew String((registrationObj->getPageName()).c_str());
			}
		}
		
		property Point LocationPx
		{
			Point get()
			{
				Point locationPt = *new Point((registrationObj->getLocationPx().x),(registrationObj->getLocationPx()).y);				
				return locationPt;
			}
		}

		void detectLocation(Bitmap ^bmp)
		{	 
			cv::Mat dst(bmp->Height, bmp->Width, CV_8UC3);
			
			System::Drawing::Imaging::BitmapData ^data = bmp->LockBits(
				*(gcnew System::Drawing::Rectangle(0, 0, bmp->Width, bmp->Height)),
				System::Drawing::Imaging::ImageLockMode::ReadOnly,
				bmp->PixelFormat);
			               
			if (System::Drawing::Imaging::PixelFormat::Format24bppRgb == bmp->PixelFormat)
				memcpy(dst.data, data->Scan0.ToPointer(), bmp->Width * bmp->Height * 3); 
			else if (System::Drawing::Imaging::PixelFormat::Format32bppArgb == bmp->PixelFormat) 
			{
				uchar *pm = dst.data;
				uchar *pb = (uchar *)data->Scan0.ToPointer();
				for (int i = 0; i < bmp->Width * bmp->Height; i++) 
					memcpy(pm + i * 3, pb + i * 4, 3);
			} else {
				uchar *pm = dst.data;
				uchar *pb = (uchar *)data->Scan0.ToPointer();
				for (int i = 0; i < bmp->Width * bmp->Height; i++) 
					*(pm + i * 3) = *(pm + i * 3 + 1) = *(pm + i * 3 + 2) = *(pb + i);					
			}

			bmp->UnlockBits(data);  		
			registrationObj->detectLocation(dst);
		}

	private: 
		paperRegistration *registrationObj;
	};
}