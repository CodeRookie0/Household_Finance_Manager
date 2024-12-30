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
            LimitsGrid.UpdateLayout();
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
    }
}
