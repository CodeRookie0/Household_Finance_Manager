using Main.Logic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for AddCategoryControl.xaml
    /// </summary>
    public partial class AddCategoryControl : Window
    {
        private int subcategoryCount = 0;
        private const int MaxSubcategories = 5;
        private MainWindow mainWindow;
        private readonly int userId;
        private bool isEditMode = false;
        private int categoryIdToEdit = -1;
        public AddCategoryControl(int loggedInUserId, MainWindow mainWindow, bool isEditMode, int categoryIdToEdit)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            this.isEditMode = isEditMode;
            this.categoryIdToEdit = categoryIdToEdit;
            InitializeComponent();
            if (isEditMode)
            {
                AddSubcategoryText.Visibility = Visibility.Collapsed;
                CategoryTitleText.Text = "Edytuj Kategorię";
                if (categoryIdToEdit > 0)
                {
                    CategoryNameTextBoxPlaceholder.Visibility = Visibility.Collapsed;
                    CategoryNameTextBox.Text = Service.GetCategoryNameByCategoryID(categoryIdToEdit);
                }
            }
        }

        private void CategoryNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            CategoryNameTextBoxPlaceholder.Visibility=Visibility.Collapsed;
        }

        private void CategoryNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CategoryNameTextBox.Text))
            {
                CategoryNameTextBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void AddSubcategoryText_Click(object sender, MouseButtonEventArgs e)
        {
            if (subcategoryCount >= MaxSubcategories)
            {
                MessageBox.Show("Możesz dodać maksymalnie 5 podkategorii w tym oknie. Aby dodać kolejne, skorzystaj z opcji 'Nowa podkategoria' w zakładce 'Kategorie'.", "Osiągnięto limit podkategorii", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Grid subcategoryGrid = new Grid
            {
                Margin = new Thickness(0, 10, 0, 0)
            };

            subcategoryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            subcategoryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Border textBoxBorder = new Border
            {
                Width = 280,
                Height = 45,
                Margin = new Thickness(20, 0, 0, 0),
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0.5)
            };

            TextBox subcategoryTextBox = new TextBox
            {
                Margin = new Thickness(10, 0, 45, 0),
                Height = 23.28,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                FontSize = 16,
                Foreground = Brushes.Black
            };

            textBoxBorder.Child = subcategoryTextBox;

            Label placeholderLabel = new Label
            {
                Content = "Nazwa podkategorii *",
                Background = Brushes.Transparent,
                Foreground = Brushes.Gray,
                Opacity = 1,
                IsHitTestVisible = false,
                Margin = new Thickness(25, 0, 0, 0),
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            };
            subcategoryTextBox.GotFocus += (s, args) =>
            {
                placeholderLabel.Visibility = Visibility.Collapsed;
            };

            subcategoryTextBox.LostFocus += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(subcategoryTextBox.Text))
                {
                    placeholderLabel.Visibility = Visibility.Visible;
                }
            };

            Button deleteButton = new Button
            {
                Content = "✖",
                Width = 30,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 5, 0),
                FontSize = 16,
                Foreground = Brushes.Red,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Cursor = Cursors.Hand
            };
            deleteButton.Click += (s, args) =>
            {
                SubcategoryContainer.Children.Remove(subcategoryGrid);
                subcategoryCount--;
            };

            subcategoryGrid.Children.Add(textBoxBorder);
            subcategoryGrid.Children.Add(placeholderLabel);
            subcategoryGrid.Children.Add(deleteButton);

            Grid.SetColumn(textBoxBorder, 1);
            Grid.SetColumn(placeholderLabel, 1);
            Grid.SetColumn(deleteButton, 2);

            SubcategoryContainer.Children.Add(subcategoryGrid);
            subcategoryCount++;
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = CategoryNameTextBox.Text.Trim();
            int categoryId;
            int minNameLength = 3;
            HashSet<string> subcategoryNames = new HashSet<string>();

            if (categoryName.Length < minNameLength)
            {
                MessageBox.Show("Nazwa kategorii musi mieć co najmniej " + minNameLength + " znaków.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (subcategoryCount > 0)
            {
                foreach (UIElement element in SubcategoryContainer.Children)
                {
                    if (element is Grid subcategoryGrid)
                    {
                        foreach (UIElement child in subcategoryGrid.Children)
                        {
                            if (child is Border subcategoryBorder && subcategoryBorder.Child is TextBox subcategoryTextBox)
                            {
                                string subcategoryName = subcategoryTextBox.Text.Trim();

                                if (subcategoryName.Length < minNameLength)
                                {
                                    MessageBox.Show("Nazwa podkategorii musi mieć co najmniej " + minNameLength + " znaków.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                                if (subcategoryNames.Contains(subcategoryName))
                                {
                                    MessageBox.Show("Wprowadzono dwie podkategorie o tej samej nazwie '" + subcategoryName + "'. Proszę podać unikalne nazwy dla każdej podkategorii.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                subcategoryNames.Add(subcategoryName);
                            }
                        }
                    }
                }
            }

            if (Service.IsCategoryPresentInDefaultOrFamily(userId, categoryName))
            {
                MessageBox.Show("Kategoria o tej nazwie już znajduje się na liście.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (isEditMode && categoryIdToEdit > 0)
            {
                categoryId = categoryIdToEdit;
                if (Service.UpdateCategoryName(categoryId, categoryName))
                {
                    MessageBox.Show("Kategoria została zaktualizowana pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    mainWindow.CategoriesButton_Click(sender, e);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas aktualizacji kategorii.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                categoryId = Service.AddCategory(categoryName, userId);

                if (categoryId > 0)
                {
                    if (subcategoryCount > 0)
                    {
                        bool allSubcategoriesAdded = true;
                        if (subcategoryCount > 0)
                        {
                            foreach (UIElement element in SubcategoryContainer.Children)
                            {
                                if (element is Grid subcategoryGrid)
                                {
                                    foreach (UIElement child in subcategoryGrid.Children)
                                    {
                                        if (child is Border subcategoryBorder && subcategoryBorder.Child is TextBox subcategoryTextBox)
                                        {
                                            string subcategoryName = subcategoryTextBox.Text;
                                            bool subcategoryAdded = Service.AddSubcategory(categoryId, subcategoryName, userId);
                                            if (!subcategoryAdded)
                                            {
                                                allSubcategoriesAdded = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (allSubcategoriesAdded)
                        {
                            MessageBox.Show("Kategoria i podkategorie zostały pomyślnie dodane.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                            mainWindow.CategoriesButton_Click(sender, e);
                            CategoryNameTextBox.Text = "";
                            SubcategoryContainer.Children.Clear();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Wystąpił błąd podczas dodawania niektórych podkategorii.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Kategoria została pomyślnie dodana.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                        mainWindow.CategoriesButton_Click(sender, e);
                        CategoryNameTextBox.Text = "";
                        SubcategoryContainer.Children.Clear();
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas dodawania kategorii.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddCategoryButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AddCategoryButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3FFFFFFF"));
        }

        private void AddCategoryButton_MouseLeave(object sender, MouseEventArgs e)
        {
            AddCategoryButton.Background = Brushes.Transparent;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show( "Czy na pewno chcesz zamknąć okno? Dane zostaną utracone, jeśli nie klikniesz 'Zapisz'.", "Potwierdzenie zamknięcia",  MessageBoxButton.YesNo, MessageBoxImage.Warning); 
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}
