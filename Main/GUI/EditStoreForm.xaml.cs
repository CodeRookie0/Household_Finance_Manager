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
        private ObservableCollection<String> stores;
        public EditStoreForm(Store argStore)
        {
            InitializeComponent();
            this.store = argStore;
            stores = new ObservableCollection<String>();

            DBSqlite dBSqlite = new DBSqlite();
            var answer = dBSqlite.ExecuteQuery("SELECT Categories.CategoryName FROM Categories WHERE Categories.UserID IS NULL OR Categories.UserID=@UserId;",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", store.UserId));

            if (answer != null)
            {
                foreach (DataRow dataRow in answer.Rows)
                {
                    stores.Add(dataRow[0].ToString());
                }
            }
            cmbCategory.ItemsSource = stores;

            txtStoreName.Text = argStore.StoreName;
            cmbCategory.SelectedValue = argStore.CategoryName;


            
        }

        private void CloseDialog_Edit(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void SaveEditStore_Click(object sender, RoutedEventArgs e)
        {
            int CategoryId = cmbCategory.SelectedIndex + 1;
            DBSqlite dBsqlite = new DBSqlite();
           int answer=dBsqlite.ExecuteNonQuery("UPDATE STORES SET StoreName=@NewName , CategoryID=@NewCategoryId WHERE StoreID=@MyStoreId",
                new Microsoft.Data.Sqlite.SqliteParameter("@NewName", txtStoreName.Text.ToString()),
                new SqliteParameter("@NewCategoryId", CategoryId),
                new SqliteParameter("@MyStoreId", store.StoreId));
            if(answer>0)
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
}
