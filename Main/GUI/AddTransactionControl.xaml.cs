using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
    /// Logika interakcji dla klasy AddTransactionControl.xaml
    /// </summary>
    public partial class AddTransactionControl : Window
    {
        private ObservableCollection<Category> Listcategories {  get; set; }
        private ObservableCollection<Store> Liststores { get; set; }
        private ObservableCollection<Subcategory> Listsubcategory { get; set; }
       
        private readonly int userid;
        
        public AddTransactionControl(ObservableCollection<Category> argCategoryList,ObservableCollection<Store> argStore,int userId)
        {
            InitializeComponent();
           
            Listcategories = argCategoryList;
            Liststores = argStore;
            CategoryComboBox.ItemsSource = Listcategories;
            StoreComboBox.ItemsSource = Liststores;
            userid = userId;

            Listsubcategory = new ObservableCollection<Subcategory>();
            
        } 

        private void CloseDialog_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            var item=CategoryComboBox.SelectedItem as Category;
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

                    foreach(DataRow row in answer.Rows) 
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

        private void AddTransaction(object sender, RoutedEventArgs e) //Miejsce gdzie można wykorzystać sql injection
        {
            Category category = CategoryComboBox.SelectedItem as Category;
            Subcategory subcategory = SubategoryComboBox.SelectedItem as Subcategory;
            Store store=StoreComboBox.SelectedItem as Store;

            string amount = InputAmount.Text;
            string note = InputNote.Text;
            string datePart = InputData.SelectedDate?.ToString("yyyy-MM-dd");
            string timePart = InputTime.Text;
            if(string.IsNullOrEmpty(amount)) 
            {
                MessageBox.Show("Prosze podać kwotę","Komunikat",MessageBoxButton.OK,MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(timePart))
            {
                MessageBox.Show("Proszę wybrać czas", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (datePart == null)
            {
                MessageBox.Show("Proszę wybrać datę", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            amount = amount.Replace(",", ".");

            StringBuilder query = new StringBuilder("INSERT INTO Transactions (UserID,Amount, Note, Date,TransactionTypeID");

          

            if (category == null)
            {
                MessageBox.Show("Proszę wybrać kategorię","Komunikat",MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            query.Append(",CategoryID");

            if (subcategory != null)
            {
                query.Append(",SubCategoryID");
            }
            if (store != null)
            {
                query.Append(",StoreID");
            }
            if(InpuTypeTransaction.SelectedIndex == 0)
            {
                if (DateTime.TryParseExact($"{datePart} {timePart}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime combinedDateTime))
                {
                    query.Append(") VALUES ('"+userid.ToString()+"','" + amount + "', '" + note + "', '" + combinedDateTime + "','"+(InpuTypeTransaction.SelectedIndex+1)+"'");
                }
                else
                {
                    MessageBox.Show("Nieprawidłowy format czasu. Upewnij się, że wpisujesz czas w formacie HH:mm. Zakres wartości: od 00:00 do 23:59.", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else if (InpuTypeTransaction.SelectedIndex == 1)
            {
                if (DateTime.TryParseExact($"{datePart} {timePart}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime combinedDateTime))
                {
                    query.Append(") VALUES ('" + userid.ToString() + "',-" + amount + ", '" + note + "', '" + combinedDateTime + "','" + (InpuTypeTransaction.SelectedIndex + 1) + "'");
                }
                else
                {
                    MessageBox.Show("Nieprawidłowy format czasu. Upewnij się, że wpisujesz czas w formacie HH:mm. Zakres wartości: od 00:00 do 23:59.", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Dodajemy wartości dla opcjonalnych pól
             query.Append(", '" + category.CategoryID + "'");
            
            if (subcategory != null)
            {
                query.Append(", '" + subcategory.SubcategoryID + "'");
            }
            if (store != null)
            {
                query.Append(", '" + store.StoreId + "'");
            }

            query.Append(");");


            DBSqlite dBSqlite=new DBSqlite();
            int answer = dBSqlite.ExecuteNonQuery(query.ToString());
            if(answer>0)
            {
                MessageBox.Show("Transakcja została dodana do bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Transakcja nie została dodana do bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddInputAmount_KeyDown(object sender, KeyEventArgs e)
        {
            if((e.Key<Key.D0 || e.Key >Key.D9)&&
                (e.Key<Key.NumPad0 || e.Key>Key.NumPad9)
                && e.Key!=Key.OemPeriod
                && e.Key!=Key.Decimal)
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
