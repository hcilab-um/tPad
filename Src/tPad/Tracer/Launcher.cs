using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UofM.HCI.tPad.App.Tracer;

namespace UofM.HCI.tPad.App.Tracer
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
        Name = "Tracer",
        Icon = UofM.HCI.tPad.App.Tracer.Properties.Resources.TracerIcon,
        AppType = typeof(TracerApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };
      descriptor.Events.Add(TPadEvent.Flipping);

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      TracerApp tracer = new TracerApp(core, container, controller, descriptor.AppUUID);
      return tracer;
    }

    public void Prepare() { }

  }
}
