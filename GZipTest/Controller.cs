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
        public static void CompressFile(string inputFileName, string outputFileName)
        {
            Writer.fileOut = new FileStream(outputFileName, FileMode.Create);
            WorkingThread workingThread = new WorkingThread(inputFileName);
            CreateArePool();

            for (int i = 0; i < WorkingThread.threadCount; i++)
            {
                WorkingThread.tPool[i] = new Thread(new ThreadStart(workingThread.CompressInThread)); 
                WorkingThread.tPool[i].Start();
            }

            JoinAll();
            Writer.fileOut.Close();
        }

        public static void DecompressFile(string inputFileName, string outputFileName)
        {
            Writer.fileOut = new FileStream(outputFileName, FileMode.Create);
            WorkingThread workingThread = new WorkingThread(inputFileName);
            CreateArePool();

            for (int i = 0; i < WorkingThread.threadCount; i++)
            {
                WorkingThread.tPool[i] = new Thread(new ThreadStart(workingThread.DecompressInThread));
                WorkingThread.tPool[i].Start();
            }

            JoinAll();
            Writer.fileOut.Close();
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
