using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RectangleBinPacking.DrawerDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Algorithm<int> algorithm { get; set; }
        private int index;
        public MainWindow()
        {
            InitializeComponent();

            var algoTypes = Assembly.GetAssembly(typeof(Algorithm<int>)).GetTypes().Where(t => t.BaseType.Name.Contains("Algorithm"));

            foreach (var value in algoTypes)
                this.algorithmComboBox.Items.Add(value);
            this.algorithmComboBox.SelectedIndex = 1;
            ResetState();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var size = new System.Numerics.Vector2(Random.Shared.Next(10, int.Parse(MaxWidth.Text)), Random.Shared.Next(10, int.Parse(MaxHeight.Text)));
            Info.Content = size;
            var result = algorithm.Insert(index, (int)size.X, (int)size.Y);
            if (result != null)
            {
                Info.Foreground = Brushes.Black;
                var rect = new Rectangle();
                Canvas.SetLeft(rect, result.X);
                Canvas.SetTop(rect, result.Y);
                if (result.Rotate)
                    rect.Stroke = new SolidColorBrush(Colors.Green);
                if (result.Rotate)
                {
                    rect.Width = size.Y;
                    rect.Height = size.X;
                }
                else
                {
                    rect.Width = size.X;
                    rect.Height = size.Y;
                }
                rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255)));
                RectsCanv.Children.Add(rect);
            }
            else
            {
                Info.Foreground = Brushes.Red;
            }
        }

        private Algorithm<int> GetAlgo(Type type, int size)
        {
            if (type.Name == typeof(MaxRectsBinPack<int>).Name)
            {
                return new MaxRectsBinPack<int>(size, size, FreeRectChoiceHeuristic.RectBestAreaFit, true);
            }
            else if (type.Name == typeof(ShelfBinPack<int>).Name)
            {
                return new ShelfBinPack<int>(size, size, false, ShelfChoiceHeuristic.ShelfNextFit);
            }
            throw new Exception("Not supported");
        }


        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetState();
        }
        private void ResetState()
        {
            var size = int.Parse(this.Size.Text);
            RectsCanv.Children.Clear();
            algorithm = GetAlgo((Type)algorithmComboBox.SelectedItem, size);

            var rect = new Rectangle();
            rect.Width = size;
            rect.Height = size;
            rect.Stroke = new SolidColorBrush(Colors.Black);
            RectsCanv.Children.Add(rect);
        }
    }
}
