 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace UofM.HCI.tPab
{
  public class TPadProfile
  {
    public Size Resolution { get; set; }
    public Size ScreenSize { get; set; }
    public Size DeviceSize { get; set; }
    public Size DocumentSize { get; set; }

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

    private Size documentSizeInPixels = Size.Empty;
    public Size DocumentSizeInPixels
    {
      get 
      {
        if (documentSizeInPixels == Size.Empty && PixelsPerCm != Size.Empty)
          documentSizeInPixels = new Size(DocumentSize.Width * PixelsPerCm.Width, DocumentSize.Height * PixelsPerCm.Height);
        return documentSizeInPixels;
      }
    }

  }
}
