using UnityEngine;
using WebSocketSharp;
using CommData;
using System.Collections.Generic;
using UnityEngine.UI;



public class Game : MonoBehaviour {

    public static PlayDol selectedDol;
    public static PlayDol targetDol;
    public static PlayDol lastMovedDol;
    public static Dols    dols;

    public static bool isMyTurn;
    public static bool isMyDolColorBlack;

    public static bool isOffLineMode = false;


    private static WebSocket ws = new WebSocket("ws://192.168.0.30/GoGame");

    protected Queue<string> packetList = new Queue<string>();

    protected Text txtServerState;

    protected Button btn_singGame;
    protected Button btn_pvpGame;

    protected string deviceId;

    // Use this for initialization
    void Start()
    {
        txtServerState = GameObject.Find("txt_serverstate").GetComponent<Text>();
        btn_singGame = GameObject.Find("Btn_Single").GetComponent<Button>();
        btn_pvpGame = GameObject.Find("Btn_Single").GetComponent<Button>();

        //btn_singGame.onClick.AddListener(delegate { test("test"); });

        txtServerState.text = "try Conecting";
        deviceId = SystemInfo.deviceUniqueIdentifier;        
        Debug.Log("GameStart");        
        ws.OnMessage += Ws_OnMessage;
        ws.OnError += Ws_OnError;
        ws.OnOpen += Ws_OnOpen;
        ws.OnClose += Ws_OnClose;        

        if(Game.isOffLineMode==false)
            ws.Connect();        
        
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

        if( 0 < Dols.checkGame(left))
        {
            //add socre

        }
        
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
                txtServerState.text = "Disconneted Server";
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
                txtServerState.text = "Start Game";
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
                if (turnInfo.isMe)
                {
                    txtTurnInfo = "your turn, your dol color is " + (turnInfo.isBlack == true ? "black" : "white") ;
                    isMyTurn = true;
                    isMyDolColorBlack = turnInfo.isBlack;
                }
                else
                {
                    txtTurnInfo = "wait other player Action, your dol color is " + (turnInfo.isBlack != true ? "black" : "white");
                    isMyTurn = false;
                    isMyDolColorBlack = !turnInfo.isBlack;

                }
                txtServerState.text = txtTurnInfo;
                break;
            case "CrashGameInfo":
                dols.CleanDols();
                txtServerState.text = "Other User Leaver, Wait Other Player";
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
    }

    // Update is called once per frame
    void Update ()
    {
        ProcessPackets();
    }
    
}
