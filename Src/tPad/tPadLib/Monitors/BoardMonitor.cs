using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CAF.ContextAdapter;

namespace UofM.HCI.tPab.Monitors
{
  public class BoardMonitor : ContextMonitor
  {
    public String COMPort { get; set; }

    protected override void CustomStart()
    {
      if (COMPort == null || COMPort.Length == 0)
        return;
    }
  }
}
