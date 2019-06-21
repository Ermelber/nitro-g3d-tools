﻿using System;
using System.IO;

namespace IntermediateConverter.Converter
{
    public class ConversionSettings
    {
        public float Magnify = 1;
        public bool RotateX180 = false;
        public bool FlipYZ = false;
        public bool NoLightOnMaterials = true;
        public bool UsePrimitiveStrip = false;
        public bool Verbose = false;

        public static ConversionSettings GetArguments(string[] args, out string outPath)
        {
            outPath = null;
            var settings = new ConversionSettings();

            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-o":
                    case "--output":
                        try
                        {
                            outPath = Path.GetFullPath(args[i + 1].Replace("\"", String.Empty));
                        }
                        catch
                        {
                            throw new Exception("Invalid/Missing Output Path");
                        }
                        i++;
                        break;
                    case "-m":
                    case "--mag":
                        try
                        {
                            settings.Magnify = float.Parse(args[i + 1]);
                        }
                        catch
                        {
                            throw new Exception("Invalid/Missing Magnification Factor");
                        }
                        i++;
                        break;
                    case "-f":
                    case "--flipYZ":
                        settings.FlipYZ = true;
                        break;
                    case "-r":
                    case "--rot180":
                        settings.RotateX180 = true;
                        break;
                    case "-l":
                    case "--light":
                        settings.NoLightOnMaterials = false;
                        break;
                    case "-v":
                    case "--verbose":
                        settings.Verbose = true;
                        break;
                }
            }

            return settings;
        }
    }
}
