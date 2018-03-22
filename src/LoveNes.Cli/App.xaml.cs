using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace LoveNes.Cli
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
