using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using UofM.HCI.tPad.Util;
using System.Drawing.Imaging;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPad.Monitors
{
  public class SimCameraMonitor : ContextMonitor
  {
    private ManagedA.wrapperFeatureMatcher Matcher;

    public ManagedA.wrapperRegistClass Tracker { get; set; }
    
    public bool Pause { get; set; }

    private ITPadAppController cameraSource = null;
    public ITPadAppController CameraSource 
    {
      get { return cameraSource; }
      set
      {
        cameraSource = value;
        cameraSource.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(cameraSource_PropertyChanged);
      }
    }

    public SimCameraMonitor()
    {
      Pause = false;
    }

    protected override void CustomStart()
    {
      StartFeatureTracker();
    }
        
    private bool isStarted = false;
    public void StartFeatureTracker()
    {
      try
      {
        if (Tracker == null)
        {
          Matcher = new ManagedA.wrapperFeatureMatcher(false, Environment.CurrentDirectory + "\\" + CameraSource.ActualDocument.Folder);
          Tracker = new ManagedA.wrapperRegistClass(false, CameraSource.SimCaptureToSourceImageRatio, Matcher);
        }
        //Tracker.createIndex(Environment.CurrentDirectory + "\\" + Controller.ActualDocument.Folder);
        Tracker.computeWarpMatrix(CameraSource.SimCaptureToSourceImageRatio);
        isStarted = true;
      }
      catch { return; }
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

      if (TPadCore.UseFeatureTracking)
      {
        Tracker.SetCameraImg(rotatedView);
        //rotatedView.Save("meinz.png");
      }
      //using (FileStream storage = CreateFileStream(angle))
      //  rotatedView.Save(storage, ImageFormat.Png);
    }

    private FileStream CreateFileStream(float angle)
    {
      String fileName = String.Format("capture-{0}-{1}.PNG", (angle + 360) % 360, DateTime.Now.Ticks);
      return new FileStream(fileName, FileMode.OpenOrCreate);
    }

    void cameraSource_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      CustomRun();
    }

  }
}

