using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPad.Monitors
{
  public class HomeMonitor : ContextMonitor, IContextMonitorListener
  {

    public DateTime LastEvent { get; set; }

    public void UpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(BoardUpdate))
        return;

      BoardUpdate update = (BoardUpdate)e.NewObject;
      if (update.Home)
      {
        LastEvent = DateTime.Now;
        NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(HomeMonitor), LastEvent));
      }
    }
  }
}
