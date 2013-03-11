using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPab.Monitors
{
  public class CameraMonitor : ContextMonitor
  {

    private bool useCamera = true;

    public ManagedA.wrapperRegistClass Tracker { get; set; }

    public ITPadAppController Controller { get; set; }

    public CameraMonitor(bool useC)
    {
      useCamera = useC;
    }

    protected override void CustomStart()
    {
      if (useCamera)
        StartFeatureTracker();
    }

    private bool isStarted = false;
    public void StartFeatureTracker()
    {
      try
      {
        if (Tracker == null)
          Tracker = new ManagedA.wrapperRegistClass(useCamera, Controller.SimCaptureToSourceImageRatio);
        Tracker.createIndex(Environment.CurrentDirectory + "\\" + Controller.ActualDocument.Folder);
        Tracker.imageWarp("homography.xml");
        isStarted = true;
      }
      catch { return; }
    }

    internal bool IsFeatureTrackerStarted()
    {
      return isStarted;
    }

    internal bool TryPort()
    {
      if (!useCamera)
        return false;

      try
      {
        if (Tracker == null)
          Tracker = new ManagedA.wrapperRegistClass(useCamera, Controller.SimCaptureToSourceImageRatio);
        if (Tracker.connectCamera() != -1)
          return true;
      }
      catch { return false; }

      return false;
    }

    protected override void CustomRun()
    {
      if (useCamera)
        Tracker.SetCameraImg();
      NotifyContextServices(this, null);
    }

    protected override void CustomStop()
    {
      try
      {
        if (Tracker == null)
          return;

        Tracker.disconnectCamera();
      }
      catch { return; }
    }
  }
}
