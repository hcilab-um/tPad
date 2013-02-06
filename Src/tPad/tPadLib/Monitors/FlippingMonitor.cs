using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPab.Monitors
{
  public class FlippingMonitor : ContextMonitor, IContextMonitorListener
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
    }
  }
}
