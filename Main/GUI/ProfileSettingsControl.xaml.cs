using Main.Logic;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for ProfileSettingsControl.xaml
    /// </summary>
    public partial class ProfileSettingsControl : Window
    {
        private readonly int LocalId;
        private readonly DataRow tmp;
        public ProfileSettingsControl(int ArgId)
        {
            this.LocalId = ArgId;
            InitializeComponent();

            DBSqlite dBSqlite = new DBSqlite();
            var wynik = dBSqlite.ExecuteQuery("SELECT UserName,Email,PasswordHash,Salt,CreatedAt FROM Users WHERE UserId=@Id", new Microsoft.Data.Sqlite.SqliteParameter("@Id", ArgId));

            tmp = wynik.Rows[0];
            UserNameTextBox.Text = tmp["UserName"].ToString();
            EmailTextBox.Text = tmp["Email"].ToString(); DateTime createdAt = DateTime.Parse(tmp["CreatedAt"].ToString());
            CreatedAtTextBox.Text = createdAt.ToString("D");
            PasswordTextBox.Text = "* * * * * * * * * * * * * *";
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Close();
        //}

        //private void Button_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    ColorAnimation colorAnimation = new ColorAnimation
        //    {
        //        To = Colors.White,
        //        Duration = TimeSpan.FromSeconds(0.2)
        //    };
        //    ColorAnimation foregroundAnimation = new ColorAnimation
        //    {
        //        To = (Color)ColorConverter.ConvertFromString("#3aa9ad"),
        //        Duration = TimeSpan.FromSeconds(0.2)
        //    };

        //    SolidColorBrush backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
        //    SolidColorBrush foregroundBrush = new SolidColorBrush(Colors.White);
        //    CloseButton.Background = backgroundBrush;
        //    CloseButton.Foreground = foregroundBrush;
        //    backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        //    foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        //}

        //private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    ColorAnimation colorAnimation = new ColorAnimation
        //    {
        //        To = Color.FromRgb(58, 169, 173),
        //        Duration = TimeSpan.FromSeconds(0.2)
        //    };
        //    ColorAnimation foregroundAnimation = new ColorAnimation
        //    {
        //        To = Colors.White,
        //        Duration = TimeSpan.FromSeconds(0.2)
        //    };

        //    SolidColorBrush backgroundBrush = new SolidColorBrush(Colors.White);
        //    SolidColorBrush foregroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
        //    CloseButton.Background = backgroundBrush;
        //    CloseButton.Foreground = foregroundBrush;
        //    backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        //    foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        //}

        //private void ChangeDataButton_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    ColorAnimation colorAnimation = new ColorAnimation
        //    {
        //        To = Colors.White,
        //        Duration = TimeSpan.FromSeconds(0.2)
        //    };
        //    ColorAnimation foregroundAnimation = new ColorAnimation
        //    {
        //        To = (Color)ColorConverter.ConvertFromString("#3aa9ad"),
        //        Duration = TimeSpan.FromSeconds(0.2)
        //    };

        //    SolidColorBrush backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
        //    SolidColorBrush foregroundBrush = new SolidColorBrush(Colors.White);
        //    backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        //    foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        //}

        //private void ChangeDataButton_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    ColorAnimation colorAnimation = new ColorAnimation
        //    {
        //        To = Color.FromRgb(58, 169, 173),
        //        Duration = TimeSpan.FromSeconds(0.2)
        //    };
        //    ColorAnimation foregroundAnimation = new ColorAnimation
        //    {
        //        To = Colors.White,
        //        Duration = TimeSpan.FromSeconds(0.2)
        //    };

        //    SolidColorBrush backgroundBrush = new SolidColorBrush(Colors.White);
        //    SolidColorBrush foregroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
        //    backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        //    foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        //}

        //private void ChangeDataButton_Click(object sender, RoutedEventArgs e)
        //{
        //    SqliteParameter[] tab =
        //    {
        //        new SqliteParameter("@UserName",(UserNameTextBox.Text.Length>0)?UserNameTextBox.Text.ToString():tmp["UserName"].ToString()),
        //        new SqliteParameter("@Email",(EmailTextBox.Text.Length>0)?EmailTextBox.Text.ToString():tmp["Email"].ToString()),
        //        new SqliteParameter("@UserId",LocalId)
        //    };

        //    DBSqlite dBSqlite = new DBSqlite();
        //    dBSqlite.ExecuteNonQuery("UPDATE Users SET UserName=@UserName , Email=@Email WHERE UserId=@UserId", tab);
        //}


        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width < 550)
            {
                this.Width = 550;
            }
            UserNameTextBox.IsEnabled = true;
            EmailTextBox.IsEnabled = true;
            EditUserDataButtonsPanel.Visibility = Visibility.Visible;
        }

        private void ChangePasswordDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width < 550)
            {
                this.Width = 550;
            }
            PasswordTextBox.IsEnabled = true;
            ConfirmPasswordTextBox.IsEnabled = true;
            PasswordTextBox.Text = "";
            ConfirmPasswordStackPanel.Visibility = Visibility.Visible;
            EditPasswordButtonsPanel.Visibility= Visibility.Visible;
        }

        private void CancelUserDataChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width >= 550 && EditPasswordButtonsPanel.Visibility!=Visibility.Visible)
            {
                this.Width = 410;
            }
            UserNameTextBox.IsEnabled = false;
            EmailTextBox.IsEnabled = false;
            EditUserDataButtonsPanel.Visibility = Visibility.Collapsed;
        }

        private void SaveUserDataChangesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelPasswordChangesButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Width >= 550 && EditUserDataButtonsPanel.Visibility != Visibility.Visible)
            {
                this.Width = 410;
            }
            PasswordTextBox.IsEnabled = false;
            PasswordTextBox.Text = "* * * * * * * * * * * * * *";
            ConfirmPasswordStackPanel.Visibility = Visibility.Collapsed;
            EditPasswordButtonsPanel.Visibility = Visibility.Collapsed;
        }

        private void SavePasswordChangesButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = "* * * * * * * * * * * * * *";
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
           this.Close();
        }
    }
}
