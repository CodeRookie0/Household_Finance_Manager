using System.Windows;
using System.Windows.Input;

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
