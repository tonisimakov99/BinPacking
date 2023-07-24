using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
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
            Rects = new ObservableCollection<RectSize>();

            InitializeComponent();

            Rects.Add(new RectSize()
            {
                Width = 100,
                Height = 100,
            });
            Rects.Add(new RectSize()
            {
                Width = 150,
                Height = 150,
            });
            Rects.Add(new RectSize()
            {
                Width = 250,
                Height = 50,
            });
            Rects.Add(new RectSize()
            {
                Width = 100,
                Height = 50,
            });
            Rects.Add(new RectSize()
            {
                Width = 100,
                Height = 100,
            });
            Rects.Add(new RectSize()
            {
                Width = 100,
                Height = 150,
            });
            Rects.Add(new RectSize()
            {
                Width = 100,
                Height = 120,
            });

            var algoTypes = Assembly.GetAssembly(typeof(Algorithm<int>)).GetTypes().Where(t => t.BaseType.Name.Contains("Algorithm"));

            foreach (var value in algoTypes)
                this.algorithmComboBox.Items.Add(value);
            this.algorithmComboBox.SelectedIndex = 1;
            ResetState();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var size = new System.Numerics.Vector2(Random.Shared.Next(10, int.Parse(MaxWidth.Text)), Random.Shared.Next(10, int.Parse(MaxHeight.Text)));
            AddItem(size);

        }
        private void AddItem(Vector2 size)
        {
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

        private void SetList_Click(object sender, RoutedEventArgs e)
        {
            ResetState();
            foreach (var item in Rects)
            {
                AddItem(new Vector2(item.Width, item.Height));
            }
        }

        public class RectSize
        {
            public int Height { get; set; }
            public int Width { get; set; }
        }

        public ObservableCollection<RectSize> Rects { get; set; }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Rects.Add(new RectSize()
            {
                Height = 0,
                Width = 0,
            });
        }

        private void Rem_Click(object sender, RoutedEventArgs e)
        {
            Rects.RemoveAt(Rects.Count - 1);
        }
    }
}
