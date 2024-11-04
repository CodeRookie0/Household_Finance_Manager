using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<Button, bool> buttonActiveStates = new Dictionary<Button, bool>();

        public MainWindow()
        {
            InitializeComponent();

            buttonActiveStates[TransactionsButton] = false;
            buttonActiveStates[MyTransactionsButton] = false;
            buttonActiveStates[FamilyTransactionsButton] = false;
            buttonActiveStates[DashboardButton] = false;
            buttonActiveStates[StatisticsButton] = false;
            buttonActiveStates[GoalsButton] = false;
            buttonActiveStates[CategoriesButton] = false;
            buttonActiveStates[StoresButton] = false;
            buttonActiveStates[LimitsButton] = false;
            buttonActiveStates[RecurringPaymentsButton] = false;
            buttonActiveStates[FamilyMembersButton] = false;
            
            DashboardButton_Click(DashboardButton, null);
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        private void TransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(TransactionsBorder, TransactionsTextBlock, "../Resources/arrow_down_white.png");
            SubMenu.Visibility = SubMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            buttonActiveStates[TransactionsButton] = true;
            SelectedUserControlTextBlock.Text = TransactionsTextBlock.Text;
        }

        private void MyTransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonStyles();
            TransactionsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EB1B6"));
            TransactionsTextBlock.Foreground = Brushes.White;
            ArrowImage.Source = new BitmapImage(new Uri("../Resources/arrow_down_white.png", UriKind.Relative));

            MyTransactionsTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EB1B6"));
            buttonActiveStates[MyTransactionsButton] = true;
            SelectedUserControlTextBlock.Text =MyTransactionsTextBlock.Text;
        }

        private void FamilyTransactionsButton_Click(object sender, RoutedEventArgs e)
        {
            ResetButtonStyles();
            TransactionsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EB1B6"));
            TransactionsTextBlock.Foreground = Brushes.White;
            ArrowImage.Source = new BitmapImage(new Uri("../Resources/arrow_down_white.png", UriKind.Relative));

            FamilyTransactionsTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EB1B6"));
            buttonActiveStates[FamilyTransactionsButton] = true;
            SelectedUserControlTextBlock.Text =FamilyTransactionsTextBlock.Text;
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(DashboardBorder, DashboardTextBlock);
            buttonActiveStates[DashboardButton] = true;
            MainContentArea.Content = new DashboardControl();
            SelectedUserControlTextBlock.Text= DashboardTextBlock.Text;
        }
        private void FamilyMembersButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(FamilyMembersBorder, FamilyMembersTextBlock);
            buttonActiveStates[FamilyMembersButton] = true;
            MainContentArea.Content = new CreateFamilyPrompt();
            SelectedUserControlTextBlock.Text = FamilyMembersTextBlock.Text;
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(StatisticsBorder, StatisticsTextBlock);
            buttonActiveStates[StatisticsButton] = true;
            SelectedUserControlTextBlock.Text = StatisticsTextBlock.Text;
        }

        private void GoalsButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(GoalsBorder, GoalsTextBlock);
            buttonActiveStates[GoalsButton] = true;
            SelectedUserControlTextBlock.Text = GoalsTextBlock.Text;
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(CategoriesBorder, CategoriesTextBlock);
            buttonActiveStates[CategoriesButton] = true;
            SelectedUserControlTextBlock.Text = CategoriesTextBlock.Text;
        }

        private void StoresButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(StoresBorder, StoresTextBlock);
            buttonActiveStates[StoresButton] = true;
            SelectedUserControlTextBlock.Text = StoresTextBlock.Text;
        }

        private void RecurringPaymentsButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(RecurringPaymentsBorder, RecurringPaymentsTextBlock);
            buttonActiveStates[RecurringPaymentsButton] = true;
            SelectedUserControlTextBlock.Text = RecurringPaymentsTextBlock.Text;
        }

        private void LimitsButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightButton(LimitsBorder, LimitsTextBlock);
            buttonActiveStates[LimitsButton] = true;
            SelectedUserControlTextBlock.Text = LimitsTextBlock.Text;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {

            ExitTextBlock.Foreground = Brushes.Red;
            MessageBoxResult result = MessageBox.Show("Czy na pewno chcesz wyjść?", "Potwierdzenie wyjścia", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginControl loginControl = new LoginControl();
                loginControl.Show();
                this.Hide();
            }
            else
            {
                ExitTextBlock.Foreground = Brushes.Black;
            }
        }

        private void DashboardButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[DashboardButton])  
            {
                DashboardBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264EB1B6"));
            }
        }

        private void DashboardButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[DashboardButton])
            {
                DashboardBorder.Background = Brushes.White;
            }
        }

        private void TransactionsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[TransactionsButton])  
            {
                TransactionsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264EB1B6"));
            }
        }

        private void TransactionsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[TransactionsButton])
            {
                TransactionsBorder.Background = Brushes.White;
            }
        }

        private void MyTransactionsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[MyTransactionsButton])  
            {
                MyTransactionsTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EB1B6"));
            }
        }

        private void MyTransactionsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[MyTransactionsButton]) 
            {
                MyTransactionsTextBlock.Foreground = Brushes.Black;
            }
        }

        private void FamilyTransactionsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[FamilyTransactionsButton])  
            {
                FamilyTransactionsTextBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EB1B6"));
            }
        }

        private void FamilyTransactionsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[FamilyTransactionsButton])
            {
                FamilyTransactionsTextBlock.Foreground = Brushes.Black;
            }
        }

        private void StatisticsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[StatisticsButton])  
            {
                StatisticsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264EB1B6"));
            }
        }

        private void StatisticsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[StatisticsButton])
            {
                StatisticsBorder.Background = Brushes.White;
            }
        }

        private void GoalsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[GoalsButton])  
            {
                GoalsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264EB1B6"));
            }
        }

        private void GoalsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[GoalsButton]) 
            {
                GoalsBorder.Background = Brushes.White;
            }
        }

        private void CategoriesButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[CategoriesButton])  
            {
                CategoriesBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264EB1B6"));
            }
        }

        private void CategoriesButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[CategoriesButton]) 
            {
                CategoriesBorder.Background = Brushes.White;
            }
        }

        private void StoresButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[StoresButton])  
            {
                StoresBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264EB1B6"));
            }
        }

        private void StoresButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[StoresButton]) 
            {
                StoresBorder.Background = Brushes.White;
            }
        }

        private void ExitButtonButton_MouseEnter(object sender, MouseEventArgs e)
        {
                ExitTextBlock.Foreground = Brushes.Red;
        }

        private void ExitButtonButton_MouseLeave(object sender, MouseEventArgs e)
        {
                ExitTextBlock.Foreground = Brushes.Black;
        }

        private void LimitsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[LimitsButton])
            {
                LimitsBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264EB1B6"));
            }
        }

        private void LimitsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[LimitsButton])
            {
                LimitsBorder.Background = Brushes.White;
            }
        }

        private void RecurringPaymentsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[RecurringPaymentsButton])
            {
                RecurringPaymentsBorder.Background =  new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264EB1B6"));
            }
        }

        private void RecurringPaymentsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[RecurringPaymentsButton])
            {
                RecurringPaymentsBorder.Background = Brushes.White;
            }
        }

        private void FamilyMembersButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[FamilyMembersButton])
            {
                FamilyMembersBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#264EB1B6"));
            }
        }

        private void FamilyMembersButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!buttonActiveStates[FamilyMembersButton])
            {
                FamilyMembersBorder.Background = Brushes.White;
            }
        }


        private void HighlightButton(Border border)
        {
            ResetButtonStyles();

            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EB1B6"));
        }

        private void HighlightButton(Border border, TextBlock textBlock)
        {
            ResetButtonStyles();

            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EB1B6"));
            textBlock.Foreground = Brushes.White;

            SubMenu.Visibility = Visibility.Collapsed;
        }
        private void HighlightButton(Border border, TextBlock textBlock, string arrowImageSource)
        {
            ResetButtonStyles();

            border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EB1B6"));
            textBlock.Foreground = Brushes.White;
            ArrowImage.Source = new BitmapImage(new Uri(arrowImageSource ?? "../Resources/arrow_down_white.png", UriKind.Relative));
        }

        private void ResetButtonStyles()
        {
            foreach (var button in buttonActiveStates.Keys.ToList())
            {
                buttonActiveStates[button] = false;
            }

            TransactionsBorder.Background = Brushes.White;
            TransactionsTextBlock.Foreground = Brushes.Black;

            MyTransactionsTextBlock.Foreground = Brushes.Black;
            FamilyTransactionsTextBlock.Foreground = Brushes.Black;

            DashboardBorder.Background = Brushes.White;
            DashboardTextBlock.Foreground = Brushes.Black;

            StatisticsBorder.Background = Brushes.White;
            StatisticsTextBlock.Foreground = Brushes.Black;

            GoalsBorder.Background = Brushes.White;
            GoalsTextBlock.Foreground = Brushes.Black;

            CategoriesBorder.Background = Brushes.White;
            CategoriesTextBlock.Foreground = Brushes.Black;

            StoresBorder.Background = Brushes.White;
            StoresTextBlock.Foreground = Brushes.Black;

            LimitsBorder.Background = Brushes.White;
            LimitsTextBlock.Foreground = Brushes.Black;

            RecurringPaymentsBorder.Background = Brushes.White;
            RecurringPaymentsTextBlock.Foreground = Brushes.Black;

            FamilyMembersBorder.Background = Brushes.White;
            FamilyMembersTextBlock.Foreground = Brushes.Black;

            ExitTextBlock.Foreground = Brushes.Black;

            ArrowImage.Source = new BitmapImage(new Uri("../Resources/arrow_down_black.png", UriKind.Relative));
        }
    }
}
