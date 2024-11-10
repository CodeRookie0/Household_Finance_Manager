using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for FamilyMembersControl.xaml
    /// </summary>
    public partial class FamilyMembersControl : UserControl
    {
        private readonly int userId;
        public FamilyMembersControl(int loggedInUserId)
        {
            userId = loggedInUserId;
            InitializeComponent();

            DBSqlite dBSqlite = new DBSqlite();
            var answerJoinReqeust = dBSqlite.ExecuteQuery("SELECT UserName,Email FROM Users INNER JOIN JoinRequests ON Users.UserID=JoinRequests.UserID WHERE JoinRequests.FamilyID=(SELECT FamilyID FROM Users WHERE Users.UserID=@UserId)",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId",userId));

            
             foreach(DataRow row in answerJoinReqeust.Rows) 
             {
                  StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(row[0].ToString());
                stringBuilder.Append(" ");
                stringBuilder.Append(row[1].ToString());
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Padding = new Thickness(10);
                listBoxItem.FontSize = 16;
                listBoxItem.Content = stringBuilder.ToString();
                JoinRequestsListBox.Items.Add(listBoxItem);

             }


            var answerFamilyMemebers = dBSqlite.ExecuteQuery("SELECT UserName,Email FROM Users WHERE FamilyID=(SELECT FamilyID FROM Users WHERE UserID=@UserId) AND UserId!=@UserId",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));

            foreach (DataRow row in answerFamilyMemebers.Rows)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(row[0].ToString());
                stringBuilder.Append(" ");
                stringBuilder.Append(row[1].ToString());
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Padding = new Thickness(10);
                listBoxItem.FontSize = 16;
                listBoxItem.Content = stringBuilder.ToString();
                FamilyMembersListBox.Items.Add(listBoxItem);

            }




        }

        private List<PendingUser> GeneratePendingUser()
        {
            DBSqlite dBSqlite = new DBSqlite();
            var answerJoinReqeust = dBSqlite.ExecuteQuery("SELECT Users.UserID,UserName,Email FROM Users INNER JOIN JoinRequests ON Users.UserID=JoinRequests.UserID WHERE JoinRequests.FamilyID=(SELECT FamilyID FROM Users WHERE Users.UserID=@UserId)",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));

            List<PendingUser> localList= new List<PendingUser>();   
            foreach (DataRow row in answerJoinReqeust.Rows)
            {
                
                localList.Add(new PendingUser(Int32.Parse(row[0].ToString())) { Name = row[1].ToString(), Role = "Partner" });
            }
            return localList;
        }

        private void Button_Click(object sender, RoutedEventArgs e) //Rozpatrzenie próśb
        {
            List<PendingUser> answer = GeneratePendingUser();
            AddFamilyMemberRequest addFamilyMemberRequest = new AddFamilyMemberRequest(answer,userId);
            addFamilyMemberRequest.ShowDialog();
        }
    }
}
