using Main.Controls;
using Main.Logic;
using Main.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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

        

        public LimitsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();


            loadLimits();

            /*Limits test=new Limits();

            Grid.SetRow(test, 1);
            Grid.SetColumn(test, 1);

            LimitsGrid.Children.Add(test);*/

        }

        private void loadLimits()
        {
            int FamilyId = Service.GetFamilyIdByMemberId(userId);
            DBSqlite dBSqlite = new DBSqlite();
            DataTable answer = dBSqlite.ExecuteQuery("SELECT LimitID,FamilyID,UserID,CategoryID,LimitAmount,FrequencyID,IsFamilyWide FROM Limits WHERE UserID=@MyUserId OR FamilyID=@MyFamilyId",
                new Microsoft.Data.Sqlite.SqliteParameter("@MyUserId", userId),
                new SqliteParameter("@MyFamilyId", FamilyId));

            int rowNumber = 0;
            int Numbercolumn = 0;
            foreach (DataRow row in answer.Rows)
            {
                if (Numbercolumn > 2)
                {
                    Numbercolumn = 0;
                    rowNumber++;
                }

                LimitsModel model = new LimitsModel();
                model.LimitId = int.Parse(row[0].ToString());
                model.FamilyId = int.Parse(row[1].ToString());
                model.UserId = int.Parse(row[2].ToString());
                model.CategoryId = int.Parse(row[3].ToString());
                model.LimitAmount = double.Parse(row[4].ToString());
                model.FrequencyId = int.Parse(row[5].ToString());
                model.IsFamilyWide = int.Parse(row[6].ToString());

                Limits tmpControl = new Limits(model);
                tmpControl.RefreshData += RunEventRefresh;
                Grid.SetRow(tmpControl, rowNumber);
                Grid.SetColumn(tmpControl, Numbercolumn);

                LimitsGrid.Children.Add(tmpControl);


                Numbercolumn++;
            }
        }

        private void RunEventRefresh(object sender, EventArgs e)
        {
            loadLimits();
        }

        private void AddLimitButton_Click(object sender, RoutedEventArgs e)
        {
            AddLimits limitsDialog = new AddLimits(userId);
            limitsDialog.Closed += (s, args) => loadLimits();
            limitsDialog.Show();
        }
    }
}
