using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace UofM.HCI.tPad.App.InfSeeking
{

  public enum SwitchingMethod { Flipping, TapNFlip, Home, RuntimeBar }

  public enum ProviderGroup { Blue, Green, Yellow, Red }

  public class InfSeekingCondition 
  {
    public InfSeekingCondition(SwitchingMethod switchingMethod, int p)
    {
      Method = switchingMethod;
      AppsNumber = p;
    }

    //input
    public SwitchingMethod Method { get; set; }
    public int AppsNumber { get; set; }

    //gets calculated by the Experimenter class
    public List<Exp1Target> Pairs { get; set; }
  }

  public class Exp1Target
  {
    public int Target { get; set; }
    public Exp1SourceApp SourceApp { get; set; }
    public InfSeekingCondition Condition { get; set; }

    public DateTime TimeStarted { get; set; }
  }

  public class Exp1SourceApp 
  {
    public ProviderGroup SourceGroup { get; set; }
    public int InstanceNro { get; set; }
    public String ImagePath { get; set; }
  }

  public delegate Exp1Target Exp1EventHandler(object sender, EventArgs e);

}
