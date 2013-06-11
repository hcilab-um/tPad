using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace UofM.HCI.tPad
{
  public class TPadProfile
  {
    public Size Resolution { get; set; }
    public Size ScreenSize { get; set; }
    public Size DeviceSize { get; set; }

    private Size pixelsPerCm = Size.Empty;
    public Size PixelsPerCm
    {
      get
      {
        if (pixelsPerCm == Size.Empty && ScreenSize.Width != 0 && ScreenSize.Height != 0)
          pixelsPerCm = new Size(Resolution.Width / ScreenSize.Width, Resolution.Height / ScreenSize.Height);
        return pixelsPerCm;
      }
    }

    public Size DocumentSizeInPixels(Size docSize)
    {
      Size docSizeInPixels = Size.Empty;
      if (PixelsPerCm != Size.Empty)
        docSizeInPixels = new Size(docSize.Width * PixelsPerCm.Width, docSize.Height * PixelsPerCm.Height);
      return docSizeInPixels;
    }
  }
}
