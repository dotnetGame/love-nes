using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using LoveNes.IO;

namespace LoveNes.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            BuildAvaloniaApp().Start<MainWindow>();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>().UsePlatformDetect();
    }
}
