using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour {
    private Transform trans;
    private float startX; //存cam位置
    private float startY;
    private GameObject map;
    private Vector3 nowHit;
    private Vector3 dragOrigin;
    private float dragSpeed = 0.5f;
	// Use this for initialization
	void Start () {
        trans = gameObject.transform;
        startX = trans.position.x;
        startY = trans.position.y;
        map = GameObject.Find("Map"); //存map主體等一下會用到
    }
	
	// Update is called once per frame
	void Update () {
        //方向建移動cam
        if (Input.GetKey(KeyCode.UpArrow))
        {
            trans.Translate(Vector3.up * 0.5f);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            trans.Translate(Vector3.down * 0.5f);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            trans.Translate(Vector3.left * 0.5f);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            trans.Translate(Vector3.right * 0.5f);
        }


        //檢查cam是否移動超過了一定距離
        RaycastHit hit;
        if (Physics.Raycast(gameObject.transform.position, Vector3.forward, out hit))
        {
            nowHit = hit.transform.position;
            //Debug.Log(nowHit);
        }
        else
        {
            Vector2 diff = new Vector2(gameObject.transform.position.x - nowHit.x, gameObject.transform.position.y - nowHit.y).normalized;
            float angle = Vector2.Angle(Vector2.right, diff);
            if (angle < 45)
            {
                //Debug.Log("右");
                map.BroadcastMessage("GetNewTile", new int[] { 1, 0 });
            }
            else if (angle >= 45 && angle <= 135)
            {
                if (diff.y > 0)
                {
                    //Debug.Log("上");
                    map.BroadcastMessage("GetNewTile", new int[] { 0, 1 });
                }
                else
                {
                    //Debug.Log("下");
                    map.BroadcastMessage("GetNewTile", new int[] { 0, -1 });
                }
            }
            else if (angle > 135)
            {
                //Debug.Log("左");
                map.BroadcastMessage("GetNewTile", new int[] { -1, 0 });
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(-pos.x * dragSpeed, -pos.y * dragSpeed, 0);
        transform.Translate(move, Space.World);  
	}
}
