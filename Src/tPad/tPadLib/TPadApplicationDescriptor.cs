using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Drawing;

namespace UofM.HCI.tPad
{

  public class TPadApplicationDescriptor
  {
    public Bitmap Icon { get; set; }
    public String Name { get; set; }
    public Type AppClass { get; set; }
    public ITPadAppLauncher Launcher { get; set; }
    public List<Glyph> Triggers { get; set; }

    public TPadApplicationDescriptor()
    {
      Triggers = new List<Glyph>();
    }
  }

  public enum Glyph { Square, Triangle };

  public enum GlyphStatus { Entered, Left };

  public struct GlyphEvent 
  {
    public Glyph Glyph { get; set; }
    public GlyphStatus Status { get; set; }
  }

}
