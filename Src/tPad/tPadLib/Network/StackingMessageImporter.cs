using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jayrock.Json.Conversion;

namespace UofM.HCI.tPad.Network
{
  public class StackingMessageImporter : IImporter
  {

    //"messageType":"LocationUpdate",
    //"sourceDeviceID":1,
    //"targetDeviceID":2,
    //"stakingRequestResponse":false,
    //"location":{
    //   "status":"Located",
    //   "pageIndex":0,
    //   "document":{
    //      "iD":0,
    //      "folder":"Documents\\Blank\\",
    //      "fileName":"Documents\\Blank\\Blank.pdf",
    //      "pages":[
    //         {
    //            "pageIndex":0,
    //            "fileName":"Documents\\Blank\\Blank-01.png"
    //         }
    //      ],
    //      "documentSize":{
    //         "width":21.59,
    //         "height":27.94
    //      }
    //   },
    //   "locationCm":{
    //      "x":14.6648820701825,
    //      "y":18.3064635963747
    //   },
    //   "rotationAngle":0
    //},
    //"touchLocation":{
    //   "x":0,
    //   "y":0
    //},
    //"touchAction":"Down"

    public object Import(ImportContext context, Jayrock.Json.JsonReader reader)
    {
      return (StackingMessage)context.Import<StackingMessage>(reader);
    }

    public Type OutputType
    {
      get { return typeof(StackingMessage); }
    }
  }
}
