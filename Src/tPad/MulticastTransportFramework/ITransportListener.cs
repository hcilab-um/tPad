using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ubicomp.Utils.NET.MTF
{

  public interface ITransportListener
  {

    int MessageType { get; }

    void MessageReceived(TransportMessage message, String rawMessage);

  }

}
