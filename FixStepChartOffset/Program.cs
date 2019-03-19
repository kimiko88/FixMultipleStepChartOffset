using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace FixStepChartOffset
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            StartFix();
            Console.WriteLine("Press esc to exit or any other key to continue");
            var key = Console.ReadKey();
            while(key.Key != ConsoleKey.Escape)
            {
                StartFix();
                Console.WriteLine("Press esc to exit or any other key to continue");
                key = Console.ReadKey();
            }
            System.Environment.Exit(1);
        }

        static void StartFix()
        {
            Console.WriteLine(@"Insert absolute path of the pack: (example C:\Users\itg\Desktop\ITGSONGSPACK)");
            var path = Console.ReadLine();
            while (!Directory.Exists(path))
            {
                Console.WriteLine("Error, no directory found");
                Console.WriteLine("Insert absolute path of the pack:");
                path = Console.ReadLine();
            }
            var files = Directory.GetFiles(path, "*.sm", SearchOption.AllDirectories);
            while (files.Length == 0)
            {
                Console.WriteLine("Error, no file found");
                Console.WriteLine("Insert absolute path of the pack:");
                path = Console.ReadLine();
                files = Directory.GetFiles(path, "*.sm", SearchOption.AllDirectories);
            }
            for (var i = 0; i < files.Length; i++)
            {
                Console.WriteLine(ReadSm(files[i]));
            }
            Console.WriteLine("Insert how many milliseconds to add: (leave empty to default 9)");
            var millisec = Console.ReadLine();
            int millisecValue;
            if (string.IsNullOrEmpty(millisec))
            {
                millisecValue = 9;
            }
            else
            {
                while (!int.TryParse(millisec, out millisecValue))
                {
                    Console.WriteLine("Error, please enter a numeric integer value!");
                    Console.WriteLine("Insert how many milliseconds to add: (leave empty to default 9)");
                    millisec = Console.ReadLine();
                }
            }
            for (var i = 0; i < files.Length; i++)
            {
                Console.WriteLine(Modify(files[i], millisecValue));
            }
            Console.WriteLine("OPERATION DONE!");
        }

        static string ReadSm(string path)
        {
            var text = File.ReadAllText(path);
            Regex regexTitle = new Regex("#TITLE:(.*);");
            Regex regexOffset = new Regex("#OFFSET:(.*);");

            var title = regexTitle.Match(text).Value;
            var offset = regexOffset.Match(text).Value;
            return title.Replace("#TITLE:", "").Trim(';') + "---- actual offset: " + offset.Replace("#OFFSET:", "").Trim(';');
        }

        static string Modify(string path, int millisec)
        {
            var text = File.ReadAllText(path);
            Regex regexOffset = new Regex("#OFFSET:(.*);");
            Regex regexTitle = new Regex("#TITLE:(.*);");

            var offset = regexOffset.Match(text).Value;
            var title = regexTitle.Match(text).Value;
            if (decimal.TryParse(offset.Replace("#OFFSET:", "").Trim(';'), NumberStyles.Any, new NumberFormatInfo { NumberDecimalSeparator = "." }, out decimal res))
            {
                var modify = res + (decimal)(0.001) * millisec;
                var modifiedText = text.Replace(offset, $"#OFFSET:{modify};");
                File.WriteAllText(path,text);
                return title.Replace("#TITLE:", "").Trim(';') + "---- past offset: " + offset.Replace("#OFFSET:", "").Trim(';') + "-----> " + modify;
            }
            else
            {
                return "ERROR in " + title.Replace("#TITLE:", "").Trim(';');
            }
        }
    }
}
