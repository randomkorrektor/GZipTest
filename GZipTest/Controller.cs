using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GZipTest
{
    class Controller
    {
        public static void FileHandling(string inputFileName, string outputFileName, bool compress)
        {
            if (new FileInfo(inputFileName).Length > WorkingThread.blockForCompress)
            {
                Writer.fileOut = new FileStream(outputFileName, FileMode.Create);
                WorkingThread workingThread = new WorkingThread(inputFileName);
                CreateArePool();

                if (compress)
                {
                    for (int i = 0; i < WorkingThread.threadCount; i++)
                    {
                        WorkingThread.tPool[i] = new Thread(new ThreadStart(workingThread.CompressInThread));
                        WorkingThread.tPool[i].Start();
                    }
                }
                else
                {
                    for (int i = 0; i < WorkingThread.threadCount; i++)
                    {
                        WorkingThread.tPool[i] = new Thread(new ThreadStart(workingThread.DecompressInThread));
                        WorkingThread.tPool[i].Start();
                    }
                }

                JoinAll();
                Writer.fileOut.Close();
            }
            else
            {
                if (compress)
                    WorkingThread.OneTradeCompress(inputFileName, outputFileName);
                else
                    WorkingThread.OneTradeDecompress(inputFileName, outputFileName);
            }
        }
        
        private static void CreateArePool()
        {
            WorkingThread.arePool[WorkingThread.arePool.Length - 1] = new AutoResetEvent(true);
            for (int i = 0; i < WorkingThread.arePool.Length - 1; i++)
            {
                WorkingThread.arePool[i] = new AutoResetEvent(false);
            }
        }

        private static void JoinAll()
        {
            for (int i = 0; i < WorkingThread.threadCount; i++)
            {
                WorkingThread.tPool[i].Join();
            }
        }
    }
}
