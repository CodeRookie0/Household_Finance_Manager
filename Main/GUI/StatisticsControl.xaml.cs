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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;

namespace Main.GUI
{
    /// <summary>
    /// Interaction logic for StatisticsControl.xaml
    /// </summary>
    public partial class StatisticsControl : UserControl
    {
        public StatisticsControl()
        {
            InitializeComponent();
            LoadPlot();
            LoadPlot2();
        }

        //private void LoadPlot()
        //{
        //    var model = new PlotModel { Title = "Budżet Domowy" };

        //    var series = new BarSeries
        //    {
        //        Title = "Wydatki",
        //        ItemsSource = new[]
        //        {
        //            new BarItem(500),  // Wynajem
        //            new BarItem(200),  // Żywność
        //            new BarItem(150),  // Transport
        //            new BarItem(100),  // Rozrywka
        //            new BarItem(50)    // Inne
        //        }
        //    };

        //    model.Series.Add(series);
        //    plotView.Model = model;
        //}

        private void LoadPlot()
        {
            var model = new PlotModel
            {
                Title = "Przychody i Wydatki za ostatnie 6 miesięcy"
            };

            // Create and configure the Y-axis (left axis)
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -1,
                Maximum = 71,
                Title = "Kwota"
            };
            model.Axes.Add(yAxis);

            // Create and configure the X-axis (bottom axis)
            var xAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Miesiące"
            };
            xAxis.Labels.AddRange(new[] { "Lis", "Paź", "Wrz", "Sie", "Lip", "Cze" });
            model.Axes.Add(xAxis);
            var s1 = new LineSeries
            {
                Title = "Przychody",
                Color = OxyColors.SkyBlue,
                MarkerType = MarkerType.Circle,
                MarkerSize = 6,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.SkyBlue,
                MarkerStrokeThickness = 1.5,
                TrackerFormatString = "{0}\n{1}: {2}\n{3}: {4:C2}"
            };
            s1.Points.Add(new DataPoint(0, 10));
            s1.Points.Add(new DataPoint(1, 40));
            s1.Points.Add(new DataPoint(2, 20));
            s1.Points.Add(new DataPoint(3, 30));
            s1.Points.Add(new DataPoint(4, 20));
            s1.Points.Add(new DataPoint(5, 30));
            model.Series.Add(s1);

            var s2 = new LineSeries
            {
                Title = "Wydatki",
                Color = OxyColors.Teal,
                MarkerType = MarkerType.Diamond,
                MarkerSize = 6,
                MarkerStroke = OxyColors.White,
                MarkerFill = OxyColors.Teal,
                MarkerStrokeThickness = 1.5,
                TrackerFormatString = "{0}\n{1}: {2}\n{3}: -{4:C2}"
            };
            s2.Points.Add(new DataPoint(0, 4));
            s2.Points.Add(new DataPoint(1, 32));
            s2.Points.Add(new DataPoint(2, 14.00));
            s2.Points.Add(new DataPoint(3, 20.99));
            s2.Points.Add(new DataPoint(4, 4));
            s2.Points.Add(new DataPoint(5, 32));
            model.Series.Add(s2);
            plotView.Model = model;
        }

        private void LoadPlot2()
        {
            var model = new PlotModel
            {
                Title = "Przychody i Wydatki za ostatnie 6 miesięcy",
            };

            var s1 = new BarSeries { Title = "Category 1", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.SkyBlue };
            s1.Items.Add(new BarItem { Value = 25 });
            s1.Items.Add(new BarItem { Value = 137 });
            s1.Items.Add(new BarItem { Value = 18 });
            s1.Items.Add(new BarItem { Value = 40 });

            var s2 = new BarSeries { Title = "Category 2", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.Teal };
            s2.Items.Add(new BarItem { Value = 42 });
            s2.Items.Add(new BarItem { Value = 64 });
            s2.Items.Add(new BarItem { Value = 120 });
            s2.Items.Add(new BarItem { Value = 26 });

            var s3 = new BarSeries { Title = "Category 3", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.MediumTurquoise };
            s3.Items.Add(new BarItem { Value = 32 });
            s3.Items.Add(new BarItem { Value = 34 });
            s3.Items.Add(new BarItem { Value = 20 });
            s3.Items.Add(new BarItem { Value = 36 });

            var s4 = new BarSeries { Title = "Category 4", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.LightCyan };
            s4.Items.Add(new BarItem { Value = 132 });
            s4.Items.Add(new BarItem { Value = 134 });
            s4.Items.Add(new BarItem { Value = 120 });
            s4.Items.Add(new BarItem { Value = 236 });

            var s5 = new BarSeries { Title = "Category 5", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.DarkTurquoise };
            s5.Items.Add(new BarItem { Value = 132 });
            s5.Items.Add(new BarItem { Value = 134 });
            s5.Items.Add(new BarItem { Value = 120 });
            s5.Items.Add(new BarItem { Value = 236 });

            var s6 = new BarSeries { Title = "Category 6", StrokeColor = OxyColors.Black, StrokeThickness = 1, IsStacked = true, FillColor = OxyColors.LightSkyBlue };
            s6.Items.Add(new BarItem { Value = 132 });
            s6.Items.Add(new BarItem { Value = 134 });
            s6.Items.Add(new BarItem { Value = 120 });
            s6.Items.Add(new BarItem { Value = 236 });

            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
            categoryAxis.Labels.Add("Category A");
            categoryAxis.Labels.Add("Category B");
            categoryAxis.Labels.Add("Category C");
            categoryAxis.Labels.Add("Category D");
            var valueAxis = new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, MaximumPadding = 0.06, AbsoluteMinimum = 0 };
            model.Series.Add(s6);
            model.Series.Add(s3);
            model.Series.Add(s4);
            model.Series.Add(s5);
            model.Series.Add(s1);
            model.Series.Add(s2);
            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            // Set the model to plotView
            plotView1.Model = model;
        }
    }
}
