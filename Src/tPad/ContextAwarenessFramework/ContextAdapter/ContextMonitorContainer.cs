using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ubicomp.Utils.NET.CAF.ContextAdapter
{

  public class ContextMonitorContainer
  {

    private bool monitorsStarted = false;
    private List<ContextMonitor> monitors = new List<ContextMonitor>();
    private Dictionary<ContextMonitor, Thread> threadsHT = new Dictionary<ContextMonitor, Thread>();

    public void AddMonitor(ContextMonitor monitor)
    {
      ThreadStart monitorStart = new ThreadStart(monitor.Run);
      Thread monitorThread = new Thread(monitorStart);
      monitorThread.IsBackground = true;

      monitors.Add(monitor);
      threadsHT.Add(monitor, monitorThread);

      if (monitorsStarted)
        StartMonitor(monitor, monitorThread);
    }

    public void RemoveMonitor(ContextMonitor monitor)
    {
      monitor.Stop();

      monitors.Remove(monitor);
      threadsHT.Remove(monitor);

      if (monitors.Count == 0)
        monitorsStarted = false;
    }

    public void StartMonitors()
    {
      foreach (ContextMonitor monitor in monitors)
      {
        Thread thread = (Thread)threadsHT[monitor];
        StartMonitor(monitor, thread);
      }
      monitorsStarted = true;
    }

    private void StartMonitor(ContextMonitor monitor, Thread thread)
    {
      monitor.Start();
      thread.Start();
    }

    public void StopMonitors()
    {
      foreach (ContextMonitor monitor in monitors)
        monitor.Stop();        
      monitorsStarted = false;
    }

    public ContextMonitor GetContextMonitor(Type cmType)
    {
      foreach (ContextMonitor monitor in monitors)
      {
        if (monitor.GetType() == cmType)
          return monitor;
      }
      return null;
    }
  }

}
