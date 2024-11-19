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
    /// Logika interakcji dla klasy AddStoreForm.xaml
    /// </summary>
    public partial class AddStoreForm : Window
    {
        private ObservableCollection<string> CategoryName;
        private readonly int privUserId;
        public AddStoreForm(int userId)
        {
            InitializeComponent();
            privUserId = userId;
            CategoryName = new ObservableCollection<string>();
            DBSqlite dBSqlite = new DBSqlite();
            var answer = dBSqlite.ExecuteQuery("SELECT Categories.CategoryName FROM Categories WHERE Categories.UserID IS NULL OR Categories.UserID=@UserId;",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", privUserId));

            if(answer != null ) 
            {
                foreach(DataRow dataRow in answer.Rows)
                {
                    CategoryName.Add(dataRow[0].ToString());
                }
            }
            cmbCategory.ItemsSource = CategoryName;
        }

        private void CloseDialog_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void AddStore_Click(object sender, RoutedEventArgs e)
        {
            int CategoryId=cmbCategory.SelectedIndex+1;
            DBSqlite dbSqlite = new DBSqlite();
           int answer=dbSqlite.ExecuteNonQuery("INSERT INTO Stores(CategoryID,StoreName,UserID) VALUES(@UserCategoryId,@UserStoreName,@MyId)",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserCategoryId",CategoryId),
                new SqliteParameter("@UserStoreName",txtStoreName.Text.ToString()),
                new SqliteParameter("@MyId",privUserId)); 
            if(answer>0)
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
}
