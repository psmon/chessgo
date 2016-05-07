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



    
}

