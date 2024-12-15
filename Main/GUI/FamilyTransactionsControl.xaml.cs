using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for FamilyTransactionsControl.xaml
    /// </summary>
    public partial class FamilyTransactionsControl : UserControl
    {
        private MainWindow mainWindow;
        private readonly int userId;
        private int userRole;
        private int familyId;

        public FamilyTransactionsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();

            userRole = Service.GetRoleIDByUserID(userId);
            familyId = Service.GetFamilyIdByMemberId(userId);

            LoadTransactions();
            LoadComboBoxValues();
        }

        private void LoadTransactions()
        {
            TransactionsDataStackPanel.Children.Clear();
            if (familyId < 0)
            {
                EmptyGrid.Visibility = Visibility.Visible;
                FilledGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                var transactions = Service.GetFamilyTransactions(familyId);
                Grid grid = CreateTransactionGrid(transactions);
                TransactionsDataStackPanel.Children.Add(grid);

                TransactionSummaryViewModel transactionSummaryViewModel = new TransactionSummaryViewModel(transactions);
                this.DataContext = transactionSummaryViewModel;
            }
        }

        private void LoadTransactions(List<Transaction> transactions)
        {
            TransactionsDataStackPanel.Children.Clear();
            if (familyId < 0)
            {
                EmptyGrid.Visibility = Visibility.Visible;
                FilledGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                Grid grid = CreateTransactionGrid(transactions);
                TransactionsDataStackPanel.Children.Add(grid);

                TransactionSummaryViewModel transactionSummaryViewModel = new TransactionSummaryViewModel(transactions);
                this.DataContext = transactionSummaryViewModel;
            }
        }

        private void LoadComboBoxValues()
        {
            if (familyId < 0)
            {
                EmptyGrid.Visibility = Visibility.Visible;
                FilledGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                List<Category> defaultCategories = Service.GetDefaultCategories();
                List<Category> familyCategories = familyId > 0 ? Service.GetFamilyCategories(familyId) : Service.GetUserCategories(userId);

                List<Category> allCategories = new List<Category>();
                foreach (var category in defaultCategories)
                {
                    allCategories.Add(category);
                }

                foreach (var category in familyCategories)
                {
                    allCategories.Add(category);
                }
                CategoryComboBox.Items.Add(new Category { CategoryID = -1, CategoryName = "Wybierz" });
                foreach (var category in allCategories)
                {
                    CategoryComboBox.Items.Add(category);
                }
                CategoryComboBox.SelectedIndex = 0;

                var users = Service.GetFamilyMembersByFamilyId(familyId);
                UserComboBox.Items.Add(new User { UserID = -1, UserName = "Wybierz" });
                foreach (var user in users)
                {
                    UserComboBox.Items.Add(user);
                }
                UserComboBox.SelectedIndex = 0;

                var stores = Service.GetFamilyStores(familyId);
                var defaultStore = new Store(-1, -1, false) { StoreName = "Wybierz" };
                StoreComboBox.Items.Add(defaultStore);
                foreach (var store in stores)
                {
                    StoreComboBox.Items.Add(store);
                }
                StoreComboBox.SelectedIndex = 0;

                List<TransactionType> transactionTypes = Service.GetTransactionTypes();
                var defaultTransactionType = new TransactionType { TransactionTypeID = -1, TypeName = "Wybierz" };
                transactionTypes.Insert(0, defaultTransactionType);
                foreach (var transactionType in transactionTypes)
                {
                    TransactionTypeComboBox.Items.Add(transactionType);
                }
                TransactionTypeComboBox.SelectedIndex = 0;
            }
        }
        private Grid CreateTransactionGrid(List<Transaction> transactions)
        {
            Grid grid = new Grid
            {
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Define columns for the grid (6 columns based on the provided layout)
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) }); // Date & Time
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.6, GridUnitType.Star) }); // User Name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Category & SubCategory
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) }); // Store Name
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) }); // Amount
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) }); // Note
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.4, GridUnitType.Star) }); // Actions (Edit/Delete)

            foreach (var transaction in transactions)
            {
                RowDefinition rowDef = new RowDefinition();
                grid.RowDefinitions.Add(rowDef);

                // Column 1: Date and Time
                StackPanel dateTimePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                dateTimePanel.Children.Add(new TextBlock { Text = transaction.DateFormatted, FontSize = 18, FontWeight = FontWeights.SemiBold });
                dateTimePanel.Children.Add(new TextBlock { Text = transaction.Time, FontSize = 16, Foreground = new SolidColorBrush(Colors.Gray) });
                Grid.SetRow(dateTimePanel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(dateTimePanel, 0);
                grid.Children.Add(dateTimePanel);

                // Column 2: UserName
                StackPanel userPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                userPanel.Children.Add(new TextBlock { Text = transaction.UserName, FontSize = 18, FontWeight = FontWeights.SemiBold });
                Grid.SetRow(userPanel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(userPanel, 1);
                grid.Children.Add(userPanel);

                // Column 3: Category and SubCategory
                StackPanel categoryPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                categoryPanel.Children.Add(new TextBlock { Text = transaction.CategoryName, FontSize = 18, FontWeight = FontWeights.SemiBold });
                categoryPanel.Children.Add(new TextBlock { Text = transaction.SubcategoryName, FontSize = 16, Foreground = new SolidColorBrush(Colors.Gray) });
                Grid.SetRow(categoryPanel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(categoryPanel, 2);
                grid.Children.Add(categoryPanel);

                // Column 4: StoreName
                StackPanel storePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5, 5, 10, 5)
                };
                storePanel.Children.Add(new TextBlock { Text = transaction.StoreName, FontSize = 18, FontWeight = FontWeights.SemiBold });
                Grid.SetRow(storePanel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(storePanel, 3);
                grid.Children.Add(storePanel);

                // Column 5: Amount
                StackPanel amountPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                if (transaction.TransactionTypeID == 1)
                {
                    amountPanel.Children.Add(new TextBlock { Text = transaction.FormattedAmount, FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Colors.Green) });
                }
                else if (transaction.TransactionTypeID == 2)
                {
                    amountPanel.Children.Add(new TextBlock { Text = transaction.FormattedAmount, FontSize = 18, FontWeight = FontWeights.SemiBold, Foreground = new SolidColorBrush(Colors.Red) , Margin=new Thickness(-8,0,0,0) });
                }
                    
                Grid.SetRow(amountPanel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(amountPanel, 4);
                grid.Children.Add(amountPanel);

                // Column 6: Note
                StackPanel notePanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(5, 5, 5, 5)
                };
                notePanel.Children.Add(new TextBlock { Text = transaction.Note, FontSize = 16, FontWeight = FontWeights.Normal, TextWrapping = TextWrapping.Wrap });
                Grid.SetRow(notePanel, grid.RowDefinitions.Count - 1);
                Grid.SetColumn(notePanel, 5);
                grid.Children.Add(notePanel);

                // Column 7: Action buttons (Edit/Delete)
                if (userRole == 1)
                {
                    StackPanel actionPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 5, 5, 5)
                    };
                    // Edit Button
                    Button editButton = new Button
                    {
                        Background = new SolidColorBrush(Colors.Transparent),
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        Height=35,
                        Width=35,
                        Padding = new Thickness(5),
                        DataContext=transaction
                    };
                    editButton.Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Resources/edit_green.png")), Width = 25, Height = 25 };
                    editButton.Click += EditTransactionFamily_Click;
                    actionPanel.Children.Add(editButton);

                    // Delete Button
                    Button deleteButton = new Button
                    {
                        Background = new SolidColorBrush(Colors.Transparent),
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        Height = 35,
                        Width = 35,
                        Padding = new Thickness(5),
                        DataContext = transaction
                    };
                    deleteButton.Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Resources/delete_red.png")), Width = 25, Height = 25 };
                    deleteButton.Click += DeleteTransactionFamily_Click;
                    actionPanel.Children.Add(deleteButton);

                    Grid.SetRow(actionPanel, grid.RowDefinitions.Count - 1);
                    Grid.SetColumn(actionPanel, 6);
                    grid.Children.Add(actionPanel);
                }
                else if (userRole == 2)
                {
                    if (transaction.UserID == userId || Service.IsChildTransaction(transaction.TransactionID))
                    {
                        StackPanel actionPanel = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(0, 5, 5, 5)
                        };
                        Button editButton = new Button
                        {
                            Background = new SolidColorBrush(Colors.Transparent),
                            BorderBrush = new SolidColorBrush(Colors.Transparent),
                            Height = 35,
                            Width = 35,
                            Padding = new Thickness(5),
                            DataContext = transaction
                        };
                        editButton.Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Resources/edit_green.png")), Width = 25, Height = 25 };
                        editButton.Click += EditTransactionFamily_Click;
                        actionPanel.Children.Add(editButton);

                        Button deleteButton = new Button
                        {
                            Background = new SolidColorBrush(Colors.Transparent),
                            BorderBrush = new SolidColorBrush(Colors.Transparent),
                            Height = 35,
                            Width = 35,
                            Padding = new Thickness(5),
                            DataContext = transaction
                        };
                        deleteButton.Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Resources/delete_red.png")), Width = 25, Height = 25 };
                        deleteButton.Click += DeleteTransactionFamily_Click;
                        actionPanel.Children.Add(deleteButton);

                        Grid.SetRow(actionPanel, grid.RowDefinitions.Count - 1);
                        Grid.SetColumn(actionPanel, 6);
                        grid.Children.Add(actionPanel);
                    }
                }
            }
            return grid;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var dateFrom = StartDatePicker.SelectedDate;
            var dateTo = EndDatePicker.SelectedDate;
            var filterUserId = UserComboBox.SelectedValue as int?;
            var categoryId = CategoryComboBox.SelectedValue as int?;
            var storeId = StoreComboBox.SelectedValue as int?;
            var transactionTypeId = TransactionTypeComboBox.SelectedValue as int?;
            var amountFromText = AmountFromTextBox.Text;
            var amountToText = AmountToTextBox.Text;

            double? amountFrom = null;
            double? amountTo = null;

            if (dateFrom.HasValue && dateTo.HasValue && dateTo < dateFrom)
            {
                MessageBox.Show("Data 'do' nie może być wcześniejsza niż data 'od'.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!string.IsNullOrWhiteSpace(amountFromText) && double.TryParse(amountFromText, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedAmountFrom))
            {
                amountFrom = parsedAmountFrom;
            }

            if (!string.IsNullOrWhiteSpace(amountToText) && double.TryParse(amountToText, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedAmountTo))
            {
                amountTo = parsedAmountTo;
            }

            if (amountTo.HasValue && amountFrom.HasValue && amountTo < amountFrom)
            {
                MessageBox.Show("Kwota 'do' nie może być mniejsza niż kwota 'od'.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<Transaction> transactions = Service.GetFilteredFamilyTransactions(familyId, filterUserId: filterUserId, startDate: dateFrom, endDate: dateTo, categoryId: categoryId, storeId: storeId, transactionTypeId: transactionTypeId, amountFrom: amountFrom, amountTo: amountTo);

            LoadTransactions(transactions);
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            UserComboBox.SelectedIndex = 0;
            CategoryComboBox.SelectedIndex = 0;
            StoreComboBox.SelectedIndex = 0;
            TransactionTypeComboBox.SelectedIndex = 0;
            AmountFromTextBox.Clear();
            AmountToTextBox.Clear();
            FilterButton_Click(sender, e);
        }

        private void AmountFromTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

        private void AmountToTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
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

        private void AmountFromTextBox_LostFocus(object sender, RoutedEventArgs e)
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

        private void AmountToTextBox_LostFocus(object sender, RoutedEventArgs e)
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

        private void AddTransactionButton_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Category> categories = new ObservableCollection<Category>();
            ObservableCollection<Store> store= new ObservableCollection<Store>();   

            foreach(var item in CategoryComboBox.Items)
            {
                Category category = item as Category;
                if (category != null)
                {
                    categories.Add(category);
                }
            }


            foreach(var item in StoreComboBox.Items)
            {
                Store thisStore = item as Store;
                
                if (thisStore != null)
                {
                    store.Add(thisStore);
                }
            }


            AddTransactionControl addTransaction = new AddTransactionControl(categories, userId);
            addTransaction.ShowDialog();
            LoadTransactions();

        }

        private void DeleteTransactionFamily_Click(object sender, RoutedEventArgs e)
        {
           
            var button = sender as Button;
            Transaction transaction = button?.DataContext as Transaction;
            if (transaction != null)
            {
                MessageBoxResult answerQuestion = MessageBox.Show("Czy na pewno chcesz usunąć transakcję o kwocie "
                    + transaction.Amount + " zrealizowaną w dniu " + transaction.Date, "Komunikat", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answerQuestion == MessageBoxResult.Yes)
                {
                    DBSqlite dBSqlite = new DBSqlite();

                    int updateHistory = dBSqlite.ExecuteNonQuery(
                        "UPDATE RecurringPaymentHistory SET ActionTypeID = @ActionTypeID AND TransactionID=NULL AND ActionDate = CURRENT_TIMESTAMP WHERE TransactionID = @TransactionID",
                        new Microsoft.Data.Sqlite.SqliteParameter("@ActionTypeID", 3),
                        new Microsoft.Data.Sqlite.SqliteParameter("@TransactionID", transaction.TransactionID));

                    int answer = dBSqlite.ExecuteNonQuery("DELETE FROM Transactions WHERE TransactionID=@MyTransactionsId",
                            new Microsoft.Data.Sqlite.SqliteParameter("@MyTransactionsId", transaction.TransactionID));
                    if (answer > 0)
                    {
                        MessageBox.Show("Transakcja została skasowana z bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTransactions();
                    }
                    else
                    {
                        MessageBox.Show("Transakcja nie została skasowana z bazy danych", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);

                    }
                }
            }
        }

        private void EditTransactionFamily_Click(object sender, RoutedEventArgs e) 
        {
            ObservableCollection<Category> categories = new ObservableCollection<Category>();
            ObservableCollection<Store> store = new ObservableCollection<Store>();

            foreach (var item in CategoryComboBox.Items)
            {
                Category category = item as Category;
                if (category != null)
                {
                    categories.Add(category);
                }
            }


            foreach (var item in StoreComboBox.Items)
            {
                Store thisStore = item as Store;

                if (thisStore != null)
                {
                    store.Add(thisStore);
                }
            }
            

            var button = sender as Button;
            Transaction transaction = button?.DataContext as Transaction;
            if(transaction != null)
            {
                int familyId = Service.GetFamilyIdByMemberId(userId);
                ObservableCollection<Store> storeListByCategory;
                if (familyId > 0)
                {
                    storeListByCategory = Service.GetFamilyStoresByCategory(familyId, transaction.CategoryID);
                }
                else
                {
                    storeListByCategory = Service.GetUserStoresByCategory(userId, transaction.CategoryID);
                }
                EditTransactionControl editTransaction = new EditTransactionControl(categories, storeListByCategory, userId, transaction);
                editTransaction.ShowDialog();
                LoadTransactions();
            }
        }
    }
}
