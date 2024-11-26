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
        private List<Category> categoryList {  get; set; }
        private List<Store> storeList { get; set; }

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
            categoryList = categories;

            var stores = Service.GetUserStores(userId);
            StoreComboBox.ItemsSource = stores;
            storeList = stores;
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            AddTransactionControl addTransactionControl = new AddTransactionControl(categoryList,storeList,userId);
            addTransactionControl.ShowDialog();

        }

        private void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
           var button=sender as Button;
           Transaction transaction = button?.DataContext as Transaction;
           if(transaction != null) 
            {
                EditTransactionControl editTransactionControl = new EditTransactionControl(categoryList, storeList, userId, transaction);
                editTransactionControl.ShowDialog();
            }
        }

        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            var button=sender as Button;
            Transaction transaction = button?.DataContext as Transaction;
            if(transaction != null)
            {
                DBSqlite dBSqlite = new DBSqlite();
                int answer=dBSqlite.ExecuteNonQuery("DELETE FROM Transactions WHERE TransactionID=@MyTransactionsId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyTransactionsId", transaction.TransactionID));
                if(answer > 0) 
                {
                    MessageBox.Show("Transakcja została skasowana z bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Transakcja nie została skasowana z bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
        }
    }
}
