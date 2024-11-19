﻿using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for AddSubategoryControl.xaml
    /// </summary>
    public partial class AddSubategoryControl : Window
    {
        private int subcategoryCount = 1;
        private const int MaxSubcategories = 5;
        private MainWindow mainWindow;
        private readonly int userId;
        private bool isEditMode = false;
        private int subcategoryIdToEdit = -1;
        private int categoryId = -1;
        public AddSubategoryControl(int loggedInUserId, MainWindow mainWindow, bool isEditMode, int subcategoryIdToEdit, int categoryId)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            this.isEditMode = isEditMode;
            this.subcategoryIdToEdit = subcategoryIdToEdit;
            this.categoryId = categoryId;
            InitializeComponent();
            SelectedCategoryNameTextBoxPlaceholder.Content = Service.GetCategoryNameByCategoryID(categoryId);
            if (isEditMode)
            {
                AddSubcategoryText.Visibility = Visibility.Collapsed;
                CategoryTitleText.Text = "Edytuj Podkategorię";
                if (subcategoryIdToEdit > 0)
                {
                    SubcategoryNameTextBoxPlaceholder.Visibility = Visibility.Collapsed;
                    SubcategoryNameTextBox.Text = Service.GetSubcategoryNameBySubcategoryID(subcategoryIdToEdit);
                }
            }

            this.categoryId = categoryId;
        }

        private void SubcategoryNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            SubcategoryNameTextBoxPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void SubcategoryNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SubcategoryNameTextBox.Text))
            {
                SubcategoryNameTextBoxPlaceholder.Visibility = Visibility.Visible;
            }
        }

        private void AddSubcategoryText_Click(object sender, MouseButtonEventArgs e)
        {
            if (subcategoryCount >= MaxSubcategories)
            {
                MessageBox.Show("Możesz dodać maksymalnie "+ MaxSubcategories + " podkategorii w tym oknie. Aby dodać kolejne, skorzystaj z opcji 'Nowa podkategoria' w zakładce 'Kategorie'.", "Osiągnięto limit podkategorii", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Grid subcategoryGrid = new Grid
            {
                Margin = new Thickness(0, 10, 0, 0)
            };

            subcategoryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            subcategoryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            TextBox subcategoryTextBox = new TextBox
            {
                Width = 300,
                Height = 45,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 16,
                Foreground = Brushes.Black,
                Padding = new Thickness(10,10,35,10),
                Margin = new Thickness(10, 0, 10, 0),
            };

            Label placeholderLabel = new Label
            {
                Content = "Nazwa podkategorii",
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
                Margin = new Thickness(10, 0, 25, 0),
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

            subcategoryGrid.Children.Add(subcategoryTextBox);
            subcategoryGrid.Children.Add(placeholderLabel);
            subcategoryGrid.Children.Add(deleteButton);

            Grid.SetColumn(subcategoryTextBox, 1);
            Grid.SetColumn(placeholderLabel, 1);
            Grid.SetColumn(deleteButton, 2);

            SubcategoryContainer.Children.Add(subcategoryGrid);
            subcategoryCount++;
        }

        private void AddSubcategoryButton_Click(object sender, RoutedEventArgs e)
        {
            string subcategoryName = SubcategoryNameTextBox.Text;
            int subcategoryId;
            int minNameLength = 3;

            if (subcategoryName.Length < minNameLength)
            {
                MessageBox.Show("Nazwa podkategorii musi mieć co najmniej " + minNameLength + " znaków.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (isEditMode && subcategoryIdToEdit > 0)
            {
                subcategoryId = subcategoryIdToEdit;
                if (Service.UpdateSubcategoryName(subcategoryId, subcategoryName)) 
                {
                    MessageBox.Show("Podkategoria została zaktualizowana pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                    mainWindow.CategoriesButton_Click(sender, e);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas aktualizacji podkategorii.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
               bool allSubcategoriesAdded = true;

               if (subcategoryName.Length < minNameLength)
               {
                   MessageBox.Show("Nazwa podkategorii musi mieć co najmniej " + minNameLength + " znaków.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                   return;
               }

               bool subcategoryAdded = Service.AddSubcategory(categoryId, subcategoryName, userId);

               if (!subcategoryAdded)
               {
                   allSubcategoriesAdded = false;
               }

               foreach (UIElement element in SubcategoryContainer.Children)
               {
                   if (element is Grid subcategoryGrid)
                   {
                       foreach (UIElement child in subcategoryGrid.Children)
                       {
                           if (child is TextBox subcategoryTextBox)
                           {
                               subcategoryName = subcategoryTextBox.Text;

                               if (subcategoryName.Length < minNameLength)
                               {
                                   MessageBox.Show("Nazwa podkategorii musi mieć co najmniej " + minNameLength + " znaków.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                                   return;
                               }

                               subcategoryAdded = Service.AddSubcategory(categoryId, subcategoryName, userId);
                               if (!subcategoryAdded)
                               {
                                   allSubcategoriesAdded = false;
                               }
                           }
                       }
                   }
               }

               if (allSubcategoriesAdded)
               {
                   MessageBox.Show("Podkategorie zostały pomyślnie dodane.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                   mainWindow.CategoriesButton_Click(sender, e);
                   SubcategoryNameTextBox.Text = "";
                   SubcategoryContainer.Children.Clear();
                   this.Close();
               }
               else
               {
                   MessageBox.Show("Wystąpił błąd podczas dodawania podkategorii.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
               }
            }
        }

        private void AddSubcategoryButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AddSubcategoryButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3FFFFFFF"));
        }

        private void AddSubcategoryButton_MouseLeave(object sender, MouseEventArgs e)
        {
            AddSubcategoryButton.Background = Brushes.Transparent;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Czy na pewno chcesz zamknąć okno? Dane zostaną utracone, jeśli nie klikniesz 'Zapisz'.", "Potwierdzenie zamknięcia", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}