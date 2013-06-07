using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Controls;

namespace Org.Juipp.Setup.Content
{
    /// <summary>
    /// Interaction logic for VisualStudioSDK.xaml
    /// </summary>
    public partial class VisualStudioSdk : UserControl, IContent
    {
        public VisualStudioSdk()
        {
            InitializeComponent();
        }

        #region IContent Members

        public void OnFragmentNavigation(FirstFloor.ModernUI.Windows.Navigation.FragmentNavigationEventArgs e)
        {
            
        }

        public void OnNavigatedFrom(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {

        }

        public void OnNavigatedTo(FirstFloor.ModernUI.Windows.Navigation.NavigationEventArgs e)
        {

        }

        public void OnNavigatingFrom(FirstFloor.ModernUI.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (false == ModernDialog.ShowMessage("Navigate away?", "navigate", System.Windows.MessageBoxButton.YesNo)) 
            {
                e.Cancel = true;
            }
        }

        #endregion
    }
}
