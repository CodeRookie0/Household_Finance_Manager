using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using System.Globalization;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using PdfSharp.Snippets.Font;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using System.IO;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Net;
using OxyPlot.Series;
using OxyPlot;
using System.Drawing;
using OxyPlot.Wpf;

namespace Main.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy RaportControls.xaml
    /// </summary>
    /// 

    public partial class RaportControls : System.Windows.Controls.UserControl
    {
        private List<TransactionSummary> transactions { get; set; }
        private List<TransactionSummary> MaxMinCategoryList { get; set; }
        public List<TransactionViewModel> recentTransactionsModel { get; set; }

        private DataGridTextColumn firstData, SecondData, ThirdData;

        private readonly int userId;
        private int userRole;
        private int familyId;
        private int selectedPeriod;

        private DataTable dataTable;

        private DataTable MaksAndMinTable;
        private List<Transaction> transactionsTest { get; set; }

        public List<Transaction> TopTransactions { get; set; }
        public List<TransactionViewModel> TopTransactionsModel { get; set; }


        private void GenerateRaportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Plik PDF (*.pdf)|*.pdf";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string extension = System.IO.Path.GetExtension(saveFileDialog.FileName);
                if (extension == ".pdf")
                {


                    var document = new Document();

                    var pdfRenderer = new PdfDocumentRenderer();
                    pdfRenderer.Document = document;

                    Section section = document.AddSection();

                    /*string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Raport.png");

                    var image = section.Headers.Primary.AddImage(imagePath);
                    image.Width = "2.5cm";
                    image.LockAspectRatio = true;
                    image.RelativeVertical = RelativeVertical.Margin;
                    image.RelativeHorizontal = RelativeHorizontal.Margin;
                    image.Top=ShapePosition.Top;
                    image.Left = ShapePosition.Right;
                    image.WrapFormat.Style = WrapStyle.Through;*/


                    var textFrame = section.AddTextFrame();
                    textFrame.Width = "15cm";
                    textFrame.Height = "2.5cm";
                    textFrame.Left = "1cm";
                    textFrame.Top = "2cm";

                    textFrame.LineFormat.Color = Colors.Black;
                    textFrame.LineFormat.Width = 1;
                    textFrame.LineFormat.DashStyle = DashStyle.Solid;

                    var paragraph = textFrame.AddParagraph();


                    paragraph.AddText("Household Finance Manager");
                    paragraph.AddLineBreak();
                    paragraph.AddText("Osoba która wygenerowała raport: " + Service.GetUserNameByUserID(userId));
                    paragraph.Format.SpaceAfter = 5;


                    paragraph = textFrame.AddParagraph();
                    paragraph.AddText("RAPORT");
                    paragraph.AddLineBreak();
                    paragraph.AddText("Z dnia " + $"{DateTime.Now:dd-MM-yyyy}");
                    paragraph.AddLineBreak();
                    paragraph.AddText("O Godzinie " + $"{DateTime.Now:HH:mm:ss}");
                    paragraph.Format.SpaceAfter = 20;


                    paragraph = section.AddParagraph();
                    paragraph.Format.SpaceBefore = 50;

                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                    paragraph.Format.Font.Bold = true;
                    paragraph.AddText("Ogólne Dane");

                    var tableFirst = document.LastSection.AddTable();
                    tableFirst.Borders.Width = 0.5;
                    tableFirst.Format.Alignment = ParagraphAlignment.Center;

                    tableFirst.AddColumn("8cm");
                    tableFirst.AddColumn("8cm");


                    Row row = tableFirst.AddRow();
                    row.HeadingFormat = true;
                    row.Format.Font.Bold = true;
                    row.Cells[0].AddParagraph("Nazwa");
                    row.Cells[1].AddParagraph("Wartość");

                    foreach (var item in ViewRaport.Items)
                    {
                        if (item is DataRowView rowView)
                        {
                            var tmpNazwa = rowView["Nazwa"];
                            var tmpWartosc = rowView["Wartość"];

                            row = tableFirst.AddRow();
                            row.Cells[0].AddParagraph(tmpNazwa.ToString());
                            row.Cells[1].AddParagraph(tmpWartosc.ToString());
                        }
                    }

                    paragraph = section.AddParagraph();
                    paragraph.Format.SpaceBefore = 50;


                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                    paragraph.Format.Font.Bold = true;
                    paragraph.AddText(TopExpensesTextBlock.Text);





                    var tableSecond = document.LastSection.AddTable();
                    tableSecond.Borders.Width = 0.5;
                    tableSecond.Format.Alignment = ParagraphAlignment.Center;

                    tableSecond.AddColumn("3.5cm");
                    tableSecond.AddColumn("3.5cm");
                    tableSecond.AddColumn("3.5cm");
                    tableSecond.AddColumn("3.5cm");
                    tableSecond.AddColumn("3.5cm");

                    Row row1 = tableSecond.AddRow();
                    row1.HeadingFormat = true;
                    row1.Format.Font.Bold = true;
                    row1.Cells[0].AddParagraph("NO.");
                    row1.Cells[1].AddParagraph("Kategoria");
                    row1.Cells[2].AddParagraph("Kwota");
                    row1.Cells[3].AddParagraph("Data");
                    row1.Cells[4].AddParagraph("User");

                    var dataCollection = TopExpensesDataGrid.ItemsSource as List<TransactionViewModel>;

                    if (dataCollection != null)
                    {
                        foreach (var item in dataCollection)
                        {
                            row1 = tableSecond.AddRow();
                            row1.Cells[0].AddParagraph(item.Rank.ToString());
                            row1.Cells[1].AddParagraph(item.CategoryName.ToString());
                            row1.Cells[2].AddParagraph(item.FormattedAmount.ToString());
                            row1.Cells[3].AddParagraph(item.DateFormatted.ToString());
                            row1.Cells[4].AddParagraph(item.UserName.ToString());
                        }
                    }



                    paragraph = section.AddParagraph();
                    paragraph.Format.SpaceBefore = 50;


                    paragraph.Format.Alignment = ParagraphAlignment.Center;
                    paragraph.Format.Font.Bold = true;
                    paragraph.AddText(MinAndMaxLabel.Text);


                    var tableThird = document.LastSection.AddTable();
                    tableThird.Borders.Width = 0.5;
                    tableThird.Format.Alignment = ParagraphAlignment.Center;

                    tableThird.AddColumn("6cm");
                    tableThird.AddColumn("6cm");
                    tableThird.AddColumn("6cm");

                    Row row2 = tableThird.AddRow();
                    row2.HeadingFormat = true;
                    row2.Format.Font.Bold = true;
                    row2.Cells[0].AddParagraph("Kategoria");
                    row2.Cells[1].AddParagraph("Maksymalna Wartość");
                    row2.Cells[2].AddParagraph("Minimalna Wartość");

                    foreach (var item in MaksAndMinAmountCategory.Items)
                    {
                        if (item is DataRowView rowView)
                        {
                            var tmpNazwa = rowView["Kategoria"];
                            var maxWartosc = rowView["Maksymalna Wartość"];
                            var minWartosc = rowView["Minimalna Wartość"];

                            var tmpRow = tableThird.AddRow();
                            tmpRow.Cells[0].AddParagraph(tmpNazwa.ToString());
                            tmpRow.Cells[1].AddParagraph(maxWartosc.ToString());
                            tmpRow.Cells[2].AddParagraph(minWartosc.ToString());
                        }
                    }



                    pdfRenderer.RenderDocument();

                    pdfRenderer.Save(saveFileDialog.FileName);

                    /*PdfDocument document = new PdfDocument();
                    document.Info.Title = "Raport";

                    PdfPage page = document.AddPage();

                    XGraphics gfx=XGraphics.FromPdfPage(page);
                    XFont font = new XFont("Arial", 10);

                    XFont TimeFont = new XFont("Arial", 30);
                    string currentDateTime = $"Data: {DateTime.Now:dd-MM-yyyy} Godzina: {DateTime.Now:HH:mm:ss}";
                    gfx.DrawString(currentDateTime, TimeFont, XBrushes.Black, new XPoint(50, 30));

                    

                    document.Save(saveFileDialog.FileName);*/


                }

            }
        }

        public RaportControls(int userId)
        {
            InitializeComponent();
            // Typ.SelectionChanged -= ComboBox_SelectionChanged;

            transactions = new List<TransactionSummary>();
            MaxMinCategoryList = new List<TransactionSummary>();
            this.userId = userId;

            familyId = Service.GetFamilyIdByMemberId(userId);
            userRole = Service.GetRoleIDByUserID(userId);


            dataTable = new DataTable();
            dataTable.Columns.Add("Nazwa", typeof(string));
            dataTable.Columns.Add("Wartość", typeof(string));

            MaksAndMinTable = new DataTable();
            MaksAndMinTable.Columns.Add("Kategoria", typeof(string));
            MaksAndMinTable.Columns.Add("Maksymalna Wartość", typeof(string));
            MaksAndMinTable.Columns.Add("Minimalna Wartość", typeof(string));





            InitializeComboBoxes();
            LoadLimitsSummary();
            LoadTransactionSummary();
            LoadTopTransactions();
            LoadMaksAndMindCategory();

            DBSqlite dBSqlite = new DBSqlite();
            var answer = dBSqlite.ExecuteQuery("SELECT   strftime('%m', Date), COUNT(*) ,ROUND(SUM(Amount),2) FROM Transactions WHERE UserID=@UserId GROUP BY strftime('%m', Date)",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
            if (answer != null)
            {
                foreach (DataRow row in answer.Rows)
                {
                    TransactionSummary obj = new TransactionSummary();
                    obj.FirstData = row[0].ToString();
                    obj.SecondData = Convert.ToInt32(row[1].ToString());
                    obj.ThirdData = Convert.ToDouble(row[2].ToString());
                    transactions.Add(obj);
                }

            }

            //Test.ItemsSource = transactions;

            //Typ.SelectionChanged += ComboBox_SelectionChanged;


            var MaxPrzychod = dBSqlite.ExecuteQuery("SELECT  SubcategoryName,ROUND(SUM(Amount), 2) AS TotalRevenue\r\nFROM Transactions INNER JOIN Subcategories ON Transactions.SubcategoryID=Subcategories.SubcategoryID\r\nWHERE TransactionTypeID=1 AND Transactions.UserID=@UserId\r\nGROUP BY TransactionID\r\nORDER BY TotalRevenue DESC\r\nLIMIT 1",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
            if (MaxPrzychod != null)
            {
                DataRow row = MaxPrzychod.Rows[0];
                // TextPrzychud.Text += row[0].ToString() + " o wartości " + row[1].ToString()+"zł";
            }

            var maxWydate = dBSqlite.ExecuteQuery("SELECT  SubcategoryName,ROUND(SUM(Amount), 2) AS TotalRevenue\r\nFROM Transactions INNER JOIN Subcategories ON Transactions.SubcategoryID=Subcategories.SubcategoryID\r\nWHERE TransactionTypeID=2 AND Transactions.UserID=@UserId\r\nGROUP BY TransactionID\r\nORDER BY TotalRevenue DESC\r\nLIMIT 1",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
            if (maxWydate != null)
            {
                DataRow row = maxWydate.Rows[0];
                //TextWydatek.Text += row[0].ToString() + " o wartości " + row[1].ToString() + "zł";
            }

            var AnswerValue = dBSqlite.ExecuteQuery("SELECT CategoryName,MAX(Amount),MIN(Amount) FROM Transactions INNER JOIN Categories ON Transactions.CategoryID=Categories.CategoryID WHERE Transactions.UserId=@UserId Group By Transactions.CategoryID\r\n",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
            if (AnswerValue != null)
            {
                foreach (DataRow row in AnswerValue.Rows)
                {
                    TransactionSummary transactionSummary = new TransactionSummary();
                    transactionSummary.FirstData = row[0].ToString();
                    if (int.TryParse(row[1]?.ToString(), out int maxAmount))
                    {
                        transactionSummary.SecondData = maxAmount;
                    }
                    else
                    {
                        transactionSummary.SecondData = 0; // Default or fallback value
                    }

                    if (int.TryParse(row[2]?.ToString(), out int minAmount))
                    {
                        transactionSummary.ThirdData = minAmount;
                    }
                    else
                    {
                        transactionSummary.ThirdData = 0; // Default or fallback value
                    }
                    MaxMinCategoryList.Add(transactionSummary);
                }
            }

            var CategoryName = new DataGridTextColumn
            {
                Header = "Kategoria",
                Binding = new System.Windows.Data.Binding("FirstData")
            };

            var MaxValue = new DataGridTextColumn
            {
                Header = "Maksymalna Wartość",
                Binding = new System.Windows.Data.Binding("SecondData")
            };

            var MinValue = new DataGridTextColumn
            {
                Header = "Minimalna Wartość",
                Binding = new System.Windows.Data.Binding("ThirdData")
            };

            // MaxMinCategory.Columns.Add(CategoryName);
            //MaxMinCategory.Columns.Add (MaxValue);
            // MaxMinCategory.Columns.Add(MinValue);

            // MaxMinCategory.ItemsSource = MaxMinCategoryList;

            ViewRaport.ItemsSource = dataTable.DefaultView;
            MaksAndMinAmountCategory.ItemsSource = MaksAndMinTable.DefaultView;

        }

        private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedPeriod = PeriodComboBox.SelectedIndex;
            DateRangePanel.Visibility = PeriodComboBox.SelectedIndex == 4 ? Visibility.Visible : Visibility.Collapsed;
            TransactionTypeTextBlock.Width = PeriodComboBox.SelectedIndex == 4 ? 70 : 95;
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
                System.Windows.MessageBox.Show("Data 'do' nie może być wcześniejsza niż data 'od'.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (PeriodComboBox.SelectedIndex == 4 && (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null))
            {
                System.Windows.MessageBox.Show("Przy wyborze okresu 'Inny' należy uzupełnić pełny przedział czasowy, wypełniając obie daty.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            dataTable.Clear();

            LoadLimitsSummary();
            LoadTransactionSummary();
            LoadTopTransactions();

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

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            System.Windows.Controls.ComboBox cmb = sender as System.Windows.Controls.ComboBox;
            if (cmb != null)
            {
                ComboBoxItem item = cmb.SelectedItem as ComboBoxItem;
                string value = item.Content.ToString();
                if (value == "Miesięczne")
                {
                    firstData.Header = "Miesiąc";
                    SecondData.Header = "Liczba Transakcji";
                    ThirdData.Header = "Suma";

                    // Test.ItemsSource = null;
                    transactions.Clear();
                    DBSqlite dBSqlite = new DBSqlite();
                    var answer = dBSqlite.ExecuteQuery("SELECT   strftime('%m', Date), COUNT(*) ,ROUND(SUM(Amount),2) FROM Transactions WHERE UserID=@UserId GROUP BY strftime('%m', Date)",
                        new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
                    if (answer != null)
                    {
                        foreach (DataRow row in answer.Rows)
                        {
                            TransactionSummary obj = new TransactionSummary();
                            obj.FirstData = row[0].ToString();
                            obj.SecondData = Convert.ToInt32(row[1].ToString());
                            obj.ThirdData = Convert.ToDouble(row[2].ToString());
                            transactions.Add(obj);
                        }

                    }

                }
                else if (value == "Kwartalne")
                {
                    //Test.ItemsSource = null;
                    transactions.Clear();

                    firstData.Header = "Kwartał";
                    SecondData.Header = "Liczba Transakcji";
                    ThirdData.Header = "Suma";

                    DBSqlite dBSqlite = new DBSqlite();
                    var answer = dBSqlite.ExecuteQuery("SELECT \r\n    CASE \r\n        WHEN strftime('%m', Date) BETWEEN '01' AND '03' THEN 'Q1'\r\n        WHEN strftime('%m', Date) BETWEEN '04' AND '06' THEN 'Q2'\r\n        WHEN strftime('%m', Date) BETWEEN '07' AND '09' THEN 'Q3'\r\n        WHEN strftime('%m', Date) BETWEEN '10' AND '12' THEN 'Q4'\r\n    END as kwartal ,\r\n    COUNT(*) ,\r\n    ROUND(SUM(amount), 2) \r\nFROM Transactions\r\nWHERE UserID=@UserId\r\nGROUP BY kwartal\r\n",
                        new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
                    if (answer != null)
                    {
                        foreach (DataRow row in answer.Rows)
                        {
                            TransactionSummary obj = new TransactionSummary();
                            obj.FirstData = row[0].ToString();
                            obj.SecondData = Convert.ToInt32(row[1].ToString());
                            obj.ThirdData = Convert.ToDouble(row[2].ToString());
                            transactions.Add(obj);
                        }

                    }

                }
                else
                {
                    //Test.ItemsSource = null;
                    transactions.Clear();
                    firstData.Header = "Rok";
                    SecondData.Header = "Liczba Transakcji";
                    ThirdData.Header = "Suma";

                    DBSqlite dBSqlite = new DBSqlite();
                    var answer = dBSqlite.ExecuteQuery("SELECT \r\n    strftime('%Y', Date) AS rok,\r\n    COUNT(*) ,\r\n    ROUND(SUM(amount), 2) \r\nFROM Transactions\r\nWHERE UserID = @UserId\r\nGROUP BY rok;\r\n\r\n",
                        new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
                    if (answer != null)
                    {
                        foreach (DataRow row in answer.Rows)
                        {
                            TransactionSummary obj = new TransactionSummary();
                            obj.FirstData = row[0].ToString();
                            obj.SecondData = Convert.ToInt32(row[1].ToString());
                            obj.ThirdData = Convert.ToDouble(row[2].ToString());
                            transactions.Add(obj);
                        }
                    }
                }
            }
            // Test.ItemsSource= transactions;
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

        private DateTime StartOfWeek(DateTime date)
        {
            int diff = date.DayOfWeek - DayOfWeek.Monday;
            if (diff < 0)
            {
                diff += 7;
            }
            return date.AddDays(-diff).Date;
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

            if (familyId >= 0 && userRole != 3)
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
            // ExceededLimitsTextBlock.Text = exceededLimits.ToString();
            //NotExceededLimitsTextBlock.Text = notExceededLimits.ToString();
            //TotalLimitsTextBlock.Text = totalLimits.ToString();

            dataTable.Clear();

            var ExcededLimitsRow = dataTable.NewRow();
            ExcededLimitsRow["Nazwa"] = "Przekroczone Limity";
            ExcededLimitsRow["Wartość"] = exceededLimits.ToString();
            dataTable.Rows.Add(ExcededLimitsRow);

            var notExceededLimitsRow = dataTable.NewRow();
            notExceededLimitsRow["Nazwa"] = "Nie Przekoroczne Limity";
            notExceededLimitsRow["Wartość"] = notExceededLimits.ToString();
            dataTable.Rows.Add(notExceededLimitsRow);

            var TotalLimitsTextRow = dataTable.NewRow();
            TotalLimitsTextRow["Nazwa"] = "Ogółem Limity";
            TotalLimitsTextRow["Wartość"] = totalLimits.ToString();
            dataTable.Rows.Add(TotalLimitsTextRow);

        }


        private void LoadTransactionSummary()
        {
            dataTable.Rows.Add(null, null);

            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1).AddDays(-1).AddTicks(-1);
            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            if (familyId < 0 || userRole == 3)
            {
                transactionsTest = Service.GetFilteredUserTransactions(userId: userId, dateFrom: startDate, dateTo: endDate);
                TransactionSummaryViewModel transactionSummaryViewModel = new TransactionSummaryViewModel(transactionsTest.ToList());

                var recentTransactions = transactionsTest
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


                var PrzychodyRow = dataTable.NewRow();
                PrzychodyRow["Nazwa"] = "Przychód";
                PrzychodyRow["Wartość"] = transactionSummaryViewModel.Przychody;
                dataTable.Rows.Add(PrzychodyRow);

                var WydatekRow = dataTable.NewRow();
                WydatekRow["Nazwa"] = "Wydatek";
                WydatekRow["Wartość"] = transactionSummaryViewModel.Wydatki;
                dataTable.Rows.Add(WydatekRow);


                var BilansRow = dataTable.NewRow();
                BilansRow["Nazwa"] = "Bilans";
                BilansRow["Wartość"] = transactionSummaryViewModel.Bilans;
                dataTable.Rows.Add(BilansRow);

                // LastTransactionsDataGrid.ItemsSource = recentTransactionsModel;
                // this.DataContext = transactionSummaryViewModel;
            }
            else
            {
                transactionsTest = Service.GetFilteredFamilyTransactions(familyId: familyId, startDate: startDate, endDate: endDate);
                TransactionSummaryViewModel transactionSummaryViewModel = new TransactionSummaryViewModel(transactionsTest);

                var recentTransactions = transactionsTest
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


                var PrzychodyRow = dataTable.NewRow();
                PrzychodyRow["Nazwa"] = "Przychody";
                PrzychodyRow["Wartość"] = transactionSummaryViewModel.Przychody;
                dataTable.Rows.Add(PrzychodyRow);

                var WydatekRow = dataTable.NewRow();
                WydatekRow["Nazwa"] = "Wydatek";
                WydatekRow["Wartość"] = transactionSummaryViewModel.Wydatki;
                dataTable.Rows.Add(WydatekRow);


                var BilansRow = dataTable.NewRow();
                BilansRow["Nazwa"] = "Bilans";
                BilansRow["Wartość"] = transactionSummaryViewModel.Bilans;
                dataTable.Rows.Add(BilansRow);


            }
        }

        private void LoadMaksAndMindCategory()
        {
            int answer = Service.GetFamilyIdByPrimaryUserId(userId);
            if (answer > 0) //<--- Is Family
            {
                DBSqlite db = new DBSqlite();
                var data = db.ExecuteQuery("SELECT Categories.CategoryName, MAX(Transactions.Amount) AS MaxAmount, MIN(Transactions.Amount) AS MinAmount FROM Transactions INNER JOIN Categories ON Transactions.CategoryID = Categories.CategoryID INNER JOIN Users ON Transactions.UserID = Users.UserID WHERE Users.FamilyId = @FamilyId GROUP BY Categories.CategoryName;",
                    new SqliteParameter("@FamilyId", answer));
                if (data != null)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        var TmpRow = MaksAndMinTable.NewRow();
                        TmpRow["Kategoria"] = row[0].ToString();
                        TmpRow["Maksymalna Wartość"] = row[1].ToString();
                        TmpRow["Minimalna Wartość"] = row[2].ToString();
                        MaksAndMinTable.Rows.Add(TmpRow);
                    }
                }

            }
            else// <--- No family
            {
                DBSqlite db = new DBSqlite();
                var data = db.ExecuteQuery("SELECT CategoryName,MAX(Amount),MIN(Amount) FROM Transactions INNER JOIN Categories ON Transactions.CategoryID=Categories.CategoryID WHERE Transactions.UserId=@UserId Group By Transactions.CategoryID",
                    new SqliteParameter("@UserId", userId));
                if (data != null)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        var TmpRow = MaksAndMinTable.NewRow();
                        TmpRow["Kategoria"] = row[0].ToString();
                        TmpRow["Maksymalna Wartość"] = row[1].ToString();
                        TmpRow["Minimalna Wartość"] = row[2].ToString();
                        MaksAndMinTable.Rows.Add(TmpRow);
                    }
                }
            }
        }
    }


}
