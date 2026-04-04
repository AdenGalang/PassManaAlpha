using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PassManaAlpha.MVVM.ViewModel
{
    class HomeViewModel
    {
        public static implicit operator UserControl(HomeViewModel v)
        {
            throw new NotImplementedException();
        }
    }
}
