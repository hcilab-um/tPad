﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Ubicomp.Utils.NET.CAF.DataModel
{

  public interface IEntity : INotifyPropertyChanged
  {
    Guid EntityGuid
    { get; set; }
  }

}
