using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad.App.SurfaceViewer
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
        Name = "Viewer",
        Icon = UofM.HCI.tPad.App.SurfaceViewer.Properties.Resources.SurfaceViewerIcon,
        AppType = typeof(SurfaceViewerApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      SurfaceViewerApp viewer = new SurfaceViewerApp(core, container, controller, descriptor.AppUUID);
      viewer.Activate(settings.Context);
      return viewer;
    }

    public void Prepare() { }

  }
}
