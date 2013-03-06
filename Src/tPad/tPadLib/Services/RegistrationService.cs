using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Ubicomp.Utils.NET.CAF.ContextService;
using Ubicomp.Utils.NET.CAF.ContextAdapter;
using System.Windows;

namespace UofM.HCI.tPab.Services
{

  public class RegistrationService : ContextService
  {

    public ITPadAppContainer Container { get; set; }

    public ITPadAppController Controller { get; set; }

    private ManagedA.wrapperRegistClass featureTracker;

    private float temp_SimCaptureToSourceImageRatio;

    private bool isProcessStopped = false;

    private bool useCamera;

    private TPadDevice Device { get; set; }

    private TPadLocation location = new TPadLocation();

    public RegistrationService(bool pUseCamera, TPadDevice device)
    {
      useCamera = pUseCamera;
      Device = device;
    }

    protected override void CustomStart()
    {
      temp_SimCaptureToSourceImageRatio = 1;

      featureTracker = new ManagedA.wrapperRegistClass(useCamera, Controller.SimCaptureToSourceImageRatio);
      featureTracker.imageWarp("homograpgy.xml");
      featureTracker.createIndex(Environment.CurrentDirectory + "\\" + Controller.ActualDocument.Folder);
      
      if (useCamera && TPadCore.UseFeatureTracking)
      {
        if (featureTracker.connectCamera() == -1)
          throw new ArgumentException("Connection to camera failed!");
      }
    }

    public void Pause()
    {
      isProcessStopped = true;

      if (useCamera && TPadCore.UseFeatureTracking)
      {
        if (featureTracker.disconnectCamera() == -1)
          throw new ArgumentException("disconnect camera failed!");
      }
    }

    public void Continue()
    {
      isProcessStopped = false;

      if (useCamera && TPadCore.UseFeatureTracking)
      {
        if (featureTracker.connectCamera() == -1)
          throw new ArgumentException("Connection to camera failed!");
      }
    }

    /// <summary>
    /// This method receives the image from the camera and finds the location of the device.
    /// Location defined as the page, the X and Y coordinates wihtin the page in cms, and the rotation angle
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected override void CustomUpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(System.Drawing.Bitmap))
        return;
      if (isProcessStopped)
        return;
      if (Container == null || Controller == null)
        return;
      if (Device.State == StackingState.StackedOnTop)
        return;
      
      if (TPadCore.UseFeatureTracking)
      {
        System.Drawing.Bitmap camView = (System.Drawing.Bitmap)e.NewObject;
        
        // Here goes the machine vision code to find where the device is located based on the camera image
        //ToDo: correct warping with camera
        if (!useCamera && temp_SimCaptureToSourceImageRatio != Controller.SimCaptureToSourceImageRatio)
        {
          temp_SimCaptureToSourceImageRatio = Controller.SimCaptureToSourceImageRatio;
          featureTracker.imageWarp(temp_SimCaptureToSourceImageRatio);
        }

        //start feature tracking
        int status = featureTracker.detectLocation(camView);
        if (status == 1)
        {
          location = new TPadLocation();
          location.Status = LocationStatus.Located;
          location.RotationAngle = ClampedAngle(featureTracker.RotationAngle);

          Point locationPx = new Point(featureTracker.LocationPxM.X / Controller.SimCaptureToSourceImageRatio, 
            featureTracker.LocationPxM.Y / Controller.SimCaptureToSourceImageRatio);
          location.LocationCm = new Point((float)(locationPx.X / Controller.WidthFactor), (float)(locationPx.Y / Controller.HeightFactor));

          //TODO: get Document object from featureTracker
          location.DocumentID = Controller.ActualDocument.ID;
          location.PageIndex = featureTracker.PageIdx;                    
        }
        else if (status == -1)
          location.Status = LocationStatus.NotLocated;

      }
      else
      {
        location = new TPadLocation();
        location.Status = LocationStatus.Located;
        location.RotationAngle = ClampedAngle(Controller.RotationAngle);
        location.LocationCm = new Point(Controller.Location.X / Controller.WidthFactor, Controller.Location.Y / Controller.HeightFactor);
        location.DocumentID = Controller.ActualDocument.ID;
        location.PageIndex = Controller.ActualPage;
      }

      NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(TPadLocation), location));
    }

    public static double ClampedAngle(double angle)
    { 
      return ((angle % 360) + 360) % 360;
    }

  }

}
