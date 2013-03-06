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
	public ref class wrapperRegistClass
	{
	public:
		wrapperRegistClass(bool IsCameraInUse, float imageRatio);

		~wrapperRegistClass();
		
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
						
		property PointF LocationPxBR
		{
			PointF get()
			{
				PointF locationPt = *new PointF((registrationObj->getLocationPxBR().x),(registrationObj->getLocationPxBR()).y);				
				return locationPt;
			}
		}

		property PointF LocationPxTL
		{
			PointF get()
			{
				PointF locationPt = *new PointF((registrationObj->getLocationPxTL().x),(registrationObj->getLocationPxTL()).y);				
				return locationPt;
			}
		}

		property PointF LocationPxTR
		{
			PointF get()
			{
				PointF locationPt = *new PointF((registrationObj->getLocationPxTR().x),(registrationObj->getLocationPxTR()).y);				
				return locationPt;
			}
		}

		property PointF LocationPxBL
		{
			PointF get()
			{
				PointF locationPt = *new PointF((registrationObj->getLocationPxBL().x),(registrationObj->getLocationPxBL()).y);				
				return locationPt;
			}
		}

		property PointF LocationPxM
		{
			PointF get()
			{
				PointF locationPt = *new PointF((registrationObj->getLocationPxM().x),(registrationObj->getLocationPxM()).y);				
				return locationPt;
			}
		}
								
		void createIndex(String^ path)
		{
			char* str = (char*)(void*)Marshal::StringToHGlobalAnsi(path);
			registrationObj->createIndex(str);
		}
		
		void imageWarp(float imageRatio)
		{
			registrationObj->imageWarp(imageRatio);
		}

		void imageWarp(String^ Path)
		{
			char* str = (char*)(void*)Marshal::StringToHGlobalAnsi(Path);
			registrationObj->imageWarp(str);
		}
				
		int detectLocation(Bitmap^ bmp1);

		int detectLocation()
		{
			return registrationObj->detectLocation();
		}

		int connectCamera()
		{
			return registrationObj->connectCamera();
		}
		
		int disconnectCamera()
		{
			return registrationObj->disconnectCamera();
		}

	private: 
		paperRegistration *registrationObj;
	};
}