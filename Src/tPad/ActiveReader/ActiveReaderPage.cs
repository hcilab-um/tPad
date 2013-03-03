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
using System.Windows.Ink;
using System.Windows;

namespace UofM.HCI.tPab.App.ActiveReader
{
  public class ActiveReaderPage : TPadPage
  {
    [XmlIgnore]
    public ObservableCollection<IActiveReaderMarker> Highlights { get; set; }

    [XmlIgnore]
    public ObservableCollection<IActiveReaderMarker> Annotations { get; set; }

    [XmlIgnore]
    public ObservableCollection<IActiveReaderMarker> SearchResults { get; set; }

    //[XmlIgnore]
    //public ObservableCollection<IActiveReaderMarker> Scribblings { get; set; }

    [XmlIgnore]
    public ObservableCollection<IActiveReaderMarker> ScribblingCollections { get; set; }

    [XmlIgnore]
    public ObservableCollection<IActiveReaderMarker> FigureLinks { get; set; }

    public Highlight[] HighlightsArray
    {
      get { return Highlights.Cast<Highlight>().ToArray(); }
      set { Highlights = new ObservableCollection<IActiveReaderMarker>(value); }
    }

    public Note[] AnnotationsArray
    {
      get { return Annotations.Cast<Note>().ToArray(); }
      set { Annotations = new ObservableCollection<IActiveReaderMarker>(value); }
    }

    public Highlight[] SearchResultsArray
    {
      get { return SearchResults.Cast<Highlight>().ToArray(); }
      set { SearchResults = new ObservableCollection<IActiveReaderMarker>(value); }
    }

    //public Scribble[] ScribblingsArray
    //{
    //  get { return Scribblings.Cast<Scribble>().ToArray(); }
    //  set { Scribblings = new ObservableCollection<IActiveReaderMarker>(value); }
    //}

    public ScribbleCollection[] ScribbleCollectionArray
    {
      get { return ScribblingCollections.Cast<ScribbleCollection>().ToArray(); }
      set { ScribblingCollections = new ObservableCollection<IActiveReaderMarker>(value); }
    }

    public Highlight[] FigureLinksArray
    {
      get { return FigureLinks.Cast<Highlight>().ToArray(); }
      set { FigureLinks = new ObservableCollection<IActiveReaderMarker>(value); }
    }

    public ActiveReaderPage()
    {
      FileName = String.Empty;
      Highlights = new ObservableCollection<IActiveReaderMarker>();
      Annotations = new ObservableCollection<IActiveReaderMarker>();
      SearchResults = new ObservableCollection<IActiveReaderMarker>();
      //Scribblings = new ObservableCollection<IActiveReaderMarker>();
      ScribblingCollections = new ObservableCollection<IActiveReaderMarker>();
      FigureLinks = new ObservableCollection<IActiveReaderMarker>();
    }

    public ActiveReaderPage(String fileName = null)
    {
      FileName = fileName;
      Highlights = new ObservableCollection<IActiveReaderMarker>();
      Annotations = new ObservableCollection<IActiveReaderMarker>();
      SearchResults = new ObservableCollection<IActiveReaderMarker>();
      //Scribblings = new ObservableCollection<IActiveReaderMarker>();
      ScribblingCollections = new ObservableCollection<IActiveReaderMarker>();
      FigureLinks = new ObservableCollection<IActiveReaderMarker>();
    }

    public override TPadPage Clone()
    {
      ActiveReaderPage clone = new ActiveReaderPage();
      clone.FileName = FileName;
      clone.PageIndex = PageIndex;

      clone.Highlights = new ObservableCollection<IActiveReaderMarker>();
      foreach (IActiveReaderMarker highlight in Highlights)
        clone.Highlights.Add(highlight.Clone() as Highlight);

      clone.Annotations = new ObservableCollection<IActiveReaderMarker>();
      foreach (IActiveReaderMarker annotation in Annotations)
        clone.Annotations.Add(annotation.Clone() as Note);

      clone.SearchResults = new ObservableCollection<IActiveReaderMarker>();
      foreach (IActiveReaderMarker result in SearchResults)
        clone.SearchResults.Add(result.Clone() as Highlight);

      //clone.Scribblings = new ObservableCollection<IActiveReaderMarker>();
      //foreach (IActiveReaderMarker scribbling in Scribblings)
      //  clone.Scribblings.Add(scribbling.Clone() as Scribble);

      clone.ScribblingCollections = new ObservableCollection<IActiveReaderMarker>();
      foreach (IActiveReaderMarker scribble in ScribblingCollections)
        clone.ScribblingCollections.Add(scribble.Clone() as ScribbleCollection);

      clone.FigureLinks = new ObservableCollection<IActiveReaderMarker>();
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

    public Point Position
    {
      get { return new Point(X, Y); }
    }

    public IActiveReaderMarker Clone()
    {
      Note clone = new Note();
      clone.AnnotationXAML = AnnotationXAML;
      clone.IconXAML = IconXAML;
      return clone;
    }
  }

  //public class Scribble : IActiveReaderMarker
  //{
  //  [XmlIgnore]
  //  public InkCanvas Scribbling { get; set; }

  //  public String ScribblingXAML
  //  {
  //    get { return XamlWriter.Save(Scribbling); }
  //    set
  //    {
  //      String scribblingXaml = value;
  //      StringReader stringReader = new StringReader(scribblingXaml);
  //      XmlReader xmlReader = XmlReader.Create(stringReader);
  //      Scribbling = (InkCanvas)XamlReader.Load(xmlReader);
  //    }
  //  }

  //  [XmlIgnore]
  //  public Image Icon { get; set; }

  //  public String IconXAML
  //  {
  //    get { return XamlWriter.Save(Icon); }
  //    set
  //    {
  //      String iconXaml = value;
  //      StringReader stringReader = new StringReader(iconXaml);
  //      XmlReader xmlReader = XmlReader.Create(stringReader);
  //      Icon = (Image)XamlReader.Load(xmlReader);
  //    }
  //  }

  //  public double X
  //  {
  //    get { return Icon.Margin.Left; }
  //  }

  //  public double Y
  //  {
  //    get { return Icon.Margin.Top; }
  //  }

  //  public System.Drawing.PointF Position
  //  {
  //    get { return new System.Drawing.PointF((float)X, (float)Y); }
  //  }

  //  public IActiveReaderMarker Clone()
  //  {
  //    Scribble clone = new Scribble();
  //    clone.ScribblingXAML = ScribblingXAML;
  //    clone.IconXAML = IconXAML;
  //    return clone;
  //  }
  //}

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

    public Point Position
    {
      get { return new Point(X, Y); }
    }

    public IActiveReaderMarker Clone()
    {
      Highlight clone = new Highlight();
      clone.LineXAML = LineXAML;
      return clone;
    }
  }

  public class ScribbleCollection : IActiveReaderMarker
  {
    [XmlIgnore]
    public StrokeCollection ScribblingCollection { get; set; }

    public String ScribblingXAML
    {
      get { return XamlWriter.Save(ScribblingCollection); }
      set
      {
        String inkCanvasXaml = value;
        StringReader stringReader = new StringReader(inkCanvasXaml);
        XmlReader xmlReader = XmlReader.Create(stringReader);
        ScribblingCollection = (StrokeCollection)XamlReader.Load(xmlReader);
      }
    }

    public double X
    {
      get { return ScribblingCollection.GetBounds().BottomLeft.X + (ScribblingCollection.GetBounds().Width / 2.0); }
    }

    public double Y
    {
      get { return ScribblingCollection.GetBounds().TopLeft.Y + (ScribblingCollection.GetBounds().Height / 2.0); }
    }

    public Point Position
    {
      get { return new Point(X, Y); }
    }

    public IActiveReaderMarker Clone()
    {
      ScribbleCollection clone = new ScribbleCollection();
      clone.ScribblingXAML = ScribblingXAML;
      return clone;
    }
  }

}
