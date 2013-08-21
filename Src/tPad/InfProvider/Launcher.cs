using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace UofM.HCI.tPad.App.InfProvider
{
  public class Launcher : ITPadAppLauncher
  {

    private String instanceIconPath = null;

    public Launcher(Provier int instanceCounter) 
    {
      instanceIconPath = String.Format(@"{0}\Images\InfProvider{1}{2}.png", Environment.CurrentDirectory, "Blue", instanceCounter)

      instanceIconPath = iconPath;
    }

    public TPadLauncherSettings GetSettings(TPadLauncherSettings settings)
    {
      throw new NotImplementedException();
    }

    public TPadApplicationDescriptor GetApplicationDescriptor()
    {
      TPadApplicationDescriptor descriptor = new TPadApplicationDescriptor()
      {
        Name = "InfProvider",
        Icon = instanceIconPath,
        AppType = typeof(InfProviderApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      InfProviderApp provider = new InfProviderApp(core, container, controller, descriptor.AppUUID);

      return provider;
    }

    public void Prepare() { }
  }
}
