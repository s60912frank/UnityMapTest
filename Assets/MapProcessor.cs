using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapProcessor : MonoBehaviour {
    public const string API_KEY = "vector-tiles-vxQ7SnN";
    private const string MAP_TYPE = "boundaries,buildings,roads";
    private int xTile;  //儲存現在camera所在的地圖格(大概 不是很準)
    private int yTile;
    public string url;
    public static float latOrigin = 25.0488153f;
    public static float lonOrigin = 121.445669f;
    private int zoom = 14; //放大倍率，1~19
    private List<MapTile> mapTiles;
    private int mapTileIndex = 0;
    private bool mapTileLock = false;
	// Use this for initialization
	void Start () {
        mapTiles = new List<MapTile>();
        mapTiles.Add(new MapTile(lonOrigin, latOrigin, zoom));
        xTile = mapTiles[mapTileIndex].xTile; //起始地圖格
        yTile = mapTiles[mapTileIndex].yTile;
        requestMap(xTile, yTile); //要第一塊地圖格
	}
	
	// Update is called once per frame
	void Update () {
        //沒幹嘛
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
                        coords.Add(new Vector2(float.Parse(co[0].ToString()), float.Parse(co[1].ToString()))); //加到list中          
                    }
                    break;
                default:
                    Debug.Log(type);
                    break;
            }
            //DrawBuildings(type, coords.ToArray()); //丟進去畫
            mapTiles[mapTileIndex].AddMapObj(type, coords);
        }

        JSONObject roads = data["roads"]["features"]; //讀取資料中的"roads"節點下的"features"節點
        foreach (JSONObject obj in roads.list)
        {
            List<Vector2> coords = new List<Vector2>();
            string type = obj["geometry"]["type"].ToString().Replace("\"", "");
            switch (type)
            {
                case "LineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list) //lineString的座標陣列存在陣列中
                    {
                        coords.Add(new Vector2(float.Parse(co[0].ToString()), float.Parse(co[1].ToString()))); 
                    }
                    break;
                case "MultiLineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list) //multiLineString的座標陣列存在陣列中
                    {
                        List<Vector2> smallLine = new List<Vector2>(); //看名稱就知道啦這個一個就有很多條線，所以一條線就丟去畫
                        foreach (JSONObject co2 in co.list)
                        {
                            smallLine.Add(new Vector2(float.Parse(co2[0].ToString()), float.Parse(co2[1].ToString()))); 
                        }
                        //DrawRoads(smallLine.ToArray()); //去畫
                        mapTiles[mapTileIndex].AddMapObj("LineString", smallLine);
                    }
                    break;
                default:
                    Debug.Log(type);
                    break;
            }
            mapTiles[mapTileIndex].AddMapObj("LineString", coords);
        }

        JSONObject boundaries = data["boundaries"]["features"]; //讀取資料中的"boundaries"節點下的"features"節點
        foreach (JSONObject obj in boundaries.list) //其實跟上面一樣
        {
            List<Vector2> coords = new List<Vector2>();
            string type = obj["geometry"]["type"].ToString().Replace("\"", "");
            switch (type)
            {
                case "LineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list)
                    {
                        coords.Add(new Vector2(float.Parse(co[0].ToString()), float.Parse(co[1].ToString()))); 
                    }
                    break;
                case "MultiLineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list)
                    {
                        List<Vector2> smallLine = new List<Vector2>();
                        foreach (JSONObject co2 in co.list)
                        {
                            smallLine.Add(new Vector2(float.Parse(co2[0].ToString()), float.Parse(co2[1].ToString()))); 
                        }
                        //DrawRoads(smallLine.ToArray());
                        mapTiles[mapTileIndex].AddMapObj("LineString", smallLine);
                    }
                    break;
                default:
                    Debug.Log(type);
                    break;
            }
            mapTiles[mapTileIndex].AddMapObj("LineString", coords);
        }

        mapTiles[mapTileIndex].Normalize();
        foreach (MapTile mapTile in mapTiles)
        {
            foreach (MapTile.MapObj mo in mapTile.mapObjs)
            {
                DrawMapObj(mo.type, mo.verticies.ToArray());
            }
        }
        mapTileLock = false;
    }

    private void DrawMapObj(string type, Vector2[] vertices2D) //畫建築物
    {
        GameObject obj = new GameObject(); //創個新物體
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++) //就只是2D轉3D補0
        {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, -0.1f); //一樣設0會有問題
        }
        switch (type)
        {
            case "Polygon":
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
                //obj.transform.parent = this.gameObject.transform; //附加在本plane下
                break;
            case "Point":
                //還是不知怎畫
                break;
            case "LineString":
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
                //line.transform.parent = this.gameObject.transform;
                line.useWorldSpace = false;
                break;
        }
    }

    public void GetNewTile(int[] diff) //跟伺服器要新地圖塊
    {
        if (!mapTileLock)
        {
            mapTileLock = true;
            xTile += diff[0]; //往右??格
            yTile -= diff[1]; //往下??格
            mapTiles.Add(new MapTile(xTile, yTile, zoom));
            mapTileIndex++;
            requestMap(xTile, yTile); //沒重複的話就要
        }
    }
}
