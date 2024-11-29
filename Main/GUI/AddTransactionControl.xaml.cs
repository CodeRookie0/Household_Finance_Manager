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
    /// Logika interakcji dla klasy AddTransactionControl.xaml
    /// </summary>
    public partial class AddTransactionControl : Window
    {
        private List<Category> Listcategories {  get; set; }
        private List<Store> Liststores { get; set; }
        private List<Subcategory> Listsubcategory { get; set; }
        private readonly int userid;
        
        public AddTransactionControl(List<Category> argcategoryList,List<Store> argStore,int userId)
        {
            InitializeComponent();
           Listcategories = argcategoryList;
            Liststores = argStore;
           ComboBoxCategory.ItemsSource = Listcategories;
           ComobBoxStore.ItemsSource = Liststores;
            userid = userId;

            Listsubcategory = new List<Subcategory>();
            
        } 

        private void CloseDialog_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void ComboBoxCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
            var item=ComboBoxCategory.SelectedItem as Category;
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

                    foreach(DataRow row in answer.Rows) 
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

        private void AddTransaction(object sender, RoutedEventArgs e) //Miejsce gdzie można wykorzystać sql injection
        {
            Category category = ComboBoxCategory.SelectedItem as Category;
            Subcategory subcategory = ComboSubCategory.SelectedItem as Subcategory;
            Store store=ComobBoxStore.SelectedItem as Store;

            string amount = InputAmount.Text;
            string note = InputNote.Text;
            string date = InputData.SelectedDate?.ToString("yyyy-MM-dd");

            amount = amount.Replace(",", ".");

            StringBuilder query = new StringBuilder("INSERT INTO Transactions (UserID,Amount, Note, Date,TransactionTypeID");

          

            if (category != null)
            {
                query.Append(",CategoryID");
            }
            if (subcategory != null)
            {
                query.Append(",SubCategoryID");
            }
            if (store != null)
            {
                query.Append(",StoreID");
            }

            query.Append(") VALUES ('"+userid.ToString()+"','" + amount + "', '" + note + "', '" + date + "','"+(InpuTypeTransaction.SelectedIndex+1)+"'");

            // Dodajemy wartości dla opcjonalnych pól
            if (category != null)
            {
                query.Append(", '" + category.CategoryID + "'");
            }
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
    }
}
