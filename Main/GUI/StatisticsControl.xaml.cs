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
                UserTextBlock.Visibility = Visibility.Visible;

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
                UserTextBlock.Visibility = Visibility.Collapsed;
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
            if (transactionTypeId == 1)
            {
                TopExpensesTextBlock.Text = "Top 10 Największych Przychodów";
            }
            else if (transactionTypeId == 2)
            {
                TopExpensesTextBlock.Text = "Top 10 Największych Wydatków";
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
                    end = DateTime.Today.AddDays(1).AddTicks(-1);
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
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
        }

        private void LoadIncomeVsExpensesPlot()
        {
            string startDate = null;
            string endDate = null;
            GetDateRange(ref startDate, ref endDate);

            DateTime? parsedStartDate = null;
            DateTime? parsedEndDate = null;

            if (DateTime.TryParse(startDate, out var tempStartDate))
            {
                parsedStartDate = tempStartDate;
            }

            if (DateTime.TryParse(endDate, out var tempEndDate))
            {
                parsedEndDate = tempEndDate;
            }

            var filterUserId = (UserComboBox.SelectedValue as int?) == -1 ? (int?)null : (int?)UserComboBox.SelectedValue;

            var transactions = (familyId < 0 || userRole == 3)
                ? new ObservableCollection<Transaction>(Service.GetFilteredUserTransactions(userId, 
                    dateFrom: parsedStartDate,
                    dateTo: parsedEndDate))
                : new ObservableCollection<Transaction>(Service.GetFilteredFamilyTransactions(
                    familyId,
                    startDate: parsedStartDate,
                    endDate: parsedEndDate,
                    filterUserId: filterUserId
                ));


            var incomeData = new Dictionary<string, decimal>();
            var expenseData = new Dictionary<string, decimal>();

            var differenceInDays = (parsedEndDate - parsedStartDate)?.TotalDays;

            if (differenceInDays.HasValue)
            {
                var incomeDataGrouped = new Dictionary<string, decimal>();
                var expenseDataGrouped = new Dictionary<string, decimal>();
                if (differenceInDays <= 1) // Pokaż godziny
                {
                    var hours = new[] { 0, 4, 8, 12, 16, 20, 24 };

                    foreach (var hour in hours)
                    {
                        string hourRange = $"{hour:00}:00";

                        var incomeSum = transactions
                            .Where(t => t.TransactionTypeID == 1 && t.Date.Hour >= hour && t.Date.Hour < hour + 4)
                            .Sum(t => t.Amount);

                        var expenseSum = transactions
                            .Where(t => t.TransactionTypeID == 2 && t.Date.Hour >= hour && t.Date.Hour < hour + 4)
                            .Sum(t => -t.Amount);

                        incomeDataGrouped[hourRange] = incomeSum;
                        expenseDataGrouped[hourRange] = expenseSum;
                    }

                    incomeData = incomeDataGrouped;
                    expenseData = expenseDataGrouped;
                }
                else if (differenceInDays > 1 && differenceInDays <= 14) // Pokaż dni
                {
                    var currentDate = parsedStartDate.Value;
                    var dayCounter = 0;

                    while (currentDate <= parsedEndDate.Value && dayCounter < differenceInDays)
                    {
                        string dateFormatted = currentDate.ToString("dd/MM");

                        var incomeSum = transactions
                            .Where(t => t.TransactionTypeID == 1 && t.Date >= currentDate.Date && t.Date < currentDate.AddDays(1).Date)
                            .Sum(t => t.Amount);

                        var expenseSum = transactions
                            .Where(t => t.TransactionTypeID == 2 && t.Date >= currentDate.Date && t.Date < currentDate.AddDays(1).Date)
                            .Sum(t => -t.Amount);

                        incomeDataGrouped[dateFormatted] = incomeSum;
                        expenseDataGrouped[dateFormatted] = expenseSum;

                        currentDate = currentDate.AddDays(1);
                        dayCounter++;
                    }

                    incomeData = incomeDataGrouped;
                    expenseData = expenseDataGrouped;
                }
                else if (differenceInDays > 14 && differenceInDays <= 42) // Pokaż tygodnie
                {
                    var weeksInRange = new List<Tuple<DateTime, DateTime>>();
                    var currentDate = parsedStartDate.Value;

                    while (currentDate <= parsedEndDate.Value)
                    {
                        var startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + (int)DayOfWeek.Monday);

                        var endOfWeek = startOfWeek.AddDays(6);

                        if (endOfWeek > parsedEndDate.Value)
                        {
                            endOfWeek = parsedEndDate.Value;
                        }

                        if (startOfWeek < parsedStartDate.Value)
                        {
                            startOfWeek = parsedStartDate.Value;
                        }

                        weeksInRange.Add(Tuple.Create(startOfWeek, endOfWeek));

                        currentDate = currentDate.AddDays(7);
                    }

                    if (currentDate > parsedEndDate.Value && weeksInRange.LastOrDefault()?.Item1 != parsedStartDate.Value)
                    {
                        var startOfLastPeriod = parsedEndDate.Value.AddDays(-(int)parsedEndDate.Value.DayOfWeek + (int)DayOfWeek.Monday);
                        var endOfLastPeriod = parsedEndDate.Value;

                        weeksInRange.Add(Tuple.Create(startOfLastPeriod, endOfLastPeriod));
                    }

                    foreach (var weekRange in weeksInRange)
                    {
                        var startOfWeek = weekRange.Item1.Date;
                        var endOfWeek = weekRange.Item2.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);

                        string weekRangeString = $"{startOfWeek:dd/MM} - {endOfWeek:dd/MM}";

                        var incomeSum = transactions
                            .Where(t => t.TransactionTypeID == 1 &&
                                        t.Date.Date >= startOfWeek && t.Date.Date <= endOfWeek)
                            .Sum(t => t.Amount);

                        var expenseSum = transactions
                            .Where(t => t.TransactionTypeID == 2 &&
                                        t.Date.Date >= startOfWeek && t.Date.Date <= endOfWeek)
                            .Sum(t => -t.Amount);

                        incomeDataGrouped[weekRangeString] = incomeSum;
                        expenseDataGrouped[weekRangeString] = expenseSum;
                    }

                    incomeData = incomeDataGrouped;
                    expenseData = expenseDataGrouped;
                }
                else if (differenceInDays > 42 && differenceInDays <= 366) // Pokaż miesiące
                {
                    var monthsInRange = Enumerable.Range(0, (parsedEndDate.Value.Year - parsedStartDate.Value.Year) * 12 + parsedEndDate.Value.Month - parsedStartDate.Value.Month + 1)
                        .Select(i => new DateTime(parsedStartDate.Value.Year, parsedStartDate.Value.Month, 1).AddMonths(i))
                        .ToList();

                    foreach (var month in monthsInRange)
                    {
                        string monthYear = month.ToString("MMM\nyyyy");

                        var startOfMonth = month.Date;
                        var endOfMonth = month.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999); 

                        var incomeSum = transactions
                            .Where(t => t.TransactionTypeID == 1 &&
                                        t.Date >= startOfMonth && t.Date <= endOfMonth)
                            .Sum(t => t.Amount);

                        var expenseSum = transactions
                            .Where(t => t.TransactionTypeID == 2 &&
                                        t.Date >= startOfMonth && t.Date <= endOfMonth)
                            .Sum(t => -t.Amount);

                        incomeDataGrouped[monthYear] = incomeSum;
                        expenseDataGrouped[monthYear] = expenseSum;
                    }

                    incomeData = incomeDataGrouped;
                    expenseData = expenseDataGrouped;
                }
                else if (differenceInDays > 366 && differenceInDays <= 730) // Pokaż kwartały
                {
                    var quartersInRange = new List<Tuple<DateTime, DateTime>>();
                    var currentDate = parsedStartDate.Value;

                    var firstQuarterStart = new DateTime(parsedStartDate.Value.Year, ((parsedStartDate.Value.Month - 1) / 3) * 3 + 1, 1);
                    var firstQuarterEnd = firstQuarterStart.AddMonths(2).AddDays(DateTime.DaysInMonth(firstQuarterStart.Year, firstQuarterStart.Month) - 1);

                    if (firstQuarterStart < parsedStartDate.Value)
                    {
                        firstQuarterStart = parsedStartDate.Value;
                    }
                    if (firstQuarterEnd > parsedEndDate.Value)
                    {
                        firstQuarterEnd = parsedEndDate.Value;
                    }

                    quartersInRange.Add(Tuple.Create(firstQuarterStart, firstQuarterEnd));

                    currentDate = firstQuarterEnd.AddDays(1);
                    while (currentDate <= parsedEndDate.Value)
                    {
                        var quarterStart = new DateTime(currentDate.Year, ((currentDate.Month - 1) / 3) * 3 + 1, 1);
                        var quarterEnd = quarterStart.AddMonths(2).AddDays(DateTime.DaysInMonth(quarterStart.Year, quarterStart.Month) - 1);

                        if (quarterEnd > parsedEndDate.Value)
                        {
                            quarterEnd = parsedEndDate.Value;
                        }

                        quartersInRange.Add(Tuple.Create(quarterStart, quarterEnd));

                        currentDate = quarterEnd.AddDays(1);
                    }

                    foreach (var quarter in quartersInRange)
                    {
                        string quarterYear = $"{quarter.Item1.ToString("MMM yy")}\n - {quarter.Item2.ToString("MMM yy")}";
                        Console.WriteLine(quarter);

                        var startOfQuarter = quarter.Item1.Date;
                        var endOfQuarter = quarter.Item2.Date.AddDays(1).AddTicks(-1);

                        var incomeSum = transactions
                            .Where(t => t.TransactionTypeID == 1 &&
                                        t.Date >= startOfQuarter && t.Date <= endOfQuarter)
                            .Sum(t => t.Amount);

                        var expenseSum = transactions
                            .Where(t => t.TransactionTypeID == 2 &&
                                        t.Date >= startOfQuarter && t.Date <= endOfQuarter)
                            .Sum(t => -t.Amount);

                        incomeDataGrouped[quarterYear] = incomeSum;
                        expenseDataGrouped[quarterYear] = expenseSum;
                    }

                    incomeData = incomeDataGrouped;
                    expenseData = expenseDataGrouped;
                }
                else if (differenceInDays > 730) // Pokaż lata
                {
                    int startYear = parsedStartDate.HasValue ? parsedStartDate.Value.Year : DateTime.Now.Year;
                    int endYear = parsedEndDate.HasValue ? parsedEndDate.Value.Year : DateTime.Now.Year;
                    for (int year = startYear; year <= endYear; year++)
                    {
                        var incomeSum = transactions
                            .Where(t => t.TransactionTypeID == 1 && t.Date.Year == year)
                            .Sum(t => t.Amount);

                        var expenseSum = transactions
                            .Where(t => t.TransactionTypeID == 2 && t.Date.Year == year)
                            .Sum(t => -t.Amount);

                        incomeDataGrouped[year.ToString()] = incomeSum;
                        expenseDataGrouped[year.ToString()] = expenseSum;
                    }

                    incomeData = incomeDataGrouped;
                    expenseData = expenseDataGrouped;
                }
            }
            else
            {
                return;
            }

            var model = new PlotModel
            {
                Title = "Przepływ pieniędzy"
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -1,
                Title = "Kwota"
            };

            model.Axes.Add(yAxis);

            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = differenceInDays <= 1 ? "Godziny" :
                (differenceInDays > 1 && differenceInDays <= 14) ? "Dni tygodnia" :
                (differenceInDays > 14 && differenceInDays <= 42) ? "Tygodnie" :
                (differenceInDays > 42 && differenceInDays <= 366) ? "Miesiące" :
                (differenceInDays > 366 && differenceInDays <= 730) ? "Kwartały" : 
                "Lata"
            };
            xAxis.Labels.AddRange(incomeData.Keys);
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
            foreach (var kvp in incomeData)
            {
                string key = kvp.Key;
                decimal value = kvp.Value;
                s1.Points.Add(new DataPoint(xAxis.Labels.IndexOf(key), (double)value));
            }

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
            foreach (var kvp in expenseData)
            {
                string key = kvp.Key;
                decimal value = kvp.Value;
                s2.Points.Add(new DataPoint(xAxis.Labels.IndexOf(key), (double)value));
            }
            model.Series.Add(s2);
            incomeVsExpensesLineChart.Model = model;
        }

        private void LoadPeriodExpensesPlot()
        {
            string startDate = null;
            string endDate = null;
            GetDateRange(ref startDate, ref endDate);

            DateTime? parsedStartDate = null;
            DateTime? parsedEndDate = null;

            if (DateTime.TryParse(startDate, out var tempStartDate))
            {
                parsedStartDate = tempStartDate;
            }

            if (DateTime.TryParse(endDate, out var tempEndDate))
            {
                parsedEndDate = tempEndDate;
            }

            var filterUserId = (UserComboBox.SelectedValue as int?) == -1 ? (int?)null : (int?)UserComboBox.SelectedValue;
            int selectedTransactionType = TransactionTypeComboBox.SelectedIndex + 1;

            var transactions = (familyId < 0 || userRole == 3)
                ? new ObservableCollection<Transaction>(Service.GetFilteredUserTransactions(userId,
                    dateFrom: parsedStartDate,
                    dateTo: parsedEndDate,
                    transactionTypeId: selectedTransactionType))
                : new ObservableCollection<Transaction>(Service.GetFilteredFamilyTransactions(
                    familyId,
                    startDate: parsedStartDate,
                    endDate: parsedEndDate,
                    filterUserId: filterUserId,
                    transactionTypeId: selectedTransactionType

                ));

            var periodCategoryTotals = new Dictionary<string, Dictionary<string, decimal>>();
            var periodLabels = new List<string>();

            var differenceInDays = (parsedEndDate - parsedStartDate)?.TotalDays;

            if (differenceInDays.HasValue)
            {
                var periodData = new Dictionary<string, Dictionary<string, decimal>>();

                if (differenceInDays <= 1) // Pokaż godziny
                {
                    var hours = new[] { 0, 4, 8, 12, 16, 20, 24 };

                    foreach (var hour in hours)
                    {
                        string hourRange = $"{hour:00}:00";
                        periodData[hourRange] = new Dictionary<string, decimal>();

                        var filteredTransactions = transactions
                            .Where(t => t.Date.Hour >= hour && t.Date.Hour < hour + 4);

                        foreach (var category in filteredTransactions.Select(t => t.CategoryName).Distinct())
                        {
                            var categorySum = filteredTransactions
                                .Where(t => t.CategoryName == category)
                                .Sum(t => t.Amount);

                            periodData[hourRange][category] = categorySum;
                        }
                    }
                    periodCategoryTotals = periodData;
                }
                else if (differenceInDays > 1 && differenceInDays <= 14) // Pokaż dni
                {
                    var currentDate = parsedStartDate.Value;
                    var dayCounter = 0;

                    while (currentDate <= parsedEndDate.Value && dayCounter < differenceInDays)
                    {
                        string dateFormatted = currentDate.ToString("dd/MM");
                        periodData[dateFormatted] = new Dictionary<string, decimal>();

                        var filteredTransactions = transactions
                            .Where(t => t.Date >= currentDate.Date && t.Date < currentDate.AddDays(1).Date);

                        foreach (var category in filteredTransactions.Select(t => t.CategoryName).Distinct())
                        {
                            var categorySum = filteredTransactions
                                .Where(t => t.CategoryName == category)
                                .Sum(t => t.Amount);

                            periodData[dateFormatted][category] = categorySum;
                        }

                        currentDate = currentDate.AddDays(1);
                        dayCounter++;
                    }
                    periodCategoryTotals = periodData;
                }
                else if (differenceInDays > 14 && differenceInDays <= 42) // Pokaż tygodnie
                {
                    var weeksInRange = new List<Tuple<DateTime, DateTime>>();
                    var currentDate = parsedStartDate.Value;

                    while (currentDate <= parsedEndDate.Value)
                    {
                        var startOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek + (int)DayOfWeek.Monday);

                        var endOfWeek = startOfWeek.AddDays(6);

                        if (endOfWeek > parsedEndDate.Value)
                        {
                            endOfWeek = parsedEndDate.Value;
                        }

                        if (startOfWeek < parsedStartDate.Value)
                        {
                            startOfWeek = parsedStartDate.Value;
                        }

                        weeksInRange.Add(Tuple.Create(startOfWeek, endOfWeek));

                        currentDate = currentDate.AddDays(7);
                    }

                    if (currentDate > parsedEndDate.Value && weeksInRange.LastOrDefault()?.Item1 != parsedStartDate.Value)
                    {
                        var startOfLastPeriod = parsedEndDate.Value.AddDays(-(int)parsedEndDate.Value.DayOfWeek + (int)DayOfWeek.Monday);
                        var endOfLastPeriod = parsedEndDate.Value;

                        weeksInRange.Add(Tuple.Create(startOfLastPeriod, endOfLastPeriod));
                    }

                    foreach (var weekRange in weeksInRange)
                    {
                        var startOfWeek = weekRange.Item1.Date;
                        var endOfWeek = weekRange.Item2.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);

                        string weekRangeString = $"{startOfWeek:dd/MM} - {endOfWeek:dd/MM}";
                        periodData[weekRangeString] = new Dictionary<string, decimal>();

                        var filteredTransactions = transactions
                            .Where(t => t.Date.Date >= startOfWeek && t.Date.Date <= endOfWeek);

                        foreach (var category in filteredTransactions.Select(t => t.CategoryName).Distinct())
                        {
                            var categorySum = filteredTransactions
                                .Where(t => t.CategoryName == category)
                                .Sum(t => t.Amount);

                            periodData[weekRangeString][category] = categorySum;
                        }
                    }
                    periodCategoryTotals = periodData;
                }
                else if (differenceInDays > 42 && differenceInDays <= 366) // Pokaż miesiące
                {
                    var monthsInRange = Enumerable.Range(0, (parsedEndDate.Value.Year - parsedStartDate.Value.Year) * 12 + parsedEndDate.Value.Month - parsedStartDate.Value.Month + 1)
                        .Select(i => new DateTime(parsedStartDate.Value.Year, parsedStartDate.Value.Month, 1).AddMonths(i))
                        .ToList();

                    foreach (var month in monthsInRange)
                    {
                        string monthYear = month.ToString("MMM yyyy");
                        periodData[monthYear] = new Dictionary<string, decimal>();

                        var startOfMonth = month.Date;
                        var endOfMonth = month.AddMonths(1).AddDays(-1).Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);

                        var filteredTransactions = transactions
                            .Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth);

                        foreach (var category in filteredTransactions.Select(t => t.CategoryName).Distinct())
                        {
                            var categorySum = filteredTransactions
                                .Where(t => t.CategoryName == category)
                                .Sum(t => t.Amount);

                            periodData[monthYear][category] = categorySum;
                        }
                    }
                    periodCategoryTotals = periodData;
                }
                else if (differenceInDays > 366 && differenceInDays <= 730) // Pokaż kwartały
                {
                    var quartersInRange = new List<Tuple<DateTime, DateTime>>();
                    var currentDate = parsedStartDate.Value;

                    var firstQuarterStart = new DateTime(parsedStartDate.Value.Year, ((parsedStartDate.Value.Month - 1) / 3) * 3 + 1, 1);
                    var firstQuarterEnd = firstQuarterStart.AddMonths(2).AddDays(DateTime.DaysInMonth(firstQuarterStart.Year, firstQuarterStart.Month) - 1);

                    if (firstQuarterStart < parsedStartDate.Value)
                    {
                        firstQuarterStart = parsedStartDate.Value;
                    }
                    if (firstQuarterEnd > parsedEndDate.Value)
                    {
                        firstQuarterEnd = parsedEndDate.Value;
                    }

                    quartersInRange.Add(Tuple.Create(firstQuarterStart, firstQuarterEnd));

                    currentDate = firstQuarterEnd.AddDays(1);
                    while (currentDate <= parsedEndDate.Value)
                    {
                        var quarterStart = new DateTime(currentDate.Year, ((currentDate.Month - 1) / 3) * 3 + 1, 1);
                        var quarterEnd = quarterStart.AddMonths(2).AddDays(DateTime.DaysInMonth(quarterStart.Year, quarterStart.Month) - 1);

                        if (quarterEnd > parsedEndDate.Value)
                        {
                            quarterEnd = parsedEndDate.Value;
                        }

                        quartersInRange.Add(Tuple.Create(quarterStart, quarterEnd));

                        currentDate = quarterEnd.AddDays(1);
                    }

                    foreach (var quarter in quartersInRange)
                    {
                        string quarterYear = $"{quarter.Item1.ToString("MMM yy")} - {quarter.Item2.ToString("MMM yy")}";
                        periodData[quarterYear] = new Dictionary<string, decimal>();

                        var startOfQuarter = quarter.Item1.Date;
                        var endOfQuarter = quarter.Item2.Date.AddDays(1).AddTicks(-1);

                        var filteredTransactions = transactions
                            .Where(t => t.Date >= startOfQuarter && t.Date <= endOfQuarter);

                        foreach (var category in filteredTransactions.Select(t => t.CategoryName).Distinct())
                        {
                            var categorySum = filteredTransactions
                                .Where(t => t.CategoryName == category)
                                .Sum(t => t.Amount);

                            periodData[quarterYear][category] = categorySum;
                        }
                    }
                    periodCategoryTotals = periodData;
                }
                else if (differenceInDays > 730) // Pokaż lata
                {
                    int startYear = parsedStartDate.HasValue ? parsedStartDate.Value.Year : DateTime.Now.Year;
                    int endYear = parsedEndDate.HasValue ? parsedEndDate.Value.Year : DateTime.Now.Year;
                    for (int year = startYear; year <= endYear; year++)
                    {
                        periodData[year.ToString()] = new Dictionary<string, decimal>();
                        var filteredTransactions = transactions
                            .Where(t => t.Date.Year == year);


                        foreach (var category in filteredTransactions.Select(t => t.CategoryName).Distinct())
                        {
                            var categorySum = filteredTransactions
                                .Where(t => t.CategoryName == category)
                                .Sum(t => t.Amount);

                            periodData[year.ToString()][category] = categorySum;
                        }
                    }
                    periodCategoryTotals = periodData;
                }
            }
            else
            {
                return;
            }

            string transactionTypeTitle = selectedTransactionType == 1 ? "Przychody" : "Wydatki";
            var model = new PlotModel
            {
                Title = $"{transactionTypeTitle} w podziale na kategorie (kwotowo)"
            };
            var yAxis = new CategoryAxis
            {
                Position = AxisPosition.Left,
                Key = "Time",
                Title = differenceInDays <= 1 ? "Godziny" :
                (differenceInDays > 1 && differenceInDays <= 14) ? "Dni tygodnia" :
                (differenceInDays > 14 && differenceInDays <= 42) ? "Tygodnie" :
                (differenceInDays > 42 && differenceInDays <= 366) ? "Miesiące" :
                (differenceInDays > 366 && differenceInDays <= 730) ? "Kwartały" :
                "Lata",
                ItemsSource = periodCategoryTotals.Keys.ToList()
            };
            model.Axes.Add(yAxis);

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Key = "Kwota",
                Title = "Kwota",
                MinimumPadding = 0.1,
                MaximumPadding = 0.1
            };
            model.Axes.Add(xAxis);

            var totalSeries = new List<BarSeries>();

            var categories = periodCategoryTotals.Values
                .SelectMany(periodData => periodData.Keys)
                .Distinct()
                .ToList();

            foreach (var category in categories)
            {
                var barSeries = new BarSeries
                {
                    Title = category,
                    IsStacked = true,
                    StrokeColor = OxyColors.Black, 
                    StrokeThickness = 1            
                };

                foreach (var period in periodCategoryTotals)
                {
                    var total = period.Value.ContainsKey(category) ? period.Value[category] : 0;

                    var periodIndex = periodCategoryTotals.Keys.ToList().IndexOf(period.Key);

                    var barItem = new BarItem
                    {
                        Value = (double)total,
                        CategoryIndex = periodIndex
                    };

                    barSeries.Items.Add(barItem);
                }

                totalSeries.Add(barSeries);
            }

            foreach (var series in totalSeries)
            {
                model.Series.Add(series);
            }
            periodExpensesBarChart.Model = model;
        }

        private void LoadCategoryExpensesPieChart()
        {
            string startDate = null;
            string endDate = null;
            GetDateRange(ref startDate, ref endDate);

            DateTime? parsedStartDate = null;
            DateTime? parsedEndDate = null;

            if (DateTime.TryParse(startDate, out var tempStartDate))
            {
                parsedStartDate = tempStartDate;
            }

            if (DateTime.TryParse(endDate, out var tempEndDate))
            {
                parsedEndDate = tempEndDate;
            }

            var filterUserId = (UserComboBox.SelectedValue as int?) == -1 ? (int?)null : (int?)UserComboBox.SelectedValue;
            int selectedTransactionType = TransactionTypeComboBox.SelectedIndex + 1;

            var transactions = (familyId < 0 || userRole == 3)
                ? new ObservableCollection<Transaction>(Service.GetFilteredUserTransactions(userId,
                    dateFrom: parsedStartDate,
                    dateTo: parsedEndDate,
                    transactionTypeId: selectedTransactionType))
                : new ObservableCollection<Transaction>(Service.GetFilteredFamilyTransactions(
                    familyId,
                    startDate: parsedStartDate,
                    endDate: parsedEndDate,
                    filterUserId: filterUserId,
                    transactionTypeId: selectedTransactionType

                ));

            var categoryTotals = new Dictionary<string, decimal>();
            decimal totalAmount = 0;

            foreach (Transaction transaction in transactions)
            {
                if(selectedTransactionType == 1)
                {
                    totalAmount += transaction.Amount;
                }
                else if (selectedTransactionType==2)
                {
                    totalAmount -= transaction.Amount;
                }
                if (categoryTotals.ContainsKey(transaction.CategoryName))
                {
                    categoryTotals[transaction.CategoryName] += transaction.Amount;
                }
                else
                {
                    categoryTotals[transaction.CategoryName] = transaction.Amount;
                }
            }

            string transactionTypeTitle = selectedTransactionType == 1 ? "Przychody" : "Wydatki";
            var model = new PlotModel
            {
                Title = $"{transactionTypeTitle} w podziale na kategorie (procentowo)"
            };

            var ps = new PieSeries
            {
                InsideLabelFormat = "",
            };

            var random = new Random();

            foreach (var category in categoryTotals)
            {
                double percentage = 0;
                if (selectedTransactionType==1)
                {
                     percentage = (double)(category.Value / totalAmount) * 100;
                }
                else if (selectedTransactionType==2)
                {
                     percentage = (double)(category.Value / -totalAmount) * 100;
                }
                double amount = (double)category.Value;
                string formattedAmount = amount.ToString("C");
                var color = OxyColor.FromRgb((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
                ps.Slices.Add(new PieSlice($"Kategoria: {category.Key}\nKwota: {formattedAmount}\nProcent: ", percentage)
                {
                    Fill = color,
                });
            }

            model.Series.Add(ps);
            categoryExpensesPieChart.Model = model;
        }
    }
}
