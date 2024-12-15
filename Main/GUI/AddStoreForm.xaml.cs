using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Logika interakcji dla klasy AddStoreForm.xaml
    /// </summary>
    public partial class AddStoreForm : Window
    {
        private ObservableCollection<Category> CategoryList;
        private readonly int privUserId;
        public AddStoreForm(int userId)
        {
            InitializeComponent();
            privUserId = userId;
            CategoryList = new ObservableCollection<Category>();
            //DBSqlite dBSqlite = new DBSqlite();
            //var answer = dBSqlite.ExecuteQuery("SELECT Categories.CategoryName FROM Categories WHERE Categories.UserID IS NULL OR Categories.UserID=@UserId;",
            //    new Microsoft.Data.Sqlite.SqliteParameter("@UserId", privUserId));

            //if(answer != null ) 
            //{
            //    foreach(DataRow dataRow in answer.Rows)
            //    {
            //        CategoryName.Add(dataRow[0].ToString());
            //    }
            //}
            List<Category> defaultCategories = Service.GetDefaultCategories();

            int familyId = Service.GetFamilyIdByMemberId(privUserId);
            List<Category> familyCategories = familyId > 0
                ? Service.GetFamilyCategories(familyId)
                : Service.GetUserCategories(privUserId);

            var allCategories = defaultCategories.Concat(familyCategories).Distinct();

            foreach (var category in allCategories.Where(c => Service.IsCategoryFavoriteForUser(privUserId, c.CategoryID)))
            {
                if (!category.CategoryName.StartsWith("❤️ "))
                {
                    category.CategoryName = $"❤️ {category.CategoryName}";
                }
                CategoryList.Add(category);
            }
            foreach (var category in allCategories.Where(c => !Service.IsCategoryFavoriteForUser(privUserId, c.CategoryID)))
            {
                CategoryList.Add(category);
            }

            CategoryComboBox.ItemsSource = CategoryList;
            CategoryComboBox.DisplayMemberPath = "CategoryName";
            CategoryComboBox.SelectedValuePath = "CategoryID";
        }

        private void CloseDialog_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void AddStore_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryComboBox.SelectedValue == null)
            {
                MessageBox.Show("Proszę wybrać kategorię.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (StoreNameTextBox.Text.Length < 3)
            {
                MessageBox.Show("Nazwa sklepu musi mieć więcej niż 2 znaki.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int categoryId = (int)CategoryComboBox.SelectedValue;

            if(!IsStore(StoreNameTextBox.Text, categoryId))
            {
                MessageBox.Show("Istnieje taki sklep", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                DBSqlite dbSqlite = new DBSqlite();
                int answer = dbSqlite.ExecuteNonQuery("INSERT INTO Stores(CategoryID,StoreName,UserID) VALUES(@UserCategoryId,@UserStoreName,@MyId)",
                     new Microsoft.Data.Sqlite.SqliteParameter("@UserCategoryId", categoryId),
                     new SqliteParameter("@UserStoreName", StoreNameTextBox.Text.ToString()),
                     new SqliteParameter("@MyId", privUserId));
                if (answer > 0)
                {
                    MessageBox.Show("Sklep został wstawiony do bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Sklep nie został wstawiony do bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


       private bool IsStore(string StoreName,int categoryId)
        {
            DBSqlite dBSqlite = new DBSqlite();
            DataTable IsFamily = dBSqlite.ExecuteQuery("SELECT FamilyID FROM Users WHERE UserID=@MyID AND FamilyID IS NOT NULL",
                new Microsoft.Data.Sqlite.SqliteParameter("@MyID", privUserId));
            if(IsFamily.Rows.Count > 0)  //Is Family
            {
                DataRow dataRow = IsFamily.Rows[0];
                List<int> familyIdUsers=new List<int>();
                DataTable AllFamilyUserId = dBSqlite.ExecuteQuery("SELECT UserID FROM Users WHERE FamilyID=@MyFamilyID",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyFamilyID", dataRow[0].ToString()));

                if (AllFamilyUserId != null)
                {
                    foreach (DataRow row in AllFamilyUserId.Rows)
                    {
                        familyIdUsers.Add(int.Parse(row[0].ToString()));
                    }
                }

                string idList=string.Join(",", familyIdUsers);

                DataTable answer = dBSqlite.ExecuteQuery($"SELECT Count(StoreID) FROM Stores WHERE CategoryID=@MyCategoryID AND StoreName=@MyStoreName AND UserID IN ({idList})",
                    new SqliteParameter("@MyCategoryID",categoryId)
                    , new SqliteParameter("@MyStoreName",StoreName));
                DataRow dataRow1 = answer.Rows[0];

                if (int.Parse(dataRow1[0].ToString())!=0)
                {
                    return false;
                }
                return true;


            }
            else //No Family
            {
                DataTable answer = dBSqlite.ExecuteQuery($"SELECT Count(StoreID) FROM Stores WHERE CategoryID=@MyCategoryID AND StoreName=@MyStoreName AND UserID=@MyUserId",
                   new SqliteParameter("@MyCategoryID", categoryId)
                   , new SqliteParameter("@MyStoreName", StoreName),
                   new SqliteParameter("@MyUserId",privUserId));
                DataRow dataRow1 = answer.Rows[0];

                if (int.Parse(dataRow1[0].ToString()) != 0)
                {
                    return false;
                }
                return true;
            }
            
        }
    }
}
