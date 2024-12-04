using Main.GUI;
using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Main.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy ListElementStore.xaml
    /// </summary>
    public partial class ListElementStore : UserControl
    {
        private Store store;
        private int userid;
        public ListElementStore(Store argStore,int argUserId)
        {
            InitializeComponent();
            store = argStore;
            userid = argUserId;

            StoreName.Text = store.StoreName;

           if(store.UserId!=argUserId)
            {
                DeleteStore.IsEnabled = false;
            }

        }

        private void CheckHeart_Loaded(object sender, RoutedEventArgs e)
        {
            var toggleButton = (ToggleButton)sender;
            var heartImage = (Image)toggleButton.Template.FindName("HeartImage", toggleButton);
            if(store.IsFavorite)
            {
                heartImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/heart_filled.png"));
            }
            else
            {
                heartImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/heart_outlined.png"));
            }
        }

        public event RoutedEventHandler ItemDeleted;

        protected virtual void OnItemDeleted()
        {
            ItemDeleted?.Invoke(this, new RoutedEventArgs());
        }
       

        private void Edit_Store(object sender, RoutedEventArgs e)
        {
            EditStoreForm editStoreForm = new EditStoreForm(store,userid);
            editStoreForm.ShowDialog();
            OnItemDeleted();
            
        }

        private void Delete_Store(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Czy na pewno chcesz usunąć sklep " + store.StoreName + " ?", "Komunikat", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                DBSqlite dBSqlite = new DBSqlite();
                int answer1 = dBSqlite.ExecuteNonQuery("UPDATE Transactions SET StoreID=NULL WHERE StoreID=@MyStoreId",
                  new SqliteParameter("@MyStoreId", store.StoreId));
                if (answer1 > 0)
                {

                    int answer = dBSqlite.ExecuteNonQuery("DELETE FROM Stores WHERE Stores.StoreID=@CurrentStoreID",
                        new Microsoft.Data.Sqlite.SqliteParameter("@CurrentStoreID", store.StoreId));

                    if (answer > 0)
                    {
                        MessageBox.Show("Sklep został usunięty", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                        OnItemDeleted();
                    }
                    else
                    {
                        MessageBox.Show("Sklep nie został usunięty", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

            }
        }

        private void HeartToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var toggleButton = (ToggleButton)sender;
            var heartImage = (Image)toggleButton.Template.FindName("HeartImage", toggleButton);
            heartImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/heart_filled.png"));
            if (heartImage != null)
            {
                DBSqlite dBSqlite=new DBSqlite();
                /*dBSqlite.ExecuteNonQuery("UPDATE Stores SET IsFavorite=1 WHERE Stores.StoreID=@MyStoreId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyStoreId", store.StoreId));*/
               int answer=dBSqlite.ExecuteNonQuery("INSERT INTO FavoriteStores(UserID,StoreID) VALUES(@MyUserId,@MyStoreId)",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyUserId", userid),
                    new SqliteParameter("@MyStoreId",store.StoreId));
                if(answer>0)
                {
                    heartImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/heart_filled.png"));
                }


            }
        }

        private void HeartToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var toggleButton = (ToggleButton)sender;
            var heartImage = (Image)toggleButton.Template.FindName("HeartImage", toggleButton);
            if (heartImage != null)
            {
                
                DBSqlite dBSqlite = new DBSqlite();
                /*dBSqlite.ExecuteNonQuery("UPDATE Stores SET IsFavorite=0 WHERE Stores.StoreID=@MyStoreId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyStoreId", store.StoreId));*/
               int answer=dBSqlite.ExecuteNonQuery("DELETE FROM FavoriteStores WHERE UserID=@MyUserID AND StoreID=@MyStoreID",
                    new SqliteParameter("@MyUserID", userid),
                    new SqliteParameter("@MyStoreID", store.StoreId));
                if(answer>0)
                {
                    heartImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/heart_outlined.png"));
                }
            }
        }
    }
}
