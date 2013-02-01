using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;

namespace UofM.HCI.tPab.App.ActiveReader
{
  public class Figure
  {
    public int PageIndex {get; set; }
    public int FigureIndex { get; set; }
    public Int32Rect FigureRect { get; set; }
    public string[] TriggerText { get; set; }

    public Figure(int figureIdx, int pageIdx, Int32Rect figRect, string[] triggerTxt)
    {
      this.PageIndex = pageIdx;
      this.FigureIndex = figureIdx;
      this.FigureRect = figRect;
      this.TriggerText = triggerTxt;
    }
  }

}
