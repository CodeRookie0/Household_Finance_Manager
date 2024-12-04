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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for ChangePasswordControl.xaml
    /// </summary>
    public partial class ChangePasswordControl : Window
    {
        public string PreviousPassword { get; private set; }
        public string NewPassword { get; private set; }
        public string NewConfirmPassword { get; private set; }
        public ChangePasswordControl()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void ConfirmPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            PreviousPassword = PreviousPasswordBox.Password;
            NewPassword = PasswordTextBox.Text;
            NewConfirmPassword = ConfirmPasswordBox.Password;
            this.DialogResult = true;
        }
    }
}
