using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextAdapter;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;

namespace UofM.HCI.tPab.Monitors
{
  public class SimCameraMonitor : ContextMonitor
  {

    public UIElement CameraSource { get; set; }

    protected override void CustomRun()
    {
      float angle = 0;
      Bitmap deviceView = (CameraSource as Simulator).GetDeviceView(out angle);
      if (deviceView == null)
        return;

      using (FileStream storage = CreateFileStream(angle))
        deviceView.Save(storage, System.Drawing.Imaging.ImageFormat.Png);
    }

    private FileStream CreateFileStream(float angle)
    {
      String fileName = String.Format("capture-{0}-{1}.PNG", (angle + 360) % 360, DateTime.Now.Ticks);
      return new FileStream(fileName, FileMode.OpenOrCreate);
    }
  }
}
