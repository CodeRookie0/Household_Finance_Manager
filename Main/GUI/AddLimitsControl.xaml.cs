using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        private List<Category> categoryList;
        private List<Frequency> _frequencies;
        private List<User> _users;

        private readonly int userid;
        public int roleId;
        private bool firstRun = true;
        public AddLimits(int userId)
        {
            InitializeComponent();

            userid = userId;
            roleId=Service.GetRoleIDByUserID(userId);

            categoryList = new List<Category>();

            List<Category> defaultCategories = Service.GetDefaultCategories();

            int familyId = Service.GetFamilyIdByMemberId(userId);
            List<Category> familyCategories = familyId > 0
                ? Service.GetFamilyCategories(familyId)
                : Service.GetUserCategories(userId);

            var allCategories = defaultCategories.Concat(familyCategories).Distinct();

            foreach (var category in allCategories.Where(c => Service.IsCategoryFavoriteForUser(userId, c.CategoryID)))
            {
                if (!category.CategoryName.StartsWith("❤️ "))
                {
                    category.CategoryName = $"❤️ {category.CategoryName}";
                }
                categoryList.Add(category);
            }
            foreach (var category in allCategories.Where(c => !Service.IsCategoryFavoriteForUser(userId, c.CategoryID)))
            {
                categoryList.Add(category);
            }

            categoryList.Insert(0, new Category { CategoryName = "Wybierz" });
            CategoryList.ItemsSource = categoryList;
            CategoryList.SelectedIndex = 0;

            _frequencies=new List<Frequency>();
            _frequencies.Insert(0,new Frequency() {FrequencyName="Wybierz"});
            DBSqlite dBSqlite = new DBSqlite();
            DataTable dt = dBSqlite.ExecuteQuery("SELECT FrequencyID,FrequencyName FROM Frequencies");
            foreach (DataRow dr in dt.Rows) 
            {
                _frequencies.Add(new Frequency(){ FrequencyID = int.Parse(dr[0].ToString()), FrequencyName = dr[1].ToString() });
            }

            Frequency.ItemsSource = _frequencies;
            Frequency.SelectedIndex = 0;

            if (roleId == 2)
            {
                TypeComboBox.IsEnabled = false;

                _users = new List<User>();
                _users.Insert(0, new User { UserName = "Wybierz" });

                User user = Service.GetUserByUserId(userId);
                _users.Add(user);

                List<User> children = Service.GetChildrenByFamilyId(familyId);
                foreach (User child in children)
                {
                    _users.Add(child);
                }
            }
            if (roleId==1)
            {
                _users = new List<User>();
                _users.Insert(0, new User { UserName = "Wybierz" });

                List<User> users = Service.GetUsersByFamilyId(familyId);
                foreach (User user in users)
                {
                    _users.Add(user);
                }
            }
            UserList.ItemsSource = _users;
            UserList.SelectedIndex = 0;
        }

        private void CloseDialog_Image(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Button_AddLimit(object sender, RoutedEventArgs e)
        {
            if (MoneyValidation(Amount.Text))
            {
                if (PanelListUser.Visibility == Visibility.Visible)
                {
                    User tmp = UserList.SelectedItem as User;
                    if (tmp.UserName == "Wybierz")
                    {
                        MessageBox.Show("Proszę wybrać użytkownika", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Category thisCategory = CategoryList.SelectedItem as Category;
                    if (thisCategory.CategoryName == "Wybierz")
                    {
                        MessageBox.Show("Proszę wybrać kategorię", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Frequency thisFrequency = Frequency.SelectedItem as Frequency;
                    if (thisFrequency.FrequencyName == "Wybierz")
                    {
                        MessageBox.Show("Proszę wybrać częstotliwość", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    int? familyId = Service.GetFamilyIdByMemberId(userid);
                    if (familyId == -1)
                    {
                        familyId = null;
                    }

                    DBSqlite dBSqlite = new DBSqlite();
                    int answer = dBSqlite.ExecuteNonQuery("INSERT INTO Limits (FamilyID,UserID,CategoryID,LimitAmount,FrequencyID,CreatedByUserID) VALUES(@MyFamilyId,@MyUserId,@MyCategoryId,@MyLimitAmount,@MyFrequencyId,@CreatedByUserID)",
                        new SqliteParameter("@MyFamilyId", familyId),
                        new SqliteParameter("@MyUserId", tmp.UserID),
                        new SqliteParameter("@MyCategoryId", thisCategory.CategoryID),
                        new SqliteParameter("@MyFrequencyId", thisFrequency.FrequencyID),
                        new SqliteParameter("@MyLimitAmount", Amount.Text.ToString()),
                        new SqliteParameter("@CreatedByUserID", userid));
                    if (answer > 0)
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
            }
            else
            {
                MessageBox.Show("Wprowadź prawidłową kwotę","Komunikat",MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool MoneyValidation(string argAmount)
        {
            if (string.IsNullOrEmpty(argAmount))
                return false;
            float num;
            bool isValid = float.TryParse(argAmount, NumberStyles.Currency, CultureInfo.GetCultureInfo("pl-PL"), out num);
            return isValid;
        }

        private void TypFamily_ComboBox_Changed(object sender, SelectionChangedEventArgs e)
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
