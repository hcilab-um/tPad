using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad.App.SurfaceCapture
{
  public class Launcher : ITPadAppLauncher
  {
    public TPadLauncherSettings GetSettings(TPadLauncherSettings settings)
    {
      throw new NotImplementedException();
    }

    public TPadApplicationDescriptor GetApplicationDescriptor()
    {
      TPadApplicationDescriptor descriptor = new TPadApplicationDescriptor()
      {
        Name = "Capture",
        Icon = UofM.HCI.tPad.App.SurfaceCapture.Properties.Resources.SurfaceCaptureIcon,
        AppType = typeof(SurfaceCaptureApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      SurfaceCaptureApp capture = new SurfaceCaptureApp(core, container, controller, descriptor.AppUUID);
      capture.Activate(settings.Context);
      return capture;
    }

    public void Prepare() { }

  }
}
