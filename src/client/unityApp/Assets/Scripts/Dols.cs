using UnityEngine;
using System.Collections;

public class Dols : MonoBehaviour {

    public GameObject playDol;

    private GameObject[] wplayDols = new GameObject[8];
    private GameObject[] bplayDols = new GameObject[8];

    // Use this for initialization
    void Start () {
        InitDols();


    }

    void Update()
    {
        Vector3 viewPos = Camera.main.ScreenToViewportPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));

        //Debug.Log(string.Format("{0} {1} == {2} {3}", Input.mousePosition.x, Input.mousePosition.y , viewPos.x, viewPos.y));
    } 

    Vector3 getPosByIdx(float idxX, float idxY)
    {
        // -0.294 / -2.86

        float firstX = -2.94f;
        float firstY = -2.94f;
        float dolGapX = 0.85f;
        float dolGapY = 0.85f;

        float screenX = idxX * dolGapX + firstX;
        float screenY = idxY * dolGapY + firstY;

        Vector3 oriPos = new Vector3(screenX, screenY, 0);
        Vector3 viewPos = Camera.main.ScreenToViewportPoint(new Vector3(screenX, screenY, 0));
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(viewPos);
        Vector3 worldPos2 = Camera.main.ViewportToWorldPoint(new Vector3(screenX,  screenY, 0));

        Debug.Log(string.Format("{0} {1} == {2} {3}", screenX, screenY, worldPos.x, worldPos.y));
        worldPos.z = 0;
        worldPos2.z = 0;
        

        return oriPos;
    }

    void InitDols()
    {
        //GameObject dol = (GameObject)Instantiate(Resources.Load("Images"));
        for(int idx=0; idx<8; idx++)
        {
            for(int idy=0; idy<8; idy++)
            {

                GameObject dol = Instantiate(playDol, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;                
                dol.transform.position = getPosByIdx(idx, idy);

                PlayDol pdol = dol.GetComponent<PlayDol>();

                bool debugAllWhite = false;

                if (idy == 0 || debugAllWhite == true)
                {
                    pdol.SetDolColor(1);
                    wplayDols[idx] = dol;
                    dol.name = "whiteDol-"+ idx;

                }
                else if (idy == 7)
                {
                    pdol.SetDolColor(2);
                    bplayDols[idx] = dol;
                    dol.name = "blackDol-" + idx;
                }
                else
                {
                    pdol.SetDolColor(0);
                    dol.name = "empty-" + idx + ":" + idy;
                }
                //((dol.GetComponentInChildren<PlayDol>) as PlayDol).SetDolColor(true);
                //dol.get

                //(dos as Sprite).

                
            }
            
        }

    }

}
