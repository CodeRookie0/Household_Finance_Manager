using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace Main.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy FamilyUser.xaml
    /// </summary>
    public partial class FamilyUser : UserControl
    {
        private PendingUser pendinguser;
        public FamilyUser(PendingUser argPendingUser)
        {
            InitializeComponent();
            pendinguser = argPendingUser;
            UserName.Text = pendinguser.Name;
            Role.Text =Service.GetRoleNameByRoleID(Convert.ToInt16(pendinguser.Role));
        }

        public event EventHandler<PendingUser> UserPending;


        protected virtual void OnItemDeleted(PendingUser user)
        {
            UserPending?.Invoke(this, user);
        }

        private void DeleteFromFamily_Click(object sender, RoutedEventArgs e)
        {
            DBSqlite dBSqlite = new DBSqlite();

            MessageBoxResult Confirmation=MessageBox.Show("Czy na pewno chcesz usunąć użytkownika "+pendinguser.Name+" ?","Komunikat",MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (Confirmation == MessageBoxResult.Yes)
            {

                int answer = dBSqlite.ExecuteNonQuery("UPDATE Users SET FamilyID=NULL  ,RoleID=1 WHERE UserID=@MyId",
                    new SqliteParameter("@MyId", pendinguser.Userid));
                if (answer > 0)
                {
                    MessageBox.Show("Użytkownik " + pendinguser.Name + " został usniety z rodziny", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnItemDeleted(pendinguser);
                }
                else
                {
                    MessageBox.Show("Użytkownik nie został usunięty z rodziny", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }

        }
    }
}
