using Main.GUI;
using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
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
            SpentAmountTextBlock.Text = "60.00 zł";
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

        }
    }
}
