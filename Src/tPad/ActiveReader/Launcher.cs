using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;

namespace UofM.HCI.tPad.App.ActiveReader
{
  public class Launcher : ITPadAppLauncher
  {

    public TPadLauncherSettings GetSettings(TPadLauncherSettings settings)
    {
      throw new NotImplementedException();
    }

    public TPadApplicationDescriptor GetApplicationDescriptor()
    {
      return new TPadApplicationDescriptor()
      {
        Name = "ActiveReader",
        Icon = UofM.HCI.tPad.App.ActiveReader.Properties.Resources.ActiveReaderIcon,
        AppClass = typeof(ActiveReaderApp),
        Launcher = this
      };
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      ActiveReaderApp appInstance = new ActiveReaderApp(core, container, controller);

      return appInstance;
    }

    public void Prepare() { }

  }

}
