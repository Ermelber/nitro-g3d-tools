using System;
using System.Globalization;
using LibNitroG3DTools.Converter.Binary.Nsbmd;

namespace BinaryTexConverter
{
    public class Program
    {
        static void Main(string[] args)
        {
            //This is needed to conform to a single standard for decimal numbers
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            try
            {
                if (args.Length > 0)
                {
                    var settings = Arguments.GetArguments(args);

                    Console.WriteLine($"\nInput: {settings.InputPath}\nOutput: {settings.OutPath}");

                    switch (settings.Mode)
                    {
                        case Mode.Convert:
                            if (!string.IsNullOrEmpty(settings.NsbmdPath))
                                TexUtils.Replace(settings.NsbmdPath, settings.OutPath, settings.InputPath);
                            else
                                TexUtils.Create(settings.OutPath, settings.InputPath);

                            Console.WriteLine("Success!");
                            
                            break;
                        case Mode.Extract:
                            throw new NotImplementedException();
                    }

                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError: " + e.Message);
                Console.WriteLine();
            }

            DisplayHelp();
        }

        private static void DisplayHelp()
        {
            Console.WriteLine("\nBinary Tex Converter\nA CLI tool capable of replacing/creating/extracting textures within NSBMD or NSBTX files.");
            Console.WriteLine("\nUsage:\n\tBinaryTexConverter [OPTIONS]");
            Console.WriteLine("\nOptions:");
            
            Console.WriteLine("\t-c/--convert\t\t\tConvert textures from the input folder containing NITRO TGA texture files.");
            Console.WriteLine("\t-e/--extract\t\t\tExtract textures from the input file.\n");

            Console.WriteLine("\t-i/--input\tpath/fileName\tPath of the NITRO TGA files folder or of the NSBMD/NSBTX file.\n\t\t\t\t\tOptional if \"--convert\" is set, in that case the current directory is used.");
            Console.WriteLine("\t-o/--output\tfileName\tPath to the output file. (Optional)");
            Console.WriteLine("\t-m/--mdl\tfileName\tPath to an NSBMD file. If specified, it will replace the textures\n\t\t\t\t\tinside the NSBMD file or output a new NSBMD (if \"--output\" is set)");

            Console.WriteLine();
        }
    }
}
