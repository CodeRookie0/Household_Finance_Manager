using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for UserTransactionsControl.xaml
    /// </summary>
    public partial class UserTransactionsControl : UserControl
    {
        private MainWindow mainWindow;
        private readonly int userId;
        public UserTransactionsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            var transactions = Service.GetUserTransactions(userId);
            TransactionsList.ItemsSource = transactions;

            var categories = Service.GetCategories(userId); 
            CategoryComboBox.ItemsSource = categories;

            var stores = Service.GetUserStores(userId);
            StoreComboBox.ItemsSource = stores;
        }
    }
}
