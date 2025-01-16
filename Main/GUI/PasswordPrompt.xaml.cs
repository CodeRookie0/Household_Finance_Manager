using System.Windows;
using System.Windows.Input;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for PasswordPrompt.xaml
    /// </summary>
    public partial class PasswordPrompt : Window
    {
        public string EnteredPassword { get; private set; }
        public PasswordPrompt()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ConfirmPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            EnteredPassword = PasswordInput.Password;
            this.DialogResult = true;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
