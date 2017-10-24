using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    class WorkingThread
    {
        public static int threadCount = Environment.ProcessorCount;
        static byte[][] dataSource = new byte[threadCount][];
        static byte[][] dataSourceCompress = new byte[threadCount][];
        static int blockForCompress = (int)Process.GetCurrentProcess().WorkingSet64 / threadCount;

        public static Thread[] tPool = new Thread[threadCount];
        public static AutoResetEvent[] arePool = new AutoResetEvent[threadCount];
        public static string inputFileName;

        static bool flag = true;
        static object lockerFilePosition = new object();
        static object outLocker = new object();
        static int enumerator = 0;
        static long filePosition = 0;
        int lockalBlock;

        public WorkingThread(string fileName)
        {
            inputFileName = fileName;
            lockalBlock = blockForCompress;
        }

        public void CompressInThread()
        {
            FileStream inputStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            long readPosition;
            long threadNumber;

            while (flag)
            {
                lock (lockerFilePosition)
                {
                    readPosition = filePosition;
                    threadNumber = readPosition % threadCount;
                    filePosition++;
                }

                inputStream.Seek(readPosition * blockForCompress, SeekOrigin.Begin);
                if (inputStream.Length - inputStream.Position < blockForCompress)
                {
                    lock (outLocker)
                    {
                        if (!flag)
                        {
                            inputStream.Close();
                            return;
                        }
                        flag = false;
                        lockalBlock = (int)(inputStream.Length - inputStream.Position);
                    }
                }

                dataSource[threadNumber] = new byte[lockalBlock];
                inputStream.Read(dataSource[threadNumber], 0, lockalBlock);

                using (MemoryStream output = new MemoryStream(dataSource[threadNumber].Length))
                {
                    using (GZipStream cs = new GZipStream(output, CompressionMode.Compress))
                    {
                        cs.Write(dataSource[threadNumber], 0, dataSource[threadNumber].Length); // данные записываются в output
                    }
                    dataSourceCompress[threadNumber] = output.ToArray();   //output переводим в массив и передаем в dataSourceZip
                }

                BitConverter.GetBytes(dataSourceCompress[threadNumber].Length + 1)  //получаем размер блока в байтах
                            .CopyTo(dataSourceCompress[threadNumber], 4);           //запись информации о размере

                arePool[(threadNumber + threadCount - 1) % threadCount].WaitOne();
                Writer.WriteInfo(dataSourceCompress[threadNumber]);
                arePool[threadNumber].Set();
            }
            inputStream.Close();
        }

        public void DecompressInThread()
        {
            FileStream inputStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            long threadNumber;

            byte[] buffer = new byte[8]; //массив для хранения информации о размере и CRC

            lock (lockerFilePosition)
            {
                threadNumber = enumerator;
                enumerator++;
            }
            while (true)
            {
                if (flag)
                    arePool[(threadNumber + threadCount - 1) % threadCount].WaitOne();

                if (filePosition >= inputStream.Length)
                {
                    flag = false;
                    inputStream.Close();
                    arePool[threadNumber].Set();
                    //arePool[(threadNumber + threadCount - 1) % threadCount].Set();
                    return;
                }

                inputStream.Seek(filePosition, SeekOrigin.Begin);
                inputStream.Read(buffer, 0, 8);                           //Чтение 8 байт в буфер (4 байта CRC32, 4 байта ISIZE)
                int compressedBlockSize = (BitConverter.ToInt32(buffer, 4)) - 1;      //вычисляем размер блока   

                filePosition += compressedBlockSize;
                arePool[threadNumber].Set();

                dataSourceCompress[threadNumber] = new byte[compressedBlockSize];   //создание массива на основе полученного размера
                buffer.CopyTo(dataSourceCompress[threadNumber], 0);                         //копирование 8 байт в массив
                inputStream.Read(dataSourceCompress[threadNumber], 8, dataSourceCompress[threadNumber].Length - 8); //чтение потока размером в длину блока, исключая 8 прочитанных байт

                int decompressedBlockSize = BitConverter.ToInt32(dataSourceCompress[threadNumber], dataSourceCompress[threadNumber].Length - 4);  //вычисляем размер распакованного блока 
                dataSource[threadNumber] = new byte[decompressedBlockSize];  //создаем массив для распакованного блока

                using (MemoryStream input = new MemoryStream(dataSourceCompress[threadNumber]))
                using (GZipStream ds = new GZipStream(input, CompressionMode.Decompress))
                {
                    try
                    {
                        ds.Read(dataSource[threadNumber], 0, dataSource[threadNumber].Length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Ошибка: " + e.Message);
                    }
                }
                if (flag)
                    arePool[(threadNumber + threadCount - 1) % threadCount].WaitOne();
                Writer.WriteInfo(dataSource[threadNumber]);
                arePool[threadNumber].Set();


            }

        }
    }
}
