using Main.Logic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
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
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for RegistrationControl.xaml
    /// </summary>
    public partial class RegistrationControl : Window
    {
        public RegistrationControl()
        {
            InitializeComponent();
        }

        private void CompleteRegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UserNameInputTextBox.Text.Trim();
            string email = EmailInputTextBox.Text.Trim();
            string password = PasswordInputBox.Password.Trim();
            string confirmPassword = ConfirmPasswordInputBox.Password.Trim();
            

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Proszę wypełnić wszystkie pola.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Hasła nie pasują do siebie.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Service.IsEmailExistis(email,-1))
            {
                MessageBox.Show("Ten adres e-mail jest już zarejestrowany.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if(!Service.ValidateEmail(email))
            {
                MessageBox.Show("Wprowadzono nieprawidłowy format adresu e-mail. Proszę wprowadzić poprawny adres e-mail.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            

            if (!Service.ValidatePassword(password))
            {
                return;
            }

            string saltBytes = Service.GenerateSalt();

            string hashedPasssword= Service.HashPassword(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(saltBytes));
            
            if(Service.AddUser(username, email, hashedPasssword,saltBytes))
            {
                MessageBox.Show("Rejestracja zakończona pomyślnie!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                UserNameInputTextBox.Clear();
                EmailInputTextBox.Clear();
                PasswordInputBox.Clear();
                ConfirmPasswordInputBox.Clear();
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas rejestracji. Spróbuj ponownie.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginControl loginWindow = new LoginControl();
            loginWindow.Show();
            this.Hide();
        }

        private void CloseImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RegistrationContainerBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void CompleteRegistrationButton_MouseEnter(object sender, MouseEventArgs e)
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
            CompleteRegistrationButton.Background = backgroundBrush;
            CompleteRegistrationButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }

        private void CompleteRegistrationButton_MouseLeave(object sender, MouseEventArgs e)
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
            CompleteRegistrationButton.Background = backgroundBrush;
            CompleteRegistrationButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }

        private void UserNameInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UserNameInputTextBoxPlaceholder.Visibility = Visibility.Hidden;
        }

        private void UserNameInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UserNameInputTextBox.Text == "")
            {
                UserNameInputTextBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void UserNameInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserNameInputTextBox.Text != "")
            {
                UserNameInputTextBoxPlaceholder.Visibility= Visibility.Hidden;
            }
            else
            {
                UserNameInputTextBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void EmailInputTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            EmailInputTextBoxPlaceholder.Visibility = Visibility.Hidden;
        }

        private void EmailInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (EmailInputTextBox.Text == "")
            {
                EmailInputTextBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void EmailInputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EmailInputTextBox.Text != "")
            {
                EmailInputTextBoxPlaceholder.Visibility = Visibility.Hidden;
            }
            else
            {
                EmailInputTextBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordInputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordInputBoxPlaceholder.Visibility = Visibility.Hidden;
        }

        private void PasswordInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordInputBox.Password))
            {
                PasswordInputBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void PasswordInputBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(PasswordInputBox.Password))
            {
                PasswordInputBoxPlaceholder.Visibility = Visibility.Hidden;
            }
            else
            {
                PasswordInputBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void ConfirmPasswordInputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordInputBoxPlaceholder.Visibility = Visibility.Hidden;
        }

        private void ConfirmPasswordInputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ConfirmPasswordInputBox.Password))
            {
                ConfirmPasswordInputBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void ConfirmPasswordInputBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ConfirmPasswordInputBox.Password))
            {
                ConfirmPasswordInputBoxPlaceholder.Visibility = Visibility.Hidden;
            }
            else
            {
                ConfirmPasswordInputBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }
    }
}
