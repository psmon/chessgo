using UnityEngine;
using CommData;


public class PlayDol : MonoBehaviour {

    public Sprite bDol;
    public Sprite wDol;
    public Sprite emtypDol;
    public GameObject indicator;
    public Camera myCamera;

    private SpriteRenderer sr;
    private int mydolType = 0;
    private Vector3 chkSize = new Vector3(0.6f, 0.6f, 0);

    private VectorDol dolPos = new VectorDol();

    // Use this for initialization
    void Start () {
        sr = GetComponent<SpriteRenderer>();
        //SetDolColor(0);

    }

    void onSelectDol()
    {
        if (Game.lastMovedDol != null)
        {
            if (Game.lastMovedDol.name == this.name)
            {
                Debug.Log(string.Format("DuplicatedBound - SelectedDol:{0}", this));
                return;
            }
        }

        Game.selectedDol = this;
        indicator.transform.position = transform.position;
        indicator.SetActive(true);
        Debug.Log(string.Format("Bound - SelectedDol:{0}", this));
    }

    void onSelectTarget()
    {
        if (Game.selectedDol != null)
        {
            Game.targetDol = this;
            Debug.Log(string.Format("Bound - Taeget:{0}", this));
        }
    }

    void onRemoveDol()
    {
        SetDolColor(0);
        indicator.SetActive(false);
    }


    void onMoveDol()
    {
        if (Game.selectedDol != null && Game.targetDol != null && mydolType == 0)
        {
            Game.lastMovedDol = Game.selectedDol;
            Debug.Log(string.Format("Bound - SwapDol:{0} {1}", Game.selectedDol.name, Game.targetDol.name));

            //SwapDolPos(ref Game.selectedDol, ref Game.targetDol);

            MoveInfoReq moveInfoReq = new MoveInfoReq();            
            moveInfoReq.source.setPos(Game.selectedDol.dolPos);
            moveInfoReq.target.setPos(Game.targetDol.dolPos);

            Game.send(moveInfoReq.ToString());

            Game.selectedDol = null;
            Game.targetDol = null;
            indicator.SetActive(false);

        }

    }

    void CheckMouse(int mouseEvt=1)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint( new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0) );
        mousePos.z = 0;

        Vector3 objPos = new Vector3(sr.transform.position.x, sr.transform.position.y, 0);        
        //Debug.Log(string.Format("Chk - Mouse:{0} Obj:{1}", mousePos, objPos ));
        
        Bounds myBound = new Bounds(sr.bounds.center, chkSize);

        if ( true == myBound.Contains(mousePos))
        {
            //Debug.Log(string.Format("Bound - {0} {1} {2}", name, mousePos, objPos));
            if (mouseEvt == 0)
            {
                if (mydolType > 0)
                {
                    onSelectDol();

                }

                if (mydolType == 0)
                {
                    onSelectTarget();                    
                }
                onMoveDol();
                
            }

            if (mouseEvt == 1)
            {
                onRemoveDol();                
            }            
        }

    }

    public int GetMyDolType()
    {
        return mydolType;
    }

    public void SetDolColor(int dolType)
    {
        mydolType = dolType;
        switch (dolType)
        {
            case 0:
                sr = GetComponent<SpriteRenderer>();
                sr.sprite = emtypDol;                
                break;
            case 1:
                sr = GetComponent<SpriteRenderer>();
                sr.sprite = wDol;
                break;
            case 2:
                sr = GetComponent<SpriteRenderer>();
                sr.sprite = bDol;
                break;
        }        
    }

    public void SetDolPos(int x , int y)
    {
        dolPos.x = x;
        dolPos.y = y;
    }

    public void SetDolPos(VectorDol pos)
    {
        dolPos.x = pos.x;
        dolPos.y = pos.y;
    }

    public VectorDol GetDolPos()
    {
        return dolPos;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonUp(0))
            CheckMouse(0);
        
        if (Input.GetMouseButtonUp(1))
            CheckMouse(1);


    }
}
