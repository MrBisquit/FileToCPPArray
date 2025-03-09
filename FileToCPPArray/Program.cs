namespace FileToCPPArray
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("File --> C/C++ Array converter\nhttps://github.com/MrBisquit/FileToCPPArray/\n\nUsage:\n\tFileToCPPArray -f \"path/to/file\" -t \"path/to/cpp/header.h\"");
                Console.WriteLine("Optional:\n\t-p Include \"#pragma once\"\n\t-d Type declaration as a string\t\tDefault: \"unsigned char\"\n\t-h Headers as a comma separated string\n\t-g Header guards\n\t-l Array line length\t\t\tDefault: 15");
                Console.WriteLine("Examples:\n\tFileToCPPArray -f \"cat.png\" -t \"cat.h\" -p -h \"<stdint.h>\" -d \"uint8_t\" -g");
                Console.WriteLine("\tFileToCPPArray -f \"cat.png\" -t \"cat.h\" -p -g -l 16");

                Environment.Exit(1);
            }

            string from = "";
            string to = "";
            string typedec = "unsigned char";
            string[] headers = new string[0];
            bool pragma = false;
            bool guards = false;
            int line = 15;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-p")
                {
                    pragma = true;
                    continue;
                } else if (args[i] == "-g")
                {
                    guards = true;
                    continue;
                }

                if (i + 1 > args.Length)
                {
                    Console.WriteLine("Invalid number of arguments provided");
                    Environment.Exit(-1);
                    return;
                }

                switch(args[i])
                {
                    case "-f":
                        from = args[i + 1];
                        i++;
                        break;
                    case "-t":
                        to = args[i + 1];
                        i++;
                        break;
                    case "-d":
                        typedec = args[i + 1];
                        i++;
                        break;
                    case "-h":
                        headers = args[i + 1].Split(",");
                        i++;
                        break;
                    case "-l":
                        try
                        {
                            line = int.Parse(args[i + 1]);
                            i++;
                        } catch
                        {
                            Console.WriteLine($"Could not parse \"{args[i + 1]}\"");
                            Environment.Exit(-1);
                        }
                        break;
                    default:
                        Console.WriteLine($"Invalid argument \"{args[i]}\"");
                        Environment.Exit(-1);
                        break;
                }
            }

            if(!File.Exists(from))
            {
                Console.WriteLine($"No file exists at the path: {from}");
                Environment.Exit(-1);
            }

            byte[] data = File.ReadAllBytes(from);
            string content = @"// This file was generated using File --> C/C++ Array converter which is available at https://github.com/MrBisquit/FileToCPPArray/
// This file contains exactly " + data.Length + " bytes\n\n";

            if (pragma) content += "#pragma once\n";

            for (int i = 0; i < headers.Length; i++)
            {
                content += $"#include {headers[i]}\n";
            }

            string guard_title = "_" + new FileInfo(from).Name.ToUpper().Replace(" ", "_").Replace(".", "_");

            if (guards) content += $"#ifndef {guard_title}\n";

            content += $"{typedec} {guard_title.Remove(0, 1)}[{data.Length}] = " + "{\n\t";

            content += BytesToString(data, line);

            content += "\n};\n";

            if (guards) content += $"#endif // {guard_title}";

            File.WriteAllText(to, content);
        }

        static string ToHex(byte b) => $"0x{b:X2}";

        static string BytesToString(byte[] bytes, int line = 15)
        {
            int current = 1;
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += ToHex(bytes[i]);
                if (i != bytes.Length - 1) str += ", ";
                if (current == 15) { str += "\n\t"; current = 0; }
                current++;
                if(i % 1000 == 0) DrawProgress((double)((double)i / (double)bytes.Length) * (double)100);
            }
            DrawProgress(100.0);
            return str;
        }

        static void DrawProgress(double progress)
        {
            int left = (int)(Math.Round(progress) / 2);
            int right = (int)(50 - left);
            Console.Write("\r[");
            ConsoleColor currentBg = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write(new string(' ', left));
            Console.BackgroundColor = currentBg;
            Console.Write(new string(' ', right));
            Console.Write($"] {Math.Round(progress, 2).ToString().PadLeft(6)}% ");
            Console.CursorLeft -= 1;
        }
    }
}
