using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ubicomp.Utils.NET.MTF;
using UofM.HCI.tPad.App.PhotoAlbum.Network;

namespace UofM.HCI.tPad.App.PhotoAlbum
{
  public class Launcher : ITPadAppLauncher
  {
    public TPadLauncherSettings GetSettings(TPadLauncherSettings settings)
    {
      return settings;
    }

    public TPadApplicationDescriptor GetApplicationDescriptor()
    {
      TPadApplicationDescriptor descriptor = new TPadApplicationDescriptor()
      {
        Name = "Photo Album",
        Icon = UofM.HCI.tPad.App.PhotoAlbum.Properties.Resources.PhotoAlbumIcon,
        AppClass = typeof(PhotoAlbumApp),
        Launcher = this
      };

      return descriptor;
    }

    public ITPadApp GetAppInstance(TPadApplicationDescriptor descriptor, ITPadAppContainer container, ITPadAppController controller, TPadCore core, TPadLauncherSettings settings)
    {
      PhotoAlbumApp photoAlbum = new PhotoAlbumApp(core, container, controller);
      return photoAlbum;
    }

    public void Prepare() { }
  }
}
