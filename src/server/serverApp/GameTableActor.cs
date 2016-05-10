using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommData;
namespace serverApp
{
    public class GameTableActor
    {
        TableInfoList serviceGameList = new TableInfoList();

        Dictionary<int, GameActor> gameList = new Dictionary<int, GameActor>(); 

        public GameTableActor()
        {
            //Todo : dummy Data to atg
            for(int gameNo = 0; gameNo < 10; gameNo++)
            {
                GameActor game = new GameActor();
                game.createGameTable(gameNo);
                gameList[gameNo] = game;
            }
        }

        public void leaveGame(GamePlayer gamePlayer)
        {
            foreach (int gameID in gameList.Keys)
            {
                GameActor curGame = gameList[gameID];
                if(curGame.getTableInfo().gameNo == gamePlayer.GetMyGameActor().getTableInfo().gameNo)
                {
                    curGame.leaveGame(gamePlayer);
                }                
            }
        }

        public GameActor quickJoin(GamePlayer gamePlayer)
        {
            GameActor bestGame = null;
            foreach (int gameID in gameList.Keys)
            {
                GameActor curGame = gameList[gameID];
                if (curGame.getAvableUserCount() == 2)
                {
                    bestGame = curGame;
                }

                if (curGame.getAvableUserCount() == 1)
                {
                    bestGame = curGame;                    
                    break;
                }
            }

            if (bestGame != null)
            {
                bestGame.joinGame(gamePlayer);
            }
            
            return bestGame;
        }        
    }
}
