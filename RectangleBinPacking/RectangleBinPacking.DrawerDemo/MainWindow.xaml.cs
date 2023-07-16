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
            //foreach (var value in Enum.GetValues<ShelfMode>())
            //    this.algorithmComboBox.Items.Add(value);
            //this.algorithmComboBox.SelectedIndex = 0;
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


        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetState();
        }
        private void ResetState()
        {
            var size = int.Parse(this.Size.Text);
            RectsCanv.Children.Clear();

            algorithm = new  ShelfBinPack<int>(size, size, false, ShelfChoiceHeuristic.ShelfNextFit);
            var rect = new Rectangle();
            rect.Width = size;
            rect.Height = size;
            rect.Stroke = new SolidColorBrush(Colors.Black);
            RectsCanv.Children.Add(rect);
        }
    }
}
