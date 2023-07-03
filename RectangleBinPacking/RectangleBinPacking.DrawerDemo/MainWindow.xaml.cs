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
        private IEnumerable<Type> algorithmTypes;

        private ShelfAlgorithm shelfAlgorithm { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            var shelfType = typeof(ShelfAlgorithm);
            algorithmTypes = Assembly.GetAssembly(shelfType).GetExportedTypes().Where(t => t.BaseType.Name == shelfType.Name);
            foreach (var algorithmType in algorithmTypes)
                this.algorithmComboBox.Items.Add(algorithmType.Name);
            this.algorithmComboBox.SelectedIndex = 0;
            ResetState();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var size = new System.Numerics.Vector2(Random.Shared.Next(10, 30), Random.Shared.Next(10, 30));
            Info.Content = size;
            var result = shelfAlgorithm.Insert(size);
            if (result != null)
            {
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

        }


        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            ResetState();
        }
        private void ResetState()
        {
            var size = int.Parse(this.Size.Text);
            RectsCanv.Children.Clear();

            var type = algorithmTypes.FirstOrDefault(t => t.Name == (string)algorithmComboBox.SelectedItem);
            shelfAlgorithm = (ShelfAlgorithm)Activator.CreateInstance(type, new System.Numerics.Vector2(size, size), this.Rotate.IsChecked.Value);
            var rect = new Rectangle();
            rect.Width = size;
            rect.Height = size;
            rect.Stroke = new SolidColorBrush(Colors.Black);
            RectsCanv.Children.Add(rect);
        }
    }
}
