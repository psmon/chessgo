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
    protected Button BtnHelp;
    
    protected string deviceId;

    protected Text txtMyPlyScore;
    protected Text txtOtherPlayScore;
    protected Text txtGameResult;

    protected Image panelResult;
    

    protected int myPlyScore = 0;
    protected int otherPlyScore = 0;

    static public int local_turn = 1;

    protected bool serverInit = false;

    protected bool isNetworkPlay = false;
    

    // Use this for initialization
    void Start()
    {
        txtServerState = GameObject.Find("txt_serverstate").GetComponent<Text>();
        txtMyPlyScore = GameObject.Find("txtYourScore").GetComponent<Text>();
        txtOtherPlayScore = GameObject.Find("txtOtherScore").GetComponent<Text>();

        panelResult = GameObject.Find("panelResult").GetComponent<Image>();
        txtGameResult = GameObject.Find("txtGameResult").GetComponent<Text>();

        panelResult.enabled = false;
        txtGameResult.enabled = false;

        btn_singGame = GameObject.Find("Btn_Single").GetComponent<Button>();
        
        btn_pvpGame = GameObject.Find("Btn_Multi").GetComponent<Button>();
        
        BtnHelp = GameObject.Find("BtnHelp").GetComponent<Button>();
        

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
        showResult("", false);
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
            ws = new WebSocket("ws://192.168.0.30/GoGame");
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
                ws.Send(loginInfo.ToString());
                txtServerState.text = "Conneted Server";
                break;
            case "Disconnected":
                dols.CleanDols();
                txtServerState.text = "Disconneted Server-You Nedd Restat Application";
                isNetworkPlay = false;
                //Application.Quit();
                break;
            case "LoginInfoRes":
                isOffLineMode = false;
                isNetworkPlay = true;
                LoginInfoRes loginRes = new LoginInfoRes();
                loginRes.FromJsonOverwrite(jsonObject.data);
                Debug.Log("LoginInfoRes: " + loginRes.ToString());
                txtServerState.text = "Wait for MultiPlayer(You Can run SingleGame at waiting)";
                QuickSeatReq quickSeat = new QuickSeatReq();
                ws.Send(quickSeat.ToString() );
                break;
            case "DolsInfo":
                isOffLineMode = false;
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
                isNetworkPlay = true;

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
                }
                else
                {
                    txtTurnInfo = string.Format("Wait other player({0}) Action", currentDolColor);
                    isMyTurn = false;
                    isMyDolColorBlack = !turnInfo.isBlack;

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
                    txtOtherPlayScore.text = "EnemyScore:" + otherPlyScore;
                }
                else
                {
                    txtMyPlyScore.text = "WhiteScore:" + myPlyScore;
                    txtOtherPlayScore.text = "BlackScore:" + otherPlyScore;
                }


                
                GameResultInfo gameResultInfo = new GameResultInfo();
                if (myPlyScore > 4)
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

                if (otherPlyScore > 4)
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
    

    // Update is called once per frame
    void Update ()
    {
        if (PlayDol.isRunAnimation == true)
            return;

        ProcessPackets();
    }

    public static void sendLocalData(string pid, string data)
    {
        WebDataRes sendData = new WebDataRes();
        sendData.pid = pid;
        sendData.data = data;
        packetList.Enqueue(sendData.ToString());
    }

}
