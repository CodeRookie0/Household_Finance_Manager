using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

namespace Main.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy EditTransactionControl.xaml
    /// </summary>
    public partial class EditTransactionControl : Window
    {
        private ObservableCollection<Category> Listcategories { get; set; }
        private ObservableCollection<Store> Liststores { get; set; }
        private ObservableCollection<Subcategory> Listsubcategory { get; set; }
        private readonly int userid;
        private readonly Transaction transaction;
        public EditTransactionControl(ObservableCollection<Category> argcategoryList, ObservableCollection<Store> argStore, int userId, Transaction argtransaction)
        {
            InitializeComponent();
            Listcategories = argcategoryList;
            Liststores = argStore;
            Listsubcategory = new ObservableCollection<Subcategory>();
            transaction = argtransaction;

            ComboBoxCategory.ItemsSource = Listcategories;

            // var thisCategory = argcategoryList.Find(c => c.CategoryID == transaction.CategoryID);
            var thisCategory = argcategoryList.FirstOrDefault(x => x.CategoryID == transaction.CategoryID);

            if (thisCategory != null)
            {
                ComboBoxCategory.SelectedIndex = argcategoryList.IndexOf(thisCategory);

                DBSqlite dBSqlite = new DBSqlite();
                DataTable answer = dBSqlite.ExecuteQuery("SELECT SubCategoryID,CategoryID,SubcategoryName,UserID FROM SubCategories WHERE CategoryID=@MyCategoryId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyCategoryId", thisCategory.CategoryID));
                if (answer != null)
                {

                    ComboSubCategory.ItemsSource = null;
                    Listsubcategory.Clear();

                    foreach (DataRow row in answer.Rows)
                    {
                        Subcategory subcategory = new Subcategory();
                        subcategory.SubcategoryID = int.Parse(row[0].ToString());
                        subcategory.CategoryID = int.Parse(row[1].ToString());
                        subcategory.SubcategoryName = row[2].ToString();
                        subcategory.UserID = (row.IsNull(3) == true) ? -1 : int.Parse(row[3].ToString());
                        Listsubcategory.Add(subcategory);
                    }
                    ComboSubCategory.ItemsSource = Listsubcategory;
                    ComboSubCategory.DisplayMemberPath = "SubcategoryName";

                    var thisSubCategory = Listsubcategory.FirstOrDefault(c => c.SubcategoryID == transaction.SubcategoryID);
                    if (thisSubCategory != null)
                    {
                        ComboSubCategory.IsEnabled = true;
                        ComboSubCategory.SelectedIndex = Listsubcategory.IndexOf(thisSubCategory);
                    }
                }
            }


            ComobBoxStore.ItemsSource = Liststores;

            var thisStore = argStore.FirstOrDefault(c => c.StoreId == transaction.StoreID);
            if (thisStore != null)
            {
                ComobBoxStore.SelectedIndex = argStore.IndexOf(thisStore);
            }

            InpuTypeTransaction.SelectedIndex = (transaction.TransactionTypeID - 1);

            if (transaction.SubcategoryID != -1)
            {
                ComboBoxCategory.IsEnabled = true;


            }

            userid = userId;

            Listsubcategory = new ObservableCollection<Subcategory>();


            InputAmount.Text = Math.Abs(transaction.Amount).ToString();
            InputData.Text = transaction.Date.ToString();
            InputNote.Text = transaction.Note.ToString();
        }

        private void CloseDialog_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ComboBoxCategory.SelectedItem as Category;
            if (item != null)
            {

                DBSqlite dBSqlite = new DBSqlite();
                DataTable answer = dBSqlite.ExecuteQuery("SELECT SubCategoryID,CategoryID,SubcategoryName,UserID FROM SubCategories WHERE CategoryID=@MyCategoryId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyCategoryId", item.CategoryID));
                if (answer != null)
                {
                    ComboSubCategory.IsEnabled = true;
                    ComboSubCategory.ItemsSource = null;
                    Listsubcategory.Clear();

                    foreach (DataRow row in answer.Rows)
                    {
                        Subcategory subcategory = new Subcategory();
                        subcategory.SubcategoryID = int.Parse(row[0].ToString());
                        subcategory.CategoryID = int.Parse(row[1].ToString());
                        subcategory.SubcategoryName = row[2].ToString();
                        subcategory.UserID = (row.IsNull(3) == true) ? -1 : int.Parse(row[3].ToString());
                        Listsubcategory.Add(subcategory);
                    }
                    ComboSubCategory.ItemsSource = Listsubcategory;
                    ComboSubCategory.DisplayMemberPath = "SubcategoryName";

                }
            }
        }

        private void EditTransaction_Click(object sender, RoutedEventArgs e) //Miejsce gdzie można wykorzystać sql injection
        {
            // Pobranie wybranych wartości z ComboBoxów i innych kontrolek
            Category category = ComboBoxCategory.SelectedItem as Category;
            Subcategory subcategory = ComboSubCategory.SelectedItem as Subcategory;
            Store store = ComobBoxStore.SelectedItem as Store;

            string amount = InputAmount.Text;
            string note = InputNote.Text;
            string date = InputData.SelectedDate?.ToString("yyyy-MM-dd");
            int transactionId = transaction.TransactionID;   // Tutaj ustaw ID transakcji, którą chcesz zaktualizować
            string transactionType = (InpuTypeTransaction.SelectedIndex + 1).ToString(); // Przykład z TransactionType
            if(date==null)
            {
                MessageBox.Show("Proszę wybrać datę", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else if (string.IsNullOrEmpty(amount))
            {
                MessageBox.Show("Prosze podać kwotę", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            amount = amount.Replace(",", ".");

            // Rozpoczęcie budowania zapytania SQL
            StringBuilder query = new StringBuilder("UPDATE Transactions SET ");
            if (InpuTypeTransaction.SelectedIndex == 0)
            {
                query.Append("Amount = '" + amount + "', ");
            }
            else if (InpuTypeTransaction.SelectedIndex == 1)
            {
                query.Append("Amount = -" + amount + ", ");
            }

            query.Append("Note = '" + note + "', ");
            query.Append("Date = '" + date + "', ");
            query.Append("TransactionTypeID = '" + transactionType + "'");

            // Dodawanie wartości dla opcjonalnych pól
            if (category == null)
            {
                MessageBox.Show("Proszę wybrać kategorię", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            query.Append(", CategoryID = '" + category.CategoryID + "'");

            if (subcategory != null)
            {
                query.Append(", SubCategoryID = '" + subcategory.SubcategoryID + "'");
            }
            if (store != null)
            {
                query.Append(", StoreID = '" + store.StoreId + "'");
            }

            // Określenie, którą transakcję zaktualizować
            query.Append(" WHERE TransactionID = '" + transactionId + "';");

            // Uruchomienie zapytania SQL
            DBSqlite dBSqlite = new DBSqlite(); 

            int updateHistory = dBSqlite.ExecuteNonQuery(
                "UPDATE RecurringPaymentHistory SET ActionTypeID = @ActionTypeID AND Amount = @Amount AND ActionDate = CURRENT_TIMESTAMP WHERE TransactionID = @TransactionID",
                new Microsoft.Data.Sqlite.SqliteParameter("@ActionTypeID", 2),
                new Microsoft.Data.Sqlite.SqliteParameter("@Amount", amount),
                new Microsoft.Data.Sqlite.SqliteParameter("@TransactionID", transaction.TransactionID));

            int answer = dBSqlite.ExecuteNonQuery(query.ToString());

            if (answer > 0)
            {
                MessageBox.Show("Transakcja została zaktualizowana w bazie danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Transakcja nie została zaktualizowana w bazie danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void EditInputAmount_KeyDown(object sender, KeyEventArgs e)
        {
           
            if ((e.Key < Key.D0 || e.Key > Key.D9) &&
                (e.Key < Key.NumPad0 || e.Key > Key.NumPad9)
                && e.Key != Key.OemPeriod
                && e.Key != Key.Decimal)
            {
                e.Handled = true;
                
            }
            
        }
    }
}
