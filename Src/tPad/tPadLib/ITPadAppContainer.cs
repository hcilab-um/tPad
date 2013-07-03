using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace UofM.HCI.tPad
{

  public interface ITPadAppContainer
  {
    void LoadTPadApp(ITPadApp tPadApp, bool foreground = true);
    void Hide(ITPadApp tPadApp);
    void Show(ITPadApp tPadApp);
  }

}

