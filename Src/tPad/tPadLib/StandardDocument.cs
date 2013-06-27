using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPad
{
  public class StandardDocument : TPadDocument
  {
    public override TPadDocument Clone()
    {
      StandardDocument clone = new StandardDocument();
      clone.ID = ID;
      clone.Folder = Folder;
      clone.FileName = FileName;
      clone.DocumentSize = DocumentSize;

      clone.Pages = new TPadPage[Pages.Length];
      for (int index = 0; index < Pages.Length; index++)
        clone[index] = this[index].Clone();

      return clone;
    }
  }

  public class StandardPage : TPadPage
  {
    public override TPadPage Clone()
    {
      StandardPage clone = new StandardPage();
      clone.FileName = FileName;
      clone.PageIndex = PageIndex;

      return clone;
    }
  }
}
