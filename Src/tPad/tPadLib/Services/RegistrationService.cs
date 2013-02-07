using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using Ubicomp.Utils.NET.CAF.ContextService;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPab.Services
{

  public class RegistrationService : ContextService
  {

    public ITPadAppContainer Container { get; set; }

    public ITPadAppController Controller { get; set; }

    private ManagedA.wrapperRegistClass featureTracker;
    
    private TPadLocation location;

    private float temp_SimCaptureToSourceImageRatio;

    private bool isProcessStopped = false;
    
    protected override void CustomStart()
    {
      featureTracker = new ManagedA.wrapperRegistClass(Controller.IsCameraInUse);
      featureTracker.createIndex(Environment.CurrentDirectory + "\\" + Controller.ActualDocument.Folder);

      location = new TPadLocation();
      temp_SimCaptureToSourceImageRatio = 1;

      //if (Controller.IsCameraInUse && TPadCore.UseFeatureTracking)
      //  featureTracker.connectCamera();
    }

    public void Pause()
    {
      isProcessStopped = true;

      //if (Controller.IsCameraInUse && TPadCore.UseFeatureTracking)
      //  featureTracker.disconnectCamera();
    }

    public void Continue()
    {
      isProcessStopped = false;

      //if (Controller.IsCameraInUse && TPadCore.UseFeatureTracking)
      //  featureTracker.connectCamera();
    }

    /// <summary>
    /// This method receives the image from the camera and finds the location of the device.
    /// Location defined as the page, the X and Y coordinates wihtin the page in cms, and the rotation angle
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected override void CustomUpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(Bitmap))
        return;
      if (isProcessStopped)
        return;
      if (Container == null || Controller == null)
        return;
      
      if (TPadCore.UseFeatureTracking)
      {
        Bitmap camView = (Bitmap)e.NewObject;
        camView.Save("neu.png");
        Stopwatch sw = new Stopwatch();
        sw.Start();
        
        // Here goes the machine vision code to find where the device is located based on the camera image
        //ToDo: correct warping with camera
        if (temp_SimCaptureToSourceImageRatio != Controller.SimCaptureToSourceImageRatio)
        {
          temp_SimCaptureToSourceImageRatio = Controller.SimCaptureToSourceImageRatio;
          featureTracker.imageWarp(temp_SimCaptureToSourceImageRatio);
        }

        //start feature tracking
        int status = featureTracker.detectLocation(camView);
        if (status == 1)
        {
          location.Status = LocationStatus.Located;
          location.RotationAngle = featureTracker.RotationAngle;

          PointF locationPx = new PointF(featureTracker.LocationPxTL.X / Controller.SimCaptureToSourceImageRatio, 
            featureTracker.LocationPxTL.Y / Controller.SimCaptureToSourceImageRatio);
          location.LocationCm = new PointF((float)(locationPx.X / Controller.WidthFactor), (float)(locationPx.Y / Controller.HeightFactor));

          //TODO: get Document object from featureTracker
          location.DocumentID = Controller.ActualDocument.ID;
          location.PageIndex = featureTracker.PageIdx;
          
          sw.Stop();
          Console.WriteLine(sw.Elapsed.TotalMilliseconds);
        }
        else if (status == -1)
          location.Status = LocationStatus.NotLocated;

        sw.Stop();
      }
      else
      {
        location.Status = LocationStatus.Located;
        location.RotationAngle = Controller.RotationAngle;
        location.LocationCm = new PointF((float)(Controller.Location.X / Controller.WidthFactor), (float)(Controller.Location.Y / Controller.HeightFactor));
        location.DocumentID = Controller.ActualDocument.ID;
        location.PageIndex = Controller.ActualPage;
      }

      NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(TPadLocation), location));
    }
  }

}
