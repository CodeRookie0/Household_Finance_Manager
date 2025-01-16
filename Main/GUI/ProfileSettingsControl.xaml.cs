using Main.Logic;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for ProfileSettingsControl.xaml
    /// </summary>
    public partial class ProfileSettingsControl : Window
    {
        private MainWindow mainWindow;
        private readonly int userId;
        private readonly DataRow tmp;

        private int accountDeletionAttempts = 0;
        private DateTime? accountDeletionBlockUntil = null;
        private int passwordChangeAttempts = 0;
        private DateTime? passwordChangeBlockUntil = null;

        private string userName;
        private string email;
        private string passwordHash;
        private string salt;
        private DateTime createdAt;
        public ProfileSettingsControl(int loggedInUserId, MainWindow mainWindow)
        {
            this.userId = loggedInUserId;
            InitializeComponent();

            var userData = Service.GetUserByUserID(userId);
            userName = userData.UserName;
            email = userData.Email;
            passwordHash = userData.PasswordHash;
            salt = userData.Salt;

            UserNameTextBox.Text = userName;
            EmailTextBox.Text = email;
            DateTime userCreatedAt = createdAt.Date;
            CreatedAtTextBox.Text = userCreatedAt.ToString("D");
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            if (accountDeletionBlockUntil.HasValue && DateTime.Now < accountDeletionBlockUntil.Value)
            {
                var remainingTime = accountDeletionBlockUntil.Value - DateTime.Now;
                MessageBox.Show($"Za dużo nieudanych prób. Spróbuj ponownie za {remainingTime.Seconds} sekundy.", "Blokada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            PasswordPrompt passwordWindow = new PasswordPrompt();
            bool? result = passwordWindow.ShowDialog();

            if (result == true)
            {
                string enteredPassword = passwordWindow.EnteredPassword;

                bool isPasswordCorrect = Service.ValidateUserPassword(userId, enteredPassword);

                if (isPasswordCorrect)
                {
                    var deleteResult = MessageBox.Show("Czy na pewno chcesz usunąć konto? Ta operacja jest nieodwracalna.", "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (deleteResult == MessageBoxResult.Yes)
                    {
                        bool success = Service.DeleteUser(userId);
                        
                        if (success)
                        {
                            foreach (Window window in Application.Current.Windows)
                            {
                                if (window != this)
                                {
                                    window.Close();
                                }
                            }
                            LoginControl loginControl = new LoginControl();
                            loginControl.Show();
                            this.Close();
                            MessageBox.Show("Konto zostało pomyślnie usunięte.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("Wystąpił błąd podczas usuwania konta. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    accountDeletionAttempts++;
                    Console.WriteLine(accountDeletionAttempts.ToString());
                    if (accountDeletionAttempts >= 3)
                    {
                        accountDeletionBlockUntil = DateTime.Now.AddSeconds(30);
                        MessageBox.Show("Wprowadzone hasło jest niepoprawne. Za dużo prób. Spróbuj ponownie za 30 sekund.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzone hasło jest niepoprawne. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ChangeUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width < 550)
            {
                this.Width = 550;
            }
            UserNameTextBox.IsEnabled = true;
            EmailTextBox.IsEnabled = true;
            EditUserDataButtonsPanel.Visibility = Visibility.Visible;
            ChangeUserDataButton.IsEnabled = false;
        }

        private void ChangePasswordDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (passwordChangeBlockUntil.HasValue && DateTime.Now < passwordChangeBlockUntil.Value)
            {
                var remainingTime = passwordChangeBlockUntil.Value - DateTime.Now;
                MessageBox.Show($"Za dużo prób. Spróbuj ponownie za {remainingTime.Seconds} sekundy.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;  
            }

            ChangePasswordDataButton.IsEnabled = false;
            ChangePasswordControl changePasswordControl = new ChangePasswordControl();
            bool? result = changePasswordControl.ShowDialog();
            if (result == true)
            {
                string previousPassword = changePasswordControl.PreviousPassword;
                string newPassword = changePasswordControl.NewPassword;
                string confirmPassword = changePasswordControl.NewConfirmPassword;

                if(!Service.ValidateUserPassword(userId, previousPassword))
                {
                    passwordChangeAttempts++; 
                    if (passwordChangeAttempts >= 3)
                    {
                        passwordChangeBlockUntil = DateTime.Now.AddSeconds(30);
                        MessageBox.Show("Za dużo nieudanych prób. Spróbuj ponownie za 30 sekund.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        ChangePasswordDataButton.IsEnabled = true;
                        return;
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzone poprzednie hasło jest nieprawidłowe.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                        ChangePasswordDataButton.IsEnabled = true;
                        return;
                    }
                }
                else
                {
                    passwordChangeAttempts = 0;
                }
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Hasła nie pasują do siebie.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                    ChangePasswordDataButton.IsEnabled = true;
                    return;
                }
                if (!Service.ValidatePassword(previousPassword))
                {
                    ChangePasswordDataButton.IsEnabled = true;
                    return;
                }
                if (Service.UpdateUserPassword(userId, newPassword))
                {
                    MessageBox.Show("Hasło zostało pomyślnie zaktualizowane.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    if (this.Width >= 550 && EditUserDataButtonsPanel.Visibility != Visibility.Visible)
                    {
                        this.Width = 410;
                    }
                    ChangePasswordDataButton.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Nie udało się zaktualizować hasła. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    ChangePasswordDataButton.IsEnabled = true;
                }
            }
            else
            {
                ChangePasswordDataButton.IsEnabled = true;
            }
        }

        private void CancelUserDataChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width >= 550)
            {
                this.Width = 410;
            }
            UserNameTextBox.IsEnabled = false;
            EmailTextBox.IsEnabled = false;
            ChangeUserDataButton.IsEnabled = true;
            EditUserDataButtonsPanel.Visibility = Visibility.Collapsed;
            UserNameTextBox.Text = userName;
            EmailTextBox.Text = email;
        }

        private void SaveUserDataChangesButton_Click(object sender, RoutedEventArgs e)
        {
            string newUserName=UserNameTextBox.Text;
            string newEmail = EmailTextBox.Text;

            if (string.IsNullOrEmpty(newUserName) || string.IsNullOrEmpty(newEmail))
            {
                MessageBox.Show("Proszę wypełnić wszystkie pola.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Service.IsEmailExistis(newEmail,userId))
            {
                MessageBox.Show("Ten adres e-mail jest już zarejestrowany.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Service.ValidateEmail(newEmail))
            {
                MessageBox.Show("Wprowadzono nieprawidłowy format adresu e-mail. Proszę wprowadzić poprawny adres e-mail.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Service.UpdateUserEmailAndUsername(userId, newEmail, newUserName))
            {
                MessageBox.Show("Dane użytkownika zostały pomyślnie zaktualizowane.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                if (this.Width >= 550)
                {
                    this.Width = 410;
                }
                UserNameTextBox.IsEnabled = false;
                EmailTextBox.IsEnabled = false;
                ChangeUserDataButton.IsEnabled = true;
                EditUserDataButtonsPanel.Visibility = Visibility.Collapsed;
                userName = UserNameTextBox.Text;
                email = EmailTextBox.Text;
            }
            else
            {
                MessageBox.Show("Nie udało się zaktualizować danych użytkownika. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
           this.Close();
        }
    }
}
