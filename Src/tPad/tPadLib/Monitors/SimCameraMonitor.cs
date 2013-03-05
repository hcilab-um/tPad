using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using UofM.HCI.tPab.Util;
using System.Drawing.Imaging;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPab.Monitors
{
  public class SimCameraMonitor : ContextMonitor
  {

    public bool Pause { get; set; }

    public ITPadAppController CameraSource { get; set; }

    public SimCameraMonitor()
    {
      Pause = false;
    }

    protected override void CustomRun()
    {
      if (Pause || CameraSource == null)
        return;

      float angle = 0;
      Bitmap deviceView = (CameraSource as SimulatorDevice).GetDeviceView(out angle);
      if (deviceView == null)
        return;

      //EUREKA!!!
      Bitmap rotatedView = ImageHelper.RotateImageByAngle(deviceView, 180 - angle, (CameraSource as SimulatorDevice).ScreenCorrectedAppBounds);
      NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(Bitmap), rotatedView));

      //using (FileStream storage = CreateFileStream(angle))
      //  rotatedView.Save(storage, ImageFormat.Png);
    }

    private FileStream CreateFileStream(float angle)
    {
      String fileName = String.Format("capture-{0}-{1}.PNG", (angle + 360) % 360, DateTime.Now.Ticks);
      return new FileStream(fileName, FileMode.OpenOrCreate);
    }
  }
}

