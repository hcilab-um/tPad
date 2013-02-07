using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace UofM.HCI.tPab
{
  public interface ITPadAppLauncher
  {
    ITPadApp GetAppInstance(ITPadAppContainer container, ITPadAppController controller, String boardPort, bool useCamera, int deviceID);
  }
}
