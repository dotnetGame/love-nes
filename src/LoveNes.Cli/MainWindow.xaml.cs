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
        private uint[] _frontBuffer;
        private uint[] _backBuffer;

        public MainWindow()
        {
            _frontBuffer = new uint[256 * 240];
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

            var file = await NesFile.FromStream(File.OpenRead("lj65.nes")).ConfigureAwait(false);

            _nesSystem.Cartridge.InsertNesFile(file);
            _nesSystem.PowerUp();
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
                _mainWindow._backBuffer[y * 256 + x] = rgb;
            }

            unsafe void IHostGraphics.Flip()
            {
                var buffer = _mainWindow._backBuffer;
                _mainWindow._backBuffer = _mainWindow._frontBuffer;
                _mainWindow._frontBuffer = _mainWindow._backBuffer;

                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    fixed (uint* p = _mainWindow._frontBuffer)
                    {
                        var bitmap = new Bitmap(Avalonia.Platform.PixelFormat.Bgra8888, (IntPtr)p, 256, 240, 256 * sizeof(uint));
                        _mainWindow.Content = new Image { Source = bitmap };
                    }
                });
            }
        }
    }
}
