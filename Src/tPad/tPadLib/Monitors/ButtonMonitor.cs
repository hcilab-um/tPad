using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPad.Monitors
{

  public enum ButtonEvent { None, Single, Double };

  public class ButtonMonitor : ContextMonitor, IContextMonitorListener
  {

    /// <summary>
    /// Here it receives the messages from the arduino sensor
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void UpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(BoardUpdate))
        return;

      BoardUpdate boardInfo = (BoardUpdate)e.NewObject;
      NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(ButtonMonitor), boardInfo.ButtonEvent));
    }
  }
}
