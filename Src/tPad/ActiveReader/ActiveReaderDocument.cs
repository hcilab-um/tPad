using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UofM.HCI.tPab.App.ActiveReader
{
  public class ActiveReaderDocument : TPadDocument
  {
    public bool HasFigureLinks
    {
      get
      {
        foreach (TPadPage page in Pages)
        {
          ActiveReaderPage arPage = (ActiveReaderPage)page;
          if (arPage.FigureLinks.Count > 0)
            return true;
        }
        return false;
      }
    }

    public ActiveReaderPage this[int index]
    {
      get
      {
        return Pages[index] as ActiveReaderPage;
      }

      set
      {
        Pages[index] = value;
      }
    }

    public override TPadDocument Clone()
    {
      ActiveReaderDocument clone = new ActiveReaderDocument();
      clone.ID = ID;
      clone.Folder = Folder;
      clone.FileName = FileName;

      clone.Pages = new TPadPage[Pages.Length];
      for (int index = 0; index < Pages.Length; index++)
        clone[index] = this[index].Clone() as ActiveReaderPage;

      return clone;
    }

  }
}
