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
            Vector2 diff = new Vector2(trans.position.x - nowHit.x, trans.position.y - nowHit.y).normalized;
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

        //if (Input.GetMouseButtonDown(0))
        //{
        //    dragOrigin = Input.mousePosition;
        //    return;
        //}

        //if (!Input.GetMouseButton(0)) return;

        //Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        //Vector3 move = new Vector3(-pos.x * dragSpeed, -pos.y * dragSpeed, 0);
        //transform.Translate(move, Space.World);

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            trans.position += Vector3.forward * 0.4f;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            trans.position -= Vector3.forward * 0.4f;
        }

        if (trans.position.z > -5)
        {
            Debug.Log("太大啦!");
            GameObject[] gos = GameObject.FindGameObjectsWithTag("MapObj");
            foreach (GameObject go in gos)
            {
                Destroy(go);
            }
            trans.position = new Vector3(trans.position.x, trans.position.y, -10);
            map.BroadcastMessage("GetNewZoomTile", new object[] { new Vector2(trans.position.x, trans.position.y), 1 });
        }
        if (trans.position.z < -20)
        {
            Debug.Log("太小啦!");
            GameObject[] gos = GameObject.FindGameObjectsWithTag("MapObj");
            foreach (GameObject go in gos)
            {
                Destroy(go);
            }
            trans.position = new Vector3(trans.position.x, trans.position.y, -10);
            map.BroadcastMessage("GetNewZoomTile", new object[] { new Vector2(trans.position.x, trans.position.y), -1 });
            
        }
	}
}
