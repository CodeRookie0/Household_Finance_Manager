using Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Main.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy ListElementStore.xaml
    /// </summary>
    public partial class ListElementStore : UserControl
    {
        private Store store;
        public ListElementStore(Store argStore)
        {
            InitializeComponent();
            store = argStore;

            StoreName.Text = store.StoryName;
            
            if(store.UserId==-1)
            {
                DeleteStore.IsEnabled = false;
                DeleteStore.Background = Brushes.Gray;
            }
        }

       

        private void Edit_Store(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Store(object sender, RoutedEventArgs e)
        {

        }

        private void HeartToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var HeartPath = (Path)((ToggleButton)sender).Template.FindName("HeartPath",(FrameworkElement)sender);
            if(HeartPath!=null)
            {
                HeartPath.Fill = Brushes.Red;
            }
        }

        private void HeartToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var HeartPath = (Path)((ToggleButton)sender).Template.FindName("HeartPath", (FrameworkElement)sender);
            if (HeartPath != null)
            {
                HeartPath.Fill = Brushes.Transparent;
            }
        }
    }
}
