using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Windows.Controls;


namespace UofM.HCI.tPab
{

  public abstract class TPadDocument
  {
    public int ID { get; set; }
    public String Folder { get; set; }
    public String FileName { get; set; }

    public TPadPage[] Pages { get; set; }

    public abstract TPadDocument Clone();
  }

}
