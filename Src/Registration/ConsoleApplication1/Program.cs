using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using ManagedA;

namespace ConsoleApplication1
{
  class Program
  {
    static void Main(string[] args)
    {
      Bitmap bmp = (Bitmap)Image.FromFile("C:\\Users/sophie/Documents/GitHub/tPad/Src/Registration/unManagedTest/images/test.png");
      
      ManagedA.wrapperRegistClass testObject = new wrapperRegistClass();
      testObject.detectLocation(bmp);

      System.Console.WriteLine(testObject.LocationPx);
      System.Console.WriteLine(testObject.PageIdx);
      System.Console.WriteLine(testObject.PageName);
      System.Console.WriteLine(testObject.RotationAngle);
      Console.ReadLine();
    }
  }
}
