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
                    try
                    {
                        switch (args[0].ToString())
                        {
                            case "compress":
                                {
                                    Console.WriteLine("Идет архивация...");

                                    Controller.FileHandling(inFileName, outFileName, true);
                                    Console.WriteLine("Успешно.");
                                    break;
                                }
                            case "decompress":
                                {

                                    Console.WriteLine("Идут разархивация...");
                                    Controller.FileHandling(inFileName, outFileName, false);
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
                        if (Writer.fileOut != null)
                            Writer.fileOut.Close();
                        for (int i = 0; i < WorkingThread.tPool.Length; i++)
                        {
                            if (WorkingThread.tPool[i] != null)
                                WorkingThread.tPool[i].Abort();
                        }

                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Исходный файл не найден. Проверьте правильность введенного пути и названия файла.");
                }
            }
            else
            {
                Console.WriteLine("Неверный формат входных данных. Ожидается формат \"compress\\decompress [имя исходного файла] [имя результирующего файла]\"");
            }
        }
    }
}