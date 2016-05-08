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
    }

    [Serializable]
    public class DolsInfo : BaseWebData
    {
        public bool isBlack;
        public bool isMe;
        public List<VectorDol> list =  new List<VectorDol>();
        
        public void writeForArray(int[] arrayData,bool _isBlack)
        {
            isBlack = _isBlack;
            // max8
            for (int i=0; i<arrayData.Length; i++)
            {
                int idx = i % 8;
                int idy = _isBlack == true ? i / 8 + 5 : i / 8;
                
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


}

