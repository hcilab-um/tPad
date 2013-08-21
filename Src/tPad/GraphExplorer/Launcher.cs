using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad.App.GraphExplorer
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
        Name = "Graph Explorer",
        Icon = UofM.HCI.tPad.App.GraphExplorer.Properties.Resources.GraphExplorerIcon,
        AppType = typeof(GraphExplorerApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      GraphExplorerApp graphExplorer = new GraphExplorerApp(core, container, controller, descriptor.AppUUID);
      return graphExplorer;
    }

    public void Prepare() { }

  }
}
