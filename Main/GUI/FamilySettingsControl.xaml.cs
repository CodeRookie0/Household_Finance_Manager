using Main.Logic;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for FamilySettingsControl.xaml
    /// </summary>
    public partial class FamilySettingsControl : Window
    {
        private MainWindow mainWindow;
        private readonly int userId;
        private readonly int familyId;
        private readonly string familyCode;
        private string familyName;
        private string familyCreatedAt;
        private readonly DispatcherTimer codeDisplayTimer;
        private readonly RotateTransform rotateTransform;
        public string newFamilyName { get; private set; }
        private int failedAttempts = 0; 
        private DateTime? blockUntil = null;
        public FamilySettingsControl(int loggedInUserId,int userFamilyId,MainWindow mainWindow)
        {
            userId = loggedInUserId;
            familyId = userFamilyId;
            this.mainWindow = mainWindow;
            InitializeComponent();

            familyCode = Service.GetCodeByFamilyId(familyId);
            familyName = Service.GetFamilyNameByFamilyId(familyId);
            familyCreatedAt = Service.GetFamilyCreatedAtByFamilyId(familyId);

            FamilyNameInput.Text = familyName;
            FamilyCreatedAtInput.Text = familyCreatedAt;

            codeDisplayTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            codeDisplayTimer.Tick += CodeDisplayTimer_Tick;

            rotateTransform = new RotateTransform();
            PreviewImage.RenderTransform = rotateTransform;
            PreviewImage.RenderTransformOrigin = new Point(0.5, 0.5);
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CodePreviewButton_Click(object sender, RoutedEventArgs e)
        {
            PreviewImage.Source = new BitmapImage(new Uri("../Resources/loading.png", UriKind.Relative));
            PreviewImage.Height = 45;
            PreviewImage.Width = 45;
            if (familyCode == null)
            {

                FamilyCodeInput.Text = "Nie znaleziono kodu rodziny.";
            }
            else
            {
                FamilyCodeInput.IsEnabled = true;
                FamilyCodeInput.Text = familyCode;
            }

            StartRotationAnimation();

            codeDisplayTimer.Start();
        }
        private void CodeDisplayTimer_Tick(object sender, EventArgs e)
        {
            codeDisplayTimer.Stop();
            PreviewImage.Source = new BitmapImage(new Uri("../Resources/preview.png", UriKind.Relative));
            PreviewImage.Height = 32;
            PreviewImage.Width = 32;
            FamilyCodeInput.IsEnabled = false;
            FamilyCodeInput.Text = "* * * * * * * * *";


            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, null);
        }

        private void StartRotationAnimation()
        {
            DoubleAnimation rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
        }

        private void ChangeNameButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeNameButton.IsEnabled = false;
            FamilyNameInput.IsEnabled = true;
            EditButtonsPanel.Visibility = Visibility.Visible;
        }

        private void CancelChangesButton_Click(object sender, RoutedEventArgs e)
        {
            FamilyNameInput.Text = familyName;
            ChangeNameButton.IsEnabled = true;
            FamilyNameInput.IsEnabled = false;
            EditButtonsPanel.Visibility = Visibility.Collapsed;
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            newFamilyName = FamilyNameInput.Text.Trim();
            int minNameLength = 3;

            if (newFamilyName.Length < 3)
            {
                MessageBox.Show("Nazwa rodziny musi mieć co najmniej " + minNameLength + " znaki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Service.UpdateFamilyName(familyId, FamilyNameInput.Text))
            {
                familyName = Service.GetFamilyNameByFamilyId(familyId);
                FamilyNameInput.Text = familyName;
                ChangeNameButton.IsEnabled = true;
                FamilyNameInput.IsEnabled = false;
                EditButtonsPanel.Visibility = Visibility.Collapsed;
                MessageBox.Show("Nazwa rodziny została pomyślnie zmieniona.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Nie udało się zmienić nazwy rodziny. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if (blockUntil.HasValue && DateTime.Now < blockUntil.Value)
            {
                var remainingTime = blockUntil.Value - DateTime.Now;
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
                    var deleteResult = MessageBox.Show("Czy na pewno chcesz usunąć rodzinę? Ta operacja jest nieodwracalna.", "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (deleteResult == MessageBoxResult.Yes)
                    {
                        bool success = Service.DeleteFamily(familyId);

                        if (success)
                        {
                            MessageBox.Show("Rodzina została pomyślnie usunięta.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                            mainWindow.FamilyMembersButton_Click(sender, e);
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Wystąpił błąd podczas usuwania rodziny. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    failedAttempts++; 
                    Console.WriteLine(failedAttempts.ToString());
                    if (failedAttempts >= 3)
                    {
                        blockUntil = DateTime.Now.AddSeconds(30);
                        MessageBox.Show("Wprowadzone hasło jest niepoprawne. Za dużo prób. Spróbuj ponownie za 30 sekund.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzone hasło jest niepoprawne. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
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
