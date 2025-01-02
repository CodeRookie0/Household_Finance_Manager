using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
using Main.Logic;
using Main.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for StatisticsControl.xaml
    /// </summary>
    public class TransactionViewModel
    {
        public int Rank { get; set; }
        public string CategoryName { get; set; }
        public string SubcategoryName { get; set; }
        public string FormattedAmount { get; set; }
        public string DateFormatted { get; set; }
        public string Time { get; set; }
        public string UserName { get; set; }
    }

    public partial class StatisticsControl : UserControl
    {
        public List<Transaction> TopTransactions { get; set; }
        public List<TransactionViewModel> TopTransactionsModel { get; set; }

        private MainWindow mainWindow;
        private readonly int userId;
        private int userRole;
        private int familyId;
        private int selectedPeriod;
        public StatisticsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            familyId = Service.GetFamilyIdByMemberId(userId);
            userRole= Service.GetRoleIDByUserID(userId);

            InitializeComponent();

            InitializeComboBoxes();
            FilterButton_Click(null, new RoutedEventArgs());
            LoadTopTransactions();
            LoadCharts();
        }

        private void InitializeComboBoxes()
        {
            PeriodComboBox.SelectedIndex = 1;

            List<TransactionType> transactionTypes = Service.GetTransactionTypes();
            foreach (var transactionType in transactionTypes)
            {
                TransactionTypeComboBox.Items.Add(transactionType);
            }
            TransactionTypeComboBox.SelectedIndex = 1;

            if (familyId >= 0 && userRole!=3)
            {
                UserComboBox.Visibility = Visibility.Visible;

                var users = Service.GetFamilyMembersByFamilyId(familyId);
                UserComboBox.Items.Add(new User { UserID = -1, UserName = "Wszyscy" });
                foreach (var user in users)
                {
                    UserComboBox.Items.Add(user);
                }
                UserComboBox.SelectedIndex = 0;
            }
            else
            {
                UserComboBox.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadTopTransactions()
        {
            if (TopTransactions != null && TopTransactions.Any())
            {
                TopTransactionsModel = TopTransactions.Select((t, index) => new TransactionViewModel
                {
                    Rank = index + 1,
                    CategoryName = t.CategoryName,
                    SubcategoryName = t.SubcategoryName,
                    FormattedAmount = t.FormattedAmount,
                    DateFormatted = t.DateFormatted,
                    Time = t.Time,
                    UserName = t.UserName
                }).ToList();
            }
            else
            {
                TopTransactionsModel = new List<TransactionViewModel>();
                Console.WriteLine("pusta lista");
            }

            TopExpensesDataGrid.ItemsSource = TopTransactionsModel;
        }

        private void LoadCharts()
        {
            LoadIncomeVsExpensesPlot();
            LoadPeriodExpensesPlot();
            LoadCategoryExpensesPieChart();
        }

        private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedPeriod = PeriodComboBox.SelectedIndex;
            DateRangePanel.Visibility = PeriodComboBox.SelectedIndex == 4 ? Visibility.Visible : Visibility.Collapsed;
            TransactionTypeTextBlock.Width = PeriodComboBox.SelectedIndex == 4 ? 70 : 95;
        }

        private DateTime StartOfWeek(DateTime date)
        {
            int diff = date.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0)
            {
                diff += 7;
            }
            return date.AddDays(-diff).Date;
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            string startDate = null;
            string endDate = null;
            int transactionTypeId = TransactionTypeComboBox.SelectedIndex + 1;

            GetDateRange(ref startDate, ref endDate);

            DateTime? parsedStartDate = null;
            DateTime? parsedEndDate = null;

            if (startDate != null)
            {
                parsedStartDate = DateTime.ParseExact(startDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            if (endDate != null)
            {
                parsedEndDate = DateTime.ParseExact(endDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }

            if (parsedStartDate.HasValue && parsedEndDate.HasValue && parsedEndDate.Value < parsedStartDate.Value)
            {
                MessageBox.Show("Data 'do' nie może być wcześniejsza niż data 'od'.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PeriodComboBox.SelectedIndex == 4 && (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null))
            {
                MessageBox.Show("Przy wyborze okresu 'Inny' należy uzupełnić pełny przedział czasowy, wypełniając obie daty.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            int? filterUserId;

            if (familyId < 0 || userRole == 3)
            {
                filterUserId = userId;
                TopTransactions = Service.GetTop10UserTransactions(filterUserId, dateFrom: startDate, dateTo: endDate, transactionTypeId: transactionTypeId);
            }
            else
            {
                filterUserId = (UserComboBox.SelectedValue as int?) == -1 ? (int?)null : (int?)UserComboBox.SelectedValue;
                TopTransactions = Service.GetTop10FamilyTransactions(familyId, filterUserId: filterUserId, startDate: startDate, endDate: endDate, transactionTypeId: transactionTypeId);
            }

            LoadTopTransactions();
            LoadCharts();
        }

        private void GetDateRange(ref string startDate, ref string endDate)
        {
            DateTime? start = null;
            DateTime? end = null;

            switch (selectedPeriod)
            {
                case 0: // Today
                    start = DateTime.Today;
                    end = DateTime.Today.AddTicks(-1);
                    break;
                case 1: // This Week
                    start = StartOfWeek(DateTime.Today);
                    end = start.Value.AddDays(7).AddTicks(-1);
                    break;
                case 2: // This Month
                    start = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    end = start.Value.AddMonths(1).AddDays(-1).AddTicks(-1);
                    break;
                case 3: // This Year
                    start = new DateTime(DateTime.Today.Year, 1, 1);
                    end = new DateTime(DateTime.Today.Year, 12, 31).AddTicks(-1);
                    break;
                case 4: // Custom Date Range
                    start = StartDatePicker.SelectedDate;
                    end = EndDatePicker.SelectedDate?.AddDays(1).AddTicks(-1);
                    break;
            }

            startDate = start?.ToString("yyyy-MM-dd HH:mm:ss");
            endDate = end?.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            PeriodComboBox.SelectedIndex = 1;
            DateRangePanel.Visibility = Visibility.Collapsed;
            TransactionTypeComboBox.SelectedIndex = 1;
            UserComboBox.SelectedIndex = 0;
        }

        private void LoadIncomeVsExpensesPlot()
        {
            var model = new PlotModel
            {
                Title = "Przychody i Wydatki za ostatnie 6 miesięcy"
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -1,
                Maximum = 71,
                Title = "Kwota"
            };
            model.Axes.Add(yAxis);

            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Miesiące"
            };
            xAxis.Labels.AddRange(new[] { "Lis", "Paź", "Wrz", "Sie", "Lip", "Cze" });
            model.Axes.Add(xAxis);
            var s1 = new LineSeries
            {
                Title = "Przychody",
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
                MarkerStrokeThickness = 1.5,
                TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:C2}"
            };
            s1.Points.Add(new DataPoint(0, 10));
            s1.Points.Add(new DataPoint(1, 40));
            s1.Points.Add(new DataPoint(2, 20));
            s1.Points.Add(new DataPoint(3, 30));
            s1.Points.Add(new DataPoint(4, 20));
            s1.Points.Add(new DataPoint(5, 30));
            model.Series.Add(s1);

            var s2 = new LineSeries
            {
                Title = "Wydatki",
                Color = OxyColors.Teal,
                MarkerType = MarkerType.Diamond,
                MarkerSize = 6,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Teal,
                MarkerStrokeThickness = 1.5,
                TrackerFormatString = "{0}\n{1}: {2}\n{3}: -{4:C2}"
            };
            s2.Points.Add(new DataPoint(0, 4));
            s2.Points.Add(new DataPoint(1, 32));
            s2.Points.Add(new DataPoint(2, 14.00));
            s2.Points.Add(new DataPoint(3, 20.99));
            s2.Points.Add(new DataPoint(4, 4));
            s2.Points.Add(new DataPoint(5, 32));
            model.Series.Add(s2);
            incomeVsExpensesLineChart.Model = model;
        }

        private void LoadPeriodExpensesPlot()
        {
            var model = new PlotModel
            {
                Title = "Przychody i Wydatki za ostatnie 6 miesięcy",
            };

            var s1 = new BarSeries { Title = "Category 1", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.SkyBlue };
            s1.Items.Add(new BarItem { Value = 25 });
            s1.Items.Add(new BarItem { Value = 137 });
            s1.Items.Add(new BarItem { Value = 18 });
            s1.Items.Add(new BarItem { Value = 40 });

            var s2 = new BarSeries { Title = "Category 2", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.Teal };
            s2.Items.Add(new BarItem { Value = 42 });
            s2.Items.Add(new BarItem { Value = 64 });
            s2.Items.Add(new BarItem { Value = 120 });
            s2.Items.Add(new BarItem { Value = 26 });

            var s3 = new BarSeries { Title = "Category 3", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.MediumTurquoise };
            s3.Items.Add(new BarItem { Value = 32 });
            s3.Items.Add(new BarItem { Value = 34 });
            s3.Items.Add(new BarItem { Value = 20 });
            s3.Items.Add(new BarItem { Value = 36 });

            var s4 = new BarSeries { Title = "Category 4", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.LightCyan };
            s4.Items.Add(new BarItem { Value = 132 });
            s4.Items.Add(new BarItem { Value = 134 });
            s4.Items.Add(new BarItem { Value = 120 });
            s4.Items.Add(new BarItem { Value = 236 });

            var s5 = new BarSeries { Title = "Category 5", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.DarkTurquoise };
            s5.Items.Add(new BarItem { Value = 132 });
            s5.Items.Add(new BarItem { Value = 134 });
            s5.Items.Add(new BarItem { Value = 120 });
            s5.Items.Add(new BarItem { Value = 236 });

            var s6 = new BarSeries { Title = "Category 6", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.LightSkyBlue };
            s6.Items.Add(new BarItem { Value = 132 });
            s6.Items.Add(new BarItem { Value = 134 });
            s6.Items.Add(new BarItem { Value = 120 });
            s6.Items.Add(new BarItem { Value = 236 });

            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
            categoryAxis.Labels.Add("Category A");
            categoryAxis.Labels.Add("Category B");
            categoryAxis.Labels.Add("Category C");
            categoryAxis.Labels.Add("Category D");
            var valueAxis = new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, MaximumPadding = 0.06, AbsoluteMinimum = 0 };
            model.Series.Add(s6);
            model.Series.Add(s3);
            model.Series.Add(s4);
            model.Series.Add(s5);
            model.Series.Add(s1);
            model.Series.Add(s2);
            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            periodExpensesBarChart.Model = model;
        }

        private void LoadCategoryExpensesPieChart()
        {
            var model = new PlotModel
            {
                Title = "Wydatki według kategorii za ostatnie 6 miesięcy",
            };

            var ps = new PieSeries();
            ps.Slices.Add(new PieSlice("Africa", 1030) { IsExploded = true, Fill = OxyColors.SkyBlue });
            ps.Slices.Add(new PieSlice("Americas", 929) { IsExploded = true, Fill = OxyColors.Teal });
            ps.Slices.Add(new PieSlice("Asia", 4157) { Fill = OxyColors.MediumTurquoise });
            ps.Slices.Add(new PieSlice("Europe", 739) { IsExploded = true, Fill = OxyColors.LightCyan });
            ps.Slices.Add(new PieSlice("Oceania", 35) { IsExploded = true, Fill = OxyColors.DarkTurquoise });
            ps.InnerDiameter = 0;
            ps.ExplodedDistance = 0.0;
            ps.Stroke = OxyColors.White;
            ps.StrokeThickness = 2.0;
            ps.InsideLabelPosition = 0.8;
            ps.AngleSpan = 360;
            ps.StartAngle = 0;
            model.Series.Add(ps);
            categoryExpensesPieChart.Model = model;
        }
    }
}
