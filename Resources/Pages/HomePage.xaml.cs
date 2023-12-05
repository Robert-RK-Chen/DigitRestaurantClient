using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace DigitRestaurantClient.Resources.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HomePage : Page
    {
        private readonly List<string> Pictures = new List<string>
        {
            "/Assets/FlipView/1.jpg",
            "/Assets/FlipView/2.jpg",
            "/Assets/FlipView/3.jpg",
            "/Assets/FlipView/4.jpg",
            "/Assets/FlipView/5.jpg",
            "/Assets/FlipView/6.jpg",
        };

        public HomePage()
        {
            this.InitializeComponent();
            GalleryAutoChange();
        }

        public void GalleryAutoChange()
        {
            GalleryMain.SelectedIndex = 0;
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (o, a) =>
                {
                    int newIndex = GalleryMain.SelectedIndex + 1;
                    if (newIndex >= Pictures.Count)
                    {
                        newIndex = 0;
                    }
                    GalleryMain.SelectedIndex = newIndex;
                };
            timer.Start();
        }
    }
}
