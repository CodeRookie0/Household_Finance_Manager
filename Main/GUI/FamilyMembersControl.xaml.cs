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
using System.Collections.ObjectModel;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for FamilyMembersControl.xaml
    /// </summary>
    public partial class FamilyMembersControl : UserControl
    {
        private MainWindow mainWindow;
        private readonly int userId;
        public FamilyMembersControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();
            if (!Service.IsPrimaryUser(userId))
            {
                FamilySettingsButton.Visibility = Visibility.Collapsed;
                JoinRequestsListBox.Visibility = Visibility.Collapsed;
                JoinRequestsTextBlock.Visibility = Visibility.Collapsed;
                ReviewRequestsButton.Visibility = Visibility.Collapsed;
                ModifyPermissionsButton.Visibility = Visibility.Collapsed;
                RemoveMemberButton.Visibility = Visibility.Collapsed;
                FamilySettingsButton.Visibility=Visibility.Collapsed;
                LeaveFamilyButton.Visibility = Visibility.Visible;
            }
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


            var answerFamilyMemebers = dBSqlite.ExecuteQuery("SELECT UserName,Email FROM Users WHERE FamilyID=(SELECT FamilyID FROM Users WHERE UserID=@UserId) AND UserID!=@UserId",
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

        private void ChangePermision(object sender, RoutedEventArgs e)
        {
            ObservableCollection<PendingUser> users = new ObservableCollection<PendingUser>();
            DBSqlite dBSqlite = new DBSqlite();
            DataTable answer = dBSqlite.ExecuteQuery("SELECT UserID,UserName,RoleName FROM Users INNER JOIN Roles ON Users.RoleID=Roles.RoleID WHERE Users.FamilyID=(SELECT FamilyID FROM Users WHERE Users.UserID=@MyId) AND Users.UserID!=@MyId"
                , new Microsoft.Data.Sqlite.SqliteParameter("@MyId", userId));

            foreach (DataRow row in answer.Rows)
            {
                PendingUser tmp = new PendingUser(Int32.Parse(row["UserID"].ToString()));
                tmp.Name = row["UserName"].ToString();
                tmp.Role = row["RoleName"].ToString();
                users.Add(tmp);

            }
            PermissionEditorControl permissionEditor = new PermissionEditorControl(users);
            permissionEditor.ShowDialog();
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

        private void FamilySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            FamilySettingsControl familySettings= new FamilySettingsControl(userId, Service.GetFamilyIdByPrimaryUserId(userId),mainWindow);
            familySettings.Show();
        }

        private void LeaveFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordPrompt passwordWindow = new PasswordPrompt();
            bool? result = passwordWindow.ShowDialog();

            if (result == true)
            {
                string enteredPassword = passwordWindow.EnteredPassword;

                bool isPasswordCorrect = Service.ValidateUserPassword(userId, enteredPassword);

                if (isPasswordCorrect)
                {
                    var deleteResult = MessageBox.Show("Czy na pewno chcesz opuścić rodzinę?", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (deleteResult == MessageBoxResult.Yes)
                    {
                        bool success = Service.LeaveFamily(userId);

                        if (success)
                        {
                            MessageBox.Show("Udało Ci się opuścić rodzinę.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                            mainWindow.FamilyMembersButton_Click(sender, e);
                        }
                        else
                        {
                            MessageBox.Show("Wystąpił błąd podczas opuszczania rodziny. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Wprowadzone hasło jest niepoprawne. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            mainWindow.FamilyMembersButton_Click(sender, e);
        }
    }
}
