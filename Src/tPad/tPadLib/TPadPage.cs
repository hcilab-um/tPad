using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace UofM.HCI.tPab
{

  public abstract class TPadPage
  {
    [XmlAttribute]
    public int PageIndex { get; set; }

    [XmlAttribute]
    public String FileName { get; set; }

    public TPadPage()
    {
      PageIndex = -1;
      FileName = String.Empty;
    }

    public TPadPage(String fileName = null)
    {
      PageIndex = -1;
      FileName = fileName;
    }

    public abstract TPadPage Clone();
  }

}
