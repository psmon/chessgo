﻿using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using CommData;
using System.Collections.Generic;
using System.Net;
//using System.Windows;
using System.Web.Script.Serialization;
using System.Linq;

namespace serverApp
{
    public class GamePlayer : WebSocketBehavior
    {
        public GameActor myGame;
        protected string myDeviceID;
        public bool isBlack;
        public DolsInfo dolsInfo;

        public GamePlayer()
        {
            
        }

        protected override void OnOpen()
        {            
            
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

        public void sendData(string pid , object dataobj)
        {
            WebDataRes sendData = new WebDataRes();
            sendData.pid = pid;            
            sendData.data = new JavaScriptSerializer().Serialize(dataobj);
            Send(new JavaScriptSerializer().Serialize(sendData));
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                ServerLog.writeLog(string.Format("Player:{0}, Msg:", ID, e.Data));
                string data = e.Data;

                object jsonObject = new JavaScriptSerializer().DeserializeObject(data);
                IDictionary<string, object> payload = (IDictionary<string, object>)jsonObject;                
                string pid = (string)payload["pid"];
                
                switch (pid)
                {
                    case "LoginInfo":                        
                        LoginInfo loginInfo = new JavaScriptSerializer().ConvertToType<LoginInfo>(jsonObject);
                        ServerLog.writeLog(string.Format("Loginin GamePlayer:{0} DeviceId:{1}", ID, loginInfo.deviceId));
                        LoginInfoRes loginRes = new LoginInfoRes();
                        //Check LoginInfo
                        if (loginInfo.deviceId.Length > 10)
                        {
                            myDeviceID = loginInfo.deviceId;
                            loginRes.loginResult = 1;
                        }                        
                        sendData("LoginInfoRes", loginRes);
                        myGame = ServerApp.getGameTableActor().quickJoin(this);
                        break;
                    case "MoveInfoReq":
                        MoveInfoReq moveInfo = new JavaScriptSerializer().ConvertToType<MoveInfoReq>(jsonObject);
                        myGame.moveInfoReq(moveInfo, isBlack);                        
                        break;
                }                
                                
            }
            catch(Exception ex)
            {
                ServerLog.writeLog(ex.Message);
            }
            
        }

        public DolsInfo createDolInfo(bool _isBlack)
        {
            var result = new DolsInfo();
            Random rnd = new Random();
            isBlack = _isBlack;

            int[] dolsarray = new int[] { 0,0,0,0,0,0,0,0,
                                          0,0,0,0,0,0,0,0,
                                          1,1,1,1,1,1,1,1 };

            dolsarray = dolsarray.OrderBy(x => rnd.Next()).ToArray();

            result.writeForArray(dolsarray, _isBlack);
            dolsInfo = result;
            
            return result;
        }
    }
}
