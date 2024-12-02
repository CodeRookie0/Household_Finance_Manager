using Main.Controls;
using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Printing;
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
    /// Logika interakcji dla klasy StoresControl.xaml
    /// </summary>
    public partial class StoresControl : UserControl
    {
        private readonly int UserId;
        private List<int> familyIdUsers;
        private bool IsFamily;
        public StoresControl(int userId)
        {
            InitializeComponent();
            UserId = userId;
            familyIdUsers = new List<int>();
            IsFamily = false;

            LogicStoreMagnament();

            //var store = new ListElementStore();
            
            DBSqlite dBSqlite = new DBSqlite();
            if (IsFamily)
            {
                DataTable ListFavorite = dBSqlite.ExecuteQuery("SELECT StoreID FROM FavoriteStores WHERE UserID=@MyId",
                            new Microsoft.Data.Sqlite.SqliteParameter("@MyId", userId));
                List<int> storeIdFavorite = new List<int>();

                if (ListFavorite != null)
                {
                    foreach (DataRow dataRow in ListFavorite.Rows)
                    {
                        storeIdFavorite.Add(int.Parse(dataRow[0].ToString()));
                    }
                }

                string IdList = string.Join(",", familyIdUsers);

                var answer = dBSqlite.ExecuteQuery($"SELECT Stores.StoreID,Categories.CategoryName,Stores.StoreName,Stores.UserID FROM Stores INNER JOIN Categories ON  Stores.CategoryID=Categories.CategoryID WHERE Stores.UserID IN ({IdList});");
                if (answer != null)
                {
                    foreach (DataRow row in answer.Rows)
                    {
                        bool boolValue = false;
                        if (storeIdFavorite.Contains(int.Parse(row[0].ToString())))
                        {
                            boolValue = true;
                        }

                        Store tmpStore = new Store(
                            Int32.Parse(row[0].ToString()),
                            int.Parse(row[3].ToString()), boolValue);
                        tmpStore.CategoryName = row[1].ToString();
                        tmpStore.StoreName = row[2].ToString();
                        var StoreElementList = new ListElementStore(tmpStore, UserId);
                        StoreElementList.ItemDeleted += UpdateOutSideClick;
                        storeList.Children.Add(StoreElementList);
                    }
                }
            }
            else
            {
                DataTable ListFavorite = dBSqlite.ExecuteQuery("SELECT StoreID FROM FavoriteStores WHERE UserID=@MyId",
                           new Microsoft.Data.Sqlite.SqliteParameter("@MyId", userId));
                List<int> storeIdFavorite = new List<int>();

                if (ListFavorite != null)
                {
                    foreach (DataRow dataRow in ListFavorite.Rows)
                    {
                        storeIdFavorite.Add(int.Parse(dataRow[0].ToString()));
                    }
                }

                var answer = dBSqlite.ExecuteQuery("SELECT Stores.StoreID,Categories.CategoryName,Stores.StoreName,Stores.UserID FROM Stores INNER JOIN Categories ON  Stores.CategoryID=Categories.CategoryID WHERE Stores.UserID=@MyUserId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyUserId", UserId));
                if (answer != null)
                {
                    foreach (DataRow row in answer.Rows)
                    {
                        bool boolValue = false;
                        if (storeIdFavorite.Contains(int.Parse(row[0].ToString())))
                        {
                            boolValue = true;
                        }

                        Store tmpStore = new Store(
                            Int32.Parse(row[0].ToString()),
                            int.Parse(row[3].ToString()), boolValue);
                        tmpStore.CategoryName = row[1].ToString();
                        tmpStore.StoreName = row[2].ToString();
                        var StoreElementList = new ListElementStore(tmpStore, UserId);
                        StoreElementList.ItemDeleted += UpdateOutSideClick;
                        storeList.Children.Add(StoreElementList);
                    }
                }

            }

           
            
        }

        private void UpdateOutSideClick(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                storeList.Children.Clear();
            });

            DBSqlite dBSqlite = new DBSqlite();
            if (IsFamily)
            {
                DataTable ListFavorite = dBSqlite.ExecuteQuery("SELECT StoreID FROM FavoriteStores WHERE UserID=@MyId",
                            new Microsoft.Data.Sqlite.SqliteParameter("@MyId", UserId));
                List<int> storeIdFavorite = new List<int>();

                if (ListFavorite != null)
                {
                    foreach (DataRow dataRow in ListFavorite.Rows)
                    {
                        storeIdFavorite.Add(int.Parse(dataRow[0].ToString()));
                    }
                }

                string IdList = string.Join(",", familyIdUsers);

                var answer = dBSqlite.ExecuteQuery($"SELECT Stores.StoreID,Categories.CategoryName,Stores.StoreName,Stores.UserID FROM Stores INNER JOIN Categories ON  Stores.CategoryID=Categories.CategoryID WHERE Stores.UserID IN ({IdList});");
                if (answer != null)
                {
                    foreach (DataRow row in answer.Rows)
                    {
                        bool boolValue = false;
                        if (storeIdFavorite.Contains(int.Parse(row[0].ToString())))
                        {
                            boolValue = true;
                        }

                        Store tmpStore = new Store(
                            Int32.Parse(row[0].ToString()),
                            int.Parse(row[3].ToString()), boolValue);
                        tmpStore.CategoryName = row[1].ToString();
                        tmpStore.StoreName = row[2].ToString();
                        var StoreElementList = new ListElementStore(tmpStore, UserId);
                        StoreElementList.ItemDeleted += UpdateOutSideClick;
                        storeList.Children.Add(StoreElementList);
                    }
                }
            }
            else
            {
                DataTable ListFavorite = dBSqlite.ExecuteQuery("SELECT StoreID FROM FavoriteStores WHERE UserID=@MyId",
                           new Microsoft.Data.Sqlite.SqliteParameter("@MyId", UserId));
                List<int> storeIdFavorite = new List<int>();

                if (ListFavorite != null)
                {
                    foreach (DataRow dataRow in ListFavorite.Rows)
                    {
                        storeIdFavorite.Add(int.Parse(dataRow[0].ToString()));
                    }
                }

                var answer = dBSqlite.ExecuteQuery("SELECT Stores.StoreID,Categories.CategoryName,Stores.StoreName,Stores.UserID FROM Stores INNER JOIN Categories ON  Stores.CategoryID=Categories.CategoryID WHERE Stores.UserID=@MyUserId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyUserId", UserId));
                if (answer != null)
                {
                    foreach (DataRow row in answer.Rows)
                    {
                        bool boolValue = false;
                        if (storeIdFavorite.Contains(int.Parse(row[0].ToString())))
                        {
                            boolValue = true;
                        }

                        Store tmpStore = new Store(
                            Int32.Parse(row[0].ToString()),
                            int.Parse(row[3].ToString()), boolValue);
                        tmpStore.CategoryName = row[1].ToString();
                        tmpStore.StoreName = row[2].ToString();
                        var StoreElementList = new ListElementStore(tmpStore, UserId);
                        StoreElementList.ItemDeleted += UpdateOutSideClick;
                        storeList.Children.Add(StoreElementList);
                    }
                }

            }


        }

        private void AddStoresButton_Click(object sender, RoutedEventArgs e)
        {
            
             AddStoreForm addStoreForm = new AddStoreForm(UserId);
             addStoreForm.ShowDialog();
            RefreshData();
            

        }

        private void LogicStoreMagnament()
        {
            DBSqlite dBSqlite = new DBSqlite();

            DataTable IsFamily = dBSqlite.ExecuteQuery("SELECT FamilyID FROM Users WHERE UserID=@MyID AND FamilyID IS NOT NULL",
                new Microsoft.Data.Sqlite.SqliteParameter("@MyID",UserId));
            if (IsFamily.Rows.Count>0) 
            {
                DataRow dataRow = IsFamily.Rows[0];
                DataTable AllFamilyUserId = dBSqlite.ExecuteQuery("SELECT UserID FROM Users WHERE FamilyID=@MyFamilyID",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyFamilyID", dataRow[0].ToString()));

                if(AllFamilyUserId!=null)
                {
                    foreach(DataRow row in AllFamilyUserId.Rows) 
                    {
                        familyIdUsers.Add(int.Parse(row[0].ToString()));
                    }
                }
                this.IsFamily = true;

            }
            else //Not Family
            {
               
                this.IsFamily= false;
            }
            
        }
    }
}
