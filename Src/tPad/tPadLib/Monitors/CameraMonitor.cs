using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPab.Monitors
{
  public class CameraMonitor : ContextMonitor
  {
    public string COMPort { get; set; }

    protected override void CustomStart()
    {
      if (COMPort == null || COMPort.Length == 0)
        return;
    }

    internal bool TryPort()
    {
      if (COMPort == null)
        return true;
      return false;
    }
  }
}
