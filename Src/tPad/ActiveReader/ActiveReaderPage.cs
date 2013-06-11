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

namespace UofM.HCI.tPad.App.ActiveReader
{
  public class ActiveReaderPage : TPadPage
  {
    [Jayrock.Json.Conversion.JsonIgnore]
    [XmlIgnore]
    public ObservableCollection<IActiveReaderMarker> Highlights { get; set; }

    [Jayrock.Json.Conversion.JsonIgnore]
    [XmlIgnore]
    public ObservableCollection<IActiveReaderMarker> Annotations { get; set; }

    [Jayrock.Json.Conversion.JsonIgnore]
    [XmlIgnore]
    public ObservableCollection<IActiveReaderMarker> SearchResults { get; set; }

    [Jayrock.Json.Conversion.JsonIgnore]
    [XmlIgnore]
    public ObservableCollection<IActiveReaderMarker> ScribblingCollections { get; set; }

    [Jayrock.Json.Conversion.JsonIgnore]
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

    [Jayrock.Json.Conversion.JsonIgnore]
    public Highlight[] SearchResultsArray
    {
      get { return SearchResults.Cast<Highlight>().ToArray(); }
      set { SearchResults = new ObservableCollection<IActiveReaderMarker>(value); }
    }

    public ScribbleCollection[] ScribbleCollectionArray
    {
      get { return ScribblingCollections.Cast<ScribbleCollection>().ToArray(); }
      set { ScribblingCollections = new ObservableCollection<IActiveReaderMarker>(value); }
    }

    [Jayrock.Json.Conversion.JsonIgnore]
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
      ScribblingCollections = new ObservableCollection<IActiveReaderMarker>();
      FigureLinks = new ObservableCollection<IActiveReaderMarker>();
    }

    public ActiveReaderPage(String fileName = null)
    {
      FileName = fileName;
      Highlights = new ObservableCollection<IActiveReaderMarker>();
      Annotations = new ObservableCollection<IActiveReaderMarker>();
      SearchResults = new ObservableCollection<IActiveReaderMarker>();
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

    public Guid ID { get; set; }

    [Jayrock.Json.Conversion.JsonIgnore]
    [XmlIgnore]
    public StickyNote Annotation { get; set; }

    public StickyNoteHelper AnnotationObject
    {
      get
      {
        StickyNoteHelper helper = new StickyNoteHelper();
        helper.Text = Annotation.getTextField().Text;
        helper.X = Annotation.Margin.Left;
        helper.Y = Annotation.Margin.Top;
        helper.Width = Annotation.Width;
        helper.Height = Annotation.Height;
        helper.WidthFactor = Annotation.WidthFactor;
        helper.HeightFactor = Annotation.HeightFactor;
        return helper;
      }
      set
      {
        StickyNoteHelper helper = value;
        Annotation = new StickyNote(helper.X, helper.Y);
        Annotation.WidthFactor = helper.WidthFactor;
        Annotation.HeightFactor = helper.HeightFactor;
        Annotation.Width = helper.Width;
        Annotation.Height = helper.Height;
        Annotation.getTextField().Text = helper.Text;
      }
    }

    [Jayrock.Json.Conversion.JsonIgnore]
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

    public Note()
    {
      ID = Guid.NewGuid();
    }

    public IActiveReaderMarker Clone()
    {
      Note clone = new Note();
      clone.AnnotationObject = AnnotationObject;
      clone.IconXAML = IconXAML;
      return clone;
    }

    public class StickyNoteHelper
    {
      public double X { get; set; }
      public double Y { get; set; }
      public double Width { get; set; }
      public double Height { get; set; }
      public double HeightFactor { get; set; }
      public double WidthFactor { get; set; }
      public String Text { get; set; }
    }

  }

  public class Highlight : IActiveReaderMarker
  {

    public Guid ID { get; set; }

    [Jayrock.Json.Conversion.JsonIgnore]
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

    public Highlight()
    {
      ID = Guid.NewGuid();
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

    public Guid ID { get; set; }

    [Jayrock.Json.Conversion.JsonIgnore]
    [XmlIgnore]
    public StrokeCollection Scribbles { get; set; }

    public String ScribblingXAML
    {
      get { return XamlWriter.Save(Scribbles); }
      set
      {
        String inkCanvasXaml = value;
        StringReader stringReader = new StringReader(inkCanvasXaml);
        XmlReader xmlReader = XmlReader.Create(stringReader);
        Scribbles = (StrokeCollection)XamlReader.Load(xmlReader);
      }
    }

    public double X
    {
      get { return Scribbles.GetBounds().BottomLeft.X + (Scribbles.GetBounds().Width / 2.0); }
    }

    public double Y
    {
      get { return Scribbles.GetBounds().TopLeft.Y + (Scribbles.GetBounds().Height / 2.0); }
    }

    public Point Position
    {
      get { return new Point(X, Y); }
    }

    public ScribbleCollection()
    {
      ID = Guid.NewGuid();
    }

    public IActiveReaderMarker Clone()
    {
      ScribbleCollection clone = new ScribbleCollection();
      clone.ScribblingXAML = ScribblingXAML;
      return clone;
    }
  }
}
