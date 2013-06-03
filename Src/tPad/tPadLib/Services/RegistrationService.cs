using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Ubicomp.Utils.NET.CAF.ContextService;
using Ubicomp.Utils.NET.CAF.ContextAdapter;
using System.Windows;
using UofM.HCI.tPab.Monitors;

namespace UofM.HCI.tPab.Services
{

  public class RegistrationService : ContextService
  {
    public ITPadAppContainer Container { get; set; }

    public ITPadAppController Controller { get; set; }

    private ManagedA.wrapperRegistClass Tracker { get; set; }
   
    private float temp_SimCaptureToSourceImageRatio;

    private bool isProcessStopped = false;

    private TPadDevice Device { get; set; }

    private TPadLocation location;

    private CameraMonitor cameraMonitor;
    private SimCameraMonitor simCameraMonitor;

    private int status = -1;
    private int trigger = 0;
    
    public RegistrationService(bool UseCamera, TPadDevice device, CameraMonitor camera, SimCameraMonitor simCamera)
    {
      Device = device;
      cameraMonitor = camera;
      simCameraMonitor = simCamera;
    }

    protected override void CustomStart()
    {
      temp_SimCaptureToSourceImageRatio = 1;
    }

    public void Pause()
    {
      isProcessStopped = true;
    }

    public void Continue()
    {
      isProcessStopped = false;
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
        if (sender is SimCameraMonitor)
        {
          if (Tracker == null)
            Tracker = (sender as SimCameraMonitor).Tracker;
          
          if (temp_SimCaptureToSourceImageRatio != Controller.SimCaptureToSourceImageRatio)
          {
            temp_SimCaptureToSourceImageRatio = Controller.SimCaptureToSourceImageRatio;
            Tracker.computeWarpMatrix(temp_SimCaptureToSourceImageRatio);
          }

          status = Tracker.detectLocation(false, status);
          GetLocationFromTracker();
        }
        else if (sender is CameraMonitor)
        {
          if (Tracker == null)
            Tracker = (sender as CameraMonitor).Tracker;

          //start feature tracking
          //Stopwatch sw = new Stopwatch();
          //sw.Start();
          status = Tracker.detectLocation(true, status);
          //sw.Stop();
          //Console.WriteLine("Elapsed={0} ", sw.ElapsedMilliseconds);
          GetLocationFromTracker();
        }
      }
      else {
        location = new TPadLocation();
        location.Status = LocationStatus.Located;
        location.RotationAngle = ClampedAngle(Controller.RotationAngle);
        location.LocationCm = new Point(Controller.Location.X / Controller.WidthFactor, Controller.Location.Y / Controller.HeightFactor);
        location.DocumentID = Controller.ActualDocument.ID;
        location.PageIndex = Controller.ActualPage;
      }
      
      NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(TPadLocation), location));
    }

    private void GetLocationFromTracker()
    {
      //status -1: not detected, status 1: location detected, 
      //status 0: previous image and current image are the same -> no new location computation necessary
      if (status == 1)
      {
        trigger = 0;

        location = new TPadLocation();
        location.Status = LocationStatus.Located;
        location.RotationAngle = ClampedAngle(Tracker.RotationAngle);

        Point locationPx = new Point(Tracker.LocationPxM.X / Controller.SimCaptureToSourceImageRatio,
         Tracker.LocationPxM.Y / Controller.SimCaptureToSourceImageRatio);
        location.LocationCm = new Point((float)(locationPx.X / Controller.WidthFactor),
          (float)(locationPx.Y / Controller.HeightFactor));

        //TODO: get Document object from featureTracker
        location.DocumentID = Controller.ActualDocument.ID;
        location.PageIndex = Tracker.PageIdx;
      }
      else if (status == -1 && trigger > 10)
      {
        location = new TPadLocation();
        location.Status = LocationStatus.NotLocated;
      }
      else if (status == -1)
      {
        trigger++;
      }
    }

    public static double ClampedAngle(double angle)
    {
      return ((angle % 360) + 360) % 360;
    }

  }

}
