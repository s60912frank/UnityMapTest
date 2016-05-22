using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour {
    private Transform trans;
    private GameObject map;
    private Vector3 nowHit;
    private float panDiff;
	// Use this for initialization
	void Start () {
        trans = gameObject.transform;
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

        //從cam射ray,檢查cam是否離開了地圖塊,是就畫地圖
        RaycastHit hit;
        if (Physics.Raycast(gameObject.transform.position, Vector3.forward, out hit))
        {
            nowHit = hit.transform.position; //紀錄最後射到的地圖塊
        }
        else
        {
            Vector2 diff = new Vector2(trans.position.x - nowHit.x, trans.position.y - nowHit.y).normalized;
            float angle = Vector2.Angle(Vector2.right, diff);
            if (angle < 45)
            {
                map.BroadcastMessage("GetNewTile", new int[] { 1, 0 }); //跟正x軸夾角<45就是右方一格的地圖
            }
            else if (angle >= 45 && angle <= 135)
            {
                if (diff.y > 0)
                {
                    //Debug.Log("上");
                    map.BroadcastMessage("GetNewTile", new int[] { 0, 1 });//跟正x軸夾角45~135且y>0就是上方一格的地圖
                }
                else
                {
                    //Debug.Log("下");
                    map.BroadcastMessage("GetNewTile", new int[] { 0, -1 });//跟正x軸夾角45~135且y<0就是下方一格的地圖
                }
            }
            else if (angle > 135)
            {
                //Debug.Log("左");
                map.BroadcastMessage("GetNewTile", new int[] { -1, 0 });//跟正x軸夾角135~180就是左方一格的地圖
            }
        }

        //滑鼠滾輪縮放
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            trans.position += Vector3.forward * 0.4f;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            trans.position -= Vector3.forward * 0.4f;
        }

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            transform.Translate(-touchDeltaPosition * 0.025f);
        }

        //雙指縮放,未測試
        if (Input.touchCount == 2)
        {
			//Vector2 one = Input.GetTouch (0).deltaPosition;
			//Vector2 two = Input.GetTouch (1).deltaPosition;
			//Debug.Log ("ONE:" + (one - two).ToString());
            //float touchDelta = Mathf.Sqrt((Input.GetTouch(0).deltaPosition - Input.GetTouch(1).deltaPosition).sqrMagnitude);
            //trans.position -= Vector3.forward * touchDelta * 0.05f;

            if(Input.touches[0].phase == TouchPhase.Began && Input.touches[1].phase == TouchPhase.Began)
            {
                panDiff = Mathf.Sqrt((Input.touches[0].position - Input.touches[1].position).sqrMagnitude);
            }
            else if (Input.touches[0].phase == TouchPhase.Moved && Input.touches[1].phase == TouchPhase.Moved)
            {
                float diff = Mathf.Sqrt((Input.touches[0].position - Input.touches[1].position).sqrMagnitude);
                float dir = panDiff - diff;
                if (dir < 0)
                {
                    trans.position -= Vector3.forward * dir * 0.0015f;
                }
                else
                {
                    trans.position -= Vector3.forward * dir * 0.0015f;
                }
                Debug.Log(dir);
            }
        }

        //起始y=-10,-5時可視面積1/4所以zoom+1,-20時可視面積4倍所以zoom-1
        if (trans.position.z > -5)
        {
            Debug.Log("太大啦!");
            RequestNewZoomMap(1);
        }
        if (trans.position.z < -20)
        {
            Debug.Log("太小啦!");
            RequestNewZoomMap(-1);     
        }
	}

    private void RequestNewZoomMap(int diff)
    {
        //清空地圖
        GameObject[] gos = GameObject.FindGameObjectsWithTag("MapObj");
        foreach (GameObject go in gos)
        {
            Destroy(go);
        }
        //cam z變回-10
        trans.position = new Vector3(trans.position.x, trans.position.y, -10);
        //request
        map.BroadcastMessage("GetNewZoomTile", new object[] { new Vector2(trans.position.x, trans.position.y), diff });
    }
}
