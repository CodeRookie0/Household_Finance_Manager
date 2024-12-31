using Main.Controls;
using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

            bool? isExceededYes = ExceededYesCheckBox.IsChecked;
            bool? isExceededNo = ExceededNoCheckBox.IsChecked;

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
                allLimits = Service.GetFilteredFamilyLimits(familyId, isFamilyWide, filterUserId, categoryId, frequencyId, amountFrom, amountTo, isExceededYes, isExceededNo);

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
                userLimits = Service.GetFilteredUserLimits(isFamilyWide, filterUserId, categoryId, frequencyId, amountFrom, amountTo, isExceededYes, isExceededNo);

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
    }
}
