using Main.Logic;
using Microsoft.Data.Sqlite;
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
    /// Interaction logic for JoinFamilyControl.xaml
    /// </summary>
    public partial class JoinFamilyControl : Window
    {
        private MainWindow mainWindow;
        private readonly int userId;
        public JoinFamilyControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            InitializeComponent();
            this.mainWindow = mainWindow;   
        }

        private void JoinFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            string familyCode = FamilyCodeTextBox.Text;

            if (string.IsNullOrEmpty(familyCode))
            {
                MessageBox.Show("Proszę wprowadzić kod rodziny.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int familyId = Service.GetFamilyIdByCode(familyCode);

            if (familyId == -1)
            {
                MessageBox.Show("Rodzina o podanym kodzie nie istnieje.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Service.UserHasFamily(userId))
            {
                MessageBox.Show("Już jesteś członkiem rodziny.\n" +
                    "Nie możesz należeć do dwóch rodzin jednocześnie. " +
                    "Jeśli chcesz dołączyć do innej rodziny, najpierw musisz opuścić obecną.",
                "Informacja",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
                return;
            }

            if (Service.HasPendingJoinRequest(userId, familyId))
            {
                MessageBox.Show("Już wysłałeś prośbę o dołączenie do tej rodziny. Obecnie czekasz na decyzję twórcy rodziny", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if(Service.AddJoinRequest(familyId, userId))
            {
                MessageBox.Show("Prośba o dołączenie do rodziny została wysłana. Oczekuj na akceptację.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                FamilyCodeTextBox.Clear();
                Close();
                mainWindow.FamilyMembersButton_Click(sender, e);
            }
            else
            {
                MessageBox.Show("Nie udało się wysłać prośby o dołączenie do rodziny. Upewnij się, że kod rodziny jest poprawny lub spróbuj ponownie później.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
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
