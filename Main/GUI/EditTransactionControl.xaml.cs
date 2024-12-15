using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

            argStore.Insert(0, new Store(-1, -1, false) { StoreName = "Wybierz" });

            Listcategories = argcategoryList;
            Liststores = argStore;
            Listsubcategory = new ObservableCollection<Subcategory>();
            transaction = argtransaction;

            CategoryComboBox.ItemsSource = Listcategories;

            // var thisCategory = argcategoryList.Find(c => c.CategoryID == transaction.CategoryID);
            var thisCategory = argcategoryList.FirstOrDefault(x => x.CategoryID == transaction.CategoryID);

            if (thisCategory != null)
            {
                CategoryComboBox.SelectedIndex = argcategoryList.IndexOf(thisCategory);

                DBSqlite dBSqlite = new DBSqlite();
                DataTable answer = dBSqlite.ExecuteQuery("SELECT SubCategoryID,CategoryID,SubcategoryName,UserID FROM SubCategories WHERE CategoryID=@MyCategoryId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyCategoryId", thisCategory.CategoryID));
                if (answer != null)
                {

                    SubategoryComboBox.ItemsSource = null;
                    Listsubcategory.Clear();
                    Listsubcategory.Insert(0, new Subcategory { SubcategoryName = "Wybierz" });

                    foreach (DataRow row in answer.Rows)
                    {
                        Subcategory subcategory = new Subcategory();
                        subcategory.SubcategoryID = int.Parse(row[0].ToString());
                        subcategory.CategoryID = int.Parse(row[1].ToString());
                        subcategory.SubcategoryName = row[2].ToString();
                        subcategory.UserID = (row.IsNull(3) == true) ? -1 : int.Parse(row[3].ToString());
                        Listsubcategory.Add(subcategory);
                    }
                    SubategoryComboBox.ItemsSource = Listsubcategory;
                    SubategoryComboBox.DisplayMemberPath = "SubcategoryName";

                    var thisSubCategory = Listsubcategory.FirstOrDefault(c => c.SubcategoryID == transaction.SubcategoryID);
                    if (thisSubCategory != null)
                    {
                        SubategoryComboBox.IsEnabled = true;
                        SubategoryComboBox.SelectedIndex = Listsubcategory.IndexOf(thisSubCategory);
                        
                    }
                    else
                    {
                        SubategoryComboBox.IsEnabled = true;
                        SubategoryComboBox.SelectedIndex = 0;
                    }
                }
            }
           


            StoreComboBox.ItemsSource = Liststores;

            var thisStore = argStore.FirstOrDefault(c => c.StoreId == transaction.StoreID);
            if (thisStore != null)
            {
                StoreComboBox.SelectedIndex = argStore.IndexOf(thisStore);
            }

            InpuTypeTransaction.SelectedIndex = (transaction.TransactionTypeID - 1);

            if (transaction.SubcategoryID != -1)
            {
                CategoryComboBox.IsEnabled = true;


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
            var item = CategoryComboBox.SelectedItem as Category;
            if (item != null)
            {

                DBSqlite dBSqlite = new DBSqlite();
                DataTable answer = dBSqlite.ExecuteQuery("SELECT SubCategoryID,CategoryID,SubcategoryName,UserID FROM SubCategories WHERE CategoryID=@MyCategoryId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyCategoryId", item.CategoryID));
                if (answer != null)
                {
                    SubategoryComboBox.IsEnabled = true;
                    SubategoryComboBox.ItemsSource = null;
                    Listsubcategory.Clear();
                    Listsubcategory.Insert(0, new Subcategory { SubcategoryName = "Wybierz" });

                    foreach (DataRow row in answer.Rows)
                    {
                        Subcategory subcategory = new Subcategory();
                        subcategory.SubcategoryID = int.Parse(row[0].ToString());
                        subcategory.CategoryID = int.Parse(row[1].ToString());
                        subcategory.SubcategoryName = row[2].ToString();
                        subcategory.UserID = (row.IsNull(3) == true) ? -1 : int.Parse(row[3].ToString());
                        Listsubcategory.Add(subcategory);
                    }
                    SubategoryComboBox.ItemsSource = Listsubcategory;
                    SubategoryComboBox.DisplayMemberPath = "SubcategoryName";
                   

                }
            }
        }

        private void EditTransaction_Click(object sender, RoutedEventArgs e) //Miejsce gdzie można wykorzystać sql injection
        {
            // Pobranie wybranych wartości z ComboBoxów i innych kontrolek
            Category category = CategoryComboBox.SelectedItem as Category;
            Subcategory subcategory = SubategoryComboBox.SelectedItem as Subcategory;
            Store store = StoreComboBox.SelectedItem as Store;

            string amount = InputAmount.Text;
            string note = InputNote.Text;
            string datePart = InputData.SelectedDate?.ToString("yyyy-MM-dd");
            string timePart = InputTime.Text;
            int transactionId = transaction.TransactionID;   // Tutaj ustaw ID transakcji, którą chcesz zaktualizować

            if (string.IsNullOrEmpty(amount))
            {
                MessageBox.Show("Prosze podać kwotę", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if(datePart==null)
            {
                MessageBox.Show("Proszę wybrać datę", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(timePart))
            {
                MessageBox.Show("Proszę wybrać czas", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string transactionType = (InpuTypeTransaction.SelectedIndex + 1).ToString(); // Przykład z TransactionType
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
            if (DateTime.TryParseExact($"{datePart} {timePart}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime combinedDateTime))
            {
                query.Append("Date = '" + combinedDateTime + "', ");
            }
            else
            {
                MessageBox.Show("Nieprawidłowy format czasu. Upewnij się, że wpisujesz czas w formacie HH:mm. Zakres wartości: od 00:00 do 23:59.", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            query.Append("TransactionTypeID = '" + transactionType + "'");

            // Dodawanie wartości dla opcjonalnych pól
            if (category == null)
            {
                MessageBox.Show("Proszę wybrać kategorię", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            query.Append(", CategoryID = '" + category.CategoryID + "'");

            if (subcategory != null && subcategory.SubcategoryName != "Wybierz") 
            {
                query.Append(", SubCategoryID = '" + subcategory.SubcategoryID + "'");
            }
            if (store != null && store.StoreName!="Wybierz")
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

        private void InputTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9:]");
            e.Handled = regex.IsMatch(e.Text);
        }
        private void InputTime_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!Regex.IsMatch(text, @"^([01]\d|2[0-3]):([0-5]\d)$"))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void InputTime_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!Regex.IsMatch(InputTime.Text, @"^([01]\d|2[0-3]):[0-5]\d$"))
            {
                MessageBox.Show("Nieprawidłowy format czasu! Użyj formatu HH:mm.", "Błąd walidacji", MessageBoxButton.OK, MessageBoxImage.Error);
                InputTime.Text = "00:00";
            }
        }
    }
}
