using UnityEngine;
using WebSocketSharp;
using CommData;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Collections;

public class Game : MonoBehaviour {

    public static PlayDol selectedDol;
    public static PlayDol targetDol;
    public static PlayDol lastMovedDol;
    protected  PlayDol curPlayDol;    
    public static Dols    dols;
    public static bool isMyTurn;
    public static bool isMyDolColorBlack;
    public static bool isOffLineMode = false;
    private static WebSocket ws = null;
    private static Queue<string> packetList = new Queue<string>();
    protected Text txtServerState;

    protected Button btn_singGame;
    protected Button btn_pvpGame;
    
    
    protected string deviceId;

    protected Text txtMyPlyScore;
    protected Text txtOtherPlayScore;
    protected Text txtGameResult;
    protected Text txtTotalRemainTime;
    protected Text txtPrivateTime;
    
    protected Image panelResult;

    protected Image pannelHelp;
    protected Text txtHelp;
    

    protected int myPlyScore = 0;
    protected int otherPlyScore = 0;

    static public int local_turn = 1;

    protected bool serverInit = false;

    protected bool isNetworkPlay = false;
    
    protected GameInfo gameInfo = new GameInfo();
    protected float globalTimeLeft = 30.0f;
    protected float privateTimeLeft = 30.0f;



    // Use this for initialization
    void Start()
    {
        txtServerState = GameObject.Find("txt_serverstate").GetComponent<Text>();
        txtMyPlyScore = GameObject.Find("txtYourScore").GetComponent<Text>();
        txtOtherPlayScore = GameObject.Find("txtOtherScore").GetComponent<Text>();

        panelResult = GameObject.Find("panelResult").GetComponent<Image>();
        txtGameResult = GameObject.Find("txtGameResult").GetComponent<Text>();

        pannelHelp = GameObject.Find("pannelHelp").GetComponent<Image>();
        txtHelp = GameObject.Find("txtHelp").GetComponent<Text>();

        txtTotalRemainTime = GameObject.Find("txtTotalRemainTime").GetComponent<Text>();
        txtPrivateTime = GameObject.Find("txtPrivateTime").GetComponent<Text>();

        panelResult.enabled = false;
        txtGameResult.enabled = false;

        btn_singGame = GameObject.Find("Btn_Single").GetComponent<Button>();
        
        btn_pvpGame = GameObject.Find("Btn_Multi").GetComponent<Button>();
        
        //btn_singGame.onClick.AddListener(delegate { test("test"); });
        
    }
    

    void showResult(string text , bool isVisible)
    {
        panelResult.enabled = isVisible;
        txtGameResult.enabled = isVisible;
        txtGameResult.text = text;
    }
    
    void OnApplicationQuit()
    {
        if(ws!=null)
            ws.Close();        
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

    public void startLocalGame()
    {
        pannelHelp.enabled = false;
        txtHelp.enabled = false;
        dols.CleanDols();
        dols.offLineInit();        
        TurnInfo turnInfo = new TurnInfo();
        turnInfo.isMe = true;
        turnInfo.isBlack = false;
        local_turn = 1;
        sendLocalData("TurnInfo", turnInfo.ToString());
    }

    public void onStartGame_Single()
    {
        if (isNetworkPlay)
            return;

        Debug.Log("onStartGame_Single");
        Game.isOffLineMode = true;
        onStageInit();
        startLocalGame();

    }

    public void onStartGame_PVP()
    {
        if (isNetworkPlay)
            return;

        Debug.Log("onStartGame_PVP");
        dols.CleanDols();
        onStageInit();
        Game.isOffLineMode = false;
        txtServerState.text = "try Conecting";
        deviceId = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("GameStart");

        if (ws != null)
        {
            ws.Close();
            ws = null;
        }

        if (ws == null)
        {
            if (Debug.isDebugBuild)
            {
                ws = new WebSocket("ws://192.168.0.30:9100/GoGame");
            }
            else
            {
                ws = new WebSocket("ws://psmon.iptime.org:9100/GoGame");
            }
            
            ws.OnMessage += Ws_OnMessage;
            ws.OnError += Ws_OnError;
            ws.OnOpen += Ws_OnOpen;
            ws.OnClose += Ws_OnClose;
        }
                
        ws.Connect();
        Debug.Log("Client Start");

        serverInit = true;
    }

    public void onGameHelp()
    {
        Debug.Log("onStartGame_PVP");
    }

    public void onStageInit()
    {
        pannelHelp.enabled = false;
        txtHelp.enabled = false;

        myPlyScore = 0;
        otherPlyScore = 0;
        txtMyPlyScore.text = "MyScore:" + myPlyScore;
        txtOtherPlayScore.text = "OtherScore:" + otherPlyScore;
        txtServerState.text = "Start Game";
        showResult("", false);

    }

    public static void send(string sendData)
    {
        ws.Send(sendData);
    }

    public void SwapDolPos(VectorDol leftPos , VectorDol rightPos)
    {
        PlayDol left = Dols.getPlayDolByIdx(leftPos.x, leftPos.y);
        PlayDol right = Dols.getPlayDolByIdx(rightPos.x, rightPos.y);

        if(left!=null && right != null)
        {
            Vector3 oriPos = left.transform.position;
            //left.transform.position = right.transform.position;

            left.move_ani(right.transform.position);

            right.transform.position = oriPos;

            VectorDol temp = new VectorDol();
            temp.setPos(left.GetDolPos());

            left.SetDolPos(right.GetDolPos());
            right.SetDolPos(temp);
        }

        curPlayDol = left;
        WebDataRes chkRes = new WebDataRes();
        chkRes.pid = "CheckGame";
        packetList.Enqueue(chkRes.ToString());        
        
    }

    private void ProcessPackets()
    {
        string curWsData = "";

        lock (packetList)
        {
            if(packetList.Count > 0)
            {
                curWsData = packetList.Dequeue();
            }
        }

        if (curWsData.Length < 1)
            return;
        
        var jsonObject = JsonUtility.FromJson<WebDataRes>(curWsData);
        string pid = jsonObject.pid;

        switch (pid)
        {
            case "Connected":
                LoginInfo loginInfo = new LoginInfo();
                loginInfo.deviceId = deviceId;
                if (Debug.isDebugBuild)
                {
                    System.Random rnd = new System.Random();
                    int random = rnd.Next(1, 1000);
                    loginInfo.deviceId = deviceId + random;
                }
                ws.Send(loginInfo.ToString());
                txtServerState.text = "Conneted Server";
                break;
            case "Disconnected":
                dols.CleanDols();
                isNetworkPlay = true;
                txtServerState.text = "Disconneted Server";
                isNetworkPlay = false;
                //Application.Quit();
                break;
            case "LoginInfoRes":
                isOffLineMode = false;                
                LoginInfoRes loginRes = new LoginInfoRes();
                loginRes.FromJsonOverwrite(jsonObject.data);
                Debug.Log("LoginInfoRes: " + loginRes.ToString());
                if (loginRes.loginResult > 0)
                {
                    txtServerState.text = "Wait for MultiPlayer(You Can run SingleGame at waiting)";
                    QuickSeatReq quickSeat = new QuickSeatReq();
                    ws.Send(quickSeat.ToString());
                }
                else
                {
                    txtServerState.text = "Login Failed...";
                }                
                break;
            case "GameInfo":
                GameInfo gameInfoRes = new GameInfo();                
                gameInfoRes.FromJsonOverwrite(jsonObject.data);
                gameInfo = gameInfoRes;
                globalTimeLeft = (float)gameInfoRes.totalTimeBank;                
                break;
            case "DolsInfo":
                globalTimeLeft = (float)gameInfo.totalTimeBank;
                isOffLineMode = false;
                isNetworkPlay = true;
                DolsInfo dolsinfo = new DolsInfo();
                dolsinfo.FromJsonOverwrite(jsonObject.data);
                Debug.Log("DolsInfo: " + dolsinfo.ToString());

                if (dolsinfo.isBlack == false)
                {
                    dols.firstWplayDols = dolsinfo;

                }
                else
                {
                    dols.firstBplayDols = dolsinfo;
                    dols.CleanDols();
                    dols.InitDols();
                }
                onStageInit();                
                break;
            case "MoveInfoRes":
                MoveInfoRes moveInfoRes = new MoveInfoRes();
                moveInfoRes.FromJsonOverwrite(jsonObject.data);
                SwapDolPos(moveInfoRes.source, moveInfoRes.target);
                break;
            case "TurnInfo":
                TurnInfo turnInfo = new TurnInfo();
                turnInfo.FromJsonOverwrite(jsonObject.data);
                string txtTurnInfo = "";
                string currentDolColor = turnInfo.isBlack == true ? "black" : "white";

                if (turnInfo.isMe)
                {
                    txtTurnInfo = string.Format("Your({0}) turn", currentDolColor)  ;
                    isMyTurn = true;
                    isMyDolColorBlack = turnInfo.isBlack;
                    privateTimeLeft = gameInfo.privateTimeBank; 
                }
                else
                {
                    txtTurnInfo = string.Format("Wait other player({0}) Action", currentDolColor);
                    isMyTurn = false;
                    isMyDolColorBlack = !turnInfo.isBlack;
                    privateTimeLeft = gameInfo.privateTimeBank;
                }
                
                if (isOffLineMode)
                {
                    isMyDolColorBlack = turnInfo.isBlack;
                    txtTurnInfo = string.Format("{0} Turn", currentDolColor);
                }
                
                txtServerState.text = txtTurnInfo;
                break;
            case "CrashGameInfo":
                dols.CleanDols();
                txtServerState.text = "Other User Leaver, Wait Other Player";
                isNetworkPlay = false;
                break;
            case "CheckGame":
                if (isOffLineMode == false)
                {
                    if (isMyDolColorBlack == true && curPlayDol.GetMyDolType() == 2)
                    {
                        myPlyScore += Dols.checkGame(curPlayDol);
                    }

                    if (isMyDolColorBlack == true && curPlayDol.GetMyDolType() == 1)
                    {
                        otherPlyScore += Dols.checkGame(curPlayDol);
                    }

                    if (isMyDolColorBlack == false && curPlayDol.GetMyDolType() == 1)
                    {
                        myPlyScore += Dols.checkGame(curPlayDol);
                    }

                    if (isMyDolColorBlack == false && curPlayDol.GetMyDolType() == 2)
                    {
                        otherPlyScore += Dols.checkGame(curPlayDol);
                    }

                }    
                else
                {
                    if( curPlayDol.GetMyDolType() == 1)
                    {
                        myPlyScore += Dols.checkGame(curPlayDol);
                    }
                    else
                    {
                        otherPlyScore += Dols.checkGame(curPlayDol);
                    }

                }
                
                if (isOffLineMode == false)
                {
                    txtMyPlyScore.text = "MyScore:" + myPlyScore;
                    txtOtherPlayScore.text = "HeScore:" + otherPlyScore;
                }
                else
                {
                    txtMyPlyScore.text = "WhiteScore:" + myPlyScore;
                    txtOtherPlayScore.text = "BlackScore:" + otherPlyScore;
                }
                
                GameResultInfo gameResultInfo = new GameResultInfo();
                if (myPlyScore > gameInfo.winScore-1 )
                {
                    gameResultInfo.winnerColor = curPlayDol.GetMyDolType();
                    gameResultInfo.wiinerScore = myPlyScore;
                    gameResultInfo.loseScore = otherPlyScore;
                    gameResultInfo.wiinnerIsme = true;

                    if (isOffLineMode == false)
                    {
                        showResult("You Win", true);
                        ws.Send(gameResultInfo.ToString());
                    }
                    else
                    {
                        showResult("White Win", true);

                    }                    
                }

                if (otherPlyScore > gameInfo.winScore - 1 )
                {
                    gameResultInfo.winnerColor = curPlayDol.GetMyDolType();
                    gameResultInfo.wiinerScore = otherPlyScore;
                    gameResultInfo.loseScore = myPlyScore;
                    gameResultInfo.wiinnerIsme = false;

                    if (isOffLineMode == false)
                    {
                        showResult("You Lose", true);
                        ws.Send(gameResultInfo.ToString());
                    }
                    else
                    {
                        showResult("Black Win", true);

                    }                    
                }                
                break;
        }

    }

    private void Ws_OnMessage(object sender, MessageEventArgs e)
    {        
        packetList.Enqueue(e.Data);
        Debug.Log("Ws_OnMessage: " + e.Data);
        
    }

    private IEnumerator<WaitForSeconds> waitThenCallback(float time, System.Action callback)
    {
        yield return new WaitForSeconds(time);
        callback();
    }
    
    private void Ws_OnOpen(object sender, System.EventArgs e)
    {
        /*
        StartCoroutine(waitThenCallback(0, () => {
            // Run with Lamda in MainThread
            
        }));*/
        WebDataRes closeMsg = new WebDataRes();
        closeMsg.pid = "Connected";
        packetList.Enqueue(closeMsg.ToString());

    }

    private void Ws_OnClose(object sender, CloseEventArgs e)
    {
        WebDataRes closeMsg = new WebDataRes();
        closeMsg.pid = "Disconnected";
        packetList.Enqueue(closeMsg.ToString());
    }

    private void Ws_OnError(object sender, ErrorEventArgs e)
    {
        Debug.Log(e.Message);
        WebDataRes closeMsg = new WebDataRes();
        closeMsg.pid = "Disconnected";
        packetList.Enqueue(closeMsg.ToString());
    }

    void FixedUpdate()
    {
        if (PlayDol.isRunAnimation == true)
            return;

        if (isNetworkPlay && isOffLineMode == false)
        {
            if (globalTimeLeft < 0)
            {
                //GameOver();
                if (isOffLineMode == false)
                {

                }

                GameResultInfo gameResultInfo = new GameResultInfo();

                if (myPlyScore > otherPlyScore)
                {
                    gameResultInfo.winnerColor = isMyDolColorBlack == true ? 2 : 1;
                    gameResultInfo.wiinerScore = myPlyScore;
                    gameResultInfo.loseScore = otherPlyScore;
                    gameResultInfo.wiinnerIsme = true;
                    showResult("You Win", true);
                    ws.Send(gameResultInfo.ToString());
                }
                else if (myPlyScore < otherPlyScore)
                {
                    gameResultInfo.winnerColor = isMyDolColorBlack == true ? 1 : 2;
                    gameResultInfo.wiinerScore = otherPlyScore;
                    gameResultInfo.loseScore = myPlyScore;
                    gameResultInfo.wiinnerIsme = false;
                    showResult("You Lost", true);
                    ws.Send(gameResultInfo.ToString());
                }
                else
                {
                    gameResultInfo.winnerColor = isMyDolColorBlack == true ? 2 : 1;
                    gameResultInfo.wiinerScore = myPlyScore;
                    gameResultInfo.loseScore = otherPlyScore;
                    gameResultInfo.wiinnerIsme = true;
                    showResult("Drawgame", true);
                    ws.Send(gameResultInfo.ToString());
                }

                globalTimeLeft = gameInfo.totalTimeBank + 10;
            }
            else
            {
                globalTimeLeft -= Time.fixedDeltaTime;
                txtTotalRemainTime.text = string.Format("RemainTime:{0}", (int)globalTimeLeft);
            }

            if (isMyTurn)
            {
                if (0 < privateTimeLeft)
                {
                    privateTimeLeft -= Time.fixedDeltaTime;
                    txtPrivateTime.text = string.Format("You,Limit:{0}", (int)privateTimeLeft);
                }
                else
                {
                    if (Game.selectedDol != null)
                    {
                        Game.selectedDol.indicatorOff();
                    }

                    Game.selectedDol = null;
                    Game.targetDol = null;
                    Dols.SetOffAllCanMove();
                    MoveInfoReq moveInfoReq = new MoveInfoReq();
                    VectorDol nullPos = new VectorDol();
                    nullPos.x = -1;
                    moveInfoReq.source.setPos(nullPos);
                    moveInfoReq.target.setPos(nullPos);
                    send(moveInfoReq.ToString());
                    privateTimeLeft = gameInfo.privateTimeBank;
                }
            }
            else
            {
                if (0 < privateTimeLeft)
                {
                    privateTimeLeft -= Time.fixedDeltaTime;
                    txtPrivateTime.text = string.Format("He,Limit:{0}", (int)privateTimeLeft);
                }
                else
                {
                    privateTimeLeft = gameInfo.privateTimeBank;

                }
            }
        }

        ProcessPackets();

    }

    // Update is called once per frame
    void Update ()
    {

    }

    public static void sendLocalData(string pid, string data)
    {
        WebDataRes sendData = new WebDataRes();
        sendData.pid = pid;
        sendData.data = data;
        packetList.Enqueue(sendData.ToString());
    }

}
