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
    /// Logika interakcji dla klasy SettingProifl.xaml
    /// </summary>
    public partial class SettingProifl : Window
    {
        private readonly int LocalId;
        private readonly DataRow tmp;
        public SettingProifl(int ArgId)
        {
            this.LocalId = ArgId;
            InitializeComponent();

            DBSqlite dBSqlite = new DBSqlite();
            var wynik = dBSqlite.ExecuteQuery("SELECT UserName,Email,PasswordHash,Salt FROM Users WHERE UserId=@Id", new Microsoft.Data.Sqlite.SqliteParameter("@Id", ArgId));

            tmp = wynik.Rows[0];
            InputUserName.Text = tmp["UserName"].ToString();
            InputEmail.Text = tmp["Email"].ToString();


        }
 

        private void Label_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = -5, // Zmień na wartość, na którą chcesz przesunąć tekst
                Duration = TimeSpan.FromSeconds(0.5),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true

            };

            // Uruchomienie animacji na właściwości TranslateTransform
            TranslateTransform transform = new TranslateTransform();
            MovingLabel.RenderTransform = transform;
            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void Label_MouseLeave(object sender, MouseEventArgs e)
        {
            MovingLabel.RenderTransform = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation
            {
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.2)
            };
            ColorAnimation foregroundAnimation = new ColorAnimation
            {
                To = (Color)ColorConverter.ConvertFromString("#3aa9ad"),
                Duration = TimeSpan.FromSeconds(0.2)
            };

            SolidColorBrush backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
            SolidColorBrush foregroundBrush = new SolidColorBrush(Colors.White);
            CloseButton.Background = backgroundBrush;
            CloseButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation
            {
                To = Color.FromRgb(58, 169, 173),
                Duration = TimeSpan.FromSeconds(0.2)
            };
            ColorAnimation foregroundAnimation = new ColorAnimation
            {
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.2)
            };

            SolidColorBrush backgroundBrush = new SolidColorBrush(Colors.White);
            SolidColorBrush foregroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
            CloseButton.Background = backgroundBrush;
            CloseButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }

        private void ChangeDataButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation
            {
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.2)
            };
            ColorAnimation foregroundAnimation = new ColorAnimation
            {
                To = (Color)ColorConverter.ConvertFromString("#3aa9ad"),
                Duration = TimeSpan.FromSeconds(0.2)
            };

            SolidColorBrush backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
            SolidColorBrush foregroundBrush = new SolidColorBrush(Colors.White);
            ChangeDataButton.Background = backgroundBrush;
            ChangeDataButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }

        private void ChangeDataButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ColorAnimation colorAnimation = new ColorAnimation
            {
                To = Color.FromRgb(58, 169, 173),
                Duration = TimeSpan.FromSeconds(0.2)
            };
            ColorAnimation foregroundAnimation = new ColorAnimation
            {
                To = Colors.White,
                Duration = TimeSpan.FromSeconds(0.2)
            };

            SolidColorBrush backgroundBrush = new SolidColorBrush(Colors.White);
            SolidColorBrush foregroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3aa9ad"));
            ChangeDataButton.Background = backgroundBrush;
            ChangeDataButton.Foreground = foregroundBrush;
            backgroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            foregroundBrush.BeginAnimation(SolidColorBrush.ColorProperty, foregroundAnimation);
        }

        private void MovingLabel1_MouseEnter(object sender, MouseEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = 0,
                To = -5, // Zmień na wartość, na którą chcesz przesunąć tekst
                Duration = TimeSpan.FromSeconds(0.5),
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true

            };

            // Uruchomienie animacji na właściwości TranslateTransform
            TranslateTransform transform = new TranslateTransform();
            MovingLabel1.RenderTransform = transform;
            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void MovingLabel1_MouseLeave(object sender, MouseEventArgs e)
        {
            MovingLabel1.RenderTransform = null;
        }

        private void ChangeDataButton_Click(object sender, RoutedEventArgs e)
        {
            SqliteParameter[] tab =
            {
                new SqliteParameter("@UserName",(InputUserName.Text.Length>0)?InputUserName.Text.ToString():tmp["UserName"].ToString()),
                new SqliteParameter("@Email",(InputEmail.Text.Length>0)?InputEmail.Text.ToString():tmp["Email"].ToString()),
                new SqliteParameter("@UserId",LocalId)
            };

            DBSqlite dBSqlite = new DBSqlite();
            dBSqlite.ExecuteNonQuery("UPDATE Users SET UserName=@UserName , Email=@Email WHERE UserId=@UserId", tab);
        }
    }
}
