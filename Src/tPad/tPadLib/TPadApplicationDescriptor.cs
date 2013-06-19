using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Drawing;
using UofM.HCI.tPad.Monitors;

namespace UofM.HCI.tPad
{

  public class TPadApplicationDescriptor
  {
    public Bitmap Icon { get; set; }
    public String Name { get; set; }
    public Type AppClass { get; set; }
    public ITPadAppLauncher Launcher { get; set; }

    public ITPadApp Instance { get; set; }
    public FlippingMode RunningSide { get; set; }

    public List<Glyph> Triggers { get; set; }
    public List<TPadEvent> Events { get; set; }

    public TPadApplicationDescriptor()
    {
      Triggers = new List<Glyph>();
      Events = new List<TPadEvent>();
    }
  }

  public enum TPadEvent { Flipping, Shaking }

  public enum Glyph { Square, Triangle };

  public enum GlyphStatus { Entered, Left };

  public struct GlyphEvent 
  {
    public Glyph Glyph { get; set; }
    public GlyphStatus Status { get; set; }
  }

}
