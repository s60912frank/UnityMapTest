using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MapProcessor : MonoBehaviour {
    public const string API_KEY = "vector-tiles-vxQ7SnN";
    private const string MAP_TYPE = "boundaries,buildings,roads";
    private int xTile;  //儲存現在camera所在的地圖格(大概 不是很準)
    private int yTile;
    public string url;
	public static float latOrigin = 25.0417534f;
	public static float lonOrigin = 121.5339142f;
    private int zoom = 14; //放大倍率，1~19
    private List<MapTile> mapTiles; //儲存現在畫面上的mapTiles
    private int mapTileIndex = 0;
    private bool mapTileLock = false; //一次只畫一張地圖塊
    
	// Use this for initialization
	void Start () {
		Debug.Log (Application.persistentDataPath);
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
        
        //如果之前有存過此地圖塊那就直接讀檔
        if (File.Exists(Application.persistentDataPath + "/" + zoom.ToString() + "_" + xTile.ToString() + "_" + yTile.ToString() + ".json"))
        {
            JsonProssor(File.ReadAllText(Application.persistentDataPath + "/" + zoom.ToString() + "_" + xTile.ToString() + "_" + yTile.ToString() + ".json"));
        }
        //沒的話跟伺服器要
        else
        {
            url = "https://vector.mapzen.com/osm/" + MAP_TYPE + "/" + zoom + "/" + xtile + "/" + ytile + ".json?api_key=" + API_KEY;
            Debug.Log(url);
            WWW request = new WWW(url);
            StartCoroutine(WaitForRequest(request));
        }
    }

    IEnumerator WaitForRequest(WWW www) //當資料從伺服器回來會執行這個
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!");
            File.WriteAllText(Application.persistentDataPath + "/" + zoom.ToString() + "_" + xTile.ToString() + "_" + yTile.ToString() + ".json", www.text);
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
            //存入該mapTile的點List
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
                        //存入該mapTile的點List
                        mapTiles[mapTileIndex].AddMapObj("LineString", smallLine);
                    }
                    break;
                default:
                    Debug.Log(type);
                    break;
            }
            //存入該mapTile的點List
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
                        //存入該mapTile的點List
                        mapTiles[mapTileIndex].AddMapObj("LineString", smallLine);
                    }
                    break;
                default:
                    Debug.Log(type);
                    break;
            }
            //存入該mapTile的點List
            mapTiles[mapTileIndex].AddMapObj("LineString", coords);
        }

        //轉換成遊戲世界座標and取得這地圖塊的大小
        mapTiles[mapTileIndex].Normalize();
        //畫地圖
        foreach (MapTile mapTile in mapTiles)
        {
            foreach (MapTile.MapObj mo in mapTile.mapObjs)
            {
                DrawMapObj(mo.type, mo.verticies.ToArray());
            }
        }
        //畫完解除鎖定
        mapTileLock = false;
    }

    public void GetNewTile(int[] diff) //跟伺服器要新地圖塊
    {
        if (!mapTileLock) //沒被鎖才畫
        {
            mapTileLock = true; //鎖
            xTile += diff[0]; //往右??格
            yTile -= diff[1]; //往下??格
            mapTiles.Add(new MapTile(xTile, yTile, zoom));
            mapTileIndex++;
            requestMap(xTile, yTile); //沒重複的話就要
        }
    }

    public void GetNewZoomTile(object[] objs) //要不同縮放層級的地圖
    {
        if (!mapTileLock)
        {
            mapTiles.Clear();  //清掉array
            mapTileIndex = 0; //index設為0
            int zoomDiff = (int)(objs[1] as int?);//zoom+1 or -1
            Vector2 camPos = (Vector2)(objs[0] as Vector2?);//cam的位置
            zoom += zoomDiff;
            float times = Mathf.Pow(2, zoom) / 10;
            //將遊戲座標轉回經緯度
            mapTiles.Add(new MapTile(camPos.x / times + lonOrigin, camPos.y / times + latOrigin, zoom));
            mapTileLock = true; //鎖個
            xTile = mapTiles[0].xTile;
            yTile = mapTiles[0].yTile;
            requestMap(mapTiles[0].xTile, mapTiles[0].yTile);
            Debug.Log(mapTiles[0].xTile + "/" + mapTiles[0].yTile);
        }
    }

    private void DrawMapObj(string type, Vector2[] vertices2D) //畫地圖物件
    {        
        GameObject obj = new GameObject(); //創個新物體
        obj.tag = "MapObj"; //方便清掉

        int verticesLength = vertices2D.Length;

        Vector3[] vertices = new Vector3[verticesLength];
        for (int i = 0; i < verticesLength; i++) //就只是2D轉3D        
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, -0.1f); //一樣設0會有問題
        
        Vector3[] vertices2 = new Vector3[verticesLength * 2];
        for (int i = 0; i < verticesLength; i++)        
            vertices2[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, -2f);//高度.高度.高度.高度.高度.高度.
        for (int i = 0; i < verticesLength; i++)
            vertices2[i+ verticesLength] = new Vector3(vertices2D[i].x, vertices2D[i].y, -0.1f);
        
        switch (type)
        {
            case "Polygon":
                // Use the triangulator to get indices for creating triangles
                Triangulator tr = new Triangulator(vertices2D);
                int[] indices = tr.Triangulate();
                int[] HeightIndices = new int[indices.Length + (verticesLength-1)*6];

                for (int i = 0; i < indices.Length; i++)
                    HeightIndices[i] = indices[i];
                for (int i = 0; i < verticesLength - 1; i++)
                {
                    HeightIndices[i*6 + indices.Length] = i + 1;
                    HeightIndices[i*6 + indices.Length + 1] = i;
                    HeightIndices[i*6 + indices.Length + 2] = i + verticesLength;
                    HeightIndices[i*6 + indices.Length + 3] = i + verticesLength;
                    HeightIndices[i*6 + indices.Length + 4] = i + verticesLength + 1;
                    HeightIndices[i*6 + indices.Length + 5] = i + 1;
                }          
                //Create the mesh
                Mesh msh = new Mesh();
                msh.vertices = vertices2;
                msh.triangles = HeightIndices;
                msh.RecalculateNormals();
                msh.RecalculateBounds();

                // Set up game object with mesh;
                MeshRenderer msgr = obj.AddComponent<MeshRenderer>();
                msgr.material = new Material(Shader.Find("Diffuse"));
                //msgr.material = Resources.Load("building") as Material;
                
                MeshFilter filter = obj.AddComponent<MeshFilter>() as MeshFilter;
                filter.mesh = msh;

                GameObject[] buildingLine = new GameObject[verticesLength * 2];
                for (int i = 0; i < verticesLength - 1; i++)
                {
                    buildingLine[i] = new GameObject();
                    buildingLine[i].tag = "MapObj";
                    LineRenderer Line2 = buildingLine[i].AddComponent<LineRenderer>();
                    Line2.SetVertexCount(2);
                    Line2.SetPosition(0, vertices2[i]);
                    Line2.SetPosition(1, vertices2[i + 1]);
                    Line2.SetWidth(0.03f, 0.03f);
                    Line2.material = Resources.Load("black") as Material;
                    Line2.useWorldSpace = false;
                    obj.SetActive(true);
                }
                for (int i = 0; i < verticesLength; i++)
                {
                    buildingLine[i + verticesLength] = new GameObject();
                    buildingLine[i + verticesLength].tag = "MapObj";
                    LineRenderer Line2 = buildingLine[i + verticesLength].AddComponent<LineRenderer>();
                    Line2.SetVertexCount(2);
                    Line2.SetPosition(0, vertices2[i]);
                    Line2.SetPosition(1, vertices2[i + verticesLength]);
                    Line2.SetWidth(0.03f, 0.03f);
                    Line2.material = Resources.Load("black") as Material;
                    Line2.useWorldSpace = false;
                    obj.SetActive(true);
                }
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
                obj.SetActive(true);

                break;
        }
    }

    //bool first = true;
    //private void DrawMapObj(string type, Vector2[] vertices2D) //畫地圖物件
    //{
    //    if (first)
    //    {
    //        first = false;
    //        DrawMapObj2(type, vertices2D);
    //    }
    //    GameObject obj = new GameObject(); //創個新物體
    //    obj.tag = "MapObj"; //方便清掉
    //    Vector3[] vertices = new Vector3[vertices2D.Length];
    //    for (int i = 0; i < vertices.Length; i++) //就只是2D轉3D
    //    {
    //        vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, -0.1f); //一樣設0會有問題
    //    }
    //    switch (type)
    //    {
    //        case "Polygon":
    //            // Use the triangulator to get indices for creating triangles
    //            Triangulator tr = new Triangulator(vertices2D);
    //            int[] indices = tr.Triangulate();
    //            //Create the mesh
    //            Mesh msh = new Mesh();
    //            msh.vertices = vertices;
    //            msh.triangles = indices;
    //            msh.RecalculateNormals();
    //            msh.RecalculateBounds();

    //            // Set up game object with mesh;
    //            MeshRenderer msgr = obj.AddComponent<MeshRenderer>();
    //            msgr.material = Resources.Load("building") as Material;
    //            MeshFilter filter = obj.AddComponent<MeshFilter>() as MeshFilter;
    //            filter.mesh = msh;
    //            break;
    //        case "Point":
    //            //還是不知怎畫
    //            break;
    //        case "LineString":
    //            LineRenderer line = obj.AddComponent<LineRenderer>();
    //            //set the number of points to the line
    //            line.SetVertexCount(vertices.Length);
    //            for (int i = 0; i < vertices.Length; i++)
    //            {
    //                line.SetPosition(i, vertices[i]);
    //            }
    //            //set the width
    //            line.SetWidth(0.1f, 0.1f);
    //            line.material = Resources.Load("road") as Material;
    //            //line.transform.parent = this.gameObject.transform;
    //            line.useWorldSpace = false;
    //            obj.SetActive(true);

    //            break;
    //    }
    //}

}
