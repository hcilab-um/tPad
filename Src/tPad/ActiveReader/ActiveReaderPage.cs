using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace UofM.HCI.tPab.App.ActiveReader
{
  public class ActiveReaderPage : TPadPage
  {
    public ObservableCollection<Highlight> Highlights { get; set; }
    public ObservableCollection<Note> Annotations { get; set; }
    public ObservableCollection<Highlight> SearchResults { get; set; }
    public ObservableCollection<Scribble> Scribblings { get; set; }
    public ObservableCollection<Highlight> FigureLinks { get; set; }

    public ActiveReaderPage()
    {
      FileName = String.Empty;
      Highlights = new ObservableCollection<Highlight>();
      Annotations = new ObservableCollection<Note>();
      SearchResults = new ObservableCollection<Highlight>();
      Scribblings = new ObservableCollection<Scribble>();
      FigureLinks = new ObservableCollection<Highlight>();
    }

    public ActiveReaderPage(String fileName = null)
    {
      FileName = fileName;
      Highlights = new ObservableCollection<Highlight>();
      Annotations = new ObservableCollection<Note>();
      SearchResults = new ObservableCollection<Highlight>();
      Scribblings = new ObservableCollection<Scribble>();
      FigureLinks = new ObservableCollection<Highlight>();
    }

    public override TPadPage Clone()
    {
      ActiveReaderPage clone = new ActiveReaderPage();
      clone.FileName = FileName;
      clone.PageIndex = PageIndex;

      clone.Highlights = new ObservableCollection<Highlight>();
      foreach (IActiveReaderMarker highlight in Highlights)
        clone.Highlights.Add(highlight.Clone() as Highlight);

      clone.Annotations = new ObservableCollection<Note>();
      foreach (IActiveReaderMarker annotation in Annotations)
        clone.Annotations.Add(annotation.Clone() as Note);

      clone.SearchResults = new ObservableCollection<Highlight>();
      foreach (IActiveReaderMarker result in SearchResults)
        clone.SearchResults.Add(result.Clone() as Highlight);

      clone.Scribblings = new ObservableCollection<Scribble>();
      foreach (IActiveReaderMarker scribbling in Scribblings)
        clone.Scribblings.Add(scribbling.Clone() as Scribble);

      clone.FigureLinks = new ObservableCollection<Highlight>();
      foreach (IActiveReaderMarker link in FigureLinks)
        clone.FigureLinks.Add(link.Clone() as Highlight);

      return clone;
    }
  }

  public class Note : IActiveReaderMarker
  {
    [XmlIgnore]
    public StickyNote Annotation { get; set; }

    public String AnnotationXAML
    {
      get { return XamlWriter.Save(Annotation); }
      set
      {
        String annotationXaml = value;
        StringReader stringReader = new StringReader(annotationXaml);
        XmlReader xmlReader = XmlReader.Create(stringReader);
        Annotation = (StickyNote)XamlReader.Load(xmlReader);
      }
    }

    [XmlIgnore]
    public Image Icon { get; set; }

    public String IconXAML
    {
      get { return XamlWriter.Save(Icon); }
      set
      {
        String iconXaml = value;
        StringReader stringReader = new StringReader(iconXaml);
        XmlReader xmlReader = XmlReader.Create(stringReader);
        Icon = (Image)XamlReader.Load(xmlReader);
      }
    }

    public double X
    {
      get { return Icon.Margin.Left; }
    }

    public double Y
    {
      get { return Icon.Margin.Top; }
    }

    public System.Drawing.PointF Position
    {
      get { return new System.Drawing.PointF((float)X, (float)Y); }
    }

    public IActiveReaderMarker Clone()
    {
      Note clone = new Note();
      clone.AnnotationXAML = AnnotationXAML;
      clone.IconXAML = IconXAML;
      return clone;
    }
  }

  public class Scribble : IActiveReaderMarker
  {
    [XmlIgnore]
    public InkCanvas Scribbling { get; set; }

    public String ScribblingXAML
    {
      get { return XamlWriter.Save(Scribbling); }
      set
      {
        String scribblingXaml = value;
        StringReader stringReader = new StringReader(scribblingXaml);
        XmlReader xmlReader = XmlReader.Create(stringReader);
        Scribbling = (InkCanvas)XamlReader.Load(xmlReader);
      }
    }

    [XmlIgnore]
    public Image Icon { get; set; }

    public String IconXAML
    {
      get { return XamlWriter.Save(Icon); }
      set
      {
        String iconXaml = value;
        StringReader stringReader = new StringReader(iconXaml);
        XmlReader xmlReader = XmlReader.Create(stringReader);
        Icon = (Image)XamlReader.Load(xmlReader);
      }
    }

    public double X
    {
      get { return Icon.Margin.Left; }
    }

    public double Y
    {
      get { return Icon.Margin.Top; }
    }

    public System.Drawing.PointF Position
    {
      get { return new System.Drawing.PointF((float)X, (float)Y); }
    }

    public IActiveReaderMarker Clone()
    {
      Scribble clone = new Scribble();
      clone.ScribblingXAML = ScribblingXAML;
      clone.IconXAML = IconXAML;
      return clone;
    }
  }

  public class Highlight : IActiveReaderMarker
  {
    [XmlIgnore]
    public Line Line { get; set; }

    public String LineXAML
    {
      get { return XamlWriter.Save(Line); }
      set
      {
        String lineXaml = value;
        StringReader stringReader = new StringReader(lineXaml);
        XmlReader xmlReader = XmlReader.Create(stringReader);
        Line = (Line)XamlReader.Load(xmlReader);
      }
    }

    public double X
    {
      get { return Line.X1 + (Line.X2 - Line.X1) * 0.5; }
    }

    public double Y
    {
      get { return Line.Y1 + (Line.Y2 - Line.Y1) * 0.5; }
    }

    public System.Drawing.PointF Position
    {
      get { return new System.Drawing.PointF((float)X, (float)Y); }
    }

    public IActiveReaderMarker Clone()
    {
      Highlight clone = new Highlight();
      clone.LineXAML = LineXAML;
      return clone;
    }
  }

}
