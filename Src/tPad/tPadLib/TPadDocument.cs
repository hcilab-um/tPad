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

  public abstract class TPadDocument
  {
    [XmlAttribute]
    public int ID { get; set; }

    [XmlAttribute]
    public String Folder { get; set; }
    [XmlAttribute]
    public String FileName { get; set; }

    [XmlArray]
    public TPadPage[] Pages { get; set; }

    public abstract TPadDocument Clone();
  }

}
