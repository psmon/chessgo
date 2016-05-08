using UnityEngine;
using WebSocketSharp;
using CommData;
using System.Collections.Generic;

public class Game : MonoBehaviour {

    public static PlayDol selectedDol;
    public static PlayDol targetDol;
    public static PlayDol lastMovedDol;
    public static Dols    dols;

    private WebSocket ws = new WebSocket("ws://192.168.0.30/GoGame");

    protected Queue<string> packetList = new Queue<string>();


    // Use this for initialization
    void Start()
    {
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        LoginInfo loginInfo = new LoginInfo();
        loginInfo.deviceId = deviceId;

        Debug.Log("GameStart");        
        ws.OnMessage += Ws_OnMessage;
        ws.OnError += Ws_OnError;
        ws.OnOpen += Ws_OnOpen;
        ws.OnClose += Ws_OnClose;        
        ws.Connect();        
        ws.Send( loginInfo.ToString() );
        
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
            case "LoginInfoRes":
                LoginInfoRes loginRes = new LoginInfoRes();
                loginRes.FromJsonOverwrite(jsonObject.data);
                Debug.Log("LoginInfoRes: " + loginRes.ToString());
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

                break;
        }

    }

    private void Ws_OnMessage(object sender, MessageEventArgs e)
    {        
        packetList.Enqueue(e.Data);
        Debug.Log("Ws_OnMessage: " + e.Data);
        
    }

    private void Ws_OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log(e.ToString());
    }

    private void Ws_OnOpen(object sender, System.EventArgs e)
    {
        Debug.Log(e.ToString());
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
