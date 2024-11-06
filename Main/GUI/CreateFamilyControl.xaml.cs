using Main.Logic;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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

            if (string.IsNullOrWhiteSpace(familyName))
            {
                MessageBox.Show("Nazwa rodziny nie może być pusta.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Service.UserHasFamily(userId))
            {
                MessageBox.Show("Już stworzyłeś rodzinę.\n" +
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
                MessageBox.Show($"Rodzina została pomyślnie stworzona!\n" +
                $"Twój unikalny kod rodziny: {familyCode}\n\n" +
                "Podziel się tym kodem, aby inni mogli dołączyć do Twojej rodziny. " +
                "Osoby, które chcą dołączyć, powinny wprowadzić ten kod w aplikacji, " +
                "aby wysłać prośbę o dołączenie do wspólnego budżetu.",
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
