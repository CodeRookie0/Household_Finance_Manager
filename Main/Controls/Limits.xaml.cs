using DocumentFormat.OpenXml.Wordprocessing;
using Main.GUI;
using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Runtime.InteropServices.ComTypes;
using System.Windows;
using System.Windows.Controls;
using Brushes = System.Windows.Media.Brushes;

namespace Main.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy Limits.xaml
    /// </summary>
    public partial class Limits : UserControl
    {
        private Limit thisLimits;
        private readonly int userId;
        private readonly int roleId;
        public bool IsLimitExceeded { get; private set; }
        public Limits(Limit argModel, int userId)
        {
            this.userId = userId;
            roleId = Service.GetRoleIDByUserID(userId);

            InitializeComponent();
            thisLimits = argModel;

            LimitName.Text="Limit dla " + Service.GetCategoryNameByCategoryID(argModel.CategoryId);
            CategoryName.Text ="Kategoria: "+ Service.GetCategoryNameByCategoryID(argModel.CategoryId);
            FrequencyUser.Text="Częstotliwość: "+Service.GetFrequencyNameByFrequencyID(argModel.FrequencyId);
            AddUser.Text = "Przypisany: " + Service.GetUserNameByUserID(argModel.UserId);

            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;

            switch (argModel.FrequencyId)
            {
                case 1: 
                    startDate = DateTime.Now.Date;
                    endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                    break;

                case 2: 
                    startDate = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek + (int)DayOfWeek.Monday);
                    endDate = startDate.AddDays(7).AddTicks(-1);
                    break;
                case 3:
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddTicks(-1);
                    break;

                case 4:
                    int quarter = (DateTime.Now.Month - 1) / 3 + 1;
                    startDate = new DateTime(DateTime.Now.Year, (quarter - 1) * 3 + 1, 1);
                    endDate = new DateTime(DateTime.Now.Year, quarter * 3, 1)
                        .AddMonths(2)
                        .AddTicks(-1);
                    break;
                case 5:
                    startDate = DateTime.Now.Date.AddMonths(-12);
                    endDate = new DateTime(DateTime.Now.Year, 12,31).AddDays(1).AddTicks(-1);
                    break;

                default:
                    startDate = DateTime.Now.Date;
                    endDate = DateTime.Now.Date.AddDays(1).AddTicks(-1);
                    break;
            }

            DBSqlite dBSqlite = new DBSqlite();
            DataTable answer = dBSqlite.ExecuteQuery("SELECT SUM(Amount) FROM Transactions WHERE CategoryID=@CategoryId AND UserID=@UserId AND TransactionTypeID=2 AND Date >= @StartDate AND Date <= @EndDate",
                new Microsoft.Data.Sqlite.SqliteParameter("@CategoryId", argModel.CategoryId),
                new SqliteParameter("@UserId", argModel.UserId),
                new Microsoft.Data.Sqlite.SqliteParameter("@StartDate", startDate.ToString("yyyy-MM-dd HH:mm:ss")),
                new Microsoft.Data.Sqlite.SqliteParameter("@EndDate", endDate.ToString("yyyy-MM-dd HH:mm:ss")));
            if(answer.Rows.Count > 0)
            {
                DataRow dataRow = answer.Rows[0];
                if (!dataRow.IsNull(0))
                {
                    double spentAmount = Math.Abs(double.Parse(dataRow[0].ToString()));
                    SpentAmountTextBlock.Text = spentAmount.ToString("F2") + " zł";
                    double spentPercentage = (spentAmount * 100) / argModel.LimitAmount;
                    IsLimitExceeded = spentPercentage > 100;

                    if (IsLimitExceeded)
                    {
                        SpentAmountTextBlock.Foreground = Brushes.Red;
                        SpentAmountTextBlock.FontWeight = FontWeights.Bold;
                    }

                    LimitAmount.Value = spentPercentage;
                    //SpentAmountTextBlock.Text = dataRow[0].ToString() + " zł";
                    //LimitAmount.Value = double.Parse(dataRow[0].ToString());
                }
                else
                {
                    SpentAmountTextBlock.Text = "00.00 zł";
                    LimitAmount.Value = 0;
                }
            }
            
            //SpentAmountTextBlock.Text = "60.00 zł";
            LimitAmountTextBlock.Text = argModel.LimitAmount.ToString("C");

            if(roleId == 3)
            {
                EditLimitButton.Visibility = Visibility.Collapsed;
                DeleteLimitButton.Visibility = Visibility.Collapsed;
            }
            else if (roleId == 2)
            {
                if(Service.GetRoleIDByUserID(argModel.UserId) != 3 && argModel.UserId!= userId || argModel.UserId == userId & Service.GetRoleIDByUserID(argModel.CreatedByUserID)==1)
                {
                    EditLimitButton.Visibility = Visibility.Collapsed;
                    DeleteLimitButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        public event EventHandler RefreshData;

        private void EditLimitButton_Click(object sender, RoutedEventArgs e)
        {
            EditLimits editLimits = new EditLimits(thisLimits, userId);
            editLimits.Closed += (s, args) => RefreshData?.Invoke(this, EventArgs.Empty);
            editLimits.Show();
        }

        private void DeleteLimitButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Czy na pewno chcesz usunąć ten limit ?", "Komunikat", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {

                DBSqlite dBSqlite = new DBSqlite();
                int answer = dBSqlite.ExecuteNonQuery("DELETE FROM Limits WHERE LimitID=@ArgLimitId",
                    new Microsoft.Data.Sqlite.SqliteParameter("@ArgLimitId", thisLimits.LimitId));
                if (answer > 0)
                {
                    MessageBox.Show("Limit został usunięty", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshData?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("Limit nie został usunięty", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            
        }
    }
}
