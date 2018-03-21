using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoveNes.IO;

namespace LoveNes.Cli
{
    public class MainWindow : Window
    {
        private readonly NesSystem _nesSystem = new NesSystem();

        public MainWindow()
        {
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
    }
}
