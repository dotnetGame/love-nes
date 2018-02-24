using System;
using System.IO;
using LoveNes.IO;

namespace LoveNes.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var file = NesFile.FromStream(File.OpenRead("lj65.nes"));

            var system = new NesSystem();
            system.PowerUp();
        }
    }
}
