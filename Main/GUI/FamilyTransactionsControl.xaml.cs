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
    /// Interaction logic for FamilyTransactionsControl.xaml
    /// </summary>
    public partial class FamilyTransactionsControl : UserControl
    {
        private MainWindow mainWindow;
        private readonly int userId;
        public FamilyTransactionsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();

            int userRole = Service.GetRoleIDByUserID(userId);
            if (userRole != 1)
            {

            }
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            int familyId = Service.GetFamilyIdByMemberId(userId);
            if (familyId < 0)
            {
                EmptyGrid.Visibility = Visibility.Visible;
                FilledGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                var transactions = Service.GetFamilyTransactions(familyId);
                TransactionsList.ItemsSource = transactions;

                var users = Service.GetFamilyMembersByFamilyId(familyId);
                UserComboBox.ItemsSource = users;

                var categories = Service.GetFamilyCategories(familyId);
                CategoryComboBox.ItemsSource = categories;

                var stores = Service.GetFamilyStores(familyId);
                StoreComboBox.ItemsSource = stores;
            }
        }
    }
}
