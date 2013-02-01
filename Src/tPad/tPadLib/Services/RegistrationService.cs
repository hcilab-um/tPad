using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextService;
using System.Drawing;
using System.IO;
using System.Diagnostics;

namespace UofM.HCI.tPab.Services
{

  public class RegistrationService : ContextService
  {

    public TPadDocument ActualDocument { get; set; }

    public ITPadAppContainer Container { get; set; }

    public ITPadAppController Controller { get; set; }

    private ManagedA.wrapperRegistClass featureTracker;

    private Bitmap oldCamView;

    private TPadLocation location;

    private float temp_SimCaptureToSourceImageRatio;

    private bool isProcessStopped = false;

    protected override void CustomStart()
    {
      featureTracker = new ManagedA.wrapperRegistClass();
      featureTracker.createIndex(Environment.CurrentDirectory + "\\" + ActualDocument.Folder);

      location = new TPadLocation();
      oldCamView = new Bitmap(10, 10);
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
    protected override void CustomUpdateMonitorReading(object sender, CAF.ContextAdapter.NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(Bitmap))
        return;
      if (isProcessStopped)
        return;

      if (TPadCore.UseFeatureTracking)
      {
        Bitmap camView = (Bitmap)e.NewObject;
        Stopwatch sw = new Stopwatch();
        camView.Save("new.png");
        oldCamView.Save("old.png");
        sw.Start();
        // Here goes the machine vision code to find where the device is located based on the camera image
        //ToDo: correct warping with camera

        //compute warping matrix
        if (temp_SimCaptureToSourceImageRatio != Controller.SimCaptureToSourceImageRatio)
        {
          temp_SimCaptureToSourceImageRatio = Controller.SimCaptureToSourceImageRatio;
          featureTracker.imageWarp(temp_SimCaptureToSourceImageRatio, true);
        }

        //start feature tracking
        int status = featureTracker.detectLocation(camView, oldCamView);
        if (status == 1)
        {
          location.Status = LocationStatus.Located;
          location.RotationAngle = featureTracker.RotationAngle;

          PointF locationPx = new PointF(featureTracker.LocationPxM.X / Controller.SimCaptureToSourceImageRatio, featureTracker.LocationPxM.Y / Controller.SimCaptureToSourceImageRatio);
          location.LocationCm = new PointF((float)(locationPx.X / Container.WidthFactor), (float)(locationPx.Y / Container.HeightFactor));

          //TODO: get Document object from featureTracker
          location.Document = ActualDocument;
          location.PageIndex = featureTracker.PageIdx;
          sw.Stop();
          //Console.WriteLine(sw.Elapsed.TotalMilliseconds);
        }
        else if (status == -1)
          location.Status = LocationStatus.NotLocated;

        sw.Stop();
        //update last image of camera
        oldCamView = camView;
      }
      else
      {
        location.Status = LocationStatus.Located;
        location.RotationAngle = Controller.RotationAngle;
        location.LocationCm = new PointF((float)(Controller.Location.X / Controller.WidthFactor), (float)(Controller.Location.Y / Controller.HeightFactor));
        location.Document = Controller.ActualDocument;
        location.PageIndex = Controller.ActualPage;
      }

      NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(TPadLocation), location));
    }

  }

}
