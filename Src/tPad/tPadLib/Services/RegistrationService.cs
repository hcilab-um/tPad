﻿using System;
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

    private ManagedA.wrapperRegistClass featureTracker;

    private Bitmap oldCamView;

    private TPadLocation location;

    private float temp_SimCaptureToSourceImageRatio;

    protected override void CustomStart()
    {
      base.CustomStart();

      featureTracker = new ManagedA.wrapperRegistClass();
      featureTracker.createIndex(Environment.CurrentDirectory + "\\" + ActualDocument.DocumentName);

      location = new TPadLocation();
      oldCamView = new Bitmap(10, 10);
      temp_SimCaptureToSourceImageRatio = 1;
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

      if (TPadCore.Instance.UseFeatureTracking)
      {
        Bitmap camView = (Bitmap)e.NewObject;
        Stopwatch sw = new Stopwatch();
        camView.Save("new.png");
        oldCamView.Save("old.png");
        sw.Start();
        // Here goes the machine vision code to find where the device is located based on the camera image
        //ToDo: correct warping with camera

        //compute warping matrix
        if (temp_SimCaptureToSourceImageRatio != Container.SimCaptureToSourceImageRatio)
        {
          temp_SimCaptureToSourceImageRatio = Container.SimCaptureToSourceImageRatio;
          featureTracker.imageWarp(temp_SimCaptureToSourceImageRatio, TPadCore.Instance.IsSimulation);
        }

        //start feature tracking
        int status = featureTracker.detectLocation(camView, oldCamView);
        if (status == 1)
        {
          location.Status = LocationStatus.Located;
          location.RotationAngle = featureTracker.RotationAngle;
          location.LocationPx = new PointF(featureTracker.LocationPxM.X / Container.SimCaptureToSourceImageRatio, featureTracker.LocationPxM.Y / Container.SimCaptureToSourceImageRatio);
          location.LocationCm = new PointF((float)(location.LocationPx.X / Container.WidthFactor), (float)(location.LocationPx.Y / Container.HeightFactor));
          //ToDo: get Document object from pageIdx
          location.Document = ActualDocument;
          location.PageIndex = featureTracker.PageIdx;
          sw.Stop();
          Console.WriteLine(sw.Elapsed.TotalMilliseconds);
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
        location.RotationAngle = Container.RotationAngle;
        location.LocationPx = Container.Location;
        location.LocationCm = new PointF((float)(Container.Location.X / Container.WidthFactor), (float)(Container.Location.Y / Container.HeightFactor));
        location.Document = ActualDocument;
        location.PageIndex = Container.ActualPage;
      }

      NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(TPadLocation), location));
    }


    public void LoadDocuments(string[] documentFolders)
    {
      if (documentFolders == null || documentFolders.Length == 0)
        throw new ArgumentException("Parameter 'documentFolders' cannot be empty");
      if (!Directory.Exists(documentFolders[0]))
        throw new ArgumentException(String.Format("Folder '{0}' does not exist!", documentFolders[0]));

      ActualDocument = new TPadDocument() { DocumentName = documentFolders[0] };
      String[] pages = Directory.GetFiles(documentFolders[0], "*.png");
      Array.Sort<String>(pages);
      ActualDocument.Pages = new TPadPage[pages.Length];
      for (int index = 0; index < pages.Length; index++)
        ActualDocument.Pages[index] = new TPadPage() { FileName = pages[index] };
    }

  }

}
