using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Main.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy AddRecurringPaymentsControl.xaml
    /// </summary>
    public partial class AddRecurringPaymentsControl : Window
    {
        private readonly int userId;

        private ObservableCollection<User> userList;
        private ObservableCollection<Store> storeList;
        private ObservableCollection<Category> categoryList;
        private ObservableCollection<Frequency> frequenciesList;
        public AddRecurringPaymentsControl(int argUserId)
        {
            InitializeComponent();
            
            userId = argUserId;

            userList= new ObservableCollection<User>(); 
            frequenciesList = new ObservableCollection<Frequency>();

            LoadUser();
            //LoadStore();
            LoadCategory();
            LoadFrequencyPayment();
        }

        //Custom Function [START]

        private void LoadUser()
        {
            int answer = Service.GetRoleIDByUserID(userId);

            userList.Add(new User { UserName = "Wybierz" });

            switch(answer)
            {
                case 1:
                    {
                        int familyId=Service.GetFamilyIdByMemberId(userId);
                        DBSqlite dBSqlite = new DBSqlite();
                        DataTable data=dBSqlite.ExecuteQuery("SELECT UserId,UserName,Email FROM Users WHERE FamilyID=@FamilyId",
                            new Microsoft.Data.Sqlite.SqliteParameter("@FamilyId", familyId));
                        foreach(DataRow rows in data.Rows)
                        {
                            User user = new User();
                            user.UserID = int.Parse(rows[0].ToString());
                            user.UserName = rows[1].ToString(); 
                            user.Email = rows[2].ToString();
                            userList.Add(user);
                        }

                        UserComboBox.ItemsSource = userList;
                    }
                    break;
                case 2:
                    {
                        int familyId = Service.GetFamilyIdByMemberId(userId);
                        DBSqlite dBSqlite=new DBSqlite();
                        DataTable data = dBSqlite.ExecuteQuery("SELECT UserId,UserName,Email FROM Users WHERE FamilyID=@FamilyID AND RoleID=3 OR UserID=@UserId",
                            new Microsoft.Data.Sqlite.SqliteParameter("@FamilyID", familyId),
                            new SqliteParameter("@UserId",userId));
                        foreach (DataRow rows in data.Rows)
                        {
                            User user = new User();
                            user.UserID = int.Parse(rows[0].ToString());
                            user.UserName = rows[1].ToString();
                            user.Email = rows[2].ToString();
                            userList.Add(user);
                        }

                        UserComboBox.ItemsSource = userList;
                    }
                    break;
            }
            UserComboBox.SelectedIndex = 0;
        }

        //private void LoadStore()
        //{
        //    storeList = Service.GetUserStores(userId);
        //    storeList.Insert(0, new Store(-1, -1, false) { StoreName = "Wybierz" });

        //    StoreComboBox.ItemsSource = storeList;
        //    StoreComboBox.SelectedIndex = 0;
        //}


        private void LoadCategory()
        {
            categoryList = new ObservableCollection<Category>();

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
            CategoryComboBox.ItemsSource = categoryList;
            CategoryComboBox.SelectedIndex = 0;
        }

        private void LoadFrequencyPayment()
        {
            frequenciesList.Add(new Frequency { FrequencyName = "Wybierz" });
            DBSqlite dBSqlite = new DBSqlite();
            DataTable answer = dBSqlite.ExecuteQuery("SELECT FrequencyID,FrequencyName FROM Frequencies");
            foreach (DataRow row in answer.Rows)
            {
                Frequency frequencies = new Frequency();
                frequencies.FrequencyID = int.Parse(row[0].ToString());
                frequencies.FrequencyName= row[1].ToString();
                frequenciesList.Add(frequencies);
            }
            FrequencyComboBox.ItemsSource = frequenciesList;
            FrequencyComboBox.SelectedIndex = 0;
        }

        //Custom Function [END]

        private void ExitDialog_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void AddRecuringPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            User user=UserComboBox.SelectedItem as User;
            Store store = StoreComboBox.SelectedItem as Store;
            Category category = CategoryComboBox.SelectedItem as Category;
            Frequency frequencies=FrequencyComboBox.SelectedItem as Frequency;
            int typePayment = TypePayments.SelectedIndex + 1;
            string amount = InputAmount.Text;
            DateTime dateTime;

            if (string.IsNullOrEmpty(InputPayment.Text))
            {
                MessageBox.Show("Podaj nazwę cyklicznej płatności", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (user.UserName=="Wybierz")
            {
                MessageBox.Show("Wybierz użytkownika z listy","Komunikat",MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

           

            if(category.CategoryName=="Wybierz") 
            {
                MessageBox.Show("Wybierz kategorię z listy","Komunikat",MessageBoxButton.OK,MessageBoxImage.Error);
                return;
            }

            decimal parsedAmount;
            if (!decimal.TryParse(amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out parsedAmount))
            {
                MessageBox.Show("Niepoprawny format kwoty", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (parsedAmount <= 0)
            {
                MessageBox.Show("Kwota musi być większa niż 0", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(InputAmount.Text))
            {
                MessageBox.Show("Podaj kwotę", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            
            if(DataPayment.SelectedDate.HasValue) 
            { 
                dateTime= DataPayment.SelectedDate.Value;
            }
            else
            {
                MessageBox.Show("Proszę wybrać datę", "Komnikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if(frequencies.FrequencyName=="Wybierz")
            {
                MessageBox.Show("Proszę wybrać częstotliowść płatności", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (typePayment == 2)
            {
                amount = "-" + amount;
            }

            if (store?.StoreName != null && store.StoreName != "Wybierz") //Jest sklep
            {
                DBSqlite dBSqlite = new DBSqlite();
                int answer = dBSqlite.ExecuteNonQuery("INSERT INTO RecurringPayments (RecurringPaymentName,UserID,StoreID,CategoryID,Amount,TransactionTypeID,PaymentDate,FrequencyID,IsActive,CreatedByUserID)" +
                    " VALUES (@TitlePayments,@ToUserId,@Store,@CategoryId,@Amount,@TransactionTypeID,@Date,@Frequency,1,@UserId)",
                    new SqliteParameter("@TitlePayments", InputPayment.Text),
                    new SqliteParameter("@UserId", user.UserID),
                    new SqliteParameter("@Store", store.StoreId),
                    new SqliteParameter("@CategoryId", category.CategoryID),
                    new SqliteParameter("@Amount", amount),
                    new SqliteParameter("@TransactionTypeID", typePayment),
                    new SqliteParameter("@Date", dateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SqliteParameter("@Frequency", frequencies.FrequencyID),
                    new SqliteParameter("@ToUserId", userId));
                if (answer > 0)
                {
                    MessageBox.Show("Płatność cykliczna została dodana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Płatność cykliczna nie została dodana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                DBSqlite dBSqlite = new DBSqlite();
                int answer = dBSqlite.ExecuteNonQuery("INSERT INTO RecurringPayments (RecurringPaymentName,UserID,CategoryID,Amount,TransactionTypeID,PaymentDate,FrequencyID,IsActive,CreatedByUserID)" +
                    " VALUES (@TitlePayments,@UserId,@CategoryId,@Amount,@TransactionTypeID,@Date,@Frequency,1,@ToUserId)",
                    new SqliteParameter("@TitlePayments", InputPayment.Text),
                    new SqliteParameter("@UserId", user.UserID),
                    new SqliteParameter("@CategoryId", category.CategoryID),
                    new SqliteParameter("@Amount", amount),
                    new SqliteParameter("@TransactionTypeID", typePayment),
                    new SqliteParameter("@Date", dateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SqliteParameter("@Frequency", frequencies.FrequencyID),
                    new SqliteParameter("@ToUserId", userId));
                if (answer > 0)
                {
                    MessageBox.Show("Płatność cykliczna została dodana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Płatność cykliczna nie została dodana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void InputAmountPreview_TextCtrl(object sender, TextCompositionEventArgs e)
        {
            string currentText = ((TextBox)sender).Text;
            bool isDigitOrSeparator = char.IsDigit(e.Text, 0) || e.Text == "." || e.Text == ",";

            if (e.Text == "." || e.Text == ",")
            {
                if (currentText.Contains(".") || currentText.Contains(","))
                {
                    e.Handled = true;
                    return;
                }
            }

            e.Handled = !isDigitOrSeparator;
        }

        private void InputAmountLostFocus_TextCtrl(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string text = textBox.Text;

            if (text.Contains(","))
            {
                text = text.Replace(",", ".");
            }

            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains("."))
                {
                    string[] parts = text.Split('.');

                    if (string.IsNullOrEmpty(parts[0]))
                    {
                        parts[0] = "0";
                    }

                    if (parts.Length > 1 && parts[1].Length > 2)
                    {
                        parts[1] = parts[1].Substring(0, 2);
                    }

                    text = parts[0] + "." + parts[1];
                }
                else
                {
                    text = text + ".00";
                }

                if (Regex.IsMatch(text, @"^\d+(\.\d{1,2})?$"))
                {
                    textBox.Text = text;
                }
                else
                {
                    MessageBox.Show("Proszę podać poprawną kwotę (do dwóch miejsc po przecinku).");
                    textBox.Text = "";
                }
            }
        }

        private void CategoryComboBox_Change(object sender, SelectionChangedEventArgs e)
        {
            var item = CategoryComboBox.SelectedItem as Category;
            if (item != null)
            {
                StoreComboBox.ItemsSource = null;
                ObservableCollection<Store> allStores = new ObservableCollection<Store>();
                int familyId = Service.GetFamilyIdByMemberId(userId);
                if (familyId > 0)
                {
                    var familyStores = Service.GetFamilyStoresByCategory(familyId, item.CategoryID);
                    allStores = familyStores ?? new ObservableCollection<Store>();
                }
                else
                {
                    var userStores = Service.GetUserStoresByCategory(userId, item.CategoryID);
                    allStores = userStores ?? new ObservableCollection<Store>();
                }

                storeList=new ObservableCollection<Store>();

                foreach (var store in allStores.Where(c => Service.IsStoreFavoriteForUser(userId,c.StoreId)))
                {
                    if (!store.StoreName.StartsWith("❤️ "))
                    {
                        store.StoreName = $"❤️ {store.StoreName}";
                    }
                    storeList.Add(store);
                }
                foreach (var store in allStores.Where(c => !Service.IsStoreFavoriteForUser(userId, c.StoreId)))
                {
                    storeList.Add(store);
                }

                StoreComboBox.IsEnabled = storeList.Any();
                if (storeList.Any())
                {
                    storeList.Insert(0, new Store(-1, -1, false) { StoreName = "Wybierz" });
                }
                StoreComboBox.ItemsSource = storeList;
                StoreComboBox.SelectedIndex = 0;
            }
        }
    }
}
