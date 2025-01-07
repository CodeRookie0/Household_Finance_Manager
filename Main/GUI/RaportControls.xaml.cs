using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
    /// Logika interakcji dla klasy RaportControls.xaml
    /// </summary>
    public partial class RaportControls : UserControl
    {
        private List<TransactionSummary> transactions{ get; set; }
        private List<TransactionSummary> MaxMinCategoryList { get; set; }

        private DataGridTextColumn firstData, SecondData, ThirdData;

        private readonly int userId;
        public RaportControls(int userId)
        {
            InitializeComponent();
            Typ.SelectionChanged -= ComboBox_SelectionChanged;

            transactions = new List<TransactionSummary>();
            MaxMinCategoryList= new List<TransactionSummary>(); 
            this.userId = userId;

            firstData = new DataGridTextColumn
            {
                Header = "Miesiąc",
                Binding = new Binding("FirstData")
            };

            SecondData = new DataGridTextColumn
            {
                Header = "Liczba Transakcji",
                Binding = new Binding("SecondData")
            };

            ThirdData= new DataGridTextColumn
            {
                Header = "Suma",
                Binding = new Binding("ThirdData")
            };

            Test.Columns.Add(firstData);
            Test.Columns.Add(SecondData);
            Test.Columns.Add(ThirdData);

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

            Test.ItemsSource = transactions;

            Typ.SelectionChanged += ComboBox_SelectionChanged;


            var MaxPrzychod = dBSqlite.ExecuteQuery("SELECT  SubcategoryName,ROUND(SUM(Amount), 2) AS TotalRevenue\r\nFROM Transactions INNER JOIN Subcategories ON Transactions.SubcategoryID=Subcategories.SubcategoryID\r\nWHERE TransactionTypeID=1 AND Transactions.UserID=@UserId\r\nGROUP BY TransactionID\r\nORDER BY TotalRevenue DESC\r\nLIMIT 1",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId",userId));
            if (MaxPrzychod != null)
            {
                DataRow row = MaxPrzychod.Rows[0];
                TextPrzychud.Text += row[0].ToString() + " o wartości " + row[1].ToString()+"zł";
            }

            var maxWydate =  dBSqlite.ExecuteQuery("SELECT  SubcategoryName,ROUND(SUM(Amount), 2) AS TotalRevenue\r\nFROM Transactions INNER JOIN Subcategories ON Transactions.SubcategoryID=Subcategories.SubcategoryID\r\nWHERE TransactionTypeID=2 AND Transactions.UserID=@UserId\r\nGROUP BY TransactionID\r\nORDER BY TotalRevenue DESC\r\nLIMIT 1",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
            if (maxWydate != null)
            {
                DataRow row = maxWydate.Rows[0];
                TextWydatek.Text += row[0].ToString() + " o wartości " + row[1].ToString() + "zł";
            }



            var AnswerValue = dBSqlite.ExecuteQuery("SELECT CategoryName,MAX(Amount),MIN(Amount) FROM Transactions INNER JOIN Categories ON Transactions.CategoryID=Categories.CategoryID WHERE Transactions.UserId=@UserId Group By Transactions.CategoryID\r\n",
                new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
            if (AnswerValue != null)
            {
                foreach(DataRow row in AnswerValue.Rows)
                {
                    TransactionSummary transactionSummary = new TransactionSummary();
                    transactionSummary.FirstData = row[0].ToString();
                    transactionSummary.SecondData = Convert.ToInt32(row[1].ToString());
                    transactionSummary.ThirdData = Convert.ToInt32(row[2].ToString());
                    MaxMinCategoryList.Add(transactionSummary);
                }
            }

            var CategoryName = new DataGridTextColumn
            {
                Header = "Kategoria",
                Binding = new Binding("FirstData")
            };

            var MaxValue = new DataGridTextColumn
            {
                Header = "Maksymalna Wartość",
                Binding = new Binding("SecondData")
            };

            var MinValue = new DataGridTextColumn
            {
                Header = "Minimalna Wartość",
                Binding = new Binding("ThirdData")
            };

            MaxMinCategory.Columns.Add(CategoryName);
            MaxMinCategory.Columns.Add (MaxValue);
            MaxMinCategory.Columns.Add(MinValue);

            MaxMinCategory.ItemsSource = MaxMinCategoryList;

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            ComboBox cmb= sender as ComboBox;   
            if (cmb!=null)
            {
                ComboBoxItem item= cmb.SelectedItem as ComboBoxItem;
                string value=item.Content.ToString();
                if(value== "Miesięczne")
                {
                    firstData.Header = "Miesiąc";
                    SecondData.Header = "Liczba Transakcji";
                    ThirdData.Header = "Suma";

                   Test.ItemsSource = null;
                   transactions.Clear();
                   DBSqlite dBSqlite = new DBSqlite();
                    var answer = dBSqlite.ExecuteQuery("SELECT   strftime('%m', Date), COUNT(*) ,ROUND(SUM(Amount),2) FROM Transactions WHERE UserID=@UserId GROUP BY strftime('%m', Date)",
                        new Microsoft.Data.Sqlite.SqliteParameter("@UserId", userId));
                    if (answer!=null)
                    {
                        foreach(DataRow row in answer.Rows)
                        {
                            TransactionSummary obj=new TransactionSummary();
                            obj.FirstData = row[0].ToString();
                            obj.SecondData = Convert.ToInt32(row[1].ToString());
                            obj.ThirdData = Convert.ToDouble(row[2].ToString());
                            transactions.Add(obj);
                        }
                    
                    }
                    
                }
                else if(value== "Kwartalne")
                {
                    Test.ItemsSource = null;
                    transactions.Clear();

                    firstData.Header = "Kwartał";
                    SecondData.Header = "Liczba Transakcji";
                    ThirdData.Header = "Suma";

                    DBSqlite dBSqlite= new DBSqlite();
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
                    Test.ItemsSource = null;
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
            Test.ItemsSource= transactions;
        }
    }
}
