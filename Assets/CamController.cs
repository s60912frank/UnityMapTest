using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour {
    private Transform trans;
    private float startX; //存cam位置
    private float startY;
    private GameObject map;
    private Vector3 dragOrigin;
    private float dragSpeed = 0.5f;
	// Use this for initialization
	void Start () {
        trans = gameObject.transform;
        startX = trans.position.x;
        startY = trans.position.y;
        map = GameObject.Find("Plane"); //存map主體等一下會用到
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
        if (trans.position.x - startX > 9)
        {
            Debug.Log(trans.position.x - startX);
            startX = trans.position.x;
            //xtile+1
            map.BroadcastMessage("GetNewTile", new int[] { 1, 0 }); //這可以呼叫該物體script中的function名稱
            
        }
        else if (trans.position.x - startX < -9)
        {
            //xtile-1
            startX = trans.position.x;
            map.BroadcastMessage("GetNewTile", new int[] { -1, 0 });
        }

        if (trans.position.y - startY > 9)
        {
            //ytile+1
            startY = trans.position.y;
            map.BroadcastMessage("GetNewTile", new int[] { 0, 1 });
        }
        else if (trans.position.y - startY < -9)
        {
            //ytile-1
            startY = trans.position.y;
            map.BroadcastMessage("GetNewTile", new int[] { 0, -1 });
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
