﻿using System;
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
using OxyPlot;
using OxyPlot.Series;


namespace Main.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy StatisticControl.xaml
    /// </summary>
    public partial class StatisticControl : UserControl
    {
      
        public StatisticControl()
        {
            InitializeComponent();
            LoadPlot();
        }

        private void LoadPlot()
        {
            var model = new PlotModel { Title = "Budżet Domowy" };

            var series = new BarSeries
            {
                Title = "Wydatki",
                ItemsSource = new[]
                {
                    new BarItem(500),  // Wynajem
                    new BarItem(200),  // Żywność
                    new BarItem(150),  // Transport
                    new BarItem(100),  // Rozrywka
                    new BarItem(50)    // Inne
                    
                }
            };

            model.Series.Add(series);
          

            plotView.Model = model;
        }
    }
}
