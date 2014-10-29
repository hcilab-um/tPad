using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad.App.Browser
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
        Name = "Browser",
        Icon = UofM.HCI.tPad.App.Browser.Properties.Resources.BrowserIcon,
        AppType = typeof(BrowserApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };
      descriptor.Actions.Add(ActionRequest.WebBrowser);

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      BrowserApp browser = new BrowserApp(core, container, controller, descriptor.AppUUID);
      browser.Activate(settings.Context);
      return browser;
    }

    public void Prepare() { }

  }
}
