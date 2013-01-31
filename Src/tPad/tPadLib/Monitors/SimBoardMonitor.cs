using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextAdapter;

namespace UofM.HCI.tPab.Monitors
{
  public class SimBoardMonitor : ContextMonitor
  {

    public bool Pause { get; set; }

    public SimBoardMonitor()
    {
      Pause = false;
    }

    protected override void CustomStart()
    {
      if (Pause)
        return;
    }

  }
}
