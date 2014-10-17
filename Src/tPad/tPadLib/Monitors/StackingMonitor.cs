using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPad.Monitors
{

  public enum StackingEvent { PhyicalStacking, PhysicalSeparation }

  public struct StackingUpdate
  {
    public int DeviceOnTopID { get; set; }
    public DateTime When { get; set; }
    public StackingEvent Event { get; set; }
  }

  public class StackingMonitor : ContextMonitor, IContextMonitorListener
  {

    private StackingUpdate statusUpdate;

    public StackingMonitor()
    {
      statusUpdate = new StackingUpdate() { DeviceOnTopID = -1, Event = StackingEvent.PhysicalSeparation, When = DateTime.MinValue };
    }

    /// <summary>
    /// Here is receives the messages from the arduino monitor
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void UpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(BoardUpdate))
        return;

      BoardUpdate boardUpdate = (BoardUpdate)e.NewObject;
      if(boardUpdate.DeviceOnTopID == 0)
      {
        if(statusUpdate.Event == StackingEvent.PhyicalStacking)
        {
          statusUpdate.Event = StackingEvent.PhysicalSeparation;
          statusUpdate.When = DateTime.Now;
          NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(StackingUpdate), statusUpdate));
        }
        else if(statusUpdate.Event == StackingEvent.PhysicalSeparation)  {}
      }
      else
      {
        if(statusUpdate.Event == StackingEvent.PhysicalSeparation)
        {
          statusUpdate.Event = StackingEvent.PhyicalStacking;
          statusUpdate.When = DateTime.Now;
          statusUpdate.DeviceOnTopID = boardUpdate.DeviceOnTopID;
          NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(StackingUpdate), statusUpdate));
        }
      }
    }
  }
}
