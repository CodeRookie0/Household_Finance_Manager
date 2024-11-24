using Main.Controls;
using Main.Models;
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
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy DeleteFamilyMemberControl.xaml
    /// </summary>
    public partial class DeleteFamilyMemberControl : Window
    {
        private ObservableCollection<PendingUser> pendinguser;
        public DeleteFamilyMemberControl(ref ObservableCollection<PendingUser> argUser)
        {
            InitializeComponent();
            pendinguser = argUser;

            foreach(PendingUser item in pendinguser)
            {
                FamilyUser tmp = new FamilyUser(item);
                tmp.UserPending += UpdateOutSideClick;
                FamilyListDelete.Items.Add(tmp);
            }
        }
        private void UpdateOutSideClick(object sender,PendingUser arg)
        {
            pendinguser.Remove(arg);
            FamilyListDelete.Items.Clear();
            foreach (PendingUser item in pendinguser)
            {
                FamilyUser tmp = new FamilyUser(item);
                tmp.UserPending += UpdateOutSideClick;
                FamilyListDelete.Items.Add(tmp);
            }
        }

        private void CloseDialog(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
