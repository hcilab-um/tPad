using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextService;
using System.Drawing;

namespace UofM.HCI.tPab.Services
{
  public class RegistrationService : ContextService
  {
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
      location.Status = LocationStatus.Locating;
      //----------------------------- MOCK CODE ------------------------------


      NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(TPadLocation), location));
    }

  }
}
