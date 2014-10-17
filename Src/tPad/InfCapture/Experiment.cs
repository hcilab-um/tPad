using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad.App.InfCapture
{
  public enum Device { Normal, tPad };
  public enum PictureMode { Normal, Clipped };
  public enum TargetSize { Quarter, Half, ThreeQuarters };

  public enum ClippingState { Capturing, Clipping };

  public class Exp2Condition
  {
    public Device Device { get; set; }
    public PictureMode PictureMode { get; set; }
    public TargetSize TargetSize { get; set; }

    public List<Exp2Capture> Captures { get; set; }

    public Exp2Condition()
    {
      Captures = new List<Exp2Capture>();
    }

    public override string ToString()
    {
      return String.Format("Device: {0} - PictureMode: {1} - TargetSize: {2}", Device, PictureMode, TargetSize);
    }
  }

  public class Exp2Capture
  {
    public int Page { get; set; }
    public int Figure { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public TimeSpan CaptureTime
    {
      get { return EndTime - StartTime; }
    }

    public override string ToString()
    {
      return String.Format("Page: {0} - Figure: {1}", Page, Figure);
    }
  }
}
