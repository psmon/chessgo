using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommData;

namespace serverApp
{    
    public class GameActor
    {
        //Todo:make asyn(akka actor:http://getakka.net/docs/Working%20with%20actors)

        private GameTableState gameState = GameTableState.NOTINIT;
        private TableInfo tableInfo = new TableInfo();

        private List<GamePlayer> gamePlayers = new List<GamePlayer>();
        private List<GamePlayer> unfinishedPlayer = new List<GamePlayer>();

        private GamePlayer blackPlayer;
        private GamePlayer whitePlayer;

        public bool createGameTable(int gameNo)
        {
            tableInfo.gameNo = gameNo;
            tableInfo.plyCount = 0;
            tableInfo.plyMaxCount = 2;
            gameState = GameTableState.READYFORPLAYER;
            return true;
        }

        public TableInfo getTableInfo()
        {
            return tableInfo;
        }

        public int getAvableUserCount()
        {
            lock (this)
            {
                return tableInfo.GetAvableUserCount();
            }            
        }
        
        public void prePareGame()
        {
            ServerLog.writeLog(string.Format("startGame:{0}", tableInfo.gameNo));
            var whiteDolInfo = whitePlayer.dolsInfo;
            var blackDolInfo = blackPlayer.dolsInfo;

            //send white
            whiteDolInfo.isMe = true;
            whitePlayer.sendData("DolsInfo", whiteDolInfo);

            whiteDolInfo.isMe = false;
            blackPlayer.sendData("DolsInfo", whiteDolInfo);

            //send black
            blackDolInfo.isMe = false;
            whitePlayer.sendData("DolsInfo", blackDolInfo);

            blackDolInfo.isMe = true;
            blackPlayer.sendData("DolsInfo", blackDolInfo);
            
        }

        public bool joinGame(GamePlayer gamePlayer)
        {
            lock (this)
            {
                if (tableInfo.GetAvableUserCount() < 1)
                {
                    ServerLog.writeLog(string.Format("joinGame Failed {0} in GameNo:{1} plyCount:{2}", gamePlayer.ID, tableInfo.gameNo, tableInfo.plyCount));
                    return false;
                }

                gamePlayers.Add(gamePlayer);
                tableInfo.plyCount++;

                if (tableInfo.plyCount == 1)
                {
                    gamePlayer.createDolInfo(false);
                    whitePlayer = gamePlayer;
                }


                if (tableInfo.plyCount == 2)
                {
                    gamePlayer.createDolInfo(true);
                    blackPlayer = gamePlayer;
                    prePareGame();
                }
                                    
                ServerLog.writeLog(string.Format("joinGame {0} in GameNo:{1} plyCount:{2}", gamePlayer.ID, tableInfo.gameNo , tableInfo.plyCount ));
            }            
            return true;
        }

        public void leaveGame(GamePlayer gamePlayer)
        {
            lock (this)
            {
                foreach(GamePlayer idxPlayer in gamePlayers)
                {
                    if(gamePlayer.ID == idxPlayer.ID)
                    {
                        gamePlayers.Remove(idxPlayer);
                        tableInfo.plyCount--;
                        ServerLog.writeLog(string.Format("leaveGame {0} in GameNo:{1} plyCount:{2}", gamePlayer.ID, tableInfo.gameNo, tableInfo.plyCount));
                        break;
                    }
                }
            }

        }        
    }
}
