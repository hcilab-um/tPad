using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

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
