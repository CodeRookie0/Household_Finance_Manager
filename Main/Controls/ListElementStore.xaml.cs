using Main.GUI;
using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace Main.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy ListElementStore.xaml
    /// </summary>
    public partial class ListElementStore : UserControl
    {
        private Store store;
        private int userid;
        private bool IsFamily;
        public ListElementStore(Store argStore,int argUserId,bool IsFamily)
        {
            InitializeComponent();
            store = argStore;
            userid = argUserId;
            this.IsFamily = IsFamily;

            StoreName.Text = store.StoreName;
            CategoryName.Text = store.CategoryName;
            if (IsFamily)
            {
                UsereName.Text = "Stworzony przez: " + store.CreatedBy;
            }
            else
            {
                UsereName.Text = "";
            }
            int roleId = Service.GetRoleIDByUserID(userid);
            if(store.UserId!=userid && roleId!=1)
            {
                EditStore.Visibility = Visibility.Collapsed;
                DeleteStore.Visibility = Visibility.Collapsed;
                ActionPanel.Margin= new Thickness(135,0,0,0);
            }
            else
            {
                EditStore.Visibility = Visibility.Visible;
                DeleteStore.Visibility = Visibility.Visible;
                ActionPanel.Margin = new Thickness(25, 0, 0, 0);
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
            if (MessageBox.Show("Czy na pewno chcesz usunąć sklep " + store.StoreName + ", stworzony przez "+store.CreatedBy+ "?\nPo usunięciu sklepu wszystkie transakcje i cykliczne płatności z nim powiązane utracą przypisanie do tego sklepu.", "Komunikat", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (Service.DeleteStore(store.StoreId))
                {
                    MessageBox.Show("Sklep został usunięty", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnItemDeleted();
                }
                else
                {
                    MessageBox.Show("Nie udało się usunąć sklepu.\n" +
                        "Sklep może być powiązany z transakcjami lub cyklicznymi płatnościami, które uniemożliwiają jego usunięcie.",
                        "Niepowodzenie",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
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
