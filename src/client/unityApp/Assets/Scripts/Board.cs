using UnityEngine;


public class Board : MonoBehaviour {

    public Material mat;
    

    private int colRowCount = 8;   //15x15
    static public int rectSize = 30;
    static public int marginLeft = 10;
    static public int marginBottom = 50;
    private int zOrder = 0;
    
    void Start()
    {

    }




    void Update()
    {

    }
    void OnRenderObject()
    {
        createLine();

    }

    //ViewportToWorldPoint
    //ScreenToWorldPoint
    //WorldToScreenPoint
    //ScreenToViewportPoint
    void createLine()
    {
        if (!mat)
        {
            Debug.LogError("Please Assign a material on the inspector");
            return;
        }
        GL.PushMatrix();
        mat.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(Color.red);

        for (int cIdx = 0; cIdx < colRowCount; cIdx++)
        {
            Vector3 startPos = Camera.main.ScreenToViewportPoint(new Vector3(marginLeft + cIdx * rectSize, marginBottom, zOrder));
            Vector3 endPos = Camera.main.ScreenToViewportPoint(new Vector3(marginLeft + cIdx * rectSize, (colRowCount - 1) * rectSize + marginBottom, zOrder));
            GL.Vertex(startPos);
            GL.Vertex(endPos);
        }

        for (int cIdx = 0; cIdx < colRowCount; cIdx++)
        {
            Vector3 startPos = Camera.main.ScreenToViewportPoint(new Vector3(marginLeft, marginBottom + cIdx * rectSize, zOrder));
            Vector3 endPos = Camera.main.ScreenToViewportPoint(new Vector3((colRowCount - 1) * rectSize + marginLeft, marginBottom + cIdx * rectSize, zOrder));
            GL.Vertex(startPos);
            GL.Vertex(endPos);
        }

        GL.End();
        GL.PopMatrix();

        
    }

    void OnPostRender()
    {

    }


}
