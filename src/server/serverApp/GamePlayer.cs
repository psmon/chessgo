﻿using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using CommData;
using System.Collections.Generic;
using System.Net;
//using System.Windows;
using System.Web.Script.Serialization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace serverApp
{
    public class GamePlayer : WebSocketBehavior
    {
        protected GameActor myGame = null;
        public string myDeviceID;
        public string myNickName;
        public bool isBlack;
        public DolsInfo dolsInfo;
        protected Timer timer = new Timer(15000);

        public GamePlayer()
        {            
            timer.Elapsed += Timer_Elapsed;
            ServerLog.writeLog(string.Format("Creator GamePlayer:{0}", GetHashCode() ));            
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            KeepAlive keepAlive = new KeepAlive();
            sendData("KeepAlive", keepAlive);
            ServerLog.writeLog("Ping..{0}"+ID);
        }

        protected override void OnOpen()
        {
            ServerLog.writeLog(string.Format("OnOpen GamePlayer:{0}", ID));            

        }

        public GameActor GetMyGameActor()
        {
            if (myGame == null)
            {
                ServerLog.writeLog(string.Format("myGame is null"));
            }
            return myGame;
        }

        protected override void OnError(ErrorEventArgs e)
        {
            timer.Stop();
            if (myGame != null)
            {                
                ServerApp.getGameTableActor().leaveGame(this);
                myGame = null;
            }

            ServerLog.writeLog( string.Format("Onerror GamePlayer:{0} , {1}" , ID , e.Message )  );
        }

        protected override void OnClose(CloseEventArgs e)
        {
            timer.Stop();
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
                            myNickName = loginInfo.nickName;
                            loginRes.loginResult = 1;
                        }                        
                        sendData("LoginInfoRes", loginRes);
                        if (loginRes.loginResult > 0)
                        {
                            GameInfo gameinfo = new GameInfo();
                            sendData("GameInfo", gameinfo);
                        }                        
                        break;
                    case "QuickSeatReq":                        
                        myGame = ServerApp.getGameTableActor().quickJoin(this);
                        break;
                    case "MoveInfoReq":
                        MoveInfoReq moveInfo = new JavaScriptSerializer().ConvertToType<MoveInfoReq>(jsonObject);
                        myGame.moveInfoReq(moveInfo, isBlack);                        
                        break;
                    case "GameResultInfo":
                        GameResultInfo gameResult = new JavaScriptSerializer().ConvertToType<GameResultInfo>(jsonObject);
                        if(gameResult.wiinnerIsme == true)
                        {
                            Task resultTask = new Task(() =>
                            {
                                Task.Delay(5000).Wait();
                                myGame.prePareGame();
                            });
                            resultTask.Start();
                        }
                        break;
                }                                                
            }
            catch(Exception ex)
            {
                ServerLog.writeLog(ex.Message);
            }
            
        }

        public DolsInfo createDolInfo(bool _isBlack , string nickName)
        {
            var result = new DolsInfo();
            Random rnd = new Random();
            isBlack = _isBlack;

            int[] dolsarray = new int[] { 1,0,0,0,1,0,0,0,
                                          0,1,0,1,0,1,0,1,
                                          0,0,1,0,0,0,1,0 };            

            //dolsarray = dolsarray.OrderBy(x => rnd.Next()).ToArray();            

            result.writeForArray(dolsarray, _isBlack);
            result.nickName = nickName;
            dolsInfo = result;
            
            return result;
        }
    }
}
