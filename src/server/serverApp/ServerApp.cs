using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WebSocketSharp;
using WebSocketSharp.Server;

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
            string listenUrl = "ws://192.168.0.30";
            var wssv = new WebSocketServer(listenUrl);
            ServerLog.writeLog(string.Format("ServerStart:{0}", listenUrl));
            wssv.Start();
            wssv.AddWebSocketService<GamePlayer>("/GoGame");
            Console.ReadKey(true);
            wssv.Stop();
        }

    }
}
