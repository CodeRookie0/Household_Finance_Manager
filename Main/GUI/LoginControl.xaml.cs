using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
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
using Main.GUI;
using Main.Logic;

namespace Main
{
    /// <summary>
    /// Interaction logic for LoginControl.xaml
    /// </summary>
    public partial class LoginControl : Window
    {
        public LoginControl()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if(Service.LoginUser(EmailInputTextBox.Text,PasswordInputBox.Password))
            {
                MessageBox.Show("Zalogowano !");
            }
            else 
            {
                MessageBox.Show("Logowanie nie powiodło się. Sprawdź poprawność adresu e-mail i hasła.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/CodeRookie0/Household_Finance_Manager/tree/main";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationControl registrationWindow = new RegistrationControl();
            registrationWindow.Show();
            this.Hide();
        }

        private void EmailHintText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            EmailInputTextBox.Focus();
        }

        private void EmailInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(EmailInputTextBox.Text) && EmailInputTextBox.Text.Length > 0)
            {
                EmailInputTextBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmailInputTextBox.Visibility = Visibility.Visible;
            }
        }

        private void PasswordHintText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PasswordInputBox.Focus();
        }

        private void PasswordInputBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PasswordInputBox.Password) && PasswordInputBox.Password.Length > 0)
            {
                PasswordInputBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                PasswordInputBox.Visibility = Visibility.Visible;
            }
        }

        private void LoginContainerBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CloseImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation
            {
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.2)
            };
            ColorAnimation foregroundAnimation = new ColorAnimation
            {
                To = (Color)ColorConverter.ConvertFromString("#3aa9ad"),
                Duration = TimeSpan.FromSeconds(0.2)
            };

            SolidColorBrush backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
            SolidColorBrush foregroundBrush = new SolidColorBrush(Colors.White);
            LoginButton.Background = backgroundBrush;
            LoginButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }

        private void LoginButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation
            {
                To = Color.FromRgb(58, 169, 173),
                Duration = TimeSpan.FromSeconds(0.2)
            };
            ColorAnimation foregroundAnimation = new ColorAnimation
            {
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.2)
            };

            SolidColorBrush backgroundBrush = new SolidColorBrush(Colors.White);
            SolidColorBrush foregroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
            LoginButton.Background = backgroundBrush;
            LoginButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }
    }
}
