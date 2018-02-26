using System;
using System.IO;
using System.Threading.Tasks;
using LoveNes.IO;

namespace LoveNes.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var file = NesFile.FromStream(File.OpenRead("lj65.nes")).Result;

            var system = new NesSystem();
            system.Cartridge.InsertNesFile(file);
            system.PowerUp();
        }
    }
}
