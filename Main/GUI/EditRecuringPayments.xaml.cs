using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Logika interakcji dla klasy EditRecuringPayments.xaml
    /// </summary>
    public partial class EditRecuringPayments : Window
    {
        private readonly int userId;
        private RecurringPayment RecurringPayment;

        private ObservableCollection<User> userList;
        private ObservableCollection<Store> storeList;
        private ObservableCollection<Category> categoryList;
        private ObservableCollection<Frequencies> frequenciesList;
        public EditRecuringPayments(int argUserId, RecurringPayment argRecuringPayment)
        {
            InitializeComponent();

            userId = argUserId;
            RecurringPayment = argRecuringPayment;

            userList = new ObservableCollection<User>();
            frequenciesList = new ObservableCollection<Frequencies>();

            LoadUser();
            LoadStore();
            LoadCategory();
            LoadFrequencyPayment();

            InputPayment.Text = argRecuringPayment.RecurringPaymentName;
            InputAmount.Text = argRecuringPayment.Amount.ToString();
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
            storeList = Service.GetUserStores(userId);
            storeList.Insert(0, new Store(-1, -1, false) { StoreName = "Wybierz" });


            StoreComboBox.ItemsSource = storeList;
            StoreComboBox.SelectedIndex = 0;

        }


        private void LoadCategory()
        {
            categoryList = new ObservableCollection<Category>(Service.GetDefaultCategories());
            categoryList.Insert(0, new Category { CategoryName = "Wybierz" });
            CategoryComboBox.ItemsSource = categoryList;
            CategoryComboBox.SelectedIndex = 0;

        }

        private void LoadFrequencyPayment()
        {
            frequenciesList.Add(new Frequencies { FrequencyName = "Wybierz" });
            DBSqlite dBSqlite = new DBSqlite();
            DataTable answer = dBSqlite.ExecuteQuery("SELECT FrequencyID,FrequencyName FROM Frequencies");
            foreach (DataRow row in answer.Rows)
            {
                Frequencies frequencies = new Frequencies();
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

            foreach(Frequencies frequencies in FrequencyComboBox.Items)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            User user = UserComboBox.SelectedItem as User;
            Store store = StoreComboBox.SelectedItem as Store;
            Category category = CategoryComboBox.SelectedItem as Category;
            Frequencies frequencies = FrequencyComboBox.SelectedItem as Frequencies;
            string typePayments = TypePayments.SelectedItem.ToString();
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

            

            DBSqlite dBSqlite = new DBSqlite();
            int answer;

            if (store.StoreName != "Wybierz") // JEST sklep
            {
                // Zapytanie do aktualizacji rekordu w tabeli RecurringPayments z uwzględnieniem sklepu
                answer = dBSqlite.ExecuteNonQuery("UPDATE RecurringPayments SET RecurringPaymentName = @TitlePayments, UserID = @UserId, StoreID = @Store, CategoryID = @CategoryId, Amount = @Amount, PaymentDate = @Date, FrequencyID = @Frequency, IsActive = 1, CreatedByUserID = @ToUserId WHERE RecurringPaymentID = @RecurringPaymentId",
                    new SqliteParameter("@TitlePayments", InputPayment.Text),
                    new SqliteParameter("@UserId", userId),
                    new SqliteParameter("@Store", store.StoreId),
                    new SqliteParameter("@CategoryId", category.CategoryID),
                    new SqliteParameter("@Amount", InputAmount.Text),
                    new SqliteParameter("@Date", dateTime.ToString()),
                    new SqliteParameter("@Frequency", frequencies.FrequencyID),
                    new SqliteParameter("@ToUserId", user.UserID),
                    new SqliteParameter("@RecurringPaymentId", RecurringPayment.RecurringPaymentID)); // Dodano identyfikator płatności

                if (answer > 0)
                {
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
                answer = dBSqlite.ExecuteNonQuery("UPDATE RecurringPayments SET RecurringPaymentName = @TitlePayments, UserID = @UserId, CategoryID = @CategoryId, Amount = @Amount, PaymentDate = @Date, FrequencyID = @Frequency, IsActive = 1, CreatedByUserID = @ToUserId WHERE RecurringPaymentID = @RecurringPaymentId",
                    new SqliteParameter("@TitlePayments", InputPayment.Text),
                    new SqliteParameter("@UserId", userId),
                    new SqliteParameter("@CategoryId", category.CategoryID),
                    new SqliteParameter("@Amount", InputAmount.Text),
                    new SqliteParameter("@Date", dateTime.ToString()),
                    new SqliteParameter("@Frequency", frequencies.FrequencyID),
                    new SqliteParameter("@ToUserId", user.UserID),
                    new SqliteParameter("@RecurringPaymentId", RecurringPayment.RecurringPaymentID)); // Dodano identyfikator płatności

                if (answer > 0)
                {
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
    }
}
