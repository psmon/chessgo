using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace serverApp
{
    public static class ServerLog
    {
        //Todo : apply LogLevel..
        public static void writeLog(string logtext, int loglevel=0)
        {
            Console.WriteLine(logtext);
            
        }

    }
}
