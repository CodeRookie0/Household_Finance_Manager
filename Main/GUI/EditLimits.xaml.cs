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
    /// Logika interakcji dla klasy EditLimits.xaml
    /// </summary>
    public partial class EditLimits : Window
    {

        private List<Category> _categories;
        private List<Frequencies> _frequencies;
        private List<User> _users;

        private readonly int userid;
        private bool firstRun = true;

        private LimitsModel _model;

        public EditLimits(LimitsModel argModel)
        {
            InitializeComponent();

            userid = argModel.UserId;
            _model = argModel;

            _categories = Service.GetDefaultCategories();
            var tmpFirst = Service.GetUserCategories(userid);
            _categories.Insert(0, new Category { CategoryName = "Wybierz" });

            foreach (var a in tmpFirst)
            {
                _categories.Add(a);
            }

            var tmp = Service.GetFamilyCategories(userid);

            foreach (var c in tmp)
            {
                _categories.Add(c);
            }

            _frequencies = new List<Frequencies>();
            _frequencies.Insert(0, new Frequencies() { FrequencyName = "Wybierz" });
            DBSqlite dBSqlite = new DBSqlite();
            DataTable dt = dBSqlite.ExecuteQuery("SELECT FrequencyID,FrequencyName FROM Frequencies");
            foreach (DataRow dr in dt.Rows)
            {
                _frequencies.Add(new Frequencies() { FrequencyID = int.Parse(dr[0].ToString()), FrequencyName = dr[1].ToString() });
            }


            _users = new List<User>();
            _users.Insert(0, new User { UserName = "Wyberz" });

            DataTable answer = dBSqlite.ExecuteQuery("SELECT UserID,UserName,Email,PasswordHash,Salt,RoleID,FamilyID FROM Users WHERE UserID=@MyUserID OR FamilyID=@MyFamilyID",
                new Microsoft.Data.Sqlite.SqliteParameter("@MyUserID", userid),
                new SqliteParameter("@MyFamilyID", Service.GetFamilyIdByMemberId(userid)));

            foreach (DataRow dr in answer.Rows)
            {
                User userTmp = new User();
                userTmp.UserID = int.Parse(dr[0].ToString());
                userTmp.UserName = dr[1].ToString();
                userTmp.Email = dr[2].ToString();
                userTmp.PasswordHash = dr[3].ToString();
                userTmp.Salt = dr[4].ToString();
                userTmp.RoleID = int.Parse(dr[5].ToString());
                userTmp.FamilyID = int.Parse(dr[6].ToString());

                _users.Add(userTmp);
            }

            UserList.ItemsSource = _users;

            User user = _users.FirstOrDefault(c => c.UserID == argModel.UserId);
            UserList.SelectedItem = user;

            CategoryList.ItemsSource = _categories;

            Category category = _categories.FirstOrDefault(c => c.CategoryID == argModel.CategoryId);
            CategoryList.SelectedItem = category;


            Frequency.ItemsSource = _frequencies;
            Frequencies frequency = _frequencies.FirstOrDefault(c=>c.FrequencyID==argModel.FrequencyId);
            Frequency.SelectedItem = frequency;
        }

        private void CloseImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (PanelListUser.Visibility == Visibility.Visible)
            {
                User tmp = UserList.SelectedItem as User;
                Category thisCategory = CategoryList.SelectedItem as Category;
                Frequencies thisFrequency = Frequency.SelectedItem as Frequencies;

                DBSqlite dBSqlite = new DBSqlite();

                // Załóżmy, że LimitID jest dostępny w 'tmp' lub 'thisCategory' lub innym obiekcie (możesz go dodać do obiektów User, Category, Frequencies jeśli to konieczne)
                int limitID =_model.LimitId; // Funkcja do pobrania identyfikatora limitu, np. z wybranego elementu w UI

                // Zapytanie UPDATE zamiast INSERT
                int answer = dBSqlite.ExecuteNonQuery(
                    "UPDATE Limits SET FamilyID = @MyFamilyId, UserID = @MyUserId, CategoryID = @MyCategoryId, LimitAmount = @MyLimitAmount, FrequencyID = @MyFrequencyId " +
                    "WHERE LimitID = @MyLimitID",
                    new SqliteParameter("@MyFamilyId", Service.GetFamilyIdByMemberId(userid)),
                    new SqliteParameter("@MyUserId", tmp.UserID),
                    new SqliteParameter("@MyCategoryId", thisCategory.CategoryID),
                    new SqliteParameter("@MyFrequencyId", thisFrequency.FrequencyID),
                    new SqliteParameter("@MyLimitAmount", Amount.Text.ToString()),
                    new SqliteParameter("@MyLimitID", limitID) // Zakładając, że limitID jest dostępny
                );

                if (answer > 0)
                {
                    MessageBox.Show("Limit został zaktualizowany", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Limit nie został zaktualizowany", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                // Tutaj możesz dodać kod dla przypadku, gdy panel jest niewidoczny
            }


        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (firstRun)
            {
                firstRun = false;
                return;
            }
            else
            {
                ComboBox answer = sender as ComboBox;
                if (answer != null && answer.SelectedItem is ComboBoxItem selectedItem)
                {
                    string type = selectedItem.Content.ToString();
                    if (type == "Członek Rodziny")
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
