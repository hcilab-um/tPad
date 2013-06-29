using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json.Conversion;
using System.Windows.Threading;

namespace UofM.HCI.tPad.App.PhotoAlbum.Network
{
  public class PhotoMessageImporter : IImporter
  {
    public object Import(ImportContext context, Jayrock.Json.JsonReader reader)
    {
      PhotoMessage pMessage = (PhotoMessage)context.Import<PhotoMessage>(reader); ;
      return pMessage;
    }

    public Type OutputType
    {
      get { return typeof(PhotoMessage); }
    }
  }
}
