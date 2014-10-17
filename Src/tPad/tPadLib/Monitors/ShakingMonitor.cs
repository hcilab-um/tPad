using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPad.Monitors
{
  public class ShakingMonitor : ContextMonitor, IContextMonitorListener
  {

    public DateTime LastShake { get; set; }

    public void UpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(BoardUpdate))
        return;

      BoardUpdate update = (BoardUpdate)e.NewObject;
      if (update.Shaked)
      {
        LastShake = DateTime.Now;
        NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(ShakingMonitor), LastShake));
      }
    }
  }
}
