using Main.Logic;
using Main.Models;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Windows.Controls;
using DocumentFormat.OpenXml.Spreadsheet;
using Main.Controls;
using System.Windows;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for DashboardControl.xaml
    /// </summary>
    public partial class DashboardControl : UserControl
    {
        private MainWindow mainWindow;
        private readonly int userId;
        private int userRole;
        private int familyId;
        public List<TransactionViewModel> recentTransactionsModel { get; set; }
        private List<Transaction> transactions { get; set; }
        public DashboardControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            familyId = Service.GetFamilyIdByMemberId(userId);
            userRole = Service.GetRoleIDByUserID(userId);

            InitializeComponent();

            var currentMonthName = DateTime.Now.ToString("MMMM");
            var currentYearName = DateTime.Now.Year.ToString();

            TransactionAnalysisPeriodTextBlock.Text = currentMonthName + " " + currentYearName;
            LimitsPeriodTextBlock.Text = currentMonthName + " " + currentYearName;

            LoadLimitsSummary();
            LoadTransactionSummary();
            LoadCharts();
        }

        private void LoadCharts()
        {
            LoadIncomeVsExpensesPlot();
        }

        private void LoadLimitsSummary()
        {
            ObservableCollection<Limit> allLimits;
            int totalLimits = 0;
            int exceededLimits = 0;
            int notExceededLimits = 0;

            if (userRole != 3 && familyId > 0)
            {
                allLimits = Service.GetFamilyLimits(familyId);
            }
            else
            {
                allLimits = Service.GetUserLimits(userId);
            }

            foreach (Limit limit in allLimits)
            {
                double actualSpentAmount = 0;

                DateTime startDate = DateTime.Now;
                DateTime endDate = DateTime.Now;

                switch (limit.FrequencyId)
                {
                    case 1: // Codziennie
                        startDate = DateTime.Now.Date;
                        endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                        break;

                    case 2: // Co tydzień
                        startDate = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Monday);
                        endDate = startDate.AddDays(7).AddTicks(-1);
                        break;

                    case 3: // Co miesiąc
                        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddTicks(-1);
                        break;

                    case 4: // Co kwartał
                        int quarter = (DateTime.Now.Month - 1) / 3 + 1;
                        startDate = new DateTime(DateTime.Now.Year, (quarter - 1) * 3 + 1, 1);
                        endDate = new DateTime(DateTime.Now.Year, quarter * 3, 1).AddMonths(2).AddTicks(-1);
                        break;

                    case 5: // Co rok
                        startDate = DateTime.Now.Date.AddMonths(-12);
                        endDate = new DateTime(DateTime.Now.Year, 12, 31).AddDays(1).AddTicks(-1);
                        break;

                    default:
                        startDate = DateTime.Now.Date;
                        endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                        break;
                }

                DBSqlite dBSqlite = new DBSqlite();
                DataTable answer = dBSqlite.ExecuteQuery("SELECT SUM(Amount) FROM Transactions WHERE CategoryID=@CategoryId AND UserID=@UserId AND TransactionTypeID=2 AND Date >= @StartDate AND Date <= @EndDate",
                    new Microsoft.Data.Sqlite.SqliteParameter("@CategoryId", limit.CategoryId),
                    new SqliteParameter("@UserId", limit.UserId),
                    new Microsoft.Data.Sqlite.SqliteParameter("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss")),
                    new Microsoft.Data.Sqlite.SqliteParameter("@EndDate", endDate.ToString("yyyy-MM-dd HH:mm:ss")));

                if (answer.Rows.Count > 0)
                {
                    DataRow dataRow = answer.Rows[0];
                    if (!dataRow.IsNull(0))
                    {
                        actualSpentAmount = Math.Abs(double.Parse(dataRow[0].ToString()));
                    }
                }

                totalLimits++;

                if (actualSpentAmount > limit.LimitAmount)
                {
                    exceededLimits++;
                }
                else
                {
                    notExceededLimits++;
                }
            }
            ExceededLimitsTextBlock.Text = exceededLimits.ToString();
            NotExceededLimitsTextBlock.Text = notExceededLimits.ToString();
            TotalLimitsTextBlock.Text = totalLimits.ToString();
        }

        private void LoadTransactionSummary()
        {
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).AddTicks(-1);
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (familyId < 0 || userRole==3)
            {
                transactions = Service.GetFilteredUserTransactions(userId: userId, dateFrom: startDate, dateTo: endDate);
                TransactionSummaryViewModel transactionSummaryViewModel = new TransactionSummaryViewModel(transactions.ToList());

                var recentTransactions = transactions
                    .OrderByDescending(t => t.Date)
                    .Take(15)
                    .ToList();

                if (recentTransactions != null && recentTransactions.Any())
                {
                    recentTransactionsModel = recentTransactions.Select((t, index) => new TransactionViewModel
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
                    recentTransactionsModel = new List<TransactionViewModel>();
                }

                LastTransactionsDataGrid.ItemsSource = recentTransactionsModel;
                this.DataContext = transactionSummaryViewModel;
            }
            else
            {
                transactions = Service.GetFilteredFamilyTransactions(familyId: familyId, startDate: startDate, endDate: endDate);
                TransactionSummaryViewModel transactionSummaryViewModel = new TransactionSummaryViewModel(transactions);

                var recentTransactions = transactions
                    .OrderByDescending(t => t.Date)
                    .Take(15)
                    .ToList();

                if (recentTransactions != null && recentTransactions.Any())
                {
                    recentTransactionsModel = recentTransactions.Select((t, index) => new TransactionViewModel
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
                    recentTransactionsModel = new List<TransactionViewModel>();
                }

                LastTransactionsDataGrid.ItemsSource = recentTransactionsModel;
                this.DataContext = transactionSummaryViewModel;
            }
        }

        private void LoadIncomeVsExpensesPlot()
        {
            DateTime currentDate = DateTime.Now;
            string startDate = null;
            string endDate = null;

            DateTime startCalculationDate = currentDate.AddMonths(-11);
            startDate = new DateTime(startCalculationDate.Year, startCalculationDate.Month, 1).ToString("yyyy-MM-dd HH:mm:ss");
            endDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(1).AddDays(-1).AddTicks(-1).ToString("yyyy-MM-dd HH:mm:ss");

            DateTime? parsedStartDate = null;
            DateTime? parsedEndDate = null;
            string moneyFlowPeriodText = string.Empty;

            if (DateTime.TryParse(startDate, out var tempStartDate))
            {
                parsedStartDate = tempStartDate;
                moneyFlowPeriodText = parsedStartDate?.ToString("MMMM") + " " + parsedStartDate?.Year.ToString() + " - ";
            }

            if (DateTime.TryParse(endDate, out var tempEndDate))
            {
                parsedEndDate = tempEndDate;
                moneyFlowPeriodText = moneyFlowPeriodText + parsedEndDate?.ToString("MMMM") + " " + parsedEndDate?.Year.ToString();
            }

            MoneyFlowPeriodTextBlock.Text = moneyFlowPeriodText;

            var transactions = (familyId < 0 || userRole == 3)
                ? new ObservableCollection<Transaction>(Service.GetFilteredUserTransactions(userId,
                    dateFrom: parsedStartDate,
                    dateTo: parsedEndDate))
                : new ObservableCollection<Transaction>(Service.GetFilteredFamilyTransactions(
                    familyId,
                    startDate: parsedStartDate,
                    endDate: parsedEndDate
                ));


            var incomeData = new Dictionary<string, decimal>();
            var expenseData = new Dictionary<string, decimal>();

            var differenceInDays = (parsedEndDate - parsedStartDate)?.TotalDays;


            var incomeDataGrouped = new Dictionary<string, decimal>();
            var expenseDataGrouped = new Dictionary<string, decimal>();

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

            var model = new PlotModel();

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
                Title = "Miesiące"
            };
            xAxis.Labels.AddRange(incomeData.Keys);
            model.Axes.Add(xAxis);
            var s1 = new LineSeries
            {
                Title = "Przychody",
                Color = OxyColors.SkyBlue,
                MarkerType = OxyPlot.MarkerType.Circle,
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
                MarkerType = OxyPlot.MarkerType.Diamond,
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
    }
}
