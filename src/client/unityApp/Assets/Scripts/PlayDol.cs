using UnityEngine;
using CommData;
using System.Collections.Generic;

public class PlayDol : MonoBehaviour {

    public Sprite bDol;
    public Sprite wDol;
    public Sprite emtypDol;
    public Sprite canPoint;

    public GameObject indicator;
    public Camera myCamera;

    private SpriteRenderer sr;
    private int mydolType = 0;
    private Vector3 chkSize = new Vector3(0.6f, 0.6f, 0);

    private VectorDol dolPos = new VectorDol();

    private bool isCanMove = false;

    public float moveTime = 0.1f;           //Time it will take object to move, in seconds.        
    private float inverseMoveTime;

    public static bool isRunAnimation = false;


    protected System.Collections.IEnumerator Movement(Vector3 end)
    {
        //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
        //Square magnitude is used instead of magnitude because it's computationally cheaper.
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        isRunAnimation = true;

        //While that distance is greater than a very small amount (Epsilon, almost zero):
        while (sqrRemainingDistance > float.Epsilon)
        {
            
            //Find a new position proportionally closer to the end, based on the moveTime
            Vector3 newPostion = Vector3.MoveTowards(this.transform.position, end,  Time.deltaTime* 4f);

            //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            //rb2D.MovePosition(newPostion);
            this.transform.position = newPostion;

            //Recalculate the remaining distance after moving.
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;

            if(sqrRemainingDistance <= 0)
            {
                isRunAnimation = false;
            }

            //Return and loop until sqrRemainingDistance is close enough to zero to end the function
            yield return null;
        }
    }

    public void move_ani(Vector3 target)
    {
        StartCoroutine(Movement(target));

    }

    // Use this for initialization
    void Start () {
        sr = GetComponent<SpriteRenderer>();
        //SetDolColor(0);

    }    

    void onSelectDol()
    {
        if(Game.isOffLineMode==false)
        {
            if (Game.isMyTurn == false)
                return;

            if (mydolType == 1 && Game.isMyDolColorBlack == true)
                return;

            if (mydolType == 2 && Game.isMyDolColorBlack == false)
                return;
        }

        /*
        if (Game.lastMovedDol != null)
        {
            if (Game.lastMovedDol.name == this.name)
            {
                Debug.Log(string.Format("DuplicatedBound - SelectedDol:{0}", this));
                return;
            }
        }*/

        Game.selectedDol = this;
        indicator.transform.position = transform.position;
        indicator.SetActive(true);


        Dols.SetOffAllCanMove();
        Dols.canMoveDolList(this);

        Debug.Log(string.Format("Bound - SelectedDol:{0}", this));
    }

    void onSelectTarget()
    {
        if (Game.selectedDol != null)
        {
            bool canMove = false;
            foreach(PlayDol chkDol in Dols.canMoveDolList(Game.selectedDol , true))
            {
                if (chkDol == null)
                    continue;

                if( chkDol.GetDolPos().x == this.GetDolPos().x  && chkDol.GetDolPos().y == this.GetDolPos().y)
                {
                    canMove = true;
                    break;
                }

            }
            if (canMove == true)
            {
                Game.targetDol = this;
                Debug.Log(string.Format("Bound - Taeget:{0}", this));

            }
            
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

            Dols.SetOffAllCanMove();
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

    public void SetOnCanMove()
    {
        if(mydolType ==0)
        {
            sr = GetComponent<SpriteRenderer>();
            sr.sprite = canPoint;
            isCanMove = true;
        }
    }

    public void SetOffCanMove()
    {
        if (mydolType == 0)
        {
            sr = GetComponent<SpriteRenderer>();
            sr.sprite = emtypDol;
            isCanMove = false;
        }
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
