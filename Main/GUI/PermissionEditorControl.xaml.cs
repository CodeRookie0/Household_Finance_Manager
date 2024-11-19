﻿using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Logika interakcji dla klasy PermissionEditorWindow.xaml
    /// </summary>
    public partial class PermissionEditorControl : Window
    {
        public ObservableCollection<PendingUser> pendingUsers { get; set; }
        public ObservableCollection<string> ComboBoxItems { get; set; }
        public PermissionEditorControl(ObservableCollection<PendingUser> argList)
        {
            InitializeComponent();
            pendingUsers = argList;





            //dataGridUserMyFamily.ItemsSource = pendingUsers;

            ComboBoxItems = new ObservableCollection<string>
            {
                "Admin",
                "Partner",
                "Child"
            };

            DataContext = this;
        }

        private void CloseDialog(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private int GetNumberRole(string Role)
        {
            if (Role == "Admin")
            {
                return 1;
            }
            else if (Role == "Partner")
            {
                return 2;

            }
            else { return 3; }
        }
        private void ChangeRole(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is string selectedRole)
            {
                var currentUser = (PendingUser)comboBox.DataContext;
                int role = GetNumberRole(currentUser.Role);
                if (MessageBox.Show("Czy chcesz zmienić uprawnienia ?", "Komunikat", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DBSqlite dBSqlite = new DBSqlite();
                    dBSqlite.ExecuteNonQuery("UPDATE Users SET RoleID=@NewRole WHERE UserID=@UserId",
                        new Microsoft.Data.Sqlite.SqliteParameter("@NewRole", role),
                        new SqliteParameter("@UserId", currentUser.Userid.ToString()));
                }

            }
        }
    }
}
