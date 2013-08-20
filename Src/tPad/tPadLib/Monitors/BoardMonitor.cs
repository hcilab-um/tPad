using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.CAF.ContextAdapter;
using System.Windows.Media.Media3D;
using System.IO.Ports;
using System.ComponentModel;
using Jayrock.Json;
using Jayrock.Json.Conversion;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

namespace UofM.HCI.tPad.Monitors
{

  /// <summary>
  /// {"FlippingSide": "FaceUp", "ButtonEvent": "None" }
  /// {"FlippingSide": "FaceDown", "ButtonEvent": "Single" }
  /// {"FlippingSide": "FaceUp", "ButtonEvent": "Double" }
  /// </summary>
  public struct BoardUpdate
  {
    public FlippingMode FlippingSide { get; set; }
    public ButtonEvent ButtonEvent { get; set; }
    public String StackCode { get; set; }

    [JsonIgnore]
    public int DeviceOnTopID { get; set; }
    [JsonIgnore]
    public bool Shaked { get; set; }
  }

  public class BoardMonitor : ContextMonitor
  {

    public String COMPort { get; set; }
    public SerialPort Port { get; set; }
    private String actualUpdateJson = String.Empty;
    private Regex validator = new Regex("{\"FlippingSide\": (\"FaceUp\"|\"FaceDown\"), \"ButtonEvent\": (\"None\"|\"Single\"|\"Double\"), \"StackCode\": \"(0|1){4}\"}");

    private ImportContext jsonImportContext { get; set; }

    private Object monitor = new Object();

    private BoardUpdate lastUpdate = new BoardUpdate()
    {
      FlippingSide = FlippingMode.Unknown,
      DeviceOnTopID = 0,
    };

    public BoardMonitor()
    {
      jsonImportContext = new ImportContext();
    }

    protected override void CustomStart()
    {
      if (COMPort == null || COMPort.Length == 0)
        return;

      try
      {
        Port = new SerialPort(COMPort, 9600);
        Port.DtrEnable = true;
        Port.DataReceived += Port_DataReceived;
        Port.Open();
      }
      catch { return; }
    }

    internal bool TryPort()
    {
      if (COMPort == null || COMPort.Length == 0)
        return false;

      try
      {
        Port = new SerialPort(COMPort, 9600);
        Port.Open();
        Port.Close();
      }
      catch (Exception e)
      {
        return false;
      }

      return true;
    }

    private String incompleteLine = String.Empty;
    void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      if (e.EventType == SerialData.Chars)
      {
        SerialPort port = (SerialPort)sender;
        String readData = port.ReadExisting();
        String[] lines = readData.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        lock (monitor)
        {
          foreach (String line in lines)
          {
            if (validator.IsMatch(line))
            {
              actualUpdateJson = line;
              continue;
            }

            if (line.StartsWith("{"))
            {
              incompleteLine = line;
              continue;
            }

            incompleteLine += line;
            if (!line.EndsWith("}"))
              continue;

            if (!validator.IsMatch(incompleteLine))
              incompleteLine = String.Empty;

            actualUpdateJson = incompleteLine;
            incompleteLine = String.Empty;
          }
        }
      }
      else if (e.EventType == SerialData.Eof)
      { }
    }

    protected override void CustomStop()
    {
      if (Port == null)
        return;

      Port.DataReceived -= Port_DataReceived;
      Port.Close();
      Port = null;
    }

    protected override void CustomRun()
    {
      if (Port == null)
      {
        Stop();
        return;
      }

      lock (monitor)
      {
        if (actualUpdateJson == null || actualUpdateJson.Length == 0)
          return;

        //Console.WriteLine(actualUpdateJson);
        JsonReader reader = new JsonTextReader(new StringReader(actualUpdateJson));
        BoardUpdate actualUpdate = jsonImportContext.Import<BoardUpdate>(reader);
        actualUpdateJson = String.Empty;

        if (actualUpdate.StackCode.Length != 4)
          return;

        String codeStr = actualUpdate.StackCode.Substring(0, 2);
        String chksStr = actualUpdate.StackCode.Substring(2, 2);

        int code = Convert.ToInt32(codeStr, 2);
        int chks = Convert.ToInt32(chksStr, 2);

        if (code != chks)
          code = lastUpdate.DeviceOnTopID;

        actualUpdate.DeviceOnTopID = code;
        NotifyContextServices(this, new NotifyContextMonitorListenersEventArgs(typeof(BoardUpdate), actualUpdate));
        lastUpdate = actualUpdate;
      }
    }
  }
}
