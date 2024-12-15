using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
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
    /// Logika interakcji dla klasy EditStoreForm.xaml
    /// </summary>
    public partial class EditStoreForm : Window
    {
        private readonly Store store;
        private ObservableCollection<Category> CategoryList;
        private readonly int UserId;
        public EditStoreForm(Store argStore,int userId)
        {
            InitializeComponent();
            this.store = argStore;
            this.UserId = userId;
            CategoryList = new ObservableCollection<Category>();

            //DBSqlite dBSqlite = new DBSqlite();
            //var answer = dBSqlite.ExecuteQuery("SELECT Categories.CategoryName FROM Categories WHERE Categories.UserID IS NULL OR Categories.UserID=@UserId;",
            //    new Microsoft.Data.Sqlite.SqliteParameter("@UserId", store.UserId));

            //if (answer != null)
            //{
            //    foreach (DataRow dataRow in answer.Rows)
            //    {
            //        stores.Add(dataRow[0].ToString());
            //    }
            //}
            List<Category> defaultCategories = Service.GetDefaultCategories();

            int familyId = Service.GetFamilyIdByMemberId(UserId);
            List<Category> familyCategories = familyId > 0
                ? Service.GetFamilyCategories(familyId)
                : Service.GetUserCategories(UserId);

            var allCategories = defaultCategories.Concat(familyCategories).Distinct();

            foreach (var category in allCategories.Where(c => Service.IsCategoryFavoriteForUser(UserId, c.CategoryID)))
            {

                if (!category.CategoryName.StartsWith("❤️ "))
                {
                    category.CategoryName = $"❤️ {category.CategoryName}";
                }
                CategoryList.Add(category);
            }
            foreach (var category in allCategories.Where(c => !Service.IsCategoryFavoriteForUser(UserId, c.CategoryID)))
            {
                CategoryList.Add(category);
            }

            CategoryComboBox.ItemsSource = CategoryList;
            CategoryComboBox.DisplayMemberPath = "CategoryName";
            CategoryComboBox.SelectedValuePath = "CategoryID";

            StoreNameTextBox.Text = argStore.StoreName;
            CategoryComboBox.SelectedValue = Service.GetCategoryIDByCategoryName(store.CategoryName);
        }

        private void CloseDialog_Edit(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void SaveEditStore_Click(object sender, RoutedEventArgs e)
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

            if (!IsStore(StoreNameTextBox.Text, categoryId))
            {
                MessageBox.Show("Istnieje taki sklep", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                DBSqlite dBsqlite = new DBSqlite();
                int answer = dBsqlite.ExecuteNonQuery("UPDATE STORES SET StoreName=@NewName , CategoryID=@NewCategoryId WHERE StoreID=@MyStoreId",
                     new Microsoft.Data.Sqlite.SqliteParameter("@NewName", StoreNameTextBox.Text.ToString()),
                     new SqliteParameter("@NewCategoryId", categoryId),
                     new SqliteParameter("@MyStoreId", store.StoreId));
                if (answer > 0)
                {
                    MessageBox.Show("Dane sklepu zostały zaktualizowane", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Dane sklepu nie zostały zaktualizowane", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

        }

        private bool IsStore(string StoreName, int categoryId)
        {
            DBSqlite dBSqlite = new DBSqlite();
            DataTable IsFamily = dBSqlite.ExecuteQuery("SELECT FamilyID FROM Users WHERE UserID=@MyID AND FamilyID IS NOT NULL",
                new Microsoft.Data.Sqlite.SqliteParameter("@MyID", UserId));
            if (IsFamily.Rows.Count > 0)  //Is Family
            {
                DataRow dataRow = IsFamily.Rows[0];
                List<int> familyIdUsers = new List<int>();
                DataTable AllFamilyUserId = dBSqlite.ExecuteQuery("SELECT UserID FROM Users WHERE FamilyID=@MyFamilyID",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyFamilyID", dataRow[0].ToString()));

                if (AllFamilyUserId != null)
                {
                    foreach (DataRow row in AllFamilyUserId.Rows)
                    {
                        familyIdUsers.Add(int.Parse(row[0].ToString()));
                    }
                }

                string idList = string.Join(",", familyIdUsers);

                DataTable answer = dBSqlite.ExecuteQuery($"SELECT Count(StoreID) FROM Stores WHERE CategoryID=@MyCategoryID AND StoreName=@MyStoreName AND UserID IN ({idList})",
                    new SqliteParameter("@MyCategoryID", categoryId)
                    , new SqliteParameter("@MyStoreName", StoreName));
                DataRow dataRow1 = answer.Rows[0];

                if (int.Parse(dataRow1[0].ToString()) != 0)
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
                   new SqliteParameter("@MyUserId", UserId));
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
