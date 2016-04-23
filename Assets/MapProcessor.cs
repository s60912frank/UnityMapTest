using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapProcessor : MonoBehaviour {
    public const string API_KEY = "vector-tiles-vxQ7SnN";
    private const string MAP_TYPE = "boundaries,buildings,roads";
    public string url;
    private float latitude = 25.0488153f;
    private float longtitude = 121.445669f;
    private int zoom = 15;
	// Use this for initialization
	void Start () {
        int[] tile = WorldToTilePos(longtitude, latitude, zoom);
        url = "https://vector.mapzen.com/osm/" + MAP_TYPE + "/" + zoom + "/" + tile[0] + "/" + tile[1] + ".json?api_key=" + API_KEY;
        Debug.Log(url);
        WWW request = new WWW(url);
        StartCoroutine(WaitForRequest(request));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public int[] WorldToTilePos(float lon, float lat, int zoom)
    {
        int worldX = (int)((lon + 180.0) / 360.0 * (1 << zoom));
        int worldY = (int)((1.0f - Mathf.Log(Mathf.Tan(lat * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos(lat * Mathf.PI / 180.0f)) / Mathf.PI) / 2.0f * (1 << zoom));

        return new int[] { worldX, worldY };
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
            switch (type)
            {
                case "Point":

                    break;
                case "Polygon":
                    foreach (JSONObject co in obj["geometry"]["coordinates"].list[0].list)
                    {
                        //Debug.Log(co);
                        float worldX = (float.Parse(co[0].ToString()) - longtitude) * 1000f;
                        float worldY = (float.Parse(co[1].ToString()) - latitude) * 1000f;
                        //Vector2 vec = new Vector2(float.Parse(co[0].ToString()), float.Parse(co[1].ToString()));
                        coords.Add(new Vector2(worldX, worldY));
                        Debug.Log("X:" + worldX + "Y:" + worldY);
                        
                    }
                    Debug.Log(obj["geometry"]["coordinates"]);
                    break;
                case "LineString":
                    break;
                case "MultiLineString":
                    break;
            }
            DrawMapObj(coords.ToArray());
        }


    }

    private void DrawMapObj(Vector2[] vertices2D)
    {
        //Debug.Log(vecs);
        //Vector2[] vertices2D = new Vector2[] {
        //    new Vector2(0,0),
        //    new Vector2(0,50),
        //    new Vector2(50,50),
        //    new Vector2(50,100),
        //    new Vector2(0,100),
        //    new Vector2(0,150),
        //    new Vector2(150,150),
        //    new Vector2(150,100),
        //    new Vector2(100,100),
        //    new Vector2(100,50),
        //    new Vector2(150,50),
        //    new Vector2(150,0),
        //};

        // Use the triangulator to get indices for creating triangles
        //Vector2[] vertices2D = vecs;
        //GameObject obj = new GameObject();
        ////Debug.Log(coords);
        //Triangulator tr = new Triangulator(vertices2D);
        //int[] indices = tr.Triangulate();
        //int[] rindices = new int[indices.Length];
        //for (int i = 0; i < indices.Length; i++)
        //{
        //    rindices[indices.Length - i - 1] = indices[i];
        //}
        //// Create the Vector3 vertices
        //Vector3[] vertices = new Vector3[vertices2D.Length];
        //for (int i = vertices.Length - 1; i >= 0; i--)
        //{
        //    vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        //    //Debug.Log("X:" + vertices2D[i].x + "Y:" + vertices2D[i].y);
        //}

        //List<Vector3> vec3 = new List<Vector3>();
        //foreach (Vector2 vec in coords)
        //{
        //    vec3.Add(new Vector3(vec.x, vec.y, 0));
        //}


        // Use the triangulator to get indices for creating triangles
        Triangulator tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[vertices2D.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        }
        //Create the mesh
        //Mesh msh = gameObject.GetComponent<MeshFilter>().mesh;
        Mesh msh = new Mesh();
        msh.Clear();
        msh.vertices = vertices;
        //msh.triangles = rindices;
        msh.triangles = indices;
        msh.RecalculateNormals();
        msh.RecalculateBounds();

        // Set up game object with mesh;
        GameObject obj = new GameObject();
        obj.AddComponent(typeof(MeshRenderer));
        MeshFilter filter = obj.AddComponent(typeof(MeshFilter)) as MeshFilter;
        filter.mesh = msh;
        obj.transform.parent = this.gameObject.transform;
        //gameObject.GetComponent<Mesh>().SetVertices(vec3);
        //gameObject.GetComponent<Mesh>().SetIndices(indices, MeshTopology.Triangles, 0);
        //gameObject.GetComponent<MeshFilter>().mesh = msh;
    }
}
