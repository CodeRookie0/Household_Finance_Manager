using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Data;
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
        private List<Category> Listcategories { get; set; }
        private List<Store> Liststores { get; set; }
        private List<Subcategory> Listsubcategory { get; set; }
        private readonly int userid;
        private readonly Transaction transaction;
        public EditTransactionControl(List<Category> argcategoryList, List<Store> argStore, int userId, Transaction argtransaction)
        {
            InitializeComponent();
            Listcategories = argcategoryList;
            Liststores = argStore;
            Listsubcategory = new List<Subcategory>();
            transaction = argtransaction;

            ComboBoxCategory.ItemsSource = Listcategories;

            var thisCategory = argcategoryList.Find(c => c.CategoryID == transaction.CategoryID);

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

                    var thisSubCategory = Listsubcategory.Find(c => c.SubcategoryID == transaction.SubcategoryID);
                    if (thisSubCategory != null)
                    {
                        ComboSubCategory.IsEnabled = true;
                        ComboSubCategory.SelectedIndex = Listsubcategory.IndexOf(thisSubCategory);
                    }
                }
            }


            ComobBoxStore.ItemsSource = Liststores;

            var thisStore = argStore.Find(c => c.StoreId == transaction.StoreID);
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

            Listsubcategory = new List<Subcategory>();


            InputAmount.Text = transaction.Amount.ToString();
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

            amount = amount.Replace(",", ".");

            // Rozpoczęcie budowania zapytania SQL
            StringBuilder query = new StringBuilder("UPDATE Transactions SET ");
            query.Append("Amount = '" + amount + "', ");
            query.Append("Note = '" + note + "', ");
            query.Append("Date = '" + date + "', ");
            query.Append("TransactionTypeID = '" + transactionType + "'");

            // Dodawanie wartości dla opcjonalnych pól
            if (category != null)
            {
                query.Append(", CategoryID = '" + category.CategoryID + "'");
            }
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
    }
}
