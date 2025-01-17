﻿using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for CategoriesControl.xaml
    /// </summary>
    public partial class CategoriesControl : UserControl
    {
        private MainWindow mainWindow;
        private readonly int userId;
        private int roleId;
        public CategoriesControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();
            roleId = Service.GetRoleIDByUserID(userId);
            LoadCategories();
        }
        private void LoadCategories()
        {
            List<Category> defaultCategories = Service.GetDefaultCategories();

            int familyId=Service.GetFamilyIdByMemberId(userId);
            List<Category> familyCategories;
            if (familyId > 0)
            {
                familyCategories = Service.GetFamilyCategories(familyId);
            }
            else
            {
                familyCategories = Service.GetUserCategories(userId);
            }

            List<Category> allCategories = new List<Category>();

            foreach (var category in defaultCategories)
            {
                allCategories.Add(category);
            }

            foreach (var category in familyCategories)
            {
                allCategories.Add(category);
            }

            foreach (var category in allCategories)
            {
                Expander expander = new Expander
                {
                    FontSize = 20,
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F4EB1B6")),
                    BorderThickness = new Thickness(2),
                    Header = new Grid
                    {
                        Height = 50,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = category.CategoryName,
                                VerticalAlignment = VerticalAlignment.Center,
                                Padding = new Thickness(10,20,10,20),
                                HorizontalAlignment = HorizontalAlignment.Left
                            }
                        }
                    },
                    Padding = new Thickness(15, 0, 0, 0),
                    Margin = new Thickness(0, 0, 0, 5)
                };

                bool isFavorite = Service.IsCategoryFavoriteForUser(userId, category.CategoryID);

                var favoriteButton = new Button
                {
                    Content = new Image
                    {
                        Source = new BitmapImage(new Uri(isFavorite
                            ? "pack://application:,,,/Resources/heart_filled.png"
                            : "pack://application:,,,/Resources/heart_outlined.png")),
                        Width = 30,
                        Height = 30
                    },
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(5)
                };

                favoriteButton.MouseEnter += (sender, e) =>
                {
                    if (!isFavorite)
                    {
                        var button = (Button)sender;
                        var image = (Image)button.Content;
                        image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/heart_filled.png"));
                    }
                };

                favoriteButton.MouseLeave += (sender, e) =>
                {
                    if (!isFavorite)
                    {
                        var button = (Button)sender;
                        var image = (Image)button.Content;
                        image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/heart_outlined.png"));
                    }
                };

                favoriteButton.Click += (sender, e) =>
                {
                    var button = (Button)sender;
                    var image = (Image)button.Content;
                    if (isFavorite)
                    {
                        if(Service.DeleteCategoryFromFavorites(userId, category.CategoryID))
                        {
                            image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/heart_outlined.png"));
                            isFavorite = false;
                        }
                        else
                        {
                            MessageBox.Show("Wystąpił błąd podczas usuwania kategorii z ulubionych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        if(Service.AddCategoryToFavorites(userId, category.CategoryID))
                        {
                            image.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/heart_filled.png"));
                            isFavorite = true;
                        }
                        else
                        {
                            MessageBox.Show("Wystąpił błąd podczas dodawania kategorii do ulubionych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                };

                if (category.UserID != -1)
                {
                    if (category.UserID == userId || roleId == 1)
                    {
                        var editButton = new Button
                        {
                            Content = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/edit_green.png")),
                                Width = 25,
                                Height = 25
                            },
                            Background = Brushes.Transparent,
                            BorderBrush = Brushes.Transparent,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(5)
                        };

                        var deleteButton = new Button
                        {
                            Content = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Resources/delete_red.png")),
                                Width = 25,
                                Height = 25
                            },
                            Background = Brushes.Transparent,
                            BorderBrush = Brushes.Transparent,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(5)
                        };

                        var headerGrid = expander.Header as Grid;
                        if (headerGrid != null)
                        {
                            headerGrid.Width = 1060;
                            headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });

                            var categoryTextBlock = (TextBlock)headerGrid.Children[0];
                            Grid.SetColumn(categoryTextBlock, 0);

                            Grid.SetColumn(editButton, 1);
                            headerGrid.Children.Add(editButton);

                            Grid.SetColumn(deleteButton, 2);
                            headerGrid.Children.Add(deleteButton);

                            Grid.SetColumn(favoriteButton, 3);
                            headerGrid.Children.Add(favoriteButton);
                        }

                        editButton.Click += (sender, e) =>
                        {
                            AddCategoryControl addCategoryControl = new AddCategoryControl(userId, mainWindow, true, category.CategoryID);
                            addCategoryControl.Show();
                        };

                        deleteButton.Click += (sender, e) =>
                        {
                            var result = MessageBox.Show("Czy na pewno chcesz usunąć tę kategorię : " + category.CategoryName + "?\n" + "Wszystkie podkategorie zostaną usunięte razem z kategorią.", "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                            if (result == MessageBoxResult.Yes)
                            {
                                bool categoryDeleted = Service.DeleteCategory(category.CategoryID);

                                if (categoryDeleted)
                                {
                                    MessageBox.Show("Kategoria i podkategorie zostały pomyślnie usunięte.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                                    mainWindow.CategoriesButton_Click(sender, e);
                                }
                                else
                                {
                                    MessageBox.Show("Nie udało się usunąć kategorii.\n" +
                                        "Kategoria może być powiązana z transakcją, cykliczną płatnością, sklepem lub limitem, który uniemożliwia jej usunięcie.",
                                        "Niepowodzenie",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                                }
                            }
                        };
                    }
                    else if (category.UserID != userId && roleId == 2)
                    {
                        var headerGrid = expander.Header as Grid;
                        if (headerGrid != null)
                        {
                            headerGrid.Width = 1060;
                            headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });

                            var categoryTextBlock = (TextBlock)headerGrid.Children[0];
                            Grid.SetColumn(categoryTextBlock, 0);

                            Grid.SetColumn(favoriteButton, 3);
                            headerGrid.Children.Add(favoriteButton);
                        }
                    }
                }
                else
                {
                    var headerGrid = expander.Header as Grid;
                    if (headerGrid != null)
                    {
                        headerGrid.Width = 1060;
                        headerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });

                        var categoryTextBlock = (TextBlock)headerGrid.Children[0];
                        Grid.SetColumn(categoryTextBlock, 0);

                        Grid.SetColumn(favoriteButton, 1);
                        headerGrid.Children.Add(favoriteButton);
                    }
                }

                List<Subcategory> subcategories = Service.GetSubcategoriesByCategoryId(category.CategoryID);

                StackPanel subcategoryPanel = new StackPanel { Margin = new Thickness(50, 0, 17, 0) };
                foreach (var subcategory in subcategories)
                {
                    var subcategoryGrid = new Grid
                    {
                        Height = 50,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    subcategoryGrid.Children.Add(new TextBlock
                    {
                        Text = subcategory.SubcategoryName,
                        VerticalAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(5, 20, 10, 20),
                        HorizontalAlignment = HorizontalAlignment.Left
                    });

                    if (subcategory.UserID != -1)
                    {
                        if (subcategory.UserID == userId || roleId == 1)
                        {
                            var editButton = new Button
                            {
                                Content = new Image
                                {
                                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/edit_green.png")),
                                    Width = 25,
                                    Height = 25
                                },
                                Background = Brushes.Transparent,
                                BorderBrush = Brushes.Transparent,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                Padding = new Thickness(5)
                            };

                            var deleteButton = new Button
                            {
                                Content = new Image
                                {
                                    Source = new BitmapImage(new Uri("pack://application:,,,/Resources/delete_red.png")),
                                    Width = 25,
                                    Height = 25
                                },
                                Background = Brushes.Transparent,
                                BorderBrush = Brushes.Transparent,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center,
                                Padding = new Thickness(5)
                            };

                            subcategoryGrid.ColumnDefinitions.Add(new ColumnDefinition());
                            subcategoryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });
                            subcategoryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45) });

                            var subcategoryTextBlock = (TextBlock)subcategoryGrid.Children[0];
                            Grid.SetColumn(subcategoryTextBlock, 0);

                            Grid.SetColumn(editButton, 1);
                            subcategoryGrid.Children.Add(editButton);

                            Grid.SetColumn(deleteButton, 2);
                            subcategoryGrid.Children.Add(deleteButton);

                            editButton.Click += (sender, e) =>
                            {
                                AddSubategoryControl addSubategoryControl = new AddSubategoryControl(userId, mainWindow, true, subcategory.SubcategoryID, category.CategoryID);
                                addSubategoryControl.Show();
                            };

                            deleteButton.Click += (sender, e) =>
                            {
                                var result = MessageBox.Show("Czy na pewno chcesz usunąć tę podkategorię : " + subcategory.SubcategoryName + "?\n" + "Transakcje powiązane z nią będą miały przypisanej podkategori.", "Potwierdzenie usunięcia", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                                if (result == MessageBoxResult.Yes)
                                {
                                    bool categoryDeleted = Service.DeleteSubcategory(subcategory.SubcategoryID);

                                    if (categoryDeleted)
                                    {
                                        MessageBox.Show("Podkategoria została pomyślnie usunięta.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                                        mainWindow.CategoriesButton_Click(sender, e);
                                    }
                                    else
                                    {
                                        MessageBox.Show("Wystąpił błąd podczas usuwania podkategorii.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                            };
                        }
                        else if (subcategory.UserID != userId && roleId == 2)
                        {
                            subcategoryGrid.ColumnDefinitions.Add(new ColumnDefinition());

                            var subcategoryTextBlock = (TextBlock)subcategoryGrid.Children[0];
                            Grid.SetColumn(subcategoryTextBlock, 0);
                        }
                    }
                    subcategoryPanel.Children.Add(subcategoryGrid);
                }

                Border buttonBorder = new Border
                {
                    Background = new SolidColorBrush(Colors.Transparent),
                    BorderThickness = new Thickness(0),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    Margin = new Thickness(-15, 5, 5, 15),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Height = 30
                };

                Button newSubcategoryButton = new Button
                {
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Width = 205,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = "+ Nowa podkategoria",
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3AA9AD")),
                                VerticalAlignment = VerticalAlignment.Center,
                            }
                        }
                    },
                    Style = (Style)FindResource("NoHoverButtonStyle"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Background = new SolidColorBrush(Colors.Transparent),
                    BorderBrush = new SolidColorBrush(Colors.Transparent),
                    Width = 205
                };

                newSubcategoryButton.Click += (sender, e) =>
                {
                    AddSubategoryControl addSubategoryControl = new AddSubategoryControl(userId, mainWindow, false,-1,category.CategoryID);
                    addSubategoryControl.Show();
                };

                newSubcategoryButton.MouseEnter += (sender, e) =>
                {
                    if (newSubcategoryButton.Content is StackPanel stackPanel)
                    {
                        foreach (var child in stackPanel.Children)
                        {
                            if (child is TextBlock textBlock)
                            {
                                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#993AA9AD"));
                            }
                        }
                    }
                };

                newSubcategoryButton.MouseLeave += (sender, e) =>
                {
                    if (newSubcategoryButton.Content is StackPanel stackPanel)
                    {
                        foreach (var child in stackPanel.Children)
                        {
                            if (child is TextBlock textBlock)
                            {
                                textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3AA9AD"));
                            }
                        }
                    }
                };

                buttonBorder.Child = newSubcategoryButton;
                subcategoryPanel.Children.Add(buttonBorder);
                expander.Content = subcategoryPanel;
                CategoriesStackPanel.Children.Add(expander);
            }
        }
        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            AddCategoryControl addCategoryControl = new AddCategoryControl(userId, mainWindow,false, -1);
            addCategoryControl.Show();
        }

        private void AddCategoryButton_MouseEnter(object sender, MouseEventArgs e)
        {
            AddCategoryBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#993AA9AD"));
        }

        private void AddCategoryButtonn_MouseLeave(object sender, MouseEventArgs e)
        {
            AddCategoryBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3AA9AD"));
        }
    }
}
