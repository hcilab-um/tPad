using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextAdapter;

namespace UofM.HCI.tPab.Monitors
{
  public class StackingMonitor : ContextMonitor, IContextMonitorListener
  {
    /// <summary>
    /// Here is receives the messages from the arduino monitor
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void UpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
      throw new NotImplementedException();
    }
  }
}
