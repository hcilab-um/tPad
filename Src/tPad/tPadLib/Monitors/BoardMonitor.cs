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

namespace UofM.HCI.tPab.Monitors
{


  /// <summary>
  /// {"Orientation": { "X": 357.43, "Y": 359.36, "Z": 255.96 }, "StackCode": "0000"}
  /// </summary>
  public struct BoardUpdate
  {
    public Point3D Orientation { get; set; }
    public String StackCode { get; set; }

    [JsonIgnore]
    public int DeviceOnTopID { get; set; }
    [JsonIgnore]
    public FlippingMode FlippingSide { get; set; }
    [JsonIgnore]
    public bool Shaked { get; set; }
  }

  public class BoardMonitor : ContextMonitor
  {

    public String COMPort { get; set; }
    public SerialPort Port { get; set; }
    private String actualUpdateJson = String.Empty;
    private Regex validator = new Regex("{\"Orientation\": { (\"(X|Y|Z)\": [0-9]{1,3}.[0-9]{2}(,| )*)+ }, \"StackCode\": \"(0|1){4}\"}");

    private ImportContext jsonImportContext { get; set; }

    private Object monitor = new Object();

    private BoardUpdate lastUpdate = new BoardUpdate() { DeviceOnTopID = 0, Orientation = new Point3D(0, 0, 0) };

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
      catch { }
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

    void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
      if (e.EventType == SerialData.Chars)
      {
        SerialPort port = (SerialPort)sender;
        String readData = port.ReadExisting();
        String[] lines = readData.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (String line in lines)
        {
          if (!validator.IsMatch(line)) //drops incomplete lines
            continue;

          lock (monitor)
            actualUpdateJson = line;
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

        JsonReader reader = new JsonTextReader(new StringReader(actualUpdateJson));
        BoardUpdate actualUpdate = jsonImportContext.Import<BoardUpdate>(reader);

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
