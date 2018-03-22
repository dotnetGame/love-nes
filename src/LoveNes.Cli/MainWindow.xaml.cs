using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using LoveNes.Host;
using LoveNes.IO;

namespace LoveNes.Cli
{
    public class MainWindow : Window
    {
        private readonly NesSystem _nesSystem;
        private readonly uint[] _backBuffer;
        private Bitmap _bitmap;

        public MainWindow()
        {
            _backBuffer = new uint[256 * 240];
            _nesSystem = new NesSystem(new HostGraphics(this));

            this.InitializeComponent();
            this.AttachDevTools();
            Renderer.DrawFps = true;
            //Renderer.DrawDirtyRects = Renderer.DrawFps = true;
        }

        private async void InitializeComponent()
        {
            // TODO: iOS does not support dynamically loading assemblies
            // so we must refer to this resource DLL statically. For
            // now I am doing that here. But we need a better solution!!
            var theme = new Avalonia.Themes.Default.DefaultTheme();
            theme.TryGetResource("Button", out _);
            AvaloniaXamlLoader.Load(this);

            this.Tapped += MainWindow_Tapped;

            var file = await NesFile.FromStream(File.OpenRead("lj65.nes")).ConfigureAwait(false);

            _nesSystem.Cartridge.InsertNesFile(file);
            _nesSystem.PowerUp();
        }

        private void MainWindow_Tapped(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if(_bitmap != null)
            {
                _bitmap.Save("bin/test.png");
                Content = new Image { Source = _bitmap };
            }
        }

        private class HostGraphics : IHostGraphics
        {
            private readonly MainWindow _mainWindow;

            public HostGraphics(MainWindow mainWindow)
            {
                _mainWindow = mainWindow;
            }

            void IHostGraphics.DrawPixel(byte x, byte y, uint rgb)
            {
                _mainWindow._backBuffer[y * 256 + x] = rgb == 0 ? 0 : 0xFFFF0000;
            }

            unsafe void IHostGraphics.Flip()
            {
                var str = new StringBuilder();

                for (int y = 0; y < 240; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        var pixel = _mainWindow._backBuffer[y * 256 + x] & 0xFFFFFF;
                        if (pixel == 0)
                            str.Append(' ');
                        else
                            str.Append('x');
                    }

                    str.AppendLine();
                }

                var text = str.ToString();

                fixed(uint* p = _mainWindow._backBuffer)
                {
                    _mainWindow._bitmap = new Bitmap(Avalonia.Platform.PixelFormat.Bgra8888, (IntPtr)p, 256, 240, 256 * sizeof(uint));
                }
            }
        }
    }
}
