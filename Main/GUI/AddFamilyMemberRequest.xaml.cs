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

        private void Button_Click(object sender, RoutedEventArgs e) //Add User
        {
            var Button=sender as Button;
            var thisUser=Button.DataContext as PendingUser;
            
            if(thisUser!=null)
            {
                DBSqlite dBSqlite = new DBSqlite();
             
            }
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
                pendingUsers.RemoveAll(user => user.Userid == thisUser.Userid);
            }
        }
    }

}
