using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private ObservableCollection<Category> categoryList {  get; set; }
        private ObservableCollection<Store> storeList { get; set; }

        

        public UserTransactionsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();
            LoadTransactions();
            LoadComboBoxValues();
        }

        private void LoadTransactions()
        {
            var transactions = Service.GetUserTransactions(userId);
            TransactionsList.ItemsSource = transactions;


            TransactionSummaryViewModel transactionSummaryViewModel = new TransactionSummaryViewModel(transactions.ToList());
            this.DataContext = transactionSummaryViewModel;
            
        }

        private void LoadTransactions(List<Transaction> transactions)
        {
            TransactionsList.ItemsSource = transactions;

            TransactionSummaryViewModel transactionSummaryViewModel = new TransactionSummaryViewModel(transactions);
            this.DataContext = transactionSummaryViewModel;
        }

        private void LoadComboBoxValues()
        {
            List<Category> defaultCategories = Service.GetDefaultCategories();

            int familyId = Service.GetFamilyIdByMemberId(userId);
            List<Category> familyCategories = familyId > 0 ? Service.GetFamilyCategories(familyId) : Service.GetUserCategories(userId);

            //ObservableCollection<Category> allCategories = new ObservableCollection<Category>();
            categoryList= new ObservableCollection<Category>();
            foreach (var category in defaultCategories)
            {
                categoryList.Add(category);
            }

            foreach (var category in familyCategories)
            {
                categoryList.Add(category);
            }

            CategoryComboBox.ItemsSource = categoryList;
           

            var stores = Service.GetUserStores(userId);
            StoreComboBox.ItemsSource = stores;
            storeList = stores;

            var transactionTypes = Service.GetTransactionTypes();
            TransactionTypeComboBox.ItemsSource = transactionTypes;
        }

        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            AddTransactionControl addTransactionControl = new AddTransactionControl(categoryList,storeList,userId);
            addTransactionControl.ShowDialog();
            LoadTransactions();

        }

        private void EditTransactionButton_Click(object sender, RoutedEventArgs e)
        {
           var button=sender as Button;
           Transaction transaction = button?.DataContext as Transaction;
           if(transaction != null) 
            {
                EditTransactionControl editTransactionControl = new EditTransactionControl(categoryList, storeList, userId, transaction);
                editTransactionControl.ShowDialog();
            }
            LoadTransactions();
        }

        private void DeleteTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            var button=sender as Button;
            Transaction transaction = button?.DataContext as Transaction;
            if(transaction != null)
            {
                MessageBoxResult answerQuestion = MessageBox.Show("Czy na pewno chcesz usunąć transakcję o kwocie "
                    + transaction.Amount + " zrealizowaną w dniu " + transaction.Date, "Komunikat", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answerQuestion == MessageBoxResult.Yes)
                {
                    DBSqlite dBSqlite = new DBSqlite();

                    int updateHistory = dBSqlite.ExecuteNonQuery(
                        "UPDATE RecurringPaymentHistory SET ActionTypeID = @ActionTypeID AND TransactionID=NULL AND ActionDate = CURRENT_TIMESTAMP WHERE TransactionID = @TransactionID",
                        new Microsoft.Data.Sqlite.SqliteParameter("@ActionTypeID", 3),
                        new Microsoft.Data.Sqlite.SqliteParameter("@TransactionID", transaction.TransactionID));

                    int answer = dBSqlite.ExecuteNonQuery("DELETE FROM Transactions WHERE TransactionID=@MyTransactionsId",
                        new Microsoft.Data.Sqlite.SqliteParameter("@MyTransactionsId", transaction.TransactionID));
                    if (answer > 0)
                    {
                        MessageBox.Show("Transakcja została skasowana z bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTransactions();
                    }
                    else
                    {
                        MessageBox.Show("Transakcja nie została skasowana z bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var startDate = StartDatePicker.SelectedDate;
            var endDate = EndDatePicker.SelectedDate;
            var categoryId = CategoryComboBox.SelectedValue as int?;
            var storeId = StoreComboBox.SelectedValue as int?;
            var transactionTypeId = TransactionTypeComboBox.SelectedValue as int?;
            var amountFromText = AmountFromTextBox.Text;
            var amountToText = AmountToTextBox.Text;

            double? amountFrom = null;
            double? amountTo = null; 
            
            if (startDate.HasValue && endDate.HasValue && endDate < startDate)
            {
                MessageBox.Show("Data 'do' nie może być wcześniejsza niż data 'od'.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (!string.IsNullOrWhiteSpace(amountFromText) && double.TryParse(amountFromText, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedAmountFrom))
            {
                amountFrom = parsedAmountFrom;
            }

            if (!string.IsNullOrWhiteSpace(amountToText) && double.TryParse(amountToText, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedAmountTo))
            {
                amountTo = parsedAmountTo;
            }

            if (amountTo.HasValue && amountFrom.HasValue && amountTo < amountFrom)
            {
                MessageBox.Show("Kwota 'do' nie może być mniejsza niż kwota 'od'.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            List<Transaction> transactions = Service.GetFilteredUserTransactions(userId: userId,dateFrom: startDate,dateTo: endDate,categoryId: categoryId,storeId: storeId,transactionTypeId: transactionTypeId,amountFrom: amountFrom,amountTo: amountTo);

            LoadTransactions(transactions);
        }

        private void CleaFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            CategoryComboBox.SelectedIndex = -1;
            StoreComboBox.SelectedIndex = -1;
            TransactionTypeComboBox.SelectedIndex = -1;
            AmountFromTextBox.Clear();
            AmountToTextBox.Clear();
        }

        private void AmountFromTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string currentText = ((TextBox)sender).Text;
            bool isDigitOrSeparator = char.IsDigit(e.Text, 0) || e.Text == "." || e.Text == ",";

            if (e.Text == "." || e.Text == ",")
            {
                if (currentText.Contains(".") || currentText.Contains(","))
                {
                    e.Handled = true;
                    return;
                }
            }

            e.Handled = !isDigitOrSeparator;
        }

        private void AmountToTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string currentText = ((TextBox)sender).Text;
            bool isDigitOrSeparator = char.IsDigit(e.Text, 0) || e.Text == "." || e.Text == ",";

            if (e.Text == "." || e.Text == ",")
            {
                if (currentText.Contains(".") || currentText.Contains(","))
                {
                    e.Handled = true;
                    return;
                }
            }

            e.Handled = !isDigitOrSeparator;
        }

        private void AmountFromTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = textBox.Text;

            if (text.Contains(","))
            {
                text = text.Replace(",", ".");
            }

            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains("."))
                {
                    string[] parts = text.Split('.');

                    if (string.IsNullOrEmpty(parts[0]))
                    {
                        parts[0] = "0";
                    }

                    if (parts.Length > 1 && parts[1].Length > 2)
                    {
                        parts[1] = parts[1].Substring(0, 2);
                    }

                    text = parts[0] + "." + parts[1];
                }
                else
                {
                    text = text + ".00";
                }

                if (Regex.IsMatch(text, @"^\d+(\.\d{1,2})?$"))
                {
                    textBox.Text = text;
                }
                else
                {
                    MessageBox.Show("Proszę podać poprawną kwotę (do dwóch miejsc po przecinku).");
                    textBox.Text = "";
                }
            }
        }

        private void AmountToTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = textBox.Text;

            if (text.Contains(","))
            {
                text = text.Replace(",", ".");
            }

            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains("."))
                {
                    string[] parts = text.Split('.');

                    if (string.IsNullOrEmpty(parts[0]))
                    {
                        parts[0] = "0";
                    }

                    if (parts.Length > 1 && parts[1].Length > 2)
                    {
                        parts[1] = parts[1].Substring(0, 2);
                    }

                    text = parts[0] + "." + parts[1];
                }
                else
                {
                    text = text + ".00";
                }

                if (Regex.IsMatch(text, @"^\d+(\.\d{1,2})?$"))
                {
                    textBox.Text = text;
                }
                else
                {
                    MessageBox.Show("Proszę podać poprawną kwotę (do dwóch miejsc po przecinku).");
                    textBox.Text = "";
                }
            }
        }
    }
}
