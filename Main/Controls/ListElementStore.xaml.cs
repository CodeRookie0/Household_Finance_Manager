﻿using Main.GUI;
using Main.Logic;
using Main.Models;
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
        public ListElementStore(Store argStore)
        {
            InitializeComponent();
            store = argStore;

            StoreName.Text = store.StoryName;
            

        }

        private void CheckHeart_Loaded(object sender, RoutedEventArgs e)
        {
            var HeartPath = (Path)((ToggleButton)sender).Template.FindName("HeartPath", (FrameworkElement)sender);
            if(store.IsFavorite)
            {
                HeartPath.Fill = Brushes.Red;
            }
            else
            {
                HeartPath.Fill = Brushes.Transparent;
            }
        }

        public event RoutedEventHandler ItemDeleted;

        protected virtual void OnItemDeleted()
        {
            ItemDeleted?.Invoke(this, new RoutedEventArgs());
        }
       

        private void Edit_Store(object sender, RoutedEventArgs e)
        {
            EditStoreForm editStoreForm = new EditStoreForm(store);
            editStoreForm.ShowDialog();
            OnItemDeleted();
            
        }

        private void Delete_Store(object sender, RoutedEventArgs e)
        {
            DBSqlite dBSqlite = new DBSqlite();
            int answer=dBSqlite.ExecuteNonQuery("DELETE FROM Stores WHERE Stores.StoreID=@CurrentStoreID",
                new Microsoft.Data.Sqlite.SqliteParameter("@CurrentStoreID", store.StoreId));
            if(answer>0)
            {
                MessageBox.Show("Sklep został usunięty", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                OnItemDeleted();
            }
            else
            {
                MessageBox.Show("Sklep nie został usunięty", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HeartToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var HeartPath = (Path)((ToggleButton)sender).Template.FindName("HeartPath",(FrameworkElement)sender);
            if(HeartPath!=null)
            {
                HeartPath.Fill = Brushes.Red;
                DBSqlite dBSqlite=new DBSqlite();
                dBSqlite.ExecuteNonQuery("UPDATE Stores SET IsFavorite=1 WHERE Stores.StoreID=@MyStoreId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyStoreId", store.StoreId));

            }
        }

        private void HeartToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var HeartPath = (Path)((ToggleButton)sender).Template.FindName("HeartPath", (FrameworkElement)sender);
            if (HeartPath != null)
            {
                HeartPath.Fill = Brushes.Transparent;
                DBSqlite dBSqlite = new DBSqlite();
                dBSqlite.ExecuteNonQuery("UPDATE Stores SET IsFavorite=0 WHERE Stores.StoreID=@MyStoreId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@MyStoreId", store.StoreId));
            }
        }
    }
}