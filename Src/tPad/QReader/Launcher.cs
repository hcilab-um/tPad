using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad.App.QReader
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
        Name = "QReader",
        Icon = UofM.HCI.tPad.App.QReader.Properties.Resources.QReaderIcon,
        AppType = typeof(QReaderApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      QReaderApp reader = new QReaderApp(core, container, controller, descriptor.AppUUID);
      reader.Activate(settings.Context);
      return reader;
    }

    public void Prepare() { }

  }
}
