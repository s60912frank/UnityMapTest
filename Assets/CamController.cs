using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour {
    private Transform trans;
    private float startX;
    private float startY;
    private GameObject map;
    private Vector3 dragOrigin;
    private float dragSpeed = 0.5f;
	// Use this for initialization
	void Start () {
        trans = gameObject.transform;
        startX = trans.position.x;
        startY = trans.position.y;
        map = GameObject.Find("Plane");
    }
	
	// Update is called once per frame
	void Update () {
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
        //if(trans.position.y > MapProcessor.mapBoundaries[0] ||
        //    trans.position.y < MapProcessor.mapBoundaries[1] ||
        //    trans.position.x < MapProcessor.mapBoundaries[2] ||
        //    trans.position.x > MapProcessor.mapBoundaries[3])
        //{
        //    //Debug.Log("OUT!!!!");
        //}

        if (trans.position.x - startX > 9)
        {
            Debug.Log(trans.position.x - startX);
            startX = trans.position.x;
            //xtile+1
            map.BroadcastMessage("GetNewTile", new int[] { 1, 0 });
            
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
