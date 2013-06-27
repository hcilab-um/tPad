using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;
using System.Drawing;

namespace UofM.HCI.tPad.Monitors
{
  public class CameraMonitor : ContextMonitor
  {
    private bool useCamera = true;

    private ManagedA.wrapperFeatureMatcher Matcher;

    public ManagedA.wrapperRegistClass Tracker { get; set; }

    public ITPadAppController Controller { get; set; }

    public CameraMonitor(bool useC)
    {
      useCamera = useC;
    }

    private bool isStarted = false;
    public void StartFeatureTracker()
    {
      try
      {
        if (Tracker == null)
        {
          //create index of features
          Matcher = new ManagedA.wrapperFeatureMatcher(true, Environment.CurrentDirectory + "\\" + Controller.ActualDocument.Folder);
          // create tracker
          Tracker = new ManagedA.wrapperRegistClass(true, Controller.SimCaptureToSourceImageRatio, Matcher);
        }
        // compute warp matrix (once)
        Tracker.computeWarpMatrix("homography.xml");
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
        {
          Matcher = new ManagedA.wrapperFeatureMatcher(true, Environment.CurrentDirectory + "\\" + Controller.ActualDocument.Folder);
          Tracker = new ManagedA.wrapperRegistClass(true, Controller.SimCaptureToSourceImageRatio, Matcher);
        }
        if (Tracker.connectCamera() != -1)
          return true;
      }
      catch { return false; }

      return false;
    }

    protected override void CustomRun()
    {
      NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(Bitmap), null));
      //NotifyContextServices(this, null);     

      if (useCamera)
      {
        if (!isStarted)
          StartFeatureTracker();

        Tracker.SetCameraImg();
      }
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
