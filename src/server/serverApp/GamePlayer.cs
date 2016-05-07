using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using CommData;
namespace serverApp
{
    public class GamePlayer : WebSocketBehavior
    {
        public GameActor myGame;

        public GamePlayer()
        {
            
        }

        protected override void OnOpen()
        {            
            ServerLog.writeLog("Come in GamePlayer:" + ID);
            myGame = ServerApp.getGameTableActor().quickJoin(this);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            if (myGame != null)
            {
                ServerApp.getGameTableActor().leaveGame(this);
                myGame = null;
            }

            ServerLog.writeLog( string.Format("Onerror GamePlayer:{0} , {1}" , ID , e.Message )  );
        }

        protected override void OnClose(CloseEventArgs e)
        {
            if (myGame != null)
            {
                ServerApp.getGameTableActor().leaveGame(this);
                myGame = null;
            }
            ServerLog.writeLog(string.Format("OnClose GamePlayer:{0} , {1}", ID, e.Reason));
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            ServerLog.writeLog(string.Format("Player:{0}, Msg:",ID , e.Data ));
        }
    }
}
