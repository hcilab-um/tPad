using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;


namespace UofM.HCI.tPad.App.ActiveReader.Converters
{
  class OffScreenItemLocationConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (values[2] == DependencyProperty.UnsetValue)
        return new System.Windows.Thickness(-50, -50, 0, 0);

      //<Binding ElementName="offScreen" Path="PageWidth"/>
      //<Binding ElementName="offScreen" Path="PageHeight"/>
      //<Binding Path="Position"/>
      //<Binding ElementName="offScreen" Path="DeviceWidth"/>
      //<Binding ElementName="offScreen" Path="DeviceHeight"/>
      //<Binding ElementName="offScreen" Path="DeviceLocation"/>
      //<Binding ElementName="offScreen" Path="DeviceRotation"/>
      //<Binding ElementName="offScreen" Path="WidthFactor"/>
      //<Binding ElementName="offScreen" Path="HeightFactor"/>
      //<Binding RelativeSource="{RelativeSource AncestorType=ListBox}" Path="ActualWidth"/>
      //<Binding RelativeSource="{RelativeSource AncestorType=ListBox}" Path="ActualHeight"/>
      //<Binding ElementName="iArrow" Path="ActualWidth"/>
      //<Binding ElementName="offScreen" Path="UIRotation"/>

      var pageWidth = float.Parse(values[0].ToString());
      var pageHeight = float.Parse(values[1].ToString());

      var highlightPosition = (Point)values[2];
      var deviceWidth = float.Parse(values[3].ToString());
      var deviceHeight = float.Parse(values[4].ToString());
      var deviceLoc = (Point)values[5];
      var deviceAngle = double.Parse(values[6].ToString());
      var widthFactor = float.Parse(values[7].ToString());
      var heightFactor = float.Parse(values[8].ToString());

      var controlWidth = float.Parse(values[9].ToString());
      var controlHeight = float.Parse(values[10].ToString());
      var sizeOffScreenIcon = float.Parse(values[11].ToString());

      var uiAngle = double.Parse(values[12].ToString());

      //The highlightPosition is not in cms, thus it must be converted to pixels
      highlightPosition.X = highlightPosition.X * widthFactor;
      highlightPosition.Y = highlightPosition.Y * heightFactor;

      Point deviceCenter = new Point(deviceLoc.X * widthFactor, deviceLoc.Y * heightFactor);
      Rect bounds = new Rect(-deviceWidth / 2, -deviceHeight / 2, deviceWidth, deviceHeight);
      bounds.Offset(new Vector(deviceCenter.X, deviceCenter.Y));
      Point topLeft = bounds.TopLeft;
      Point topRight = bounds.TopRight;
      Point bottomRight = bounds.BottomRight;
      Point bottomLeft = bounds.BottomLeft;

      Matrix rotationM = Matrix.Identity;
      rotationM.RotateAt(deviceAngle, deviceCenter.X, deviceCenter.Y);
      topLeft = rotationM.Transform(topLeft);
      topRight = rotationM.Transform(topRight);
      bottomRight = rotationM.Transform(bottomRight);
      bottomLeft = rotationM.Transform(bottomLeft);

      //Taken from: http://community.topcoder.com/tc?module=Static&d1=tutorials&d2=geometry2
      //Assuming you have two lines of the form Ax + By = C, you can find it pretty easily:
      //float delta = A1*B2 - A2*B1;
      //if(delta == 0) 
      //  throw new ArgumentException("Lines are parallel");
      //float x = (B2*C1 - B1*C2)/delta;
      //float y = (A1*C2 - A2*C1)/delta;

      Line lineCenterToHL = new Line(deviceCenter, highlightPosition);
      Line topFrame = new Line(topLeft, topRight);
      Line rightFrame = new Line(topRight, bottomRight);
      Line bottomFrame = new Line(bottomRight, bottomLeft);
      Line leftFrame = new Line(bottomLeft, topLeft);

      Point? intersectWithTop = lineCenterToHL.Interset(topFrame);
      Point? intersectWithRight = lineCenterToHL.Interset(rightFrame);
      Point? intersectWithBottom = lineCenterToHL.Interset(bottomFrame);
      Point? intersectWithLeft = lineCenterToHL.Interset(leftFrame);

      Vector? distanceCenterToTopIntersect = CalculateDistance(deviceCenter, intersectWithTop);
      Vector? distanceCenterToRigthIntersect = CalculateDistance(deviceCenter, intersectWithRight);
      Vector? distanceCenterToBottomIntersect = CalculateDistance(deviceCenter, intersectWithBottom);
      Vector? distanceCenterToLeftIntersect = CalculateDistance(deviceCenter, intersectWithLeft);

      Vector? distanceHLToTopIntersect = CalculateDistance(highlightPosition, intersectWithTop);
      Vector? distanceHLToRightIntersect = CalculateDistance(highlightPosition, intersectWithRight);
      Vector? distanceHLToBottomIntersect = CalculateDistance(highlightPosition, intersectWithBottom);
      Vector? distanceHLToLeftIntersect = CalculateDistance(highlightPosition, intersectWithLeft);

      Point?[] intersects = new Point?[] { intersectWithTop, intersectWithRight, intersectWithBottom, intersectWithLeft };
      Vector?[] distancesToCenter = new Vector?[] { distanceCenterToTopIntersect, distanceCenterToRigthIntersect, distanceCenterToBottomIntersect, distanceCenterToLeftIntersect };
      Vector?[] distancesToHL = new Vector?[] { distanceHLToTopIntersect, distanceHLToRightIntersect, distanceHLToBottomIntersect, distanceHLToLeftIntersect };

      double minDistanceToCenter = distancesToCenter.Min(tmp => tmp.HasValue ? tmp.Value.Length : double.PositiveInfinity);
      var closestDistanceToCenter = distancesToCenter.Where(tmp => !Equals(tmp.Value.Length, minDistanceToCenter, 1.5d));
      foreach (Vector? distance in closestDistanceToCenter)
      {
        int tmpIndex = Array.IndexOf(distancesToCenter, distance);
        distancesToHL[tmpIndex] = null;
      }

      double minDistanceToHL = distancesToHL.Min(tmp => tmp.HasValue ? tmp.Value.Length : double.PositiveInfinity);
      var closestIntersectToHL = distancesToHL.Single(tmp => tmp.HasValue && tmp.Value.Length == minDistanceToHL);
      int index = Array.IndexOf<Vector?>(distancesToHL, closestIntersectToHL);
      Point intersect = intersects[index].Value;

      //Checks whether it's inside
      Vector directionHighlight = new Vector(highlightPosition.X - deviceCenter.X, highlightPosition.Y - deviceCenter.Y);
      Vector directionIntersect = new Vector(intersect.X - deviceCenter.X, intersect.Y - deviceCenter.Y);
      if (directionHighlight.Length < directionIntersect.Length)
        return new System.Windows.Thickness(-50, -50, 0, 0);

      //Now we calculate the position of the intersect in relation to the reference point of the fixed layers
      Point reference = topLeft;
      Point square = topRight;
      if (uiAngle == 90)
      {
        reference = topRight;
        square = bottomRight;
      }
      else if (uiAngle == 180)
      {
        reference = bottomRight;
        square = bottomLeft;
      }
      else if (uiAngle == 270)
      {
        reference = bottomLeft;
        square = topLeft;
      }

      //1- find the adjacent and hypotenuse of the triangle
      Vector adjacent = square - reference;
      Vector hypotenuse = intersect - reference;
      double refAngle = Vector.AngleBetween(adjacent, hypotenuse);

      double markerFixedX = hypotenuse.Length * Math.Cos(refAngle * Math.PI / 180.0);
      double markerFixedY = hypotenuse.Length * Math.Sin(refAngle * Math.PI / 180.0);

      if (markerFixedX >= controlWidth - 1)
        markerFixedX -= sizeOffScreenIcon;
      if (markerFixedY >= controlHeight - 1)
        markerFixedY -= sizeOffScreenIcon;

      return new System.Windows.Thickness(markerFixedX, markerFixedY, 0, 0);
    }

    private bool Equals(double number1, double number2, double range)
    {
      if (number1 - range <= number2 && number2 <= number1 + range &&
        number2 - range <= number1 && number1 <= number2 + range)
        return true;
      return false;
    }

    private static Vector? CalculateDistance(Point origin, Point? target)
    {
      if (target == null)
        return null;

      Vector distanceToIntersect = new Vector(origin.X - target.Value.X, origin.Y - target.Value.Y);
      return distanceToIntersect;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public class Line
    {

      public Point Point1;
      public Point Point2;

      public double A { get; set; }
      public double B { get; set; }
      public double C { get; set; }

      public Line(Point point1, Point point2)
      {
        Point1 = point1;
        Point2 = point2;

        A = point2.Y - point1.Y;
        B = point1.X - point2.X;
        C = A * point1.X + B * point1.Y;
      }

      public Point? Interset(Line line)
      {
        double det = A * line.B - line.A * B;
        if (det == 0)
          return null;

        double x = (line.B * C - B * line.C) / det;
        double y = (A * line.C - line.A * C) / det;
        return new Point(x, y);
      }

    }

  }
}
