using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Shapes;

namespace UofM.HCI.tPab
{
  public class Document
  {
    public String DocumentName { get; set; }
    public Page[] Pages { get; set; }
  }

  public class Page
  {
    public String FileName { get; set; }
    public Collection<Shape> Highlights { get; set; }
  }
}
