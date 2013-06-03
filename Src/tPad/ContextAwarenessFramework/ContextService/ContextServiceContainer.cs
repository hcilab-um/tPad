using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ubicomp.Utils.NET.CAF.ContextService
{

  public class ContextServiceContainer
  {

    public event EventHandler OnInitialize;
    public event EventHandler OnFinalize;

    private bool servicesStarted = false;
    private List<ContextService> services = new List<ContextService>();
    private Dictionary<ContextService, Thread> threadsHT = new Dictionary<ContextService, Thread>();

    public void AddContextService(ContextService service)
    {
      ThreadStart serviceStart = new ThreadStart(service.Run);
      Thread serviceThread = new Thread(serviceStart);
      serviceThread.IsBackground = true;

      if (!services.Contains(service))
      {
        services.Add(service);
        threadsHT.Add(service, serviceThread);
      }

      if (servicesStarted)
      {
        serviceThread.Start();
        service.Start();
      }
    }

    public ContextService GetContextService(Type contextServiceType)
    {
      foreach (ContextService service in services)
      {
        if (service.GetType() == contextServiceType)
          return service;
      }
      return null;
    }

    public void StartServices()
    {
      if (OnInitialize != null)
        OnInitialize(null, EventArgs.Empty);

      foreach (ContextService service in services)
      {
        Thread serviceThread = threadsHT[service];
        serviceThread.Start();
        service.Start();
      }

      servicesStarted = true;
    }

    public void StopServices()
    {
      if (OnFinalize != null)
        OnFinalize(null, EventArgs.Empty);

      foreach (ContextService service in services)
      {
        service.Stop();
        Thread serviceThread = threadsHT[service];
        serviceThread.Abort();
      }

      servicesStarted = false;
    }

  }

}
