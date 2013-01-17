using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace UofM.HCI.tPab
{
  public class TPadDocument
  {
    public String DocumentName { get; set; }
    public TPadPage[] Pages { get; set; }
  }

  public class Notes
  {
    public TextBox annotation { get; set; }
    public Image icon { get; set; }
  }

  public class TPadPage
  {
    public int PageIndex { get; set; }
    public String FileName { get; set; }
    public ObservableCollection<Line> Highlights { get; set; }
    public ObservableCollection<Notes> Annotations { get; set; }

    public TPadPage(String fileName = null)
    {
      FileName = fileName;
      Highlights = new ObservableCollection<Line>();
      Annotations = new ObservableCollection<Notes>();
    }
  }
}
