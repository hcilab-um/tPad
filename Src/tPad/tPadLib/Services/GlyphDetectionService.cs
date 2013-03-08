using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextService;
using System.Drawing;
using UofM.HCI.tPab.Monitors;

namespace UofM.HCI.tPab.Services
{
  public class GlyphDetectionService : ContextService
  {   

    private ManagedA.wrapperRegistClass featureTracker;

    private TPadDevice Device { get; set; }

    public GlyphDetectionService(TPadDevice device)
    {
      Device = device;
      featureTracker = new ManagedA.wrapperRegistClass(false, 1);
    }

    protected override void CustomUpdateMonitorReading(object sender, Ubicomp.Utils.NET.CAF.ContextAdapter.NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(Bitmap))
        return;
      if (Device.State == StackingState.StackedOnTop)
        return;
      if (!(sender is SimCameraMonitor))
        return;

      //System.Drawing.Bitmap camView = (System.Drawing.Bitmap)e.NewObject;
      //featureTracker.SetCameraImg(camView);
      //ManagedA.Glyphs result = featureTracker.DetectFigures(25, 80, 105);
      //Console.WriteLine(result.numberSquares + " " + result.numberTriangles);
      
      List<Applications.Glyph> detectedGlyphs = new List<Applications.Glyph>();
      var simulatorDevice = ((sender as SimCameraMonitor).CameraSource as SimulatorDevice).CalculatorGlyph;
      if (simulatorDevice == true)
        detectedGlyphs.Add(Applications.Glyph.Square);

      NotifyContextServiceListeners(this, new NotifyContextServiceListenersEventArgs(typeof(GlyphDetectionService), detectedGlyphs));
    }

  }
}
