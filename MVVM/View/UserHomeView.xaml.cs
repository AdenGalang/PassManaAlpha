using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PassManaAlpha.MVVM.View
{
    /// <summary>
    /// Interaction logic for UserHomeView.xaml
    /// </summary>
    public partial class UserHomeView : UserControl
    {
        public UserHomeView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e) //who made this bs! 
        {
            kazusa.BeginAnimation(UIElement.OpacityProperty, null);
            kazusa.Opacity = 1;
            Storyboard sb = (Storyboard)FindResource("FadeOutStoryboard");

            sb.Stop(); 

            kazusa.Visibility = Visibility.Visible;
            kazusa.Opacity = 1;

            sb.Begin(); 
        }
    }
}