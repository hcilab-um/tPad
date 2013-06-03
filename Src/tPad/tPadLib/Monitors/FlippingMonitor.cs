using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;

namespace UofM.HCI.tPab.Monitors
{

  public enum FlippingMode { Unknown, FaceUp, FaceDown };

  public class FlippingMonitor : ContextMonitor, IContextMonitorListener
  {

    private FlippingMode ActualFlippingMode { get; set; }

    /// <summary>
    /// Here it receives the messages from the arduino sensor
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void UpdateMonitorReading(object sender, NotifyContextMonitorListenersEventArgs e)
    {
      if (e.Type != typeof(BoardUpdate))
        return;

      BoardUpdate boardInfo = (BoardUpdate)e.NewObject;
      if (boardInfo.FlippingSide == FlippingMode.Unknown)
      {
        //It's comming from the actual board and needs the processing
      }
      else
      {
        if (ActualFlippingMode == boardInfo.FlippingSide)
          return;

        ActualFlippingMode = boardInfo.FlippingSide;
        NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(FlippingMode), ActualFlippingMode));
      }
    }
  }
}
