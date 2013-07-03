using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad
{

  public delegate bool BoolEventHandler(object sender, EventArgs e);

  public delegate void RequestUserFocus(object sender, String message, String buttonOK, String buttonCancel);

  public interface ITPadApp
  {
    event EventHandler Closed;
    event BoolEventHandler IsTopApp;
    event RequestUserFocus RequestFocus;

    TPadCore Core { get; set; }
    ITPadAppContainer Container { get; set; }
    ITPadAppController Controller { get; set; }

    Dictionary<String, String> Context { get; }
    void LoadInitContext(Dictionary<String, String> init);

    void Close();
  }

}
