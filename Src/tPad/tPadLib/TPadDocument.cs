using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Xml.Serialization;
using System.Windows;


namespace UofM.HCI.tPad
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

    [XmlIgnore]
    public TPadPage this[int index]
    {
      get { return Pages[index]; }
      set { Pages[index] = value; }
    }

    public Size DocumentSize { get; set; }

    public abstract TPadDocument Clone();

    public bool Equals(TPadDocument other)
    {
      if (ID != other.ID)
        return false;
      if (Folder != other.Folder)
        return false;
      if (FileName != other.FileName)
        return false;
      if (DocumentSize != other.DocumentSize)
        return false;

      return true;
    }
  }

}
