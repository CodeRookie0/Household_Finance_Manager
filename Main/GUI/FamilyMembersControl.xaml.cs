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
        private int failedAttempts = 0;
        private DateTime? blockUntil = null;

        private ObservableCollection<PendingUser> members {  get; set; }
        private ObservableCollection<PendingUser> joinRequestMembers { get; set; }
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
            
            joinRequestMembers = new ObservableCollection<PendingUser>(GeneratePendingUser());
            JoinRequestsListBox.ItemsSource = joinRequestMembers;
            JoinRequestsTextBlock.DataContext = joinRequestMembers;
           /* DBSqlite dBSqlite = new DBSqlite();
            var answerJoinReqeust = dBSqlite.ExecuteQuery("SELECT UserName,Email FROM Users INNER JOIN JoinRequests ON Users.UserID=JoinRequests.UserID WHERE JoinRequests.FamilyID=(SELECT FamilyID FROM Users WHERE Users.UserID=@UserId)",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId",userId));

            
             foreach(DataRow row in answerJoinReqeust.Rows) 
             {
                
                  /*StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(row[0].ToString());
                stringBuilder.Append(" ");
                stringBuilder.Append(row[1].ToString());
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Padding = new Thickness(10);
                listBoxItem.FontSize = 16;
                listBoxItem.Content = stringBuilder.ToString();
                JoinRequestsListBox.Items.Add(listBoxItem);

             }*/
            
            DBSqlite dBSqlite = new DBSqlite();
            members = new ObservableCollection<PendingUser>();
            var answerFamilyMemebers = dBSqlite.ExecuteQuery("SELECT UserID,UserName,RoleID FROM Users WHERE FamilyID=(SELECT FamilyID FROM Users WHERE UserID=@UserId) AND UserID!=@UserId",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));

            foreach (DataRow row in answerFamilyMemebers.Rows)
            {
                PendingUser tmp = new PendingUser(int.Parse(row[0].ToString()));
                tmp.Name= row[1].ToString();
                tmp.Role = row[2].ToString();
                members.Add(tmp);
            }
            FamilyMembersListBox.DataContext = members;
            FamilyMembersListBox.ItemsSource = members;
        }

        private void ChangePermision(object sender, RoutedEventArgs e)
        {
           /* ObservableCollection<PendingUser> users = new ObservableCollection<PendingUser>();
            DBSqlite dBSqlite = new DBSqlite();
            DataTable answer = dBSqlite.ExecuteQuery("SELECT UserID,UserName,RoleName FROM Users INNER JOIN Roles ON Users.RoleID=Roles.RoleID WHERE Users.FamilyID=(SELECT FamilyID FROM Users WHERE Users.UserID=@MyId) AND Users.UserID!=@MyId"
                , new Microsoft.Data.Sqlite.SqliteParameter("@MyId", userId));

            foreach (DataRow row in answer.Rows)
            {
                PendingUser tmp = new PendingUser(Int32.Parse(row["UserID"].ToString()));
                tmp.Name = row["UserName"].ToString();
                tmp.Role = row["RoleName"].ToString();
                users.Add(tmp);

            }*/
            PermissionEditorControl permissionEditor = new PermissionEditorControl(members);
            permissionEditor.ShowDialog();
        }


        private string GetRoleName(int id)
        {
            if(id==1)
            {
                return "Admin";
            }
            else if(id==2) 
            {
                return "Partner";
            }
            else
            {
                return "Child";
            }
        }


        private List<PendingUser> GeneratePendingUser()
        {
            DBSqlite dBSqlite = new DBSqlite();
            var answerJoinReqeust = dBSqlite.ExecuteQuery("SELECT Users.UserID,UserName,Email,RoleID FROM Users INNER JOIN JoinRequests ON Users.UserID=JoinRequests.UserID WHERE JoinRequests.FamilyID=(SELECT FamilyID FROM Users WHERE Users.UserID=@UserId) AND JoinRequests.RequestStatusID=1",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));

            List<PendingUser> localList= new List<PendingUser>();   
            foreach (DataRow row in answerJoinReqeust.Rows)
            {
                
                localList.Add(new PendingUser(Int32.Parse(row[0].ToString())) { Name = row[1].ToString()});
            }
            return localList;
        }

        private void Button_Click(object sender, RoutedEventArgs e) //Rozpatrzenie próśb
        {
            //List<PendingUser> answer = GeneratePendingUser();
            AddFamilyMemberRequest addFamilyMemberRequest = new AddFamilyMemberRequest(joinRequestMembers,userId);
            addFamilyMemberRequest.ShowDialog();

            members.Clear();
            DBSqlite dBSqlite = new DBSqlite();
            var answerFamilyMemebers = dBSqlite.ExecuteQuery("SELECT UserID,UserName,RoleID FROM Users WHERE FamilyID=(SELECT FamilyID FROM Users WHERE UserID=@UserId) AND UserID!=@UserId",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));

            foreach (DataRow row in answerFamilyMemebers.Rows)
            {
                PendingUser tmp = new PendingUser(int.Parse(row[0].ToString()));
                tmp.Name = row[1].ToString();
                tmp.Role = row[2].ToString();
                members.Add(tmp);
            }
            FamilyMembersListBox.DataContext = members;
            FamilyMembersListBox.ItemsSource = members;

        }

        private void FamilySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            FamilySettingsControl familySettings= new FamilySettingsControl(userId, Service.GetFamilyIdByPrimaryUserId(userId),mainWindow);
            familySettings.Show();
        }

        private void LeaveFamilyButton_Click(object sender, RoutedEventArgs e)
        {
            if (blockUntil.HasValue && DateTime.Now < blockUntil.Value)
            {
                var remainingTime = blockUntil.Value - DateTime.Now;
                MessageBox.Show($"Za dużo nieudanych prób. Spróbuj ponownie za {remainingTime.Seconds} sekundy.", "Blokada", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
                    failedAttempts++;

                    if (failedAttempts >= 3)
                    {
                        blockUntil = DateTime.Now.AddSeconds(30);
                        MessageBox.Show("Wprowadzone hasło jest niepoprawne. Za dużo prób. Spróbuj ponownie za 30 sekund.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Wprowadzone hasło jest niepoprawne. Spróbuj ponownie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RemoveFamilyMember_Click(object sender, RoutedEventArgs e)
        {
           /*ObservableCollection<PendingUser> users = new ObservableCollection<PendingUser>();
           DBSqlite dbsqite=new DBSqlite();
           DataTable answer=dbsqite.ExecuteQuery("SELECT UserID,UserName,RoleName FROM Users INNER JOIN Family ON Users.FamilyID=Family.FamilyID INNER JOIN Roles ON Users.RoleID=Roles.RoleID WHERE Users.UserID<>Family.PrimaryUserID AND Users.FamilyID=(" +
                "SELECT FamilyID FROM Users WHERE Users.UserID=@UserId)",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId",userId));
            if(answer != null)
            {
                foreach(DataRow row in answer.Rows) 
                {
                    PendingUser tmp = new PendingUser(Int32.Parse(row[0].ToString()));
                    tmp.Name = row[1].ToString();
                    tmp.Role = row[2].ToString();
                    users.Add(tmp);

                }
            }*/
            DeleteFamilyMemberControl deleteFamilyMemberControl = new DeleteFamilyMemberControl(members);
            deleteFamilyMemberControl.Show();

          

        }
    }
}
