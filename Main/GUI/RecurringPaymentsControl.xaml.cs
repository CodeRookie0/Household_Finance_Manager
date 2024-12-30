using Main.Logic;
using Main.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
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
        public ObservableCollection<RecurringPayment> ActivePayments { get; set; }
        public ObservableCollection<RecurringPayment> InactivePayments { get; set; }

        public RecurringPaymentsControl(int loggedInUserId, MainWindow mainWindow)
        {
            userId = loggedInUserId;
            this.mainWindow = mainWindow;
            InitializeComponent();

            ActivePayments = new ObservableCollection<RecurringPayment>();
            InactivePayments = new ObservableCollection<RecurringPayment>();

            userRole = Service.GetRoleIDByUserID(userId);
            familyId = Service.GetFamilyIdByMemberId(userId);

            if (userRole == 3)
            {
                AddRecurringPaymentButton.Visibility=Visibility.Collapsed;
            }

            LoadPayments();
        }
        public void LoadPayments()
        {
            ActivePayments.Clear();
            InactivePayments.Clear();

            if (familyId < 0 || userRole==3)
            {
                var userRecurringPayments = Service.GetRecurringPaymentsByUserId(userId);
                foreach (RecurringPayment payment in userRecurringPayments)
                {
                    if (payment.IsActive)
                    {
                        payment.CanEditAndDeactivate = false;
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
                        if(userRole == 2 && payment.CreatedByUserID!= userId && Service.GetRoleIDByUserID(payment.UserID)!=3)
                        {
                            payment.CanEditAndDeactivate = false;
                        }

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

        private void AddRecurringPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            AddRecurringPaymentsControl addRecurringPayment=new AddRecurringPaymentsControl(userId);
            addRecurringPayment.Closed += (s, args) => LoadPayments();
            addRecurringPayment.Show();
            
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button=sender as Button;
            RecurringPayment obj=button.DataContext as RecurringPayment;
            if(obj!=null)
            {
                EditRecuringPayments editRecuring=new EditRecuringPayments(userId, obj);

                editRecuring.Closed += (s, args) => LoadPayments();

                editRecuring.Show();
             
            }
        }

        private void DeactivatePeymentButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            RecurringPayment obj=button.DataContext as RecurringPayment;
            if(obj!=null) 
            {
                if (MessageBox.Show("Czy chcesz zdeaktywować płatność " + obj.RecurringPaymentName + " ?", "Komunikat", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    DBSqlite dBSqlite = new DBSqlite();
                    int answer = dBSqlite.ExecuteNonQuery("UPDATE RecurringPayments SET IsActive = 0 WHERE RecurringPaymentID = @RecurringPaymentID",
                        new Microsoft.Data.Sqlite.SqliteParameter("@RecurringPaymentID", obj.RecurringPaymentID));
                    if (answer > 0)
                    {
                        MessageBox.Show("Płatność " + obj.RecurringPaymentName + " została zdeaktywowana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadPayments();
                    }
                    else
                    {
                        MessageBox.Show("Płatność " + obj.RecurringPaymentName + " nie została zdeaktywowana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                }
            }        }

        private void ActivatePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            RecurringPayment obj = button.DataContext as RecurringPayment;
            if (obj != null)
            {
                if (MessageBox.Show("Czy chcesz ponownie aktywować płatność " + obj.RecurringPaymentName + " ?", "Komunikat", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (Service.ActivateRecurringPayment(obj.RecurringPaymentID))
                    {
                        MessageBox.Show("Płatność " + obj.RecurringPaymentName + " została aktywowana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadPayments();
                    }
                    else
                    {
                        MessageBox.Show("Płatność " + obj.RecurringPaymentName + " nie została aktywowana", "Komunikat", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                }
            }
        }
    }
}
