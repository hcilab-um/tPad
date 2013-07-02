using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.MTF;

namespace UofM.HCI.tPad.App.WhatUp
{
  public class Launcher : ITPadAppLauncher
  {
    public TPadLauncherSettings GetSettings(TPadLauncherSettings settings)
    {
      settings.Context.Add("ITransportListener.MessageType", WhatUpMessage.MessageID.ToString());
      return settings;
    }

    public TPadApplicationDescriptor GetApplicationDescriptor()
    {
      TPadApplicationDescriptor descriptor = new TPadApplicationDescriptor()
      {
        Name = "WhatUp",
        Icon = UofM.HCI.tPad.App.WhatUp.Properties.Resources.WhatUpIcon,
        AppClass = typeof(WhatUpApp),
        Launcher = this
      };
      descriptor.Events.Add(TPadEvent.Flipping);
      descriptor.Events.Add(TPadEvent.NetworkWakeup);

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      WhatUpApp whatUp = new WhatUpApp(core, container, controller);
      return whatUp;
    }

    public void Prepare() 
    {
      if (!TransportMessageExporter.Exporters.ContainsKey(WhatUpMessage.MessageID))
        TransportMessageExporter.Exporters.Add(WhatUpMessage.MessageID, new WhatUpMessageExporter());
      if (!TransportMessageImporter.Importers.ContainsKey(WhatUpMessage.MessageID))
        TransportMessageImporter.Importers.Add(WhatUpMessage.MessageID, new WhatUpMessageImporter());
    }

  }
}
