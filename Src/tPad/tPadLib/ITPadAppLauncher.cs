using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace UofM.HCI.tPad
{
  public interface ITPadAppLauncher
  {
    TPadLauncherSettings GetSettings(TPadLauncherSettings settings);
    TPadApplicationDescriptor GetApplicationDescriptor();
    ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings);
  }

  public class TPadLauncherSettings
  {
    public String BoardPort { get; set; }
    public bool UseCamera { get; set; }
    public int DeviceID { get; set; }
    public String MulticastGroup { get; set; }
    public int MulticastPort { get; set; }
    public int MulticastTTL { get; set; }

    public Dictionary<String, String> Context { get; set; }
  }

}
