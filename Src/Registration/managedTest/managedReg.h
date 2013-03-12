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
	public value struct Glyphs
	{		
		int numberSquares;
		int numberTriangles;
	};

	public ref class wrapperFeatureMatcher
	{
	public:
		wrapperFeatureMatcher() {}
		wrapperFeatureMatcher(bool IsCameraInUse, String^ pathPDFImg);
		~wrapperFeatureMatcher();

		FeatureMatcher GetFeatureMatcher()
		{
			return *matcherObj;
		}

	private:
		FeatureMatcher *matcherObj;
	};

	public ref class wrapperRegistClass
	{
	public:
		wrapperRegistClass(bool IsCameraInUse, float imageRatio, wrapperFeatureMatcher^ fMatcher);

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
				
		void imageWarp(float imageRatio)
		{
			registrationObj->imageWarp(imageRatio);
		}

		void imageWarp(String^ Path)
		{
			char* str = (char*)(void*)Marshal::StringToHGlobalAnsi(Path);
			registrationObj->imageWarp(str);
		}
				
		void SetCameraImg(Bitmap^ bmp1);

		void SetCameraImg()
		{
			registrationObj->setCameraImg();
		}

		int detectLocation(bool camInUse, int previousStatus)
		{
			return registrationObj->detectLocation(camInUse, previousStatus);
		}

		Glyphs DetectFigures(float minLength, float maxLength, int tresh_binary);

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