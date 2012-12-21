using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Shapes;

namespace UofM.HCI.tPab
{
  public class TPadDocument
  {
    public String DocumentName { get; set; }
    public TPadPage[] Pages { get; set; }
  }

  public class TPadPage
  {
    public String FileName { get; set; }
    public Collection<Shape> Highlights { get; set; }

    public TPadPage(String fileName = null)
    {
      FileName = fileName;
      Highlights = new Collection<Shape>();
    }

  }
}
