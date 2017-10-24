using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GZipTest
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 3)
            {
                string inFileName = args[1].ToString();
                string outFileName = args[2].ToString();

                if (File.Exists(inFileName))
                {
                    if (Directory.Exists(Path.GetDirectoryName(outFileName)))
                    {
                        try
                        {
                            switch (args[0].ToString())
                            {
                                case "compress":
                                    {
                                        Console.WriteLine("Идет архивация...");
                                        Controller.CompressFile(inFileName, outFileName);
                                        Console.WriteLine("Успешно.");
                                        break;
                                    }
                                case "decompress":
                                    {
                                        Console.WriteLine("Идут разархивация...");
                                        Controller.DecompressFile(inFileName, outFileName);
                                        Console.WriteLine("Успешно.");
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine("Неверно введена команда. Поддерживаются только команды \"compress\" и \"decompress\"");
                                        break;
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неверно указан путь вывода.");
                    }
                }
                else
                {
                    Console.WriteLine("Исходный файл не найден. Проверьте правильность введенного пути и названия файла.");
                }
            }
            else
            {
                Console.WriteLine("Неверный формат входных данных");
            }
        }

    }
}