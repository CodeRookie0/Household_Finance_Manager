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
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Main.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy RaportControls.xaml
    /// </summary>
    public partial class RaportControls : System.Windows.Controls.UserControl
    {
        private List<TransactionSummary> transactions{ get; set; }
        private List<TransactionSummary> MaxMinCategoryList { get; set; }

        private DataGridTextColumn firstData, SecondData, ThirdData;

        private readonly int userId;

        private void GenerateRaportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Plik PDF (*.pdf)|*.pdf|Plik Word (*.docx)|*.docx";

            if(saveFileDialog.ShowDialog()==DialogResult.OK)
            {
                string extension= System.IO.Path.GetExtension(saveFileDialog.FileName);
                if(extension ==".pdf")
                {
                    PdfDocument document = new PdfDocument();
                    document.Info.Title = "Raport";

                    PdfPage page = document.AddPage();

                    XGraphics gfx=XGraphics.FromPdfPage(page);
                    XFont font = new XFont("Arial", 10);

                    double yPosition = 20;
                    double xPosition = 40;

                    foreach(DataGridColumn column in Test.Columns)
                    {
                        gfx.DrawString(column.Header.ToString(),font,XBrushes.Black,new XPoint(xPosition,yPosition));
                        xPosition += 100;
                    }

                    yPosition += 20;

                    foreach (var item in Test.Items)
                    {
                        xPosition = 40;
                        foreach (var column in Test.Columns)
                        {
                            // Pobieramy wartość komórki
                            var cellValue = column.GetCellContent(item) as TextBlock;
                            if (cellValue != null)
                            {
                                gfx.DrawString(cellValue.Text, font, XBrushes.Black, new XPoint(xPosition, yPosition));
                            }
                            xPosition += 100; // Przesuwamy się w prawo po każdej kolumnie
                        }
                        yPosition += 20; // Przesuwamy się w dół po każdym wierszu
                    }

                    document.Save(saveFileDialog.FileName);
                    

                }
                else if(extension ==".docx") //Tworzy plik ale jest błąd
                {
                   using(WordprocessingDocument wordDocument=WordprocessingDocument.Create(saveFileDialog.FileName,DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
                   {
                        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                        mainPart.Document = new Document(new Body());

                        DocumentFormat.OpenXml.Drawing.Table table =new DocumentFormat.OpenXml.Drawing.Table();
                        Body body = mainPart.Document.Body;
                        body.Append(table);


                        var columns = Test.Columns.Select(c => c.Header.ToString()).ToList();
                        DocumentFormat.OpenXml.Drawing.TableRow headerRow = new DocumentFormat.OpenXml.Drawing.TableRow();

                        foreach(var column in columns)
                        {
                            DocumentFormat.OpenXml.Drawing.TableCell headerCell = new DocumentFormat.OpenXml.Drawing.TableCell(new DocumentFormat.OpenXml.Drawing.Paragraph(new DocumentFormat.OpenXml.Drawing.Run(new Text(column))));
                            headerRow.Append(headerCell);
                        }
                        table.Append(headerRow);

                        foreach(var item in Test.Items)
                        {
                            DocumentFormat.OpenXml.Drawing.TableRow row = new DocumentFormat.OpenXml.Drawing.TableRow();

                            if(item is TransactionSummary dataRow)
                            {
                                row.Append(new DocumentFormat.OpenXml.Drawing.TableCell(new DocumentFormat.OpenXml.Drawing.Paragraph(new DocumentFormat.OpenXml.Drawing.Run(new Text(dataRow.FirstData.ToString())))));
                                row.Append(new DocumentFormat.OpenXml.Drawing.TableCell(new DocumentFormat.OpenXml.Drawing.Paragraph(new DocumentFormat.OpenXml.Drawing.Run(new Text(dataRow.SecondData.ToString())))));
                                row.Append(new DocumentFormat.OpenXml.Drawing.TableCell(new DocumentFormat.OpenXml.Drawing.Paragraph(new DocumentFormat.OpenXml.Drawing.Run(new Text(dataRow.ThirdData.ToString())))));

                            }

                            table.Append(row);
                        }

                        mainPart.Document.Save();

                   }
                }
            }
        }

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
                Binding = new System.Windows.Data.Binding("FirstData")
            };

            SecondData = new DataGridTextColumn
            {
                Header = "Liczba Transakcji",
                Binding = new System.Windows.Data.Binding("SecondData")
            };

            ThirdData= new DataGridTextColumn
            {
                Header = "Suma",
                Binding = new System.Windows.Data.Binding("ThirdData")
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

            MaxMinCategory.Columns.Add(CategoryName);
            MaxMinCategory.Columns.Add (MaxValue);
            MaxMinCategory.Columns.Add(MinValue);

            MaxMinCategory.ItemsSource = MaxMinCategoryList;

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            System.Windows.Controls.ComboBox cmb = sender as System.Windows.Controls.ComboBox;   
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
