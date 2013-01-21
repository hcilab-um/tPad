using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;


namespace UofM.HCI.tPab.App.ActiveReader.Converters
{
  class OffScreenItemLocationConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      var deviceWidthInPage = float.Parse(values[0].ToString());
      var deviceHeightInPage = float.Parse(values[1].ToString());
      var highlightPositionX = float.Parse(values[2].ToString());
      var highlightPositionY = float.Parse(values[3].ToString());
      var deviceLoc = (System.Drawing.PointF)values[4];
      var widthFactor = float.Parse(values[5].ToString());
      var heightFactor = float.Parse(values[6].ToString());
      var deviceWidth = float.Parse(values[7].ToString());
      var deviceHeight = float.Parse(values[8].ToString());
      var devicePageWidthFactor = deviceWidth / deviceWidthInPage;
      var devicePageHeightFactor = deviceHeight/ deviceHeightInPage;
      var sizeOffScreenIcon = float.Parse(values[9].ToString());
      var angle = double.Parse(values[10].ToString());

      Vector deviceLocation = new Vector(deviceLoc.X * widthFactor, deviceLoc.Y * heightFactor);
      Vector deviceCenter = new Vector(deviceLocation.X + (deviceWidthInPage / 2.0f), deviceLocation.Y + (deviceHeightInPage / 2.0f)); //center within the page
      
      //parametric line equation: deviceCenter + s * directionHighlight
      Vector directionHighlight = new Vector(highlightPositionX - deviceCenter.X, highlightPositionY - deviceCenter.Y);
             
      //rotate corners of device
      Vector lowerLeft = rotateAroundPoint(new Vector(deviceLocation.X, deviceLocation.Y + deviceHeightInPage), deviceCenter, angle);
      Vector lowerRight = rotateAroundPoint(new Vector(deviceLocation.X + deviceWidthInPage, deviceLocation.Y + deviceHeightInPage), deviceCenter, angle);
      Vector upperRight = rotateAroundPoint(new Vector(deviceLocation.X + deviceWidthInPage, deviceLocation.Y), deviceCenter, angle);
      deviceLocation = rotateAroundPoint(deviceLocation, deviceCenter, angle);
        
      //Compute intersections:
      //intersection with left device border
      Vector directionVertBorder = new Vector(deviceLocation.X - lowerLeft.X, deviceLocation.Y - lowerLeft.Y);
      Vector leftIntersection = computeIntersection(lowerLeft, directionVertBorder, deviceCenter, directionHighlight);
      if (isBetween(deviceLocation, lowerLeft, leftIntersection) && isPointLeft(new Vector(highlightPositionX, highlightPositionY), lowerLeft, deviceLocation))
      {
        deviceLocation = rotateAroundPoint(deviceLocation, deviceCenter, -angle);
        leftIntersection = rotateAroundPoint(leftIntersection, deviceCenter, -angle);
        return new System.Windows.Thickness(0, (leftIntersection.Y - deviceLocation.Y) * devicePageHeightFactor, 0, 0);
      }

      //intersection with right device border
      Vector rightIntersection = computeIntersection(lowerRight, directionVertBorder, deviceCenter, directionHighlight);
      if (isBetween(upperRight, lowerRight, rightIntersection) && !isPointLeft(new Vector(highlightPositionX, highlightPositionY), lowerRight, upperRight))
      {
        deviceLocation = rotateAroundPoint(deviceLocation, deviceCenter, -angle);
        rightIntersection = rotateAroundPoint(rightIntersection, deviceCenter, -angle);
        return new System.Windows.Thickness(deviceWidth - sizeOffScreenIcon - 42,
          (rightIntersection.Y - deviceLocation.Y) * devicePageHeightFactor, 0, 0);
      }

      //intersection with upper device border
      Vector directionHorizBorder = new Vector(deviceLocation.X - upperRight.X, deviceLocation.Y - upperRight.Y);
      Vector upperIntersection = computeIntersection(upperRight, directionHorizBorder, deviceCenter, directionHighlight);
      if (isBetween(deviceLocation, upperRight, upperIntersection) && isPointLeft(new Vector(highlightPositionX, highlightPositionY), deviceLocation, upperRight))
      {
        deviceLocation = rotateAroundPoint(deviceLocation, deviceCenter, -angle);
        upperIntersection = rotateAroundPoint(upperIntersection, deviceCenter, -angle);
        return new System.Windows.Thickness((upperIntersection.X - deviceLocation.X) * devicePageWidthFactor, 0, 0, 0);
      }

      //intersection with lower device border
      Vector lowerIntersection = computeIntersection(lowerRight, directionHorizBorder, deviceCenter, directionHighlight);
      if (isBetween(lowerLeft, lowerRight, lowerIntersection) && !isPointLeft(new Vector(highlightPositionX, highlightPositionY), lowerLeft, lowerRight))
      {
        deviceLocation = rotateAroundPoint(deviceLocation, deviceCenter, -angle);
        lowerIntersection = rotateAroundPoint(lowerIntersection, deviceCenter, -angle);
        return new System.Windows.Thickness((lowerIntersection.X - deviceLocation.X) * devicePageWidthFactor, deviceHeight - sizeOffScreenIcon, 0, 0);
      }
     
      return new System.Windows.Thickness(-50, -50, 0, 0);
    }

    private Vector rotateAroundPoint(Vector point, Vector center, double angle)
    {
      angle = (angle * Math.PI) / 180.0;
      //counterclockwise

      double x = Math.Cos(angle) * (point.X - center.X) - Math.Sin(angle) * (point.Y - center.Y) + center.X;
      double y = Math.Sin(angle) * (point.X - center.X) + Math.Cos(angle) * (point.Y - center.Y) + center.Y;

      return new Vector(x, y);
    }

    private Vector computeIntersection(Vector point1, Vector direction1, Vector point2, Vector direction2)
    {
      double t = ((-(point2.X - point1.X) * direction1.Y) + (direction1.X * (point2.Y - point1.Y))) /
        ((direction2.X * direction1.Y) - (direction2.Y * direction1.X));
      double s;
      if (direction1.X != 0)
        s = (point2.X - point1.X + (t * direction2.X)) / direction1.X;
      else s = (point2.Y - point1.Y + (t * direction2.Y)) / direction1.Y;

      double x = point2.X + direction2.X * t;
      double y = point2.Y + direction2.Y * t;

      return new Vector(x, y);
    }

    private bool isPointLeft(Vector point, Vector linePoint1, Vector linePoint2)
    {
      int position = Math.Sign((linePoint2.X - linePoint1.X) * (point.Y - linePoint1.Y) - (linePoint2.Y - linePoint1.Y) * (point.X - linePoint1.X));

      if (position > 0)
        return false;
      else return true;
    }

    private bool isBetween(Vector startPoint, Vector endPoint, Vector point)
    {
      double dotproduct = (point.X - startPoint.X) * (endPoint.X - startPoint.X) + (point.Y - startPoint.Y)*(endPoint.Y - startPoint.Y);      
      if (dotproduct < 0)
        return false;
      
      double sqrtLength = (endPoint.X - startPoint.X)*(endPoint.X - startPoint.X) + (endPoint.Y - startPoint.Y)*(endPoint.Y - startPoint.Y);
      if (dotproduct > sqrtLength)
        return false;

      return true;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
