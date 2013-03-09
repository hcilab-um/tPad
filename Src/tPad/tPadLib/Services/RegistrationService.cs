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

    private int status = -1;

    public RegistrationService(bool pUseCamera, TPadDevice device, CameraMonitor camera)
    {
      Device = device;
      cameraMonitor = camera;
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

      if (sender is SimCameraMonitor)
      {
        if (TPadCore.UseFeatureTracking)
        {
          if (Tracker == null)
          {
            if (!cameraMonitor.IsFeatureTrackerStarted())
              cameraMonitor.StartFeatureTracker();
            Tracker = cameraMonitor.Tracker;
          }

          if (temp_SimCaptureToSourceImageRatio != Controller.SimCaptureToSourceImageRatio)
          {
            temp_SimCaptureToSourceImageRatio = Controller.SimCaptureToSourceImageRatio;
            Tracker.imageWarp(temp_SimCaptureToSourceImageRatio);
          }
          System.Drawing.Bitmap camView = (System.Drawing.Bitmap)e.NewObject;
          status = Tracker.detectLocation(camView, status);
          GetLocationFromTracker();
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
      }
      else if (sender is CameraMonitor)
      {
        if (Tracker == null)
          Tracker = (sender as CameraMonitor).Tracker;

        //start feature tracking
        status = Tracker.detectLocation(status);
        GetLocationFromTracker();
      }

      NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(TPadLocation), location));
    }

    private void GetLocationFromTracker()
    {
      if (status == 1)
      {
        location = new TPadLocation();
        location.Status = LocationStatus.Located;
        location.RotationAngle = ClampedAngle(Tracker.RotationAngle);

        Point locationPx = new Point(Tracker.LocationPxM.X / Controller.SimCaptureToSourceImageRatio,
          Tracker.LocationPxM.Y / Controller.SimCaptureToSourceImageRatio);
        location.LocationCm = new Point((float)(locationPx.X / Controller.WidthFactor), (float)(locationPx.Y / Controller.HeightFactor));

        //TODO: get Document object from featureTracker
        location.DocumentID = Controller.ActualDocument.ID;
        location.PageIndex = Tracker.PageIdx;
      }
      else if (status == -1)
      {
        location = new TPadLocation();
        location.Status = LocationStatus.NotLocated;
      }
    }

    public static double ClampedAngle(double angle)
    {
      return ((angle % 360) + 360) % 360;
    }

  }

}
