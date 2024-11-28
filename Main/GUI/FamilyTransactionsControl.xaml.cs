using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public FamilyTransactionsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();

            userRole = Service.GetRoleIDByUserID(userId);
            if (userRole != 1)
            {
                HeaderGrid.ColumnDefinitions[6].Width = new GridLength(0);
            }
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            int familyId = Service.GetFamilyIdByMemberId(userId);
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

                var users = Service.GetFamilyMembersByFamilyId(familyId);
                UserComboBox.ItemsSource = users;

                var categories = Service.GetFamilyCategories(familyId);
                CategoryComboBox.ItemsSource = categories;

                var stores = Service.GetFamilyStores(familyId);
                StoreComboBox.ItemsSource = stores;
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
                        Padding = new Thickness(5)
                    };
                    editButton.Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Resources/edit_green.png")), Width = 25, Height = 25 };
                    actionPanel.Children.Add(editButton);

                    // Delete Button
                    Button deleteButton = new Button
                    {
                        Background = new SolidColorBrush(Colors.Transparent),
                        BorderBrush = new SolidColorBrush(Colors.Transparent),
                        Height = 35,
                        Width = 35,
                        Padding = new Thickness(5)
                    };
                    deleteButton.Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Resources/delete_red.png")), Width = 25, Height = 25 };
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
                            Padding = new Thickness(5)
                        };
                        editButton.Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Resources/edit_green.png")), Width = 25, Height = 25 };
                        actionPanel.Children.Add(editButton);

                        Button deleteButton = new Button
                        {
                            Background = new SolidColorBrush(Colors.Transparent),
                            BorderBrush = new SolidColorBrush(Colors.Transparent),
                            Height = 35,
                            Width = 35,
                            Padding = new Thickness(5)
                        };
                        deleteButton.Content = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Resources/delete_red.png")), Width = 25, Height = 25 };
                        actionPanel.Children.Add(deleteButton);

                        Grid.SetRow(actionPanel, grid.RowDefinitions.Count - 1);
                        Grid.SetColumn(actionPanel, 6);
                        grid.Children.Add(actionPanel);
                    }
                }
            }
            return grid;
        }
    }
}
