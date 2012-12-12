using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace UofM.HCI.tPab.App.ActiveReader
{
  public partial class AcrobatReaderControl : UserControl
  {
    private string DocumentPath;

    public AcrobatReaderControl(string pdfPath)
    {
      InitializeComponent();

      this.DocumentPath = pdfPath;
      this.axAcroPDF1.LoadFile(DocumentPath);
    }
  }
}
