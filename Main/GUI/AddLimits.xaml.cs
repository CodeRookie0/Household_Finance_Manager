using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
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
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy AddLimits.xaml
    /// </summary>
    public partial class AddLimits : Window
    {
        private List<Category> _categories;
        private List<Frequencies> _frequencies;
        private List<User> _users;

        private readonly int userid;
        private bool firstRun = true;
        public AddLimits(int userId)
        {
            InitializeComponent();


            userid = userId;
            _categories = Service.GetDefaultCategories();
            var tmpFirst = Service.GetUserCategories(userId);
            _categories.Insert(0, new Category { CategoryName = "Wybierz" });

            foreach(var a in tmpFirst)
            {
                _categories.Add(a);
            }

            var tmp=Service.GetFamilyCategories(userId);

            foreach (var c in tmp)
            {
                _categories.Add(c);
            }

            _frequencies=new List<Frequencies>();
            _frequencies.Insert(0,new Frequencies() {FrequencyName="Wybierz"});
            DBSqlite dBSqlite = new DBSqlite();
            DataTable dt = dBSqlite.ExecuteQuery("SELECT FrequencyID,FrequencyName FROM Frequencies");
            foreach (DataRow dr in dt.Rows) 
            {
                _frequencies.Add(new Frequencies(){ FrequencyID = int.Parse(dr[0].ToString()), FrequencyName = dr[1].ToString() });
            }

            
            _users= new List<User>();
            _users.Insert(0, new User { UserName = "Wyberz" });

            DataTable answer = dBSqlite.ExecuteQuery("SELECT UserID,UserName,Email,PasswordHash,Salt,RoleID,FamilyID FROM Users WHERE UserID=@MyUserID OR FamilyID=@MyFamilyID",
                new Microsoft.Data.Sqlite.SqliteParameter("@MyUserID", userId),
                new SqliteParameter("@MyFamilyID",Service.GetFamilyIdByMemberId(userId)));

            foreach (DataRow dr in answer.Rows)
            {
                User user = new User();
                user.UserID = int.Parse(dr[0].ToString());
                user.UserName = dr[1].ToString();
                user.Email = dr[2].ToString();
                user.PasswordHash = dr[3].ToString();
                user.Salt = dr[4].ToString();
                user.RoleID = int.Parse(dr[5].ToString());
                user.FamilyID = int.Parse(dr[6].ToString());

                _users.Add(user);
            }

            UserList.ItemsSource= _users;
            UserList.SelectedIndex = 0;

            CategoryList.ItemsSource = _categories;
            CategoryList.SelectedIndex = 0;

            Frequency.ItemsSource= _frequencies;
            Frequency.SelectedIndex = 0;
            
        }

        private void CloseImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(PanelListUser.Visibility==Visibility.Visible)
            {
                User tmp=UserList.SelectedItem as User;
                Category thisCategory=CategoryList.SelectedItem as Category;
                Frequencies thisFrequency=Frequency.SelectedItem as Frequencies;

                DBSqlite dBSqlite = new DBSqlite();
                int answer = dBSqlite.ExecuteNonQuery("INSERT INTO Limits (FamilyID,UserID,CategoryID,LimitAmount,FrequencyID) VALUES(@MyFamilyId,@MyUserId,@MyCategoryId,@MyLimitAmount,@MyFrequencyId)",
                    new SqliteParameter("@MyFamilyId", Service.GetFamilyIdByMemberId(userid)),
                    new SqliteParameter("@MyUserId",tmp.UserID),
                    new SqliteParameter("@MyCategoryId",thisCategory.CategoryID),
                    new SqliteParameter("@MyFrequencyId",thisFrequency.FrequencyID),
                    new SqliteParameter("@MyLimitAmount",Amount.Text.ToString()));
                if(answer>0)
                {
                    MessageBox.Show("Limit został dodany", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Limit nie został dodany", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                    
            }
            else
            {

            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(firstRun)
            {
                firstRun = false;
                return;
            }
            else
            {
                ComboBox answer=sender as ComboBox;
                if(answer!=null && answer.SelectedItem is ComboBoxItem selectedItem) 
                {
                    string type=selectedItem.Content.ToString();
                    if(type== "Członek Rodziny")
                    {
                        PanelListUser.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        PanelListUser.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
}
