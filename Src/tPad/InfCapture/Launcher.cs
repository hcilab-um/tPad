using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad.App.InfCapture
{
  public class Launcher : ITPadAppLauncher
  {

    private List<Exp2Condition> Conditions { get; set; }
    private int[] ExperimentalOrder { get; set; }

    public TPadLauncherSettings GetSettings(TPadLauncherSettings settings)
    {
      throw new NotImplementedException();
    }

    public TPadApplicationDescriptor GetApplicationDescriptor()
    {
      TPadApplicationDescriptor descriptor = new TPadApplicationDescriptor()
      {
        Name = "InfCapture",
        Icon = UofM.HCI.tPad.App.InfCapture.Properties.Resources.InfCaptureIcon,
        AppType = typeof(InfCaptureApp),
        AppUUID = Guid.NewGuid(),
        Launcher = this
      };
      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      InfCaptureApp capture = new InfCaptureApp(core, container, controller, descriptor.AppUUID);
      capture.Conditions = Conditions;
      capture.ExperimentalOrder = ExperimentalOrder;
      capture.Activate(settings.Context);
      return capture;
    }

    public void Prepare() { }


    public void SetInfCaptureExperiment(List<Exp2Condition> conditions, int[] order)
    {
      Conditions = conditions;
      ExperimentalOrder = order;
    }
  }
}
