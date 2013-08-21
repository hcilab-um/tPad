using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad.App.InfSeeking
{
  public class SeekLauncher : ITPadAppLauncher
  {
    public TPadLauncherSettings GetSettings(TPadLauncherSettings settings)
    {
      throw new NotImplementedException();
    }

    public TPadApplicationDescriptor GetApplicationDescriptor()
    {
      TPadApplicationDescriptor descriptor = new TPadApplicationDescriptor()
      {
        Name = "InfSeeking",
        Icon = UofM.HCI.tPad.App.InfSeeking.Properties.Resources.InfSeekingIcon,
        AppType = typeof(InfSeekingApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      InfSeekingApp seeker = new InfSeekingApp(core, container, controller, descriptor.AppUUID);

      return seeker;
    }

    public void Prepare() { }

  }
}
