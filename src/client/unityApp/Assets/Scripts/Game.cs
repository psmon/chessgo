using UnityEngine;
using WebSocketSharp;

public class Game : MonoBehaviour {

    public static PlayDol selectedDol;
    public static PlayDol targetDol;
    public static PlayDol lastMovedDol;

    private WebSocket ws = new WebSocket("ws://192.168.0.30/GoGame");

    // Use this for initialization
    void Start()
    {
        Debug.Log("GameStart");
        
        ws.OnMessage += Ws_OnMessage;
        ws.OnError += Ws_OnError;
        ws.OnOpen += Ws_OnOpen;
        ws.OnClose += Ws_OnClose;        
        ws.Connect();        
        ws.Send("BALUS");
        
    }

    private void Ws_OnMessage(object sender, MessageEventArgs e)
    {
        Debug.Log("Laputa says: " + e.Data);
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
    void Update () {
	
	}
    
}
