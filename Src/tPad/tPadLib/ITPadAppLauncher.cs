using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace UofM.HCI.tPab
{
  public interface ITPadAppLauncher
  {
    void StartCore(String comPort = null, String cameraPort = null);
    void CloseAll(UIElement sender);
  }
}
