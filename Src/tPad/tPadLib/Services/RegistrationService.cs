using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextService;
using System.Drawing;
using System.IO;

namespace UofM.HCI.tPab.Services
{

  public class RegistrationService : ContextService
  {

    public TPadDocument ActualDocument { get; set; }

    public ITPadAppContainer Container { get; set; }

    private ManagedA.wrapperRegistClass featureTracker;

    protected override void CustomStart()
    {
      base.CustomStart();

      featureTracker = new ManagedA.wrapperRegistClass();
      featureTracker.createIndex(Environment.CurrentDirectory + "\\" + ActualDocument.DocumentName);
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

      TPadLocation location = new TPadLocation();
      // Here goes the machine vision code to find where the device is located based on the camera image
      // -- Beware you can use TPadCore.IsSimulation to determine the parameters for the image, e.g. whether it needs warping.
      
      //ToDo: real images
      //Bitmap cameraView = (Bitmap)e.NewObject;
      //Bitmap bmp2 = (Bitmap)System.Drawing.Image.FromFile("C:\\Users/sophie/Desktop/Registration/unManagedTest/images/LCD3.png");
  
      ////ToDo: Warping
      
      //if (featureTracker.detectLocation(cameraView, bmp2) == 1)
      //{
      //  location.Status = LocationStatus.Located;
      //  location.RotationAngle = featureTracker.RotationAngle;
      //  location.LocationPx = featureTracker.LocationPxM;
      //  location.LocationCm = new PointF((float)(Container.Location.X / Container.WidthFactor), (float)(Container.Location.Y / Container.HeightFactor));
      //  //ToDo: get Document object from pageIdx
      //  location.Document = ActualDocument;
      //  location.PageIndex = featureTracker.PageIdx;// featureTracker.PageIdx;
      //}
      //else location.Status = LocationStatus.NotLocated;

      //----------------------------- MOCK CODE ------------------------------
      if (TPadCore.Instance.IsSimulation)
      {
        location.Status = LocationStatus.Located;
        location.RotationAngle = Container.RotationAngle;
        location.LocationPx = Container.Location;
        location.LocationCm = new PointF((float)(Container.Location.X / Container.WidthFactor), (float)(Container.Location.Y / Container.HeightFactor));
        location.Document = ActualDocument;
        location.PageIndex = Container.ActualPage;
      }
      //----------------------------- MOCK CODE ------------------------------

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
