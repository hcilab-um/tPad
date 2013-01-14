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
using UofM.HCI.tPab.Util;
using System.Drawing.Imaging;

namespace UofM.HCI.tPab.Monitors
{
  public class SimCameraMonitor : ContextMonitor
  {

    public ITPadAppController CameraSource { get; set; }

    protected override void CustomRun()
    {
      float angle = 0;
      Bitmap deviceView = (CameraSource as Simulator).GetDeviceView(out angle);
      if (deviceView == null)
        return;

      //EUREKA!!!
      Bitmap rotatedView = ImageHelper.RotateImageByAngle(deviceView, 180 - angle, (CameraSource as Simulator).TPadAppBounds);
      NotifyContextServices(this, new CAF.ContextAdapter.NotifyContextMonitorListenersEventArgs(typeof(Bitmap), rotatedView));

      //using (FileStream storage = CreateFileStream(angle))
      //  rotatedView.Save(storage, ImageFormat.Bmp);
    }

    private FileStream CreateFileStream(float angle)
    {
      String fileName = String.Format("capture-{0}-{1}.BMP", (angle + 360) % 360, DateTime.Now.Ticks);
      return new FileStream(fileName, FileMode.OpenOrCreate);
    }

  }
}

