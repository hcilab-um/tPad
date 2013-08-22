using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad.App.Calculator
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
        Name = "Calculator",
        Icon = UofM.HCI.tPad.App.Calculator.Properties.Resources.CalculatorIcon,
        AppType = typeof(CalculatorApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };
      descriptor.Triggers.Add(Glyph.Square);

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      CalculatorApp calculator = new CalculatorApp(core, container, controller, descriptor.AppUUID);
      calculator.LoadInitContext(settings.Context);
      return calculator;
    }

    public void Prepare() { }

  }
}
