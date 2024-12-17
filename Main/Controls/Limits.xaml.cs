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
        private LimitsModel thisLimits;
        public Limits(LimitsModel argModel)
        { 
            InitializeComponent();
            thisLimits = argModel;

            CategoryName.Text ="Kategoria: "+Service.GetCategoryNameByCategoryID(argModel.CategoryId);
            FrequencyUser.Text="Częstotliwość: "+Service.GetFrequencyNameByFrequencyID(argModel.FrequencyId);
            AddUser.Text = "Przypisany: " + Service.GetUserNameByUserID(argModel.UserId);
            LabelTextAmountLimit.Text=argModel.LimitAmount.ToString()+" zł";



        }

        public event EventHandler RefreshData;

        private void Button_Click(object sender, RoutedEventArgs e) //Edit Limits
        {
            EditLimits editLimits = new EditLimits(thisLimits);
            editLimits.Closed += (s, args) => RefreshData?.Invoke(this, EventArgs.Empty);
            editLimits.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) //Delete Limits
        {

        }
    }
}
