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

        private GamePlayer blackPlayer = null;
        private GamePlayer whitePlayer = null;
                
        //private GamePlayer lastLeavePlayer = null;

        private bool isNowPlayerBlack;

        public bool createGameTable(int gameNo)
        {
            tableInfo.gameNo = gameNo;
            tableInfo.plyCount = 0;
            tableInfo.plyMaxCount = 2;
            gameState = GameTableState.READYFORPLAYER;
            isNowPlayerBlack = false;
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
        
        public void sendTurnInfo()
        {
            var turnInfo = new TurnInfo();
            
            if (isNowPlayerBlack == false)
            {
                turnInfo.isBlack = false;
                turnInfo.isMe = true;
                whitePlayer.sendData("TurnInfo", turnInfo);
                turnInfo.isMe = false;
                blackPlayer.sendData("TurnInfo", turnInfo);
            }
            else
            {
                turnInfo.isBlack = true;
                turnInfo.isMe = true;
                blackPlayer.sendData("TurnInfo", turnInfo);
                turnInfo.isMe = false;
                whitePlayer.sendData("TurnInfo", turnInfo);
            }
        }

        public void prePareGame()
        {
            ServerLog.writeLog(string.Format("startGame:{0}", tableInfo.gameNo));
            var whiteDolInfo = whitePlayer.dolsInfo;
            var blackDolInfo = blackPlayer.dolsInfo;

            isNowPlayerBlack = false;
            
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

            gameState = GameTableState.PLAYING;
            sendTurnInfo();
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

                foreach(GamePlayer player in gamePlayers)
                {
                    if (gamePlayer.myDeviceID == player.myDeviceID)
                    {
                        gamePlayers.Remove(player);
                        break;
                    }
                }

                gamePlayers.Add(gamePlayer);
                tableInfo.plyCount++;

                if (tableInfo.plyCount == 1)
                {
                    
                }

                if (tableInfo.plyCount == 2)
                {
                    int idx = 0;
                    foreach(GamePlayer player in gamePlayers)
                    {
                        if (idx == 0)
                        {
                            player.createDolInfo(false, player.myNickName);
                            whitePlayer = player;

                        }
                        else
                        {
                            gamePlayer.createDolInfo(true, player.myNickName);
                            blackPlayer = gamePlayer;

                        }                        
                        idx++;
                    }                    
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
                List<GamePlayer> removeList = new List<GamePlayer>();
                foreach(GamePlayer idxPlayer in gamePlayers)
                {
                    if(gamePlayer.myDeviceID == idxPlayer.myDeviceID)
                    {                        
                        removeList.Add(gamePlayer);                        
                        ServerLog.writeLog(string.Format("leaveGame {0} in GameNo:{1} plyCount:{2} isblack:{3}", gamePlayer.ID, tableInfo.gameNo, tableInfo.plyCount , gamePlayer.isBlack ));
                    }
                    else
                    {
                        CrashGameInfo crashinfo = new CrashGameInfo();
                        crashinfo.reason = 0;
                        idxPlayer.sendData("CrashGameInfo", crashinfo);
                    }
                }

                foreach(GamePlayer removeply in removeList)
                {
                    gamePlayers.Remove(removeply);
                    tableInfo.plyCount--;
                }                
            }
        }
        
        public void sendAllData(string pid , object sendObj)
        {
            if (whitePlayer!=null && blackPlayer != null)
            {
                whitePlayer.sendData(pid, sendObj);
                blackPlayer.sendData(pid, sendObj);
            }
        }

        public void moveInfoReq(MoveInfoReq req, bool isBlack)
        {
            if(gameState == GameTableState.PLAYING)
            {
                MoveInfoRes moveInfoRes = new MoveInfoRes();
                moveInfoRes.writeFromReqData(req);

                if(req.source.x != -1)
                {
                    sendAllData("MoveInfoRes", moveInfoRes);
                }                
                isNowPlayerBlack = !isNowPlayerBlack;
                sendTurnInfo();
            }            
        }                
    }
}
