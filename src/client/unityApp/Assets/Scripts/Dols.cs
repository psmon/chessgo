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
    //private float firstY = -2.94f;
    private float firstY = -1.95f;
    private float dolGapX = 0.85f;
    private float dolGapY = 0.85f;

    // Use this for initialization
    void Start () {
        
        if(Game.isOffLineMode==true)
            offLineInit();

        Game.dols = this;
    }

    public void offLineInit()
    {
        firstBplayDols = new DolsInfo();
        firstBplayDols.isBlack = true;
        firstBplayDols.isMe = true;

        firstWplayDols = new DolsInfo();
        firstWplayDols.isBlack = false;
        firstWplayDols.isMe = false;

        for (int i = 0; i < 8; i++)
        {
            var addData = new VectorDol();
            addData.x = i;
            addData.y = 7;
            firstBplayDols.list.Add(addData);

            addData = new VectorDol();
            addData.x = i;
            addData.y = 0;
            firstWplayDols.list.Add(addData);
        }        

        InitDols();
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

    public static void SetOffAllCanMove()
    {             
        foreach (GameObject gameObj in allDols)
        {
            PlayDol pdol = gameObj.GetComponent<PlayDol>();
            pdol.SetOffCanMove();
        }        
    }

    public void askAIAction(int dolType, ref PlayDol sourceDol, ref PlayDol targetDol)
    {

        List<PlayDol> canMoveList = new List<PlayDol>();

        foreach (GameObject gameObj in allDols)
        {
            PlayDol pdol = gameObj.GetComponent<PlayDol>();

            if(pdol.GetMyDolType() == dolType)
            {
                List<PlayDol> moveList = canMoveDolList(pdol, true);
                if ( moveList.Count > 0)
                {
                    canMoveList.Add(pdol);                    
                }
            }            
        }

        System.Random breakRnd = new System.Random();
        int brkrandom = breakRnd.Next(0, canMoveList.Count);
        int addIdx = 0;
        int canObtain = 0;

        foreach (PlayDol aiDol in canMoveList)
        {
            List<PlayDol> moveList = canMoveDolList(aiDol, true);
            System.Random rnd = new System.Random();
            int random = rnd.Next(0, moveList.Count);
            sourceDol = aiDol;
            targetDol = moveList[random];
            foreach (PlayDol moveDol in moveList)
            {
                if( 0 < checkGame(moveDol, true, aiDol.GetMyDolType()))
                {
                    targetDol = moveDol;
                    return;
                }
            }            
        }

        foreach (PlayDol aiDol in canMoveList)
        {
            List<PlayDol> moveList = canMoveDolList(aiDol, true);
            System.Random rnd = new System.Random();
            int random = rnd.Next(0, moveList.Count);
            sourceDol = aiDol;
            targetDol = moveList[random];
                        
            foreach (PlayDol moveDol in moveList)
            {
                if (0 < checkGame(moveDol, true, aiDol.GetMyDolType()))
                {
                    targetDol = moveDol;
                    return;
                }
            }

            if (addIdx == brkrandom)
                break;

            addIdx++;
        }
        
    }

    public static List<PlayDol> canMoveDolList(PlayDol selectedDol, bool justGetData=false)
    {
        List<PlayDol> result = new List<PlayDol>();
        VectorDol selectPost =  selectedDol.GetDolPos();

        //LeftCechk...
        for(int idx = selectPost.x -1 ; -1<idx; idx--)
        {
            bool canMove = false;
            PlayDol curDol = getPlayDolByIdx(idx, selectPost.y);
            if (curDol == null)
                break;

            if(curDol.GetMyDolType() == 0)
            {
                canMove = true;
                if(justGetData==false)
                    curDol.SetOnCanMove();

                result.Add(curDol);
            }
            if (canMove == false)
                break;
        }

        //RightCheck
        for (int idx = selectPost.x +1; idx < 8; idx++)
        {
            bool canMove = false;
            PlayDol curDol = getPlayDolByIdx(idx, selectPost.y);
            if (curDol == null)
                break;

            if (curDol.GetMyDolType() == 0)
            {
                canMove = true;
                if (justGetData == false)
                    curDol.SetOnCanMove();
                result.Add(curDol);
            }
            if (canMove == false)
                break;

        }

        //Upcheck
        for (int idy = selectPost.y +1; idy < 8; idy++)
        {
            bool canMove = false;
            PlayDol curDol = getPlayDolByIdx(selectPost.x, idy );
            if (curDol == null)
                break;

            if (curDol.GetMyDolType() == 0)
            {
                canMove = true;
                if (justGetData == false)
                    curDol.SetOnCanMove();
                result.Add(curDol);
            }

            if (canMove == false)
                break;

        }

        //DownCheck
        for (int idy = selectPost.y -1; -1 <idy ; idy--)
        {
            bool canMove = false;
            PlayDol curDol = getPlayDolByIdx(selectPost.x, idy);
            if (curDol == null)
                break;

            if (curDol.GetMyDolType() == 0)
            {
                canMove = true;
                if (justGetData == false)
                    curDol.SetOnCanMove();
                result.Add(curDol);
            }

            if (canMove == false)
                break;
        }        
        return result;
    }
    

    public static int checkGame(PlayDol selectedDol,bool isPreCheck = false, int chkDolType = 1 )
    {
        int score = 0;
        List<PlayDol> result = new List<PlayDol>();

        VectorDol selectPost = selectedDol.GetDolPos();
        int selDolType = isPreCheck == false ? selectedDol.GetMyDolType() : chkDolType;

        //LeftCechk...                
        List<PlayDol> removeDols = new List<PlayDol>();
        for (int idx = selectPost.x - 1; -1 < idx; idx--)
        {
            PlayDol curDol = getPlayDolByIdx(idx, selectPost.y);
            if (curDol == null)
                break;

            if (curDol.GetMyDolType() == 0)
                break;

            if(idx == selectPost.x - 1)
            {
                if (curDol.GetMyDolType() == selDolType )
                    break;
            }

            if(selDolType != curDol.GetMyDolType())
            {
                removeDols.Add(curDol);
            }

            if (selDolType == curDol.GetMyDolType())
            {
                if (removeDols.Count > 0)
                {
                    score += removeDols.Count;
                    foreach(PlayDol removedol in removeDols)
                    {
                        if(isPreCheck==false)
                            removedol.SetDolColor(0);
                    }
                }
                break;
            }
        }

        //RightCheck
        removeDols = new List<PlayDol>();
        for (int idx = selectPost.x + 1; idx < 8; idx++)
        {            
            PlayDol curDol = getPlayDolByIdx(idx, selectPost.y);
            if (curDol == null)
                break;

            if (curDol.GetMyDolType() == 0)
                break;

            if (idx == selectPost.x + 1)
            {
                if (curDol.GetMyDolType() == selDolType )
                    break;
            }

            if (selDolType != curDol.GetMyDolType())
            {
                removeDols.Add(curDol);
            }

            if (selDolType == curDol.GetMyDolType())
            {
                if (removeDols.Count > 0)
                {
                    score += removeDols.Count;
                    foreach (PlayDol removedol in removeDols)
                    {
                        if (isPreCheck == false)
                            removedol.SetDolColor(0);
                    }
                }
                break;
            }

        }

        //Upcheck
        removeDols = new List<PlayDol>();
        for (int idy = selectPost.y + 1; idy < 8; idy++)
        {            
            PlayDol curDol = getPlayDolByIdx(selectPost.x, idy);
            if (curDol == null)
                break;

            if (curDol.GetMyDolType() == 0)
                break;

            if (idy == selectPost.y + 1)
            {
                if (curDol.GetMyDolType() == selDolType )
                    break;
            }

            if (selDolType != curDol.GetMyDolType())
            {
                removeDols.Add(curDol);
            }

            if (selDolType == curDol.GetMyDolType())
            {
                if (removeDols.Count > 0)
                {
                    score += removeDols.Count;
                    foreach (PlayDol removedol in removeDols)
                    {
                        if (isPreCheck == false)
                            removedol.SetDolColor(0);
                    }
                }
                break;
            }

        }

        //DownCheck
        removeDols = new List<PlayDol>();
        for (int idy = selectPost.y - 1; -1 < idy; idy--)
        {            
            PlayDol curDol = getPlayDolByIdx(selectPost.x, idy);
            if (curDol == null)
                break;

            if (curDol.GetMyDolType() == 0)
                break;

            if (idy == selectPost.y - 1)
            {
                if (curDol.GetMyDolType() == selDolType )
                    break;
            }

            if (selDolType != curDol.GetMyDolType())
            {
                removeDols.Add(curDol);
            }

            if (selDolType == curDol.GetMyDolType())
            {
                if (removeDols.Count > 0)
                {
                    score += removeDols.Count;
                    foreach (PlayDol removedol in removeDols)
                    {
                        if (isPreCheck == false)
                            removedol.SetDolColor(0);
                    }
                }
                break;
            }
        }
        return score;
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
