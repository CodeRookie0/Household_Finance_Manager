using Main.Logic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for CreateFamilyPrompt.xaml
    /// </summary>
    public partial class CreateFamilyPrompt : UserControl
    {
        private MainWindow mainWindow;
        private readonly int userId;

        private CreateFamilyControl createFamilyWindow;
        private JoinFamilyControl joinFamilyWindow;
        public CreateFamilyPrompt(int loggedInUserId,MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();
            LoadJoinRequests();
        }

        private void LoadJoinRequests()
        {
            var joinRequests = Service.GetJoinRequestsByUserId(userId);
            JoinRequestsListView.ItemsSource = joinRequests;
        }

        private void CreateFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if ((createFamilyWindow != null && createFamilyWindow.IsVisible) ||
                (joinFamilyWindow != null && joinFamilyWindow.IsVisible))
            {
                MessageBox.Show("Jedno z okien jest już otwarte. Zamknij je przed otwarciem nowego.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            createFamilyWindow = new CreateFamilyControl(userId,mainWindow);
            createFamilyWindow.Show();
        }

        private void JoinFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if ((createFamilyWindow != null && createFamilyWindow.IsVisible) ||
                (joinFamilyWindow != null && joinFamilyWindow.IsVisible))
            {
                MessageBox.Show("Jedno z okien jest już otwarte. Zamknij je przed otwarciem nowego.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            joinFamilyWindow = new JoinFamilyControl(userId, mainWindow);
            joinFamilyWindow.Show();
        }
    }
}
