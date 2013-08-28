using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace UofM.HCI.tPad.App.InfSeeking
{
  public class ProviderLauncher : ITPadAppLauncher
  {

    private Guid instanceUUID = Guid.Empty;
    private String instanceIconPath = null;

    public ProviderLauncher(ProviderGroup group, int instanceCounter) 
    {
      instanceUUID = Guid.Parse(String.Format("00000000-0000-0000-000{0}-0000000000{1:D2}", (int)group, instanceCounter));
      instanceIconPath = String.Format(@"{0}\Images\InfProvider{1}{2}.png", Environment.CurrentDirectory, group, instanceCounter);
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
        AppUUID = instanceUUID,
        Launcher = this
      };

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      InfProviderApp provider = new InfProviderApp(core, container, controller, descriptor);
      provider.Activate(settings.Context);
      return provider;
    }

    public void Prepare() { }
  }
}
