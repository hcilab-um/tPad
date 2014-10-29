using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextService;
using System.Drawing;
using UofM.HCI.tPad.Monitors;

namespace UofM.HCI.tPad.Services
{
  public class GlyphDetectionService : ContextService
  {

    private bool isProcessStopped = false;

    private ManagedA.wrapperRegistClass Tracker { get; set; }

    private TPadDevice Device { get; set; }

    public GlyphDetectionService(TPadDevice device)
    {
      Device = device;
      //featureTracker = new ManagedA.wrapperRegistClass(false, 1);
    }

    public void Pause()
    {
      isProcessStopped = true;
    }

    public void Continue()
    {
      isProcessStopped = false;
    }

    protected override void CustomUpdateMonitorReading(object sender, Ubicomp.Utils.NET.CAF.ContextAdapter.NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(Bitmap))
        return;
      if (Device.State == StackingState.StackedOnTop)
        return;
      if (isProcessStopped)
        return;

      if (sender is SimCameraMonitor)
      {
        List<Glyph> detectedGlyphs = new List<Glyph>();
        var simulatorDevice = ((sender as SimCameraMonitor).CameraSource as SimulatorDevice).CalculatorGlyph;
        if (simulatorDevice == true)
          detectedGlyphs.Add(Glyph.Square);
        NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(GlyphDetectionService), detectedGlyphs));
      }
      else if (sender is CameraMonitor)
      {
        if (Tracker == null)
          Tracker = (sender as CameraMonitor).Tracker;
        if (Tracker == null)
          return;

        List<Glyph> detectedGlyphs = new List<Glyph>();
        ManagedA.Glyphs result = Tracker.DetectFigures(20, 80, 150);
        for(int i = 0 ; i < result.numberSquares ; i++)
          detectedGlyphs.Add(Glyph.Square);
        for (int i = 0; i < result.numberTriangles; i++)
          detectedGlyphs.Add(Glyph.Triangle);

        NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(GlyphDetectionService), detectedGlyphs));
      }
    }

  }
}
