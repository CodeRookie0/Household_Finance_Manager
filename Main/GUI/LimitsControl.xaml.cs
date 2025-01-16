using Main.Controls;
using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for LimitsControl.xaml
    /// </summary>
    public partial class LimitsControl : UserControl
    {
        private MainWindow mainWindow;
        private readonly int userId;
        public int roleId;

        public LimitsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            roleId = Service.GetRoleIDByUserID(userId);
            this.mainWindow = mainWindow;
            InitializeComponent();

            if(roleId==3)
            {
                AddLimitButton.Visibility = Visibility.Collapsed;
                UserComboBox.IsEnabled = false;
                LimitTypeComboBox.IsEnabled = false;
            }

            LoadLimits();
            LoadComboBoxValues();
        }

        private void LoadLimits()
        {
            LimitsGrid.RowDefinitions.Clear();
            LimitsGrid.Children.Clear();

            int familyId;
            ObservableCollection<Limit> userLimits;
            ObservableCollection<Limit> allLimits;
            int rowNumber = 0;
            int Numbercolumn = 0;
            LimitsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(190) });

            if (roleId!= 3)
            {
                familyId = Service.GetFamilyIdByMemberId(userId);
                allLimits=Service.GetFamilyLimits(familyId);

                foreach (Limit limit in allLimits)
                {
                    if (Numbercolumn > 2)
                    {
                        Numbercolumn = 0;
                        rowNumber++;

                        LimitsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(190) });
                    }

                    Limits tmpControl = new Limits(limit, userId);
                    tmpControl.RefreshData += RunEventRefresh;
                    tmpControl.MaxHeight = 190;
                    Grid.SetRow(tmpControl, rowNumber);
                    Grid.SetColumn(tmpControl, Numbercolumn);

                    LimitsGrid.Children.Add(tmpControl);

                    Numbercolumn++;
                }
            }
            else
            {
                userLimits = Service.GetUserLimits(userId);

                foreach (Limit limit in userLimits)
                {
                    if (Numbercolumn > 2)
                    {
                        Numbercolumn = 0;
                        rowNumber++;

                        LimitsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(190) });
                    };

                    Limits tmpControl = new Limits(limit, userId);
                    tmpControl.RefreshData += RunEventRefresh;
                    tmpControl.MaxHeight = 190;
                    Grid.SetRow(tmpControl, rowNumber);
                    Grid.SetColumn(tmpControl, Numbercolumn);

                    LimitsGrid.Children.Add(tmpControl);

                    Numbercolumn++;
                }
            }
        }

        private void LoadComboBoxValues()
        {
            int familyId = Service.GetFamilyIdByMemberId(userId);
            if (familyId < 0)
            {
                UserComboBox.IsEnabled = false;
            }
            else
            {
                var users = Service.GetFamilyMembersByFamilyId(familyId);
                UserComboBox.Items.Add(new User { UserID = -1, UserName = "Wybierz" });
                foreach (var user in users)
                {
                    UserComboBox.Items.Add(user);
                }
                UserComboBox.SelectedIndex = 0;
            }
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

            List<Frequency> frequencies = Service.GetFrequencies();
            var defaultFrequency = new Frequency { FrequencyID = -1, FrequencyName = "Wybierz" };
            frequencies.Insert(0, defaultFrequency);
            foreach (var frequency in frequencies)
            {
                FrequencyComboBox.Items.Add(frequency);
            }
            FrequencyComboBox.SelectedIndex = 0;
        }

        private void RunEventRefresh(object sender, EventArgs e)
        {
            LoadLimits();
        }

        private void AddLimitButton_Click(object sender, RoutedEventArgs e)
        {
            AddLimits limitsDialog = new AddLimits(userId);
            limitsDialog.Closed += (s, args) => LoadLimits();
            limitsDialog.Show();
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var isFamilyWide = LimitTypeComboBox.SelectedIndex;
            var filterUserId = UserComboBox.SelectedValue as int?;
            var categoryId = CategoryComboBox.SelectedValue as int?;
            var frequencyId = FrequencyComboBox.SelectedValue as int?;

            var minAmountText = AmountFromTextBox.Text;
            var maxAmountText = AmountToTextBox.Text;

            double? amountFrom = null;
            double? amountTo = null;

            int exceededLimit = -1;
            if(ExceededYesCheckBox.IsChecked == true && ExceededNoCheckBox.IsChecked == false)
            {
                exceededLimit = 1;
            }
            else if(ExceededYesCheckBox.IsChecked == false && ExceededNoCheckBox.IsChecked == true){
                exceededLimit = 0;
            }

            if (!string.IsNullOrWhiteSpace(minAmountText) && double.TryParse(minAmountText, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedAmountFrom))
            {
                amountFrom = parsedAmountFrom;
            }

            if (!string.IsNullOrWhiteSpace(maxAmountText) && double.TryParse(maxAmountText, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsedAmountTo))
            {
                amountTo = parsedAmountTo;
            }

            if (amountTo.HasValue && amountFrom.HasValue && amountTo < amountFrom)
            {
                MessageBox.Show("Kwota 'do' nie może być mniejsza niż kwota 'od'.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LimitsGrid.RowDefinitions.Clear();
            LimitsGrid.Children.Clear();

            int familyId;
            ObservableCollection<Limit> userLimits;
            ObservableCollection<Limit> allLimits;
            int rowNumber = 0;
            int Numbercolumn = 0;
            LimitsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(190) });

            if (roleId != 3)
            {
                familyId = Service.GetFamilyIdByMemberId(userId);
                allLimits = Service.GetFilteredFamilyLimits(familyId, isFamilyWide, filterUserId, categoryId, frequencyId, amountFrom, amountTo);

                foreach (Limit limit in allLimits)
                {
                    if (Numbercolumn > 2)
                    {
                        Numbercolumn = 0;
                        rowNumber++;

                        LimitsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(190) });
                    }

                    Limits tmpControl = new Limits(limit, userId);
                    if ((tmpControl.IsLimitExceeded && exceededLimit == 1) ||
                        (!tmpControl.IsLimitExceeded && exceededLimit == 0) ||
                        (exceededLimit == -1))
                    {
                        tmpControl.RefreshData += RunEventRefresh;
                        tmpControl.MaxHeight = 190;
                        Grid.SetRow(tmpControl, rowNumber);
                        Grid.SetColumn(tmpControl, Numbercolumn);

                        LimitsGrid.Children.Add(tmpControl);

                        Numbercolumn++;
                    }
                }
            }
            else
            {
                userLimits = Service.GetFilteredUserLimits(isFamilyWide, userId, categoryId, frequencyId, amountFrom, amountTo);

                foreach (Limit limit in userLimits)
                {
                    if (Numbercolumn > 2)
                    {
                        Numbercolumn = 0;
                        rowNumber++;

                        LimitsGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(190) });
                    };

                    Limits tmpControl = new Limits(limit, userId);
                    if ((tmpControl.IsLimitExceeded && exceededLimit == 1) ||
                        (!tmpControl.IsLimitExceeded && exceededLimit == 0) ||
                        (exceededLimit == -1))
                    {
                        tmpControl.RefreshData += RunEventRefresh;
                        tmpControl.MaxHeight = 190;
                        Grid.SetRow(tmpControl, rowNumber);
                        Grid.SetColumn(tmpControl, Numbercolumn);

                        LimitsGrid.Children.Add(tmpControl);

                        Numbercolumn++;
                    }
                }
            }
        }

        private void CleaFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            LimitTypeComboBox.SelectedIndex = 0;
            UserComboBox.SelectedIndex = 0;
            CategoryComboBox.SelectedIndex = 0;
            FrequencyComboBox.SelectedIndex = 0;
            AmountFromTextBox.Clear();
            AmountToTextBox.Clear();
            ExceededYesCheckBox.IsChecked = false;
            ExceededNoCheckBox.IsChecked = false;
            LoadLimits();
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
                    MessageBox.Show("Proszę podać poprawną kwotę (do dwóch miejsc po przecinku).", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
