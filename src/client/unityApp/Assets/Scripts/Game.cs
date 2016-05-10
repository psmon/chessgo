using UnityEngine;
using WebSocketSharp;
using CommData;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

public class Game : MonoBehaviour {

    public static PlayDol selectedDol;
    public static PlayDol targetDol;
    public static PlayDol lastMovedDol;
    protected  PlayDol curPlayDol;    
    public static Dols    dols;
    public static bool isMyTurn;
    public static bool isMyDolColorBlack;
    public static bool isOffLineMode = false;
    private static WebSocket ws = new WebSocket("ws://192.168.0.30/GoGame");
    protected Queue<string> packetList = new Queue<string>();
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
        btn_singGame.enabled = false;
        btn_pvpGame = GameObject.Find("Btn_Single").GetComponent<Button>();
        btn_pvpGame.enabled = false;
        BtnHelp = GameObject.Find("BtnHelp").GetComponent<Button>();
        btn_pvpGame.enabled = false;


        //btn_singGame.onClick.AddListener(delegate { test("test"); });

        txtServerState.text = "try Conecting";
        deviceId = SystemInfo.deviceUniqueIdentifier;        
        Debug.Log("GameStart");        
        ws.OnMessage += Ws_OnMessage;
        ws.OnError += Ws_OnError;
        ws.OnOpen += Ws_OnOpen;
        ws.OnClose += Ws_OnClose;        

        if (Game.isOffLineMode == false)
        {
            ws.Connect();
        }
        Debug.Log("Client Start");
    }

    void showResult(string text , bool isVisible)
    {
        panelResult.enabled = isVisible;
        txtGameResult.enabled = isVisible;
        txtGameResult.text = text;
    }

    void OnApplicationQuit()
    {
        ws.Close();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

    public void onStartGame_Single()
    {
        Debug.Log("onStartGame_Single");
    }

    public void onStartGame_PVP()
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
                Application.Quit();
                break;
            case "LoginInfoRes":
                LoginInfoRes loginRes = new LoginInfoRes();
                loginRes.FromJsonOverwrite(jsonObject.data);
                Debug.Log("LoginInfoRes: " + loginRes.ToString());
                txtServerState.text = "Wair for Other Player";
                break;
            case "DolsInfo":
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
                }
                else
                {
                    txtTurnInfo = string.Format("Wait other player({0}) Action", currentDolColor);
                    isMyTurn = false;
                    isMyDolColorBlack = !turnInfo.isBlack;

                }
                txtServerState.text = txtTurnInfo;
                break;
            case "CrashGameInfo":
                dols.CleanDols();
                txtServerState.text = "Other User Leaver, Wait Other Player";
                break;
            case "CheckGame":                                
                if(isMyDolColorBlack==true && curPlayDol.GetMyDolType() == 2)
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
                
                txtMyPlyScore.text = "MyScore:" + myPlyScore;
                txtOtherPlayScore.text = "EnemyScore:" + otherPlyScore;

                GameResultInfo gameResultInfo = new GameResultInfo();
                if (myPlyScore > 4)
                {
                    gameResultInfo.winnerColor = curPlayDol.GetMyDolType();
                    gameResultInfo.wiinerScore = myPlyScore;
                    gameResultInfo.loseScore = otherPlyScore;
                    gameResultInfo.wiinnerIsme = true;
                    showResult("You Win", true);
                    ws.Send(gameResultInfo.ToString());
                }

                if (otherPlyScore > 4)
                {
                    gameResultInfo.winnerColor = curPlayDol.GetMyDolType();
                    gameResultInfo.wiinerScore = otherPlyScore;
                    gameResultInfo.loseScore = myPlyScore;
                    gameResultInfo.wiinnerIsme = false;
                    showResult("You Lose", true);
                    ws.Send(gameResultInfo.ToString());
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

    private void Ws_OnClose(object sender, CloseEventArgs e)
    {
        WebDataRes closeMsg = new WebDataRes();
        closeMsg.pid = "Disconnected";
        packetList.Enqueue(closeMsg.ToString() );        
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
    
}
