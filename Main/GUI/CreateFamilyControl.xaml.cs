using Main.Logic;
using System.Windows;
using System.Windows.Input;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for CreateFamilyControl.xaml
    /// </summary>
    public partial class CreateFamilyControl : Window
    {
        private MainWindow mainWindow; 
        public string familyName { get; private set; }
        private readonly int userId;
        public CreateFamilyControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            InitializeComponent();
            this.mainWindow = mainWindow;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            familyName = FamilyNameTextBox.Text.Trim();
            int minNameLength = 3;

            if (string.IsNullOrWhiteSpace(familyName))
            {
                MessageBox.Show("Nazwa rodziny nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (familyName.Length < 3)
            {
                MessageBox.Show("Nazwa rodziny musi mieć co najmniej " + minNameLength + " znaki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (Service.UserHasFamily(userId))
            {
                MessageBox.Show("Już utworzyłeś rodzinę.\n" +
                    "Nie możesz należeć do dwóch rodzin jednocześnie. " +
                    "Jeśli chcesz dołączyć do innej rodziny, najpierw musisz opuścić obecną.",
                "Informacja",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
                return;
            }

            var familyCodeGenerator = new FamilyCodeGenerator();
            string familyCode = familyCodeGenerator.GenerateUniqueFamilyCode();

            if (Service.AddFamily(userId, familyName, familyCode))
            {
                MessageBox.Show($"Rodzina została pomyślnie utworzona!\n" +
                $"Unikalny kod rodziny znajdziesz w ustawieniach rodziny.\n\n" +
                "Podziel się nim, aby inni mogli dołączyć do Twojej rodziny. Osoby, które chcą dołączyć, mogą wprowadzić ten kod w aplikacji, aby wysłać prośbę o dołączenie do rodziny.",
                "Sukces",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
                FamilyNameTextBox.Clear();
                Close();
                mainWindow.FamilyMembersButton_Click(sender, e);
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas tworzenia rodziny. Proszę spróbować ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
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
