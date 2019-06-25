using System;
using System.Globalization;
using System.IO;
using LibNitroG3DTools.Converter.Binary.Nsbmd;

namespace BinaryConverter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //This is needed to conform to a single standard for decimal numbers
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            if (args.Length > 0)
            {
                //var settings = ConversionSettings.GetArguments(args, out var outPath);

                string outPath = null;

                if (outPath == null)
                    outPath = Path.Combine(Path.GetDirectoryName(args[0].Replace("\"", String.Empty)),
                        Path.GetFileNameWithoutExtension(args[0]) + ".nsbmd");

                /*if (settings.Verbose)*/
                Console.WriteLine($"\nInput: {args[0]}\nOutput: {outPath}");

                var a = new NsbmdConverter(args[0]);

                //a.Write(outPath);

                /*if (settings.Verbose)*/
                Console.WriteLine("Success!");

                return;
            }

            try
            {
                
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError: " + e.Message);
                Console.WriteLine();
                Console.ReadKey();
                return;
            }

            DisplayHelp();
        }


        private static void DisplayHelp()
        {
            Console.WriteLine($"\nNitro G3D Binary Converter\nA CLI tool capable of converting Nitro G3D Intermediates (IMD and such) into Binaries (NSBMD and such).");
            Console.WriteLine("Written by Ermelber.");
            Console.WriteLine("\nUsage:\n\tbinconv INPUT [OPTIONS]");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("\t-t/--etex\t\t\tFor IMD files, it exports NSBTX separately from NSBMD.");
            Console.WriteLine("\t-v/--verbose\t\t\tOutput warnings and other info.");
            Console.WriteLine();
        }
    }
}
