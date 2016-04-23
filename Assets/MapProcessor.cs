using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapProcessor : MonoBehaviour {
    public const string API_KEY = "vector-tiles-vxQ7SnN";
    private const string MAP_TYPE = "boundaries,buildings,roads";
    public static float[] mapBoundaries = new float[] { 0, 0, 0, 0 };
    private int xTile;
    private int yTile;
    private List<int[]> drewTile;
    public string url;
    private float latitude = 25.0488153f;
    private float longtitude = 121.445669f;
    private int zoom = 15;
	// Use this for initialization
	void Start () {
        int[] tile = WorldToTilePos(longtitude, latitude, zoom);
        xTile = tile[0];
        yTile = tile[1];
        requestMap(tile[0], tile[1]);
        drewTile = new List<int[]>();
        drewTile.Add(new int[] { tile[0], tile[1] });
        //requestMap(tile[0] + 1, tile[1]);
        //requestMap(tile[0] - 1, tile[1]);
        //requestMap(tile[0], tile[1] + 1);
        //requestMap(tile[0], tile[1] - 1);
        //requestMap(tile[0] + 1, tile[1] + 1);
        //requestMap(tile[0] + 1, tile[1] - 1);
        //requestMap(tile[0] - 1, tile[1] + 1);
        //requestMap(tile[0] - 1, tile[1] - 1);
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    public int[] WorldToTilePos(float lon, float lat, int zoom)
    {
        int worldX = (int)Mathf.Floor((lon + 180.0f) / 360.0f * (1 << zoom));
        int worldY = (int)Mathf.Floor(((1.0f - Mathf.Log(Mathf.Tan(lat * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos(lat * Mathf.PI / 180.0f)) / Mathf.PI) / 2.0f * (1 << zoom)));
        float[] latlon = TileToWorldPos(worldX, worldY, zoom);
        //tilePlane.transform.position = new Vector3(latlon[0] - latitude, latlon[1] - latitude, 0);
        longtitude = latlon[0];
        latitude = latlon[1];
        return new int[] { worldX, worldY };
    }

    public float[] TileToWorldPos(float tile_x, float tile_y, int zoom)
    {
        float n = Mathf.PI - ((2.0f * Mathf.PI * tile_y) / Mathf.Pow(2.0f, zoom));

        float x = (float)((tile_x / Mathf.Pow(2.0f, zoom) * 360.0) - 180.0);
        float y = (float)(180.0 / Mathf.PI * Mathf.Atan((Mathf.Exp(n) - Mathf.Exp(-n)) / 2));
        //Debug.Log("x:" + x + "y:" + y);
        return new float[] { x, y };
    }

    private void requestMap(int xtile, int ytile)
    {
        url = "https://vector.mapzen.com/osm/" + MAP_TYPE + "/" + zoom + "/" + xtile + "/" + ytile + ".json?api_key=" + API_KEY;
        Debug.Log(url);
        WWW request = new WWW(url);
        StartCoroutine(WaitForRequest(request));
    }

    IEnumerator WaitForRequest(WWW www)
    {
        yield return www;
        // check for errors
        if (www.error == null)
        {
            Debug.Log("WWW Ok!");
            JsonProssor(www.text);
        }
        else
        {
            Debug.Log("WWW Error: " + www.error);
        }
    }

    private void JsonProssor(string rawData)
    {
        

        JSONObject data = new JSONObject(rawData);
        JSONObject buildings = data["buildings"]["features"];
        foreach (JSONObject obj in buildings.list)
        {
            List<Vector2> coords = new List<Vector2>();
            string type = obj["geometry"]["type"].ToString().Replace("\"", "");
            //Debug.Log(type);
            switch (type)
            {
                case "Point":

                    break;
                case "Polygon":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list[0].list)
                    {
                        float[] world = coordTrans(float.Parse(co[0].ToString()), float.Parse(co[1].ToString()));
                        coords.Add(new Vector3(world[0], world[1], 0));                      
                    }
                    break;
                default:
                    Debug.Log(type);
                    break;
            }
            DrawBuildings(type, coords.ToArray());
        }

        JSONObject roads = data["roads"]["features"];
        foreach (JSONObject obj in roads.list)
        {
            List<Vector3> coords = new List<Vector3>();
            string type = obj["geometry"]["type"].ToString().Replace("\"", "");
            switch (type)
            {
                case "LineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list)
                    {
                        float[] world = coordTrans(float.Parse(co[0].ToString()), float.Parse(co[1].ToString()));
                        coords.Add(new Vector3(world[0], world[1], -0.1f));
                    }
                    break;
                case "MultiLineString":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list)
                    {
                        List<Vector3> smallLine = new List<Vector3>();
                        foreach (JSONObject co2 in co.list)
                        {
                            float[] world = coordTrans(float.Parse(co2[0].ToString()), float.Parse(co2[1].ToString()));
                            smallLine.Add(new Vector3(world[0], world[1], -0.1f));
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

        JSONObject boundaries = data["boundaries"]["features"];
        foreach (JSONObject obj in boundaries.list)
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

    private void DrawBuildings(string type, Vector2[] vertices2D)
    {
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, -0.1f);
            setBoundaries(vertices[i].x, vertices[i].y);
        }

        switch (type)
        {
            case "Polygon":
                GameObject obj = new GameObject();
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
                obj.transform.parent = this.gameObject.transform;
                break;
            case "Point":
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
            setBoundaries(vertices[i].x, vertices[i].y);
        }
        //set the width
        line.SetWidth(0.1f, 0.1f);
        line.material = Resources.Load("road") as Material;
        line.transform.parent = this.gameObject.transform;
        line.useWorldSpace = false;
    }

    private float[] coordTrans(float lon, float lat)
    {
        float times = Mathf.Pow(2, zoom) / 30;
        float worldX = (lon - longtitude) * times + 5f;
        float worldY = (lat - latitude) * times + 5f;
        //Debug.Log("WORLDX:" + worldX + "WORLDY:" + worldY);
        return new float[] { worldX, worldY };
    }

    private void setBoundaries(float x, float y)
    {
        //上下左右
        if (y > mapBoundaries[0])
        {
            mapBoundaries[0] = y;
        }
        if (y < mapBoundaries[1])
        {
            mapBoundaries[1] = y;
        }
        if (x < mapBoundaries[2])
        {
            mapBoundaries[2] = x;
        }
        if (x > mapBoundaries[3])
        {
            mapBoundaries[3] = x;
        }
    }

    public bool GetNewTile(int[] diff)
    {
        xTile += diff[0];
        yTile -= diff[1];
        foreach (int[] tile in drewTile)
        {
            if (tile[0] == xTile && tile[1] == yTile)
            {
                Debug.Log("5515415435");
                return false;
            }
        }
        requestMap(xTile, yTile);
        drewTile.Add(new int[] { xTile, yTile });
        Debug.Log("XTILE:" + xTile + "YTILE:" + yTile);
        return true;
    }
}
