using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommData
{

    //Server Share with Client
    public enum GamePlayState : int
    {

    }

    public enum GameTableState : int
    {
        NOTINIT = 0,
        READYFORPLAYER = 1,
        PLAYING = 2
    }

    [Serializable]
    public class BaseWebData
	{
        public override string ToString()
        {
            string json = JsonUtility.ToJson(this);
            return json;
        }

        public void FromJsonOverwrite(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
        //myObject = JsonUtility.FromJson<MyClass>(json);
        //JsonUtility.FromJsonOverwrite(json, myObject);
    }

    [Serializable]
    public class TableInfo : BaseWebData
    {
        public int gameNo;
        public int plyCount;
        public int plyMaxCount = 2;
        public int GetAvableUserCount()
        {
            return plyMaxCount - plyCount;
        }
    }

    [Serializable]
    public class TableInfoList : BaseWebData
    {
        public List<TableInfo> data = new List<TableInfo>();        
    }

    [Serializable]
    public class LoginInfo : BaseWebData
    {
        public string pid = "LoginInfo";
        public string deviceId;
        public string nickName;        
    }    

    [Serializable]
    public class LoginInfoRes : BaseWebData
    {
        public string pid = "LoginInfoRes";
        public int loginResult=0;
    }

    [Serializable]
    public class WebDataRes : BaseWebData
    {
        public string pid = "none";
        public string data = "";
    }

    [Serializable]
    public class VectorDol : BaseWebData
    {
        public int x;
        public int y;

        public void setPos(int _x, int _y)
        {
            x = _x;
            y = _y;
        }

        public void setPos(VectorDol copyDaya)
        {
            x = copyDaya.x;
            y = copyDaya.y;
        }

    }

    [Serializable]
    public class DolsInfo : BaseWebData
    {
        public bool isBlack;
        public bool isMe;
        public List<VectorDol> list =  new List<VectorDol>();
        public string nickName;

        public void writeForArray(int[] arrayData,bool _isBlack)
        {
            isBlack = _isBlack;
            // max8
            for (int i=0; i<arrayData.Length; i++)
            {
                int idx = i % 8;
                int idy = i / 8;

                if (_isBlack)
                {
                    if (idy == 0)
                        idy += 7;

                    if (idy == 1)
                        idy += 5;

                    if (idy == 2)
                        idy += 3;
                }
                
                if (arrayData[i] > 0)
                {                    
                    VectorDol dol = new VectorDol
                    {
                        x = idx,
                        y = idy
                    };
                    list.Add(dol);
                }               
            }
        }                
    }


    [Serializable]
    public class GameInfo : BaseWebData
    {
        public int totalTimeBank = 60 * 8;
        public int privateTimeBank = 30;
        public int winScore = 3;
    }
    

    [Serializable]
    public class MoveInfoReq : BaseWebData
    {
        public string pid = "MoveInfoReq";
        public VectorDol source = new VectorDol();
        public VectorDol target = new VectorDol();
    }

    [Serializable]
    public class MoveInfoRes : BaseWebData
    {
        public string pid = "MoveInfoRes";
        public VectorDol source = new VectorDol();
        public VectorDol target = new VectorDol();
        public void writeFromReqData(MoveInfoReq sourceData)
        {
            source.x = sourceData.source.x;
            source.y = sourceData.source.y;
            target.x = sourceData.target.x;
            target.y = sourceData.target.y;
        }
        
    }

    [Serializable]
    public class TurnInfo : BaseWebData
    {
        public bool isMe;
        public bool isBlack;
        public int limitTime = 30;
    }

    [Serializable]
    public class CrashGameInfo : BaseWebData
    {
        public int reason;        //0:다른유져 접속종료..
    }

    [Serializable]
    public class GameResultInfo : BaseWebData
    {
        public string pid = "GameResultInfo";
        public int winnerColor; // 0 -Draw , 1-White , 2-Black
        public bool wiinnerIsme;
        public int wiinerScore;
        public int loseScore;
    }

    [Serializable]
    public class KeepAlive : BaseWebData
    {
        public string pid = "KeepAlive";
        public int res;
    }

    [Serializable]
    public class QuickSeatReq : BaseWebData
    {
        public string pid = "QuickSeatReq";
        public int res;
    }

}

