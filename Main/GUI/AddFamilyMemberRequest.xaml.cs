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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy AddFamilyMemberRequest.xaml
    /// </summary>
    public partial class AddFamilyMemberRequest : Window
    {
        private List<PendingUser> pendingUsers;
        private readonly int userId;
        public AddFamilyMemberRequest(List<PendingUser> argUser,int userId)
        {
            InitializeComponent();
            pendingUsers = argUser;
            this.userId = userId;
            ListUser.ItemsSource = argUser;

        }

        private int GetNumberPermision(string Role)
        {
            if (Role == "Admin")
            {
                return 1;
            }
            else if (Role == "Partner")
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e) //Add User
        {
            var Button = sender as Button;
            var thisUser = Button.DataContext as PendingUser;
            if (thisUser != null)
            {
                if (ListUser.SelectedIndex != -1)
                {
                    int index = ListUser.SelectedIndex;
                    ListBoxItem item = (ListBoxItem)ListUser.ItemContainerGenerator.ContainerFromIndex(index);
                    ComboBox cb = FindDescendant<ComboBox>(item);
                    ComboBoxItem tmp = (ComboBoxItem)cb.SelectedValue;
                    string content = (string)tmp.Content;
                    int idRole = GetNumberPermision(content);

                    DBSqlite dBSqlite = new DBSqlite();
                    dBSqlite.ExecuteNonQuery("UPDATE Users SET RoleID=@NewRole , FamilyID=(SELECT FamilyID FROM Users WHERE Users.UserID=@UserID) WHERE Users.UserID=@MyId",
                        new SqliteParameter("@NewRole", idRole),
                        new SqliteParameter("@UserID", userId),
                        new SqliteParameter("@MyId", thisUser.Userid));
                    dBSqlite.ExecuteNonQuery("DELETE FROM JoinRequests WHERE JoinRequests.UserID=@UserID AND JoinRequests.FamilyID=(SELECT FamilyID FROM Users WHERE Users.UserID=@MyId)",
                        new SqliteParameter("@UserID", thisUser.Userid),
                        new SqliteParameter("@MyId", userId));
                    pendingUsers.Remove(thisUser);
                    ListUser.ItemsSource = null;
                    ListUser.ItemsSource = pendingUsers;


                }
            }
        }

        private T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            // Check if this object is the specified type
            if (obj is T)
                return obj as T;

            // Check for children
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childrenCount < 1)
                return null;

            // First check all the children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
            }

            // Then check the childrens children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = FindDescendant<T>(VisualTreeHelper.GetChild(obj, i));
                if (child != null && child is T)
                    return child as T;
            }

            return null;
        }



        private void Button_Click_1(object sender, RoutedEventArgs e) //Remove User
        {
            var Button = sender as Button;
            var thisUser = Button.DataContext as PendingUser;

            if (thisUser != null)
            {
                DBSqlite dBSqlite = new DBSqlite();
                dBSqlite.ExecuteNonQuery("DELETE FROM JoinRequests WHERE JoinRequests.UserId=@UserId AND JoinRequests.FamilyID=(SELECT FamilyID FROM Users WHERE Users.UserID=@UserId1)",
                    new Microsoft.Data.Sqlite.SqliteParameter("@UserId", thisUser.Userid),
                    new SqliteParameter("@UserId1",userId));
                pendingUsers.Remove(thisUser);
                ListUser.ItemsSource = null;
                ListUser.ItemsSource = pendingUsers;
            }
        }

        private void CloseDialog(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }

}
