using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using FontAtlasBuilding;
using Serilog;
using Serilog.Extensions.Logging;

namespace RectangleBinPacking.FontDrawer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FontAtlasBuilder atlasBuilder;
        public MainWindow()
        {
            Log.Logger = new LoggerConfiguration()
                               .MinimumLevel.Debug()
                               .WriteTo.File("file.log")
                               .CreateLogger();
            var logger = new SerilogLoggerFactory(Log.Logger).CreateLogger("Client logger");
            atlasBuilder = new FontAtlasBuilder(logger);
            var fonts = Directory.GetFiles(".").Where(t => t.EndsWith(".ttf"));
            Fonts = new ObservableCollection<Font>();
            foreach (var font in fonts)
            {
                var data = File.ReadAllBytes(font);
                Fonts.Add(new Font()
                {
                    Data = data,
                    Name = font,
                });
            }
            InitializeComponent();
            FontsComboBox.SelectedIndex = 1;
            this.Closing += MainWindow_Closing;
            Reset();
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            atlasBuilder.Dispose();
        }

        private void ButtonBuild_Click(object sender, RoutedEventArgs e)
        {
            Reset();

            for (var i = 0; i < fontAtlas.Positions.Count; i++)
                Draw(i);
        }

        private FontAtlas fontAtlas;
        private unsafe void Reset()
        {
            currentChr = 0;
            Canv.Children.Clear();

            fontAtlas = atlasBuilder.BuildAtlas(((Font)FontsComboBox.SelectedValue).Data, 40);
            Canv.Height = fontAtlas.Height;
            Canv.Width = fontAtlas.Width;
        }


        public class Font
        {
            public string Name { get; set; }

            public byte[] Data { get; set; }
        }

        public ObservableCollection<Font> Fonts { get; set; }

        private int currentChr = 0;
        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            Draw(currentChr);

            currentChr++;
            currentChr %= fontAtlas.Positions.Count;
        }

        private void Draw(int ind)
        {
            var key = fontAtlas.Positions.Keys.ElementAt(ind);
            var data = fontAtlas.Data[key];
            var pos = fontAtlas.Positions[key];
            if (pos.Rotate)
            {
                for (var i = 0; i != data.GetLength(1); i++)
                {
                    for (var j = 0; j != data.GetLength(0); j++)
                    {
                        var rect = new Rectangle();
                        rect.Width = 1;
                        rect.Height = 1;
                        rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(fontAtlas.Atlas[pos.X + i, pos.Y + j], fontAtlas.Atlas[pos.X + i, pos.Y + j], fontAtlas.Atlas[pos.X + i, pos.Y + j]));
                        Canvas.SetLeft(rect, pos.X + i);
                        Canvas.SetTop(rect, pos.Y + j);
                        Canv.Children.Add(rect);
                    }
                }
            }
            else
            {
                for (var i = 0; i != data.GetLength(0); i++)
                {
                    for (var j = 0; j != data.GetLength(1); j++)
                    {
                        var rect = new Rectangle();
                        rect.Width = 1;
                        rect.Height = 1;
                        rect.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(fontAtlas.Atlas[pos.X + i, pos.Y + j], fontAtlas.Atlas[pos.X + i, pos.Y + j], fontAtlas.Atlas[pos.X + i, pos.Y + j]));
                        Canvas.SetLeft(rect, pos.X + i);
                        Canvas.SetTop(rect, pos.Y + j);
                        Canv.Children.Add(rect);
                    }
                }
            }
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }
    }
}
