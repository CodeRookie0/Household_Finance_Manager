﻿using Main.Logic;
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

        private List<Category> categoryList;
        private List<Frequency> _frequencies;
        private List<User> _users;

        private readonly int userId;
        private readonly int loggedInUserId;
        public int roleId;
        private bool firstRun = true;

        private Limit limit;

        public EditLimits(Limit argModel, int loggedInUserId)
        {
            InitializeComponent();

            userId = argModel.UserId;
            this.loggedInUserId = loggedInUserId;
            roleId =Service.GetRoleIDByUserID(loggedInUserId);
            limit = argModel;

            categoryList = new List<Category>();

            List<Category> defaultCategories = Service.GetDefaultCategories();

            int familyId = Service.GetFamilyIdByMemberId(loggedInUserId);
            List<Category> familyCategories = familyId > 0
                ? Service.GetFamilyCategories(familyId)
                : Service.GetUserCategories(loggedInUserId);

            var allCategories = defaultCategories.Concat(familyCategories).Distinct();

            foreach (var category in allCategories.Where(c => Service.IsCategoryFavoriteForUser(loggedInUserId, c.CategoryID)))
            {
                if (!category.CategoryName.StartsWith("❤️ "))
                {
                    category.CategoryName = $"❤️ {category.CategoryName}";
                }
                categoryList.Add(category);
            }
            foreach (var category in allCategories.Where(c => !Service.IsCategoryFavoriteForUser(loggedInUserId, c.CategoryID)))
            {
                categoryList.Add(category);
            }

            categoryList.Insert(0, new Category { CategoryName = "Wybierz" });

            CategoryList.ItemsSource = categoryList;
            Category selectedCategory = categoryList.FirstOrDefault(c => c.CategoryID == argModel.CategoryId);
            CategoryList.SelectedItem = selectedCategory;

            _frequencies = new List<Frequency>();
            _frequencies.Insert(0, new Frequency() { FrequencyName = "Wybierz" });
            DBSqlite dBSqlite = new DBSqlite();
            DataTable dt = dBSqlite.ExecuteQuery("SELECT FrequencyID,FrequencyName FROM Frequencies");
            foreach (DataRow dr in dt.Rows)
            {
                _frequencies.Add(new Frequency() { FrequencyID = int.Parse(dr[0].ToString()), FrequencyName = dr[1].ToString() });
            }

            Frequency.ItemsSource = _frequencies;
            Frequency frequency = _frequencies.FirstOrDefault(c => c.FrequencyID == argModel.FrequencyId);
            Frequency.SelectedItem = frequency;

            if (roleId == 2)
            {
                TypeComboBox.IsEnabled = false;

                _users = new List<User>();
                _users.Insert(0, new User { UserName = "Wyberz" });

                User user = Service.GetUserByUserId(loggedInUserId);
                _users.Add(user);

                List<User> children = Service.GetChildrenByFamilyId(familyId);
                foreach (User child in children)
                {
                    _users.Add(child);
                }
            }
            if (roleId == 1)
            {
                _users = new List<User>();
                _users.Insert(0, new User { UserName = "Wyberz" });

                List<User> users = Service.GetUsersByFamilyId(familyId);
                foreach (User user in users)
                {
                    _users.Add(user);
                }
            }

            UserList.ItemsSource = _users;
            User selectedUser = _users.FirstOrDefault(c => c.UserID == argModel.UserId);
            UserList.SelectedItem = selectedUser;
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
                Frequency thisFrequency = Frequency.SelectedItem as Frequency;

                DBSqlite dBSqlite = new DBSqlite();

                // Załóżmy, że LimitID jest dostępny w 'tmp' lub 'thisCategory' lub innym obiekcie (możesz go dodać do obiektów User, Category, Frequencies jeśli to konieczne)
                int limitID =limit.LimitId; // Funkcja do pobrania identyfikatora limitu, np. z wybranego elementu w UI

                // Zapytanie UPDATE zamiast INSERT
                int answer = dBSqlite.ExecuteNonQuery(
                    "UPDATE Limits SET FamilyID = @MyFamilyId, UserID = @MyUserId, CategoryID = @MyCategoryId, LimitAmount = @MyLimitAmount, FrequencyID = @MyFrequencyId " +
                    "WHERE LimitID = @MyLimitID",
                    new SqliteParameter("@MyFamilyId", Service.GetFamilyIdByMemberId(userId)),
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
