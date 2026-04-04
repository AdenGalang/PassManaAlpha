using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PassManaAlpha.MVVM.View
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
            BgVideo.MediaEnded += BgVideo_MediaEnded;
            BgVideo.Loaded += (s, e) => BgVideo.Play();
        }

        private void BgVideo_MediaEnded(object sender, System.Windows.RoutedEventArgs e)
        {
            BgVideo.Stop();
            BgVideo.Position = TimeSpan.Zero;
            BgVideo.Play();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}