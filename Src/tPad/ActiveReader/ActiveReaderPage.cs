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

namespace UofM.HCI.tPab.App.ActiveReader
{
  public class ActiveReaderPage : TPadPage
  {
    public ObservableCollection<IActiveReaderMarker> Highlights { get; set; }
    public ObservableCollection<IActiveReaderMarker> Annotations { get; set; }
    public ObservableCollection<IActiveReaderMarker> SearchResults { get; set; }
    public ObservableCollection<IActiveReaderMarker> Scribblings { get; set; }
    public ObservableCollection<IActiveReaderMarker> FigureLinks { get; set; }

    public ActiveReaderPage()
    {
      FileName = String.Empty;
      Highlights = new ObservableCollection<IActiveReaderMarker>();
      Annotations = new ObservableCollection<IActiveReaderMarker>();
      SearchResults = new ObservableCollection<IActiveReaderMarker>();
      Scribblings = new ObservableCollection<IActiveReaderMarker>();
      FigureLinks = new ObservableCollection<IActiveReaderMarker>();
    }

    public ActiveReaderPage(String fileName = null)
    {
      FileName = fileName;
      Highlights = new ObservableCollection<IActiveReaderMarker>();
      Annotations = new ObservableCollection<IActiveReaderMarker>();
      SearchResults = new ObservableCollection<IActiveReaderMarker>();
      Scribblings = new ObservableCollection<IActiveReaderMarker>();
      FigureLinks = new ObservableCollection<IActiveReaderMarker>();
    }

    public override TPadPage Clone()
    {
      ActiveReaderPage clone = new ActiveReaderPage();
      clone.FileName = FileName;
      clone.PageIndex = PageIndex;

      clone.Highlights = new ObservableCollection<IActiveReaderMarker>();
      foreach (IActiveReaderMarker highlight in Highlights)
        clone.Highlights.Add(highlight.Clone());

      clone.Annotations = new ObservableCollection<IActiveReaderMarker>();
      foreach (IActiveReaderMarker annotation in Annotations)
        clone.Annotations.Add(annotation.Clone());

      clone.SearchResults = new ObservableCollection<IActiveReaderMarker>();
      foreach (IActiveReaderMarker result in SearchResults)
        clone.SearchResults.Add(result.Clone());

      clone.Scribblings = new ObservableCollection<IActiveReaderMarker>();
      foreach (IActiveReaderMarker scribbling in Scribblings)
        clone.Scribblings.Add(scribbling.Clone());

      clone.FigureLinks = new ObservableCollection<IActiveReaderMarker>();
      foreach (IActiveReaderMarker link in FigureLinks)
        clone.FigureLinks.Add(link.Clone());

      return clone;
    }
  }

  public class Note : IActiveReaderMarker
  {
    public TextBox Annotation { get; set; }
    public Image Icon { get; set; }

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

      string annotationXaml = XamlWriter.Save(Annotation);
      StringReader stringReader = new StringReader(annotationXaml);
      XmlReader xmlReader = XmlReader.Create(stringReader);
      clone.Annotation = (TextBox)XamlReader.Load(xmlReader);

      string iconXaml = XamlWriter.Save(Icon);
      stringReader = new StringReader(iconXaml);
      xmlReader = XmlReader.Create(stringReader);
      clone.Icon = (Image)XamlReader.Load(xmlReader);

      return clone;
    }
  }

  public class Scribble : IActiveReaderMarker
  {
    public InkCanvas Scribbling { get; set; }
    public Image Icon { get; set; }

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

      string scribblingXaml = XamlWriter.Save(Scribbling);
      StringReader stringReader = new StringReader(scribblingXaml);
      XmlReader xmlReader = XmlReader.Create(stringReader);
      clone.Scribbling = (InkCanvas)XamlReader.Load(xmlReader);

      string iconXaml = XamlWriter.Save(Icon);
      stringReader = new StringReader(iconXaml);
      xmlReader = XmlReader.Create(stringReader);
      clone.Icon = (Image)XamlReader.Load(xmlReader);

      return clone;
    }
  }

  public class Highlight : IActiveReaderMarker
  {
    public Line Line { get; set; }

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

      string lineXaml = XamlWriter.Save(Line);
      StringReader stringReader = new StringReader(lineXaml);
      XmlReader xmlReader = XmlReader.Create(stringReader);
      clone.Line = (Line)XamlReader.Load(xmlReader);

      return clone;
    }
  }

}
