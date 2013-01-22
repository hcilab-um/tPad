﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace UofM.HCI.tPab.App.ActiveReader
{
  public struct ContentLocation
  {
    public String Content { get; set; }
    public int PageIndex { get; set; }
    public Rect ContentBounds { get; set; }
  }
}