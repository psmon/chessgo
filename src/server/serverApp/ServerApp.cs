using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebSocketSharp;
using WebSocketSharp.Server;

using UnityEngine;

namespace serverApp
{
    
    public static class ServerApp
    {
        private static GameTableActor gameTableActor = new GameTableActor();

        public static GameTableActor getGameTableActor()
        {
            return gameTableActor;
        }

        public static void runServer()
        {

            var wssv = new WebSocketServer(9100);
            
            ServerLog.writeLog(string.Format("ServerStart:{0}", 9100));
            wssv.Start();
            wssv.AddWebSocketService<GamePlayer>("/GoGame");            
            Console.ReadKey(true);
            wssv.Stop();
        }

    }
}
