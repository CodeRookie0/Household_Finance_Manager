using Main.Controls;
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
        public StoresControl(int userId)
        {
            InitializeComponent();
            UserId = userId;

            //var store = new ListElementStore();
            
            DBSqlite dBSqlite = new DBSqlite();
            var answer = dBSqlite.ExecuteQuery("SELECT Stores.StoreID,Categories.CategoryName,Stores.StoreName,Stores.UserID,Stores.IsFavorite FROM Stores INNER JOIN Categories ON  Stores.CategoryID=Categories.CategoryID;");
            if (answer != null) 
            {
                foreach(DataRow row in answer.Rows) 
                {
                    int dbValue = Convert.ToInt32(row["IsFavorite"]);
                    bool boolValue = dbValue == 1;
                    Store tmpStore = new Store(
                        Int32.Parse(row[0].ToString()),
                        UserId, boolValue);
                    tmpStore.CategoryName = row[1].ToString();
                    tmpStore.StoreName = row[2].ToString(); 
                    var StoreElementList = new ListElementStore(tmpStore);
                    StoreElementList.ItemDeleted += UpdateOutSideClick;
                    storeList.Children.Add(StoreElementList);
                }
            }

           
            
        }

        private void UpdateOutSideClick(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void RefreshData()
        {
           storeList.Children.Clear();
            DBSqlite dBSqlite = new DBSqlite();
            var answer = dBSqlite.ExecuteQuery("SELECT Stores.StoreID,Categories.CategoryName,Stores.StoreName,Stores.UserID,Stores.IsFavorite FROM Stores INNER JOIN Categories ON  Stores.CategoryID=Categories.CategoryID;");
            if (answer != null)
            {
                foreach (DataRow row in answer.Rows)
                {
                    int dbValue = Convert.ToInt32(row["IsFavorite"]);
                    bool boolValue = dbValue == 1;
                    Store tmpStore = new Store(
                        Int32.Parse(row[0].ToString()),
                        UserId,
                        boolValue);
                    tmpStore.CategoryName = row[1].ToString();
                    tmpStore.StoreName = row[2].ToString();
                    var StoreElementList = new ListElementStore(tmpStore);
                    StoreElementList.ItemDeleted += UpdateOutSideClick;
                    storeList.Children.Add(StoreElementList);
                }
            }

        }

        private void AddStoresButton_Click(object sender, RoutedEventArgs e)
        {
             AddStoreForm addStoreForm = new AddStoreForm(UserId);
             addStoreForm.ShowDialog();
            RefreshData();
            

        }
    }
}
