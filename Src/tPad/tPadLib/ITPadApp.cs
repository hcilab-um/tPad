using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad
{

  public interface ITPadApp
  {
    event EventHandler Closed;

    TPadCore Core { get; set; }
    ITPadAppContainer Container { get; set; }
    ITPadAppController Controller { get; set; }

    void Close();
  }

}
