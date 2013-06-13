using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UofM.HCI.tPad.App.Ruler;

namespace UofM.HCI.tPad.App.Ruler
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
        Name = "Ruler",
        Icon = UofM.HCI.tPad.App.Ruler.Properties.Resources.RulerIcon,
        AppClass = typeof(RulerApp),
        Launcher = this
      };

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      RulerApp ruler = new RulerApp(core, container, controller);
      return ruler;
    }
  }
}
