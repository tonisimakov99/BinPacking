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

        private PackingAlgorithm shelfAlgorithm { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            foreach (var value in Enum.GetValues<ShelfMode>())
                this.algorithmComboBox.Items.Add(value);
            this.algorithmComboBox.SelectedIndex = 0;
            ResetState();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var size = new System.Numerics.Vector2(Random.Shared.Next(10, int.Parse(MaxWidth.Text)), Random.Shared.Next(10, int.Parse(MaxHeight.Text)));
            Info.Content = size;
            var result = shelfAlgorithm.Insert(size);
            if (result != null)
            {
                Info.Foreground = Brushes.Black;
                var rect = new Rectangle();
                Canvas.SetLeft(rect, result.Value.Position.X);
                Canvas.SetTop(rect, result.Value.Position.Y);
                if (result.Value.Rotate)
                    rect.Stroke = new SolidColorBrush(Colors.Green);
                if (result.Value.Rotate)
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

            shelfAlgorithm = new ShelfBestFit(new System.Numerics.Vector2(size, size), this.Rotate.IsChecked.Value, (ShelfMode)algorithmComboBox.SelectedValue);
            var rect = new Rectangle();
            rect.Width = size;
            rect.Height = size;
            rect.Stroke = new SolidColorBrush(Colors.Black);
            RectsCanv.Children.Add(rect);
        }
    }
}
