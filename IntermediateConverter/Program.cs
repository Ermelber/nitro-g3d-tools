using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using LibNitro.Intermediate;
using LibNitroG3DTools.Converter.Intermediate.Imd;

namespace IntermediateConverter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //This is needed to conform to a single standard for decimal numbers
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            //Test();
            //return;

            try
            {
                if (args.Length > 0)
                {
                    var settings = ImdConverterSettings.GetArguments(args, out var outPath);

                    if (outPath == null)
                        outPath = Path.Combine(Path.GetDirectoryName(args[0].Replace("\"", String.Empty)),
                            Path.GetFileNameWithoutExtension(args[0]) + ".imd");

                    if (settings.Verbose)
                        Console.WriteLine($"\nInput: {args[0]}\nOutput: {outPath}");

                    new ImdConverter(args[0], settings).Write(outPath);

                    if (settings.Verbose)
                        Console.WriteLine("Success!");

                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\nError: " + e.Message);
                Console.WriteLine();
            }

            DisplayHelp();

            //TestTristrip();
            //TestBattery();
            //Console.ReadKey();
        }

        private static void DisplayHelp()
        {
            Console.WriteLine($"\nASS to IMD (Version {ImdConverter.GeneratorInfo.Version})\nA CLI tool capable of converting Assimp compatible 3D Models to Nitro Intermediate Model.");
            Console.WriteLine("Designed to remove the need of Nitro Plugins for MKDS Custom Tracks and Custom Karts.");
            Console.WriteLine("Written by Ermelber with Gericom's help.");
            Console.WriteLine("\nUsage:\n\tAssToImd INPUT [OPTIONS]");
            Console.WriteLine("\nOptions:");
            Console.WriteLine("\t-o/--output\tfileName\tPath to the output file.");
            Console.WriteLine("\t-m/--mag\tscaleFactor\tScales by the model by a magnification factor.");
            Console.WriteLine("\t-f/--flipYZ\t\t\tFlips the Y and Z Axis.");
            Console.WriteLine("\t-r/--rot180\t\t\tRotates the model by 180 degrees on the X Axis.");
            Console.WriteLine("\t-l/--light\t\t\tEnables Light 0 on all materials. Needed for shaded models.");
            Console.WriteLine("\t-n/--nostrip\t\t\tDisables primitive stripping.");
            Console.WriteLine("\t-v/--verbose\t\t\tOutput warnings and other info.");
            Console.WriteLine();
        }

        private static void Test()
        {
            var test = new Dictionary<VecFx32, int>();

            test.Add(new VecFx32(0,0,0), 134);
            Console.WriteLine(test[new VecFx32(0, 0, 0)]);


            var vec = new VecFx32(34.5654f, 3423.54f, 9087.3f);

            Console.ReadLine();

            /*new ImdConverter(@"E:\ermel\Desktop\conversion_tests\DK_FBX_2\DK_FBX\new_tex_18\dk_new_test.fbx",
                new ConversionSettings
                {
                    Magnify = 0.0625f
                }
            ).Write(@"E:\ermel\Desktop\conversion_tests\_imd\dk.imd");*/

            //new ImdConverter(@"E:\ermel\Desktop\conversion_tests\test.fbx",
            //    new ImdConverterSettings
            //    {
            //        RotateX180 = true
            //    }
            //).Write(@"E:\ermel\Desktop\conversion_tests\_imd\test.imd");

            /*
            new ImdConverter(@"E:\ermel\Desktop\conversion_tests\ac.fbx",
                new ConversionSettings
                {
                    Magnify = 0.0625f,
                    RotateX180 = true
                }
            ).Write(@"E:\ermel\Desktop\conversion_tests\_imd\ac.imd");*/

            /*new ImdConverter(@"E:\ermel\Desktop\conversion_tests\testBox.fbx",
                new ConversionSettings
                {
                    RotateX180 = true
                }
            ).Write(@"E:\ermel\Desktop\conversion_tests\_imd\testBox.imd");*/

            //new ImdConverter(@"E:\ermel\Hackdom\DSHack\EKDS\GRAPHICS\GRAPHICS\Models\Characters\Isabelle\tex\isabelle.obj").Write(@"E:\ermel\Desktop\conversion_tests\_imd\isabelle.imd");
        }
    }
}
