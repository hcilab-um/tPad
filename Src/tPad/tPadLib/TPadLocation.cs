using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace UofM.HCI.tPab
{

  public enum LocationStatus { NotLocated, Locating, Located }

  public struct TPadLocation
  {
    /// <summary>
    /// This is the LocationStatus of the registration process.
    /// </summary>
    public LocationStatus Status { get; set; }

    /// <summary>
    /// These page-variables correspond to the index of the current page on the collection of pages the RegistrationService searches on.
    /// </summary>
    public int PageIndex { get; set; }
    public String PageName { get; set; }

    /// <summary>
    /// This is the location of the center of the device.
    /// </summary>
    public Size LocationPx { get; set; }
    public Size LocationCm { get; set; }

    /// <summary>
    /// This angle is calculated relative to the vertical axis of the document.
    /// </summary>
    public float RotationAngle { get; set; }

  }

}
