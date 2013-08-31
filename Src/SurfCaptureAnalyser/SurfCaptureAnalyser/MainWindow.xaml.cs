using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SurfCaptureAnalyser
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : System.Windows.Window
  {

    public ObservableCollection<Capture> Captures { get; set; }

    public MainWindow()
    {
      Captures = new ObservableCollection<Capture>();

      InitializeComponent();
    }

    private void bLoadPicture_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      FolderBrowserDialog openFolderDialog = new FolderBrowserDialog();

      String path = String.Format("{0}\\..\\..\\..\\..\\..\\Data", Environment.CurrentDirectory);
      openFolderDialog.RootFolder = Environment.SpecialFolder.Desktop;

      if (openFolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        try
        {
          String folder = openFolderDialog.SelectedPath;
          String taggingFile = String.Format("{0}\\{1}", folder, "tagging.xml");
          if (File.Exists(taggingFile))
          {
            XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Capture>));
            TextReader reader = new StreamReader(taggingFile);
            ObservableCollection<Capture> load = (ObservableCollection<Capture>)serializer.Deserialize(reader);
            reader.Close();

            Captures.Clear();
            foreach (Capture loadC in load)
            {
              loadC.Folder = folder;
              Captures.Add(loadC);
            }
          }
          else
          {
            Captures.Clear();
            var captureFiles = Directory.EnumerateFiles(folder, "*.jpg");
            foreach (String file in captureFiles)
            {
              Captures.Add(new Capture(folder, file));
              DetectSquares(Captures[Captures.Count - 1]);
            }
          }
        }
        catch (Exception ex)
        {
          System.Windows.MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
        }
      }
    }

    /// <summary>
    /// COde mainly taken from:http://www.emgu.com/wiki/index.php/Shape_(Triangle,_Rectangle,_Circle,_Line)_Detection_in_CSharp
    /// </summary>
    /// <param name="capture"></param>
    private void DetectSquares(Capture capture)
    {
      //Load the image from file
      Image<Bgr, Byte> img = new Image<Bgr, byte>(capture.FullPath);

      //Convert the image to grayscale and filter out the noise
      Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

      //Gets the edges for looking for the squares
      Image<Gray, Byte> cannyEdges = GetCannyEdges(gray);

      List<MCvBox2D> boxList = new List<MCvBox2D>();

      using (MemStorage storage = new MemStorage()) //allocate storage for contour approximation
        for (Contour<System.Drawing.Point> contours = cannyEdges.FindContours(); contours != null; contours = contours.HNext)
        {
          Contour<System.Drawing.Point> currentContour = contours.ApproxPoly(contours.Perimeter * 0.05, storage);

          if (contours.Area > 250) //only consider contours with area greater than 250
          {
            if (currentContour.Total == 4) //The contour has 4 vertices.
            {
              #region determine if all the angles in the contour are within the range of [80, 100] degree
              bool isRectangle = true;
              System.Drawing.Point[] pts = currentContour.ToArray();
              LineSegment2D[] edges = Emgu.CV.PointCollection.PolyLine(pts, true);

              for (int i = 0; i < edges.Length; i++)
              {
                double angle = Math.Abs(
                   edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                if (angle < 80 || angle > 100)
                {
                  isRectangle = false;
                  break;
                }
              }
              #endregion

              if (isRectangle)
              {
                MCvBox2D box = currentContour.GetMinAreaRect();
                MCvBox2D exists = boxList.FirstOrDefault(pBox => Math.Abs(pBox.size.Width - box.size.Width) <= 2 && Math.Abs(pBox.size.Height - box.size.Height) <= 2);
                if (MCvBox2D.Empty.Equals(exists))
                  boxList.Add(box);
                else if (CalculateArea(box) < CalculateArea(exists))
                {
                  boxList.Remove(exists);
                  boxList.Add(box);
                }
              }
            }
          }
        }

      if (boxList.Count == 0)
        return;

      //The smaller box goes to black, the bigger goes to red
      boxList.Sort(delegate(MCvBox2D box1, MCvBox2D box2)
      {
        return (int)(CalculateArea(box1) - CalculateArea(box2));
      });

      double ratio = gWorkArea.ActualHeight / img.Height;
      double anchorX = img.Width / 2;
      double anchorY = img.Height / 2;

      System.Drawing.PointF[] vertices = boxList[0].GetVertices();
      capture.TargetRectangle.BottomLeft.X = (vertices[0].X - anchorX) * ratio;
      capture.TargetRectangle.BottomLeft.Y = (vertices[0].Y - anchorY) * ratio;
      capture.TargetRectangle.TopLeft.X = (vertices[1].X - anchorX) * ratio;
      capture.TargetRectangle.TopLeft.Y = (vertices[1].Y - anchorY) * ratio;
      capture.TargetRectangle.TopRight.X = (vertices[2].X - anchorX) * ratio;
      capture.TargetRectangle.TopRight.Y = (vertices[2].Y - anchorY) * ratio;
      capture.TargetRectangle.BottomRight.X = (vertices[3].X - anchorX) * ratio;
      capture.TargetRectangle.BottomRight.Y = (vertices[3].Y - anchorY) * ratio;
      capture.TargetCenter.X = (boxList[0].center.X - anchorX) * ratio;
      capture.TargetCenter.Y = (boxList[0].center.Y - anchorY) * ratio;

      if (boxList.Count == 1)
        return;

      vertices = boxList[1].GetVertices();
      capture.CaptureRectangle.BottomLeft.X = (vertices[0].X - anchorX) * ratio;
      capture.CaptureRectangle.BottomLeft.Y = (vertices[0].Y - anchorY) * ratio;
      capture.CaptureRectangle.TopLeft.X = (vertices[1].X - anchorX) * ratio;
      capture.CaptureRectangle.TopLeft.Y = (vertices[1].Y - anchorY) * ratio;
      capture.CaptureRectangle.TopRight.X = (vertices[2].X - anchorX) * ratio;
      capture.CaptureRectangle.TopRight.Y = (vertices[2].Y - anchorY) * ratio;
      capture.CaptureRectangle.BottomRight.X = (vertices[3].X - anchorX) * ratio;
      capture.CaptureRectangle.BottomRight.Y = (vertices[3].Y - anchorY) * ratio;
      capture.CaptureCenter.X = (boxList[0].center.X - anchorX) * ratio;
      capture.CaptureCenter.Y = (boxList[0].center.Y - anchorY) * ratio;

    }

    private static Image<Gray, Byte> GetCannyEdges(Image<Gray, Byte> gray)
    {
      double cannyThreshold = 60;
      double cannyThresholdLinking = 60;
      Gray circleAccumulatorThreshold = new Gray(120);
      Image<Gray, Byte> cannyEdges = gray.Canny(cannyThreshold, cannyThresholdLinking);
      return cannyEdges;
    }

    private static double CalculateArea(MCvBox2D box)
    {
      return box.size.Width * box.size.Height;
    }

    private void bGenerate_Click(object sender, System.Windows.RoutedEventArgs e)
    {

    }

    private double[] sizes = { 1.85, 3.78, 5.68 };

    private Capture CurrentCapture { get { return (Capture)lbCaptures.SelectedItem; } }
    private BindablePoint movingPoint = null;
    private bool isMoving = false;

    private void eTargetCenter_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (CurrentCapture == null)
        return;

      movingPoint = (sender as System.Windows.FrameworkElement).DataContext as BindablePoint;
      isMoving = true;
    }

    private void eTargetCenter_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (!isMoving)
        return;

      UpdateLocation(e.GetPosition(iCapture));
    }

    private void eTargetCenter_MouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!isMoving)
        return;

      UpdateLocation(e.GetPosition(iCapture));
      isMoving = false;
      movingPoint = null;
    }

    private void UpdateLocation(System.Windows.Point click)
    {
      System.Windows.Point location = click - new System.Windows.Vector(iCapture.ActualWidth / 2, iCapture.ActualHeight / 2);
      movingPoint.X = location.X;
      movingPoint.Y = location.Y;
    }

    private void bSave_Click(object sender, System.Windows.RoutedEventArgs e)
    {
      if (Captures == null)
        return;

      String filename = String.Format("{0}\\{1}", Captures[0].Folder, "tagging.xml");
      if (File.Exists(filename))
        File.Delete(filename);

      XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<Capture>));
      TextWriter writer = new StreamWriter(filename);
      serializer.Serialize(writer, Captures);
      writer.Close();
    }
  }

  public enum Device { Normal, tPad };
  public enum Method { Normal, Clipped };
  public enum Size { Quarter, Half, ThreeQuarters };

  public class Capture
  {
    public String Folder { get; set; }
    public String FileName { get; set; }

    [XmlIgnore]
    public String FullPath { get { return String.Format("{0}\\{1}", Folder, FileName); } }

    [XmlAttribute]
    public long Ticks { get; set; }

    [XmlAttribute]
    public Device Device { get; set; }

    [XmlAttribute]
    public Method Method { get; set; }

    [XmlAttribute]
    public Size Size { get; set; }

    public BindablePoint TargetCenter { get; set; }
    public BindableRect TargetRectangle { get; set; }
    public BindablePoint CaptureCenter { get; set; }
    public BindableRect CaptureRectangle { get; set; }

    public Capture() { }

    public Capture(String folderPath, String filePath)
    {
      Folder = folderPath;
      FileName = System.IO.Path.GetFileName(filePath);
      var pieces = FileName.Split(new String[] { "-" }, StringSplitOptions.RemoveEmptyEntries);

      Ticks = long.Parse(pieces[0]);
      Device = (Device)Enum.Parse(typeof(Device), pieces[1]);
      Method = (Method)Enum.Parse(typeof(Method), pieces[2]);
      Size = (Size)Enum.Parse(typeof(Size), pieces[3]);

      TargetCenter = new BindablePoint(-10, -10);
      TargetRectangle = new BindableRect(-50, -50, 100, 100);

      CaptureCenter = new BindablePoint(0, 0);
      CaptureRectangle = new BindableRect(-60, -60, 120, 120);
    }

    public override string ToString()
    {
      return FileName;
    }
  }

}
