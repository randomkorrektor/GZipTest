using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    class Writer
    {
        public static FileStream fileOut;

        public static void WriteInfo(byte[] dataArray)
        {
            fileOut.Write(dataArray, 0, dataArray.Length);
        }
    }
}
