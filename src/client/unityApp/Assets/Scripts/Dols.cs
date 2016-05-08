using UnityEngine;
using System.Collections.Generic;
using CommData;

public class Dols : MonoBehaviour {

    public GameObject playDol;

    private GameObject[] wplayDols = new GameObject[8];
    private GameObject[] bplayDols = new GameObject[8];

    private static List<GameObject> allDols = new List<GameObject>();

    
    //private int[] firstBplayDols = new int[] { 0 * 100+ 7 , 1*100+ 7 ,  2*100 + 7 , 3*100 + 7 , 4*100 + 7 , 5*100 + 7 , 6*100+ 7, 7 * 100 + 7 };
    //private int[] firstWplayDols = new int[] { 0 * 100+ 0, 1 * 100 + 0 , 2 * 100 + 0 , 3 * 100 + 0 , 4 * 100 + 0 , 5 * 100 + 0 , 6 * 100 + 0, 7 * 100 + 0 };

    public DolsInfo firstBplayDols;
    public DolsInfo firstWplayDols;


    private float firstX = -2.94f;
    private float firstY = -2.94f;
    private float dolGapX = 0.85f;
    private float dolGapY = 0.85f;

    // Use this for initialization
    void Start () {
        //InitDols();
        Game.dols = this;

    }

    void Update()
    {
        Vector3 viewPos = Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        //Debug.Log(string.Format("{0} {1} == {2} {3}", Input.mousePosition.x, Input.mousePosition.y , viewPos.x, viewPos.y));
    }
    
    int getDolTypeByIdx(int idx , int idy)
    {
        // 0:none ,1:white , 2:black
        int result = 0;

        foreach(VectorDol curValue in firstWplayDols.list)
        {
            int parserX = curValue.x;
            int parserY = curValue.y;
            
            if (parserX == idx && parserY == idy)
                return 1;
        }

        foreach (VectorDol curValue in firstBplayDols.list)
        {
            int parserX = curValue.x;
            int parserY = curValue.y;

            if (parserX == idx && parserY == idy)
                return 2;
        }
        return result;
    } 

    Vector3 getPosByIdx(float idxX, float idxY)
    {
        // -0.294 / -2.86        
        float screenX = idxX * dolGapX + firstX;
        float screenY = idxY * dolGapY + firstY;

        Vector3 oriPos = new Vector3(screenX, screenY, 0);
        Vector3 viewPos = Camera.main.ScreenToViewportPoint(new Vector3(screenX, screenY, 0));
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewPos);
        Vector3 worldPos2 = Camera.main.ViewportToWorldPoint(new Vector3(screenX,  screenY, 0));

        //Debug.Log(string.Format("{0} {1} == {2} {3}", screenX, screenY, worldPos.x, worldPos.y));
        worldPos.z = 0;
        worldPos2.z = 0;
        

        return oriPos;
    }

    public void CleanDols()
    {
        foreach(GameObject obj in allDols)
        {
            Destroy(obj);
        }
        allDols.Clear();
        
    }

    public static PlayDol getPlayDolByIdx(int idx , int idy)
    {
        PlayDol result = null;
        foreach(GameObject gameObj in allDols)
        {
            PlayDol pdol = gameObj.GetComponent<PlayDol>();

            if(pdol.GetDolPos().x == idx && pdol.GetDolPos().y == idy)
            {
                result = pdol;
                break;
            }
        }
        return result;
    }

    public void InitDols()
    {
        //GameObject dol = (GameObject)Instantiate(Resources.Load("Images"));
        for(int idx=0; idx<8; idx++)
        {
            for(int idy=0; idy<8; idy++)
            {
                GameObject dol = Instantiate(playDol, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;                
                dol.transform.position = getPosByIdx(idx, idy);
                PlayDol pdol = dol.GetComponent<PlayDol>();
;

                if ( getDolTypeByIdx(idx, idy) ==1 )
                {
                    pdol.SetDolColor(1);
                    pdol.SetDolPos(idx, idy);
                    wplayDols[idx] = dol;
                    dol.name = "whiteDol-"+ idx;

                }
                else if (getDolTypeByIdx(idx, idy) == 2)
                {
                    pdol.SetDolColor(2);
                    pdol.SetDolPos(idx, idy);
                    bplayDols[idx] = dol;
                    dol.name = "blackDol-" + idx;
                }
                else
                {
                    pdol.SetDolColor(0);
                    pdol.SetDolPos(idx, idy);
                    dol.name = "empty-" + idx + ":" + idy;
                }

                allDols.Add(dol);

            }
            
        }

    }

}
