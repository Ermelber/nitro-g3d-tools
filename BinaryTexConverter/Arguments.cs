using System;
using System.IO;

namespace BinaryTexConverter
{
    public enum Mode
    {
        None,
        Convert,
        Extract
    }

    public class Arguments
    {
        public string OutPath;
        public string InputPath;
        public string NsbmdPath = null;
        public Mode Mode = Mode.None;

        public static Arguments GetArguments(string[] args)
        {
            var arguments = new Arguments();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-c":
                    case "--convert":
                        arguments.Mode = Mode.Convert;
                        break;
                    case "-e":
                    case "--extract":
                        arguments.Mode = Mode.Extract;
                        break;
                    case "-o":
                    case "--output":
                        try
                        {
                            arguments.OutPath = Path.GetFullPath(args[i + 1].Replace("\"", String.Empty));
                        }
                        catch
                        {
                            throw new Exception("Invalid Output Path");
                        }
                        i++;
                        break;
                    case "-i":
                    case "--input":
                        try
                        {
                            arguments.InputPath = Path.GetFullPath(args[i + 1].Replace("\"", String.Empty));
                        }
                        catch
                        {
                            throw new Exception("Invalid Input Path");
                        }
                        i++;
                        break;
                    case "-m":
                    case "--mdl":
                        try
                        {
                            arguments.NsbmdPath = Path.GetFullPath(args[i + 1].Replace("\"", String.Empty));
                        }
                        catch
                        {
                            throw new Exception("Invalid NSBMD Path");
                        }
                        i++;
                        break;
                }
            }

            if (arguments.Mode == Mode.None)
            {
                throw new Exception("Missing mode");
            }

            if (string.IsNullOrEmpty(arguments.InputPath) && arguments.Mode == Mode.Convert)
            {
                arguments.InputPath = Directory.GetCurrentDirectory();
            }

            if (string.IsNullOrEmpty(arguments.InputPath) && arguments.Mode == Mode.Extract)
            {
                throw new Exception("Missing NSBMD/NSBTX path");
            }

            if (string.IsNullOrEmpty(arguments.NsbmdPath) && string.IsNullOrEmpty(arguments.OutPath))
            {
                arguments.OutPath = Path.Combine(Directory.GetCurrentDirectory(), Path.GetFileName(arguments.InputPath) + ".nsbtx");
            }
            else if (!string.IsNullOrEmpty(arguments.NsbmdPath) && string.IsNullOrEmpty(arguments.OutPath))
            {
                arguments.OutPath = arguments.NsbmdPath;
            }

            return arguments;
        }
    }
}
