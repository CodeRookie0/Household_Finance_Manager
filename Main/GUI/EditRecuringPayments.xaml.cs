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
    /// Logika interakcji dla klasy EditRecuringPayments.xaml
    /// </summary>
    public partial class EditRecuringPayments : Window
    {
        private readonly int userId;
        private RecurringPayment RecurringPayment;
        private int? categoryId;

        private ObservableCollection<User> userList;
        private ObservableCollection<Store> storeList;
        private ObservableCollection<Category> categoryList;
        private ObservableCollection<Frequency> frequenciesList;
        public EditRecuringPayments(int argUserId, RecurringPayment argRecuringPayment)
        {
            InitializeComponent();

            userId = argUserId;
            RecurringPayment = argRecuringPayment;

            userList = new ObservableCollection<User>();
            frequenciesList = new ObservableCollection<Frequency>();

            categoryId = argRecuringPayment.CategoryID; 

            LoadUser();
            LoadStore();
            LoadCategory();
            LoadFrequencyPayment();
            InputPayment.Text = argRecuringPayment.RecurringPaymentName;

            if (argRecuringPayment.TransactionTypeID.HasValue)
            {
                TypePayments.SelectedIndex = argRecuringPayment.TransactionTypeID.Value-1;
            }
            else
            {
                TypePayments.SelectedIndex = 0;
            }

            string amount = argRecuringPayment.Amount.ToString("F2");
            if (!char.IsDigit(amount[0]))
            {
                amount = amount.Substring(1);
            }
            InputAmount.Text = amount.Replace(',','.');

            DataPayment.Text = argRecuringPayment.PaymentDate.ToString();

            SetValueComboBox();
        }

        //Custom Function [START]
        private void LoadUser()
        {
            int answer = Service.GetRoleIDByUserID(userId);

            userList.Add(new User { UserName = "Wybierz" });

            switch (answer)
            {
                case 1:
                    {
                        int familyId = Service.GetFamilyIdByMemberId(userId);
                        DBSqlite dBSqlite = new DBSqlite();
                        DataTable data = dBSqlite.ExecuteQuery("SELECT UserId,UserName,Email FROM Users WHERE FamilyID=@FamilyId",
                            new Microsoft.Data.Sqlite.SqliteParameter("@FamilyId", familyId));
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
                case 2:
                    {
                        int familyId = Service.GetFamilyIdByMemberId(userId);
                        DBSqlite dBSqlite = new DBSqlite();
                        DataTable data = dBSqlite.ExecuteQuery("SELECT UserId,UserName,Email FROM Users WHERE FamilyID=@FamilyID AND RoleID=3 OR UserID=@UserId",
                            new Microsoft.Data.Sqlite.SqliteParameter("@FamilyID", familyId),
                            new SqliteParameter("@UserId", userId));
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

        private void LoadStore()
        {
            StoreComboBox.ItemsSource = null;
            ObservableCollection<Store> allStores = new ObservableCollection<Store>();

            int familyId = Service.GetFamilyIdByMemberId(userId);
            if (familyId > 0)
            {
                var familyStores = Service.GetFamilyStoresByCategory(familyId, categoryId.GetValueOrDefault(-1));
                allStores = familyStores ?? new ObservableCollection<Store>();
            }
            else
            {
                var userStores = Service.GetUserStoresByCategory(userId, categoryId.GetValueOrDefault(-1));
                allStores = userStores ?? new ObservableCollection<Store>();
            }

            storeList = new ObservableCollection<Store>();

            foreach (var store in allStores.Where(c => Service.IsStoreFavoriteForUser(userId, c.StoreId)))
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
                storeList.Insert(0, new Store(-1, -1,false) { StoreName = "Wybierz" });
            }

            // Przypisanie właściwości ComboBox
            StoreComboBox.ItemsSource = storeList;
            StoreComboBox.DisplayMemberPath = "StoreName";
            StoreComboBox.SelectedValuePath = "StoreId";

            var selectedStore = storeList.FirstOrDefault(store => store.StoreId == RecurringPayment.StoreID);
            if (selectedStore != null)
            {
                StoreComboBox.SelectedItem = selectedStore;
            }
            else
            {
                StoreComboBox.SelectedIndex = 0;
            }
        }


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
                frequencies.FrequencyName = row[1].ToString();
                frequenciesList.Add(frequencies);
            }
            FrequencyComboBox.ItemsSource = frequenciesList;
            FrequencyComboBox.SelectedIndex = 0;
        }


        private void SetValueComboBox()
        {
            foreach (User user in UserComboBox.Items)
            {
                if (user.UserID == RecurringPayment.UserID)
                {
                    UserComboBox.SelectedItem = user;
                    break;
                }
            }

            if (RecurringPayment.StoreID == null)
            {
                foreach (Store store in StoreComboBox.Items)
                {
                    if (store.StoreId == RecurringPayment.StoreID)
                    {
                        StoreComboBox.SelectedItem = store;
                        break;
                    }
                }
            }


            foreach (Category category in CategoryComboBox.Items)
            {
                if (category.CategoryID == RecurringPayment.CategoryID)
                {
                    CategoryComboBox.SelectedItem = category;
                    break;
                }
            }

            foreach(Frequency frequencies in FrequencyComboBox.Items)
            {
                if(frequencies.FrequencyID== RecurringPayment.FrequencyID)
                {
                    FrequencyComboBox.SelectedItem= frequencies;
                    break;
                }
            }

            
        }

        //Custom Function [END]

        private void CloseImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void SaveEditRecuringPaymentButton_Click(object sender, RoutedEventArgs e)
        {

            User user = UserComboBox.SelectedItem as User;
            Store store = StoreComboBox.SelectedItem as Store;
            Category category = CategoryComboBox.SelectedItem as Category;
            Frequency frequencies = FrequencyComboBox.SelectedItem as Frequency;
            int typePayment = TypePayments.SelectedIndex + 1;
            string amount = InputAmount.Text;
            DateTime dateTime;

            if (string.IsNullOrEmpty(InputPayment.Text))
            {
                MessageBox.Show("Podaj nazwę cyklicznej płatności", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (user.UserName == "Wybierz")
            {
                MessageBox.Show("Wybierz użytkownika z listy", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (category.CategoryName == "Wybierz")
            {
                MessageBox.Show("Wybierz kategorię z listy", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (DataPayment.SelectedDate.HasValue)
            {
                dateTime = DataPayment.SelectedDate.Value;
            }
            else
            {
                MessageBox.Show("Proszę wybrać datę", "Komnikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (frequencies.FrequencyName == "Wybierz")
            {
                MessageBox.Show("Proszę wybrać częstotliwość płatności", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (typePayment == 2)
            {
                amount = "-" + amount;
            }
            string previousPaymentName = Service.GetRecurringPaymentNameByRecurringPaymentID(RecurringPayment.RecurringPaymentID);

            DBSqlite dBSqlite = new DBSqlite();
            int answer;

            if (store != null && !string.IsNullOrEmpty(store.StoreName) &&  store.StoreName != "Wybierz") // JEST sklep
            {
                // Zapytanie do aktualizacji rekordu w tabeli RecurringPayments z uwzględnieniem sklepu
                answer = dBSqlite.ExecuteNonQuery("UPDATE RecurringPayments SET RecurringPaymentName = @TitlePayments, UserID = @UserId, StoreID = @Store, CategoryID = @CategoryId, Amount = @Amount, TransactionTypeID = @TransactionTypeID, PaymentDate = @Date, FrequencyID = @Frequency, IsActive = 1, CreatedByUserID = @ToUserId WHERE RecurringPaymentID = @RecurringPaymentId",
                    new SqliteParameter("@TitlePayments", InputPayment.Text),
                    new SqliteParameter("@UserId", user.UserID),
                    new SqliteParameter("@Store", store.StoreId),
                    new SqliteParameter("@CategoryId", category.CategoryID),
                    new SqliteParameter("@Amount", amount),
                    new SqliteParameter("@TransactionTypeID", typePayment),
                    new SqliteParameter("@Date", dateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SqliteParameter("@Frequency", frequencies.FrequencyID),
                    new SqliteParameter("@ToUserId", userId),
                    new SqliteParameter("@RecurringPaymentId", RecurringPayment.RecurringPaymentID)); // Dodano identyfikator płatności

                if (answer > 0)
                {
                    if(previousPaymentName!= InputPayment.Text)
                    {
                        bool success = Service.UpdateNotesForRecurringTransactions(RecurringPayment.RecurringPaymentID, InputPayment.Text);
                        if (success)
                        {
                            Console.WriteLine("Notatki transakcji zostały pomyślnie zaktualizowane.");
                        }
                        else
                        {
                            Console.WriteLine("Wystąpił błąd podczas aktualizacji notatek dla transakcji.");
                        }
                    }
                    MessageBox.Show("Płatność cykliczna została zaktualizowana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Płatność cykliczna nie została zaktualizowana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                // Zapytanie do aktualizacji rekordu w tabeli RecurringPayments BEZ sklepu
                answer = dBSqlite.ExecuteNonQuery("UPDATE RecurringPayments SET RecurringPaymentName = @TitlePayments, UserID = @UserId, StoreID = NULL, CategoryID = @CategoryId, Amount = @Amount, TransactionTypeID = @TransactionTypeID, PaymentDate = @Date, FrequencyID = @Frequency, IsActive = 1, CreatedByUserID = @ToUserId WHERE RecurringPaymentID = @RecurringPaymentId",
                    new SqliteParameter("@TitlePayments", InputPayment.Text),
                    new SqliteParameter("@UserId", user.UserID),
                    new SqliteParameter("@CategoryId", category.CategoryID),
                    new SqliteParameter("@Amount", amount),
                    new SqliteParameter("@TransactionTypeID", typePayment),
                    new SqliteParameter("@Date", dateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SqliteParameter("@Frequency", frequencies.FrequencyID),
                    new SqliteParameter("@ToUserId", userId),
                    new SqliteParameter("@RecurringPaymentId", RecurringPayment.RecurringPaymentID)); // Dodano identyfikator płatności

                if (answer > 0)
                {
                    if (previousPaymentName != InputPayment.Text)
                    {
                        bool success = Service.UpdateNotesForRecurringTransactions(RecurringPayment.RecurringPaymentID, InputPayment.Text);
                        if (success)
                        {
                            Console.WriteLine("Notatki transakcji zostały pomyślnie zaktualizowane.");
                        }
                        else
                        {
                            Console.WriteLine("Wystąpił błąd podczas aktualizacji notatek dla transakcji.");
                        }
                    }
                    MessageBox.Show("Płatność cykliczna została zaktualizowana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Płatność cykliczna nie została zaktualizowana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
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
                StoreComboBox.SelectedItem = null;
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

                storeList = new ObservableCollection<Store>();

                foreach (var store in allStores.Where(c => Service.IsStoreFavoriteForUser(userId, c.StoreId)))
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
                StoreComboBox.DisplayMemberPath = "StoreName";
                StoreComboBox.SelectedValuePath = "StoreId";

                var selectedStore = storeList.FirstOrDefault(store => store.StoreId == RecurringPayment.StoreID);
                if (selectedStore != null)
                {
                    StoreComboBox.SelectedItem = selectedStore;
                }
                else
                {
                    StoreComboBox.SelectedIndex = 0;
                }
            }
        }
    }
}
