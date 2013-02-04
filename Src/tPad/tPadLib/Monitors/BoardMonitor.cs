using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;
using System.Windows.Media.Media3D;

namespace UofM.HCI.tPab.Monitors
{

  public struct BoardUpdate 
  {
    public int DeviceOnTopID { get; set; }
    public Point3D Orientation { get; set; }
  }

  public class BoardMonitor : ContextMonitor
  {
    public String COMPort { get; set; }

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
