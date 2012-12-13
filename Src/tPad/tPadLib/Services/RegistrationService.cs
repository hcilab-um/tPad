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

    public Document ActualDocument { get; set; }

    public ITPadAppContainer Container { get; set; }

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

      // Here goes the machine vision code to find where the device is located based on the camera image
      // -- Beware you can use TPadCore.IsSimulation to determine the parameters for the image, e.g. whether it needs warping.

      //----------------------------- MOCK CODE ------------------------------
      TPadLocation location = new TPadLocation();
      if (TPadCore.Instance.IsSimulation)
      {
        location.Status = LocationStatus.Located;
        location.RotationAngle = Container.RotationAngle;
        location.LocationPx = Container.Location;
        location.LocationCm = new PointF((float)(Container.Location.X / Container.WidthFactor), (float)(Container.Location.Y / Container.HeightFactor));
        location.Document = ActualDocument;
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

      ActualDocument = new Document() { DocumentName = documentFolders[0] };
      String[] pages = Directory.GetFiles(documentFolders[0], "*.png");
      Array.Sort<String>(pages);
      ActualDocument.PageFileNames = pages;
    }

  }

}
