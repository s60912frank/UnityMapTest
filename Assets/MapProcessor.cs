using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapProcessor : MonoBehaviour {
    public const string API_KEY = "vector-tiles-vxQ7SnN";
    private const string MAP_TYPE = "boundaries,buildings,roads";
    private int xTile;  //儲存現在camera所在的地圖格(大概 不是很準)
    private int yTile;
    private List<int[]> drewTile; //儲存已畫地圖格的編號，判斷用
    public string url;
    private float latitude = 25.0488153f;
    private float longtitude = 121.445669f;
    private int zoom = 16; //放大倍率，1~19
	// Use this for initialization
	void Start () {
        int[] tile = WorldToTilePos(longtitude, latitude, zoom);
        xTile = tile[0]; //起始地圖格
        yTile = tile[1];
        requestMap(tile[0], tile[1]); //要第一塊地圖格
        drewTile = new List<int[]>(); 
        drewTile.Add(new int[] { tile[0], tile[1] }); //存第一塊
	}
	
	// Update is called once per frame
	void Update () {
        //沒幹嘛
	}

    public int[] WorldToTilePos(float lon, float lat, int zoom) //經緯度轉地圖格編號
    {
        int worldX = (int)Mathf.Floor((lon + 180.0f) / 360.0f * (1 << zoom));
        int worldY = (int)Mathf.Floor(((1.0f - Mathf.Log(Mathf.Tan(lat * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos(lat * Mathf.PI / 180.0f)) / Mathf.PI) / 2.0f * (1 << zoom)));
        float[] latlon = TileToWorldPos(worldX, worldY, zoom);
        longtitude = latlon[0]; //因為地圖格編號只能是整數，所以這裡將(0,0)設為第一塊地圖塊的中心
        latitude = latlon[1];
        return new int[] { worldX, worldY };
    }

    public float[] TileToWorldPos(float tile_x, float tile_y, int zoom) //地圖格編號轉經緯度
    {
        float n = Mathf.PI - ((2.0f * Mathf.PI * tile_y) / Mathf.Pow(2.0f, zoom));

        float x = (float)((tile_x / Mathf.Pow(2.0f, zoom) * 360.0) - 180.0);
        float y = (float)(180.0 / Mathf.PI * Mathf.Atan((Mathf.Exp(n) - Mathf.Exp(-n)) / 2));
        return new float[] { x, y };
    }

    private void requestMap(int xtile, int ytile) //要地圖塊
    {
        url = "https://vector.mapzen.com/osm/" + MAP_TYPE + "/" + zoom + "/" + xtile + "/" + ytile + ".json?api_key=" + API_KEY;
        Debug.Log(url);
        WWW request = new WWW(url);
        StartCoroutine(WaitForRequest(request));
    }

    IEnumerator WaitForRequest(WWW www) //當資料從伺服器回來會執行這個
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!");
            JsonProssor(www.text); //有資料就去處理囉
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    private void JsonProssor(string rawData)
    {
        JSONObject data = new JSONObject(rawData);
        JSONObject buildings = data["buildings"]["features"]; //讀取資料中的"building"節點下的"features"節點
        foreach (JSONObject obj in buildings.list) //讀取這個節點的陣列
        {
            List<Vector2> coords = new List<Vector2>();
            string type = obj["geometry"]["type"].ToString().Replace("\"", ""); //因為type讀出來會有兩個雙引號，所以去掉再判斷
            switch (type)
            {
                case "Point":
                    //點不知怎畫，然而好像也不是很重要
                    break;
                case "Polygon":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list[0].list) //polygon的座標陣列存在陣列[0]的陣列中
                    {
                        float[] world = coordTrans(float.Parse(co[0].ToString()), float.Parse(co[1].ToString())); //座標xy做一下轉換
                        coords.Add(new Vector3(world[0], world[1], 0)); //加到list中          
                    }
                    break;
                default:
                    Debug.Log(type);
                    break;
            }
            DrawBuildings(type, coords.ToArray()); //丟進去畫
        }

        JSONObject roads = data["roads"]["features"]; //讀取資料中的"roads"節點下的"features"節點
        foreach (JSONObject obj in roads.list)
        {
            List<Vector3> coords = new List<Vector3>();
            string type = obj["geometry"]["type"].ToString().Replace("\"", "");
            switch (type)
            {
                case "LineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list) //lineString的座標陣列存在陣列中
                    {
                        float[] world = coordTrans(float.Parse(co[0].ToString()), float.Parse(co[1].ToString()));
                        coords.Add(new Vector3(world[0], world[1], -0.1f)); //設成0好像會有點問題
                    }
                    break;
                case "MultiLineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list) //multiLineString的座標陣列存在陣列中
                    {
                        List<Vector3> smallLine = new List<Vector3>(); //看名稱就知道啦這個一個就有很多條線，所以一條線就丟去畫
                        foreach (JSONObject co2 in co.list)
                        {
                            float[] world = coordTrans(float.Parse(co2[0].ToString()), float.Parse(co2[1].ToString()));
                            smallLine.Add(new Vector3(world[0], world[1], -0.1f));
                        }
                        DrawRoads(smallLine.ToArray()); //去畫
                    }
                    break;
                default:
                    Debug.Log(type);
                    break;
            }
            DrawRoads(coords.ToArray()); //去畫
        }

        JSONObject boundaries = data["boundaries"]["features"]; //讀取資料中的"boundaries"節點下的"features"節點
        foreach (JSONObject obj in boundaries.list) //其實跟上面一樣
        {
            List<Vector3> coords = new List<Vector3>();
            string type = obj["geometry"]["type"].ToString().Replace("\"", "");
            switch (type)
            {
                case "LineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list)
                    {
                        float[] world = coordTrans(float.Parse(co[0].ToString()), float.Parse(co[1].ToString()));
                        coords.Add(new Vector3(world[0], world[1], 0));
                    }
                    break;
                case "MultiLineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list)
                    {
                        List<Vector3> smallLine = new List<Vector3>();
                        foreach (JSONObject co2 in co.list)
                        {
                            float[] world = coordTrans(float.Parse(co2[0].ToString()), float.Parse(co2[1].ToString()));
                            smallLine.Add(new Vector3(world[0], world[1], 0));
                        }
                        DrawRoads(smallLine.ToArray());
                    }
                    break;
                default:
                    Debug.Log(type);
                    break;
            }
            DrawRoads(coords.ToArray());
        }
    }

    private void DrawBuildings(string type, Vector2[] vertices2D) //畫建築物
    {
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++) //就只是2D轉3D補0
        {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, -0.1f); //一樣設0會有問題
        }
        switch (type)
        {
            case "Polygon":
                GameObject obj = new GameObject(); //創個新物體
                // Use the triangulator to get indices for creating triangles
                Triangulator tr = new Triangulator(vertices2D);
                int[] indices = tr.Triangulate();
                //Create the mesh
                Mesh msh = new Mesh();
                msh.Clear();
                msh.vertices = vertices;
                msh.triangles = indices;
                msh.RecalculateNormals();
                msh.RecalculateBounds();

                // Set up game object with mesh;
                MeshRenderer msgr = obj.AddComponent<MeshRenderer>();
                msgr.material = Resources.Load("building") as Material;
                MeshFilter filter = obj.AddComponent<MeshFilter>() as MeshFilter;
                filter.mesh = msh;
                obj.transform.parent = this.gameObject.transform; //附加在本plane下
                break;
            case "Point":
                //還是不知怎畫
                break;
        }
    }

    private void DrawRoads(Vector3[] vertices)
    {
        GameObject obj = new GameObject();
        LineRenderer line = obj.AddComponent<LineRenderer>();
        //set the number of points to the line
        line.SetVertexCount(vertices.Length);
        for (int i = 0; i < vertices.Length; i++)
        {
            line.SetPosition(i, vertices[i]);
        }
        //set the width
        line.SetWidth(0.1f, 0.1f);
        line.material = Resources.Load("road") as Material;
        line.transform.parent = this.gameObject.transform;
        line.useWorldSpace = false;
    }

    private float[] coordTrans(float lon, float lat) //經緯度轉成遊戲內座標，因為zoom+1是兩倍所以用2的次方
    {
        float times = Mathf.Pow(2, zoom) / 30; //除30試試出來的@@
        float worldX = (lon - longtitude) * times;
        float worldY = (lat - latitude) * times;
        return new float[] { worldX, worldY };
    }

    public bool GetNewTile(int[] diff) //跟伺服器要新地圖塊
    {
        xTile += diff[0]; //往右??格
        yTile -= diff[1]; //往下??格
        foreach (int[] tile in drewTile) //檢查這格是否有畫過了，避免重畫好幾次一樣的東西
        {
            if (tile[0] == xTile && tile[1] == yTile)
            {
                return false;
            }
        }
        requestMap(xTile, yTile); //沒重複的話就要
        drewTile.Add(new int[] { xTile, yTile }); //這格存入list
        return true;
    }
}
