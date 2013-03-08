using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json.Conversion;
using System.Windows.Threading;

namespace UofM.HCI.tPab.App.ActiveReader.Network
{
  public class ActiveReaderMessageImporter : IImporter
  {

    private Dispatcher Dispatcher { get; set; }

    public ActiveReaderMessageImporter(Dispatcher dispatcher)
    {
      Dispatcher = dispatcher;
    }

    public object Import(ImportContext context, Jayrock.Json.JsonReader reader)
    {
      ActiveReaderMessage arMessage = null;
      Dispatcher.Invoke(DispatcherPriority.Background,
        (Action)delegate()
        {
          arMessage = (ActiveReaderMessage)context.Import<ActiveReaderMessage>(reader); ;
        });
      return arMessage;
    }

    public Type OutputType
    {
      get { return typeof(ActiveReaderMessage); }
    }
  }
}
