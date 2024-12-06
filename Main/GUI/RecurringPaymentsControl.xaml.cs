using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for RecurringPaymentsControl.xaml
    /// </summary>
    public partial class RecurringPaymentsControl : UserControl
    {
        private MainWindow mainWindow;
        private readonly int userId;
        private int userRole;
        private int familyId;
        public List<RecurringPayment> ActivePayments { get; set; }
        public List<RecurringPayment> InactivePayments { get; set; }
        public List<RecurringPayment> recurringPayments { get; set; }

        public RecurringPaymentsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();

            ActivePayments = new List<RecurringPayment>();
            InactivePayments = new List<RecurringPayment>();

            userRole = Service.GetRoleIDByUserID(userId);
            familyId = Service.GetFamilyIdByMemberId(userId);

            LoadPayments();
        }
        public void LoadPayments()
        {
            if (familyId < 0)
            {
                var userRecurringPayments = Service.GetRecurringPaymentsByUserId(userId);
                foreach (RecurringPayment payment in userRecurringPayments)
                {
                    if (payment.IsActive)
                    {
                        ActivePayments.Add(payment);
                    }
                    else if (!payment.IsActive)
                    {
                        InactivePayments.Add(payment);
                    }
                }
            }
            else
            {
                var familyRecurringPayments = Service.GetFamilyRecurringPayments(familyId);
                foreach (RecurringPayment payment in familyRecurringPayments)
                {
                    if (payment.IsActive)
                    {
                        ActivePayments.Add(payment);
                    }
                    else if (!payment.IsActive) 
                    {
                        InactivePayments.Add(payment);
                    }
                }
            }
            ActivePaymentsList.ItemsSource = ActivePayments;
            InactivePaymentsList.ItemsSource = InactivePayments;
        }
    }
}
