using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad
{

  public class ObjectEventArgs : EventArgs 
  {
    public Object Parameter { get; set; }
  }

  public delegate bool BoolEventHandler(object sender, ObjectEventArgs e);

  public delegate void RequestUserFocus(object sender, String message, String buttonOK, String buttonCancel);

  public enum ActionRequest { WebBrowser, Sleep };
  public delegate void RequestAction(object sender, ActionRequest action, Dictionary<String, Object> context);

  public interface ITPadApp
  {
    event EventHandler Closed;
    event BoolEventHandler IsTopApp;
    event RequestUserFocus RequestFocus;
    event RequestAction RequestAction;

    TPadCore Core { get; set; }
    ITPadAppContainer Container { get; set; }
    ITPadAppController Controller { get; set; }
    Guid AppUUID { get; }

    Dictionary<String, Object> Context { get; }
    void Activate(Dictionary<String, Object> remoteContext);
    void DeActivate();
    void Close();
  }
}
