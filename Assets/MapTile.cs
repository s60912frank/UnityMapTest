using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapTile 
{
    public int xTile;
    public int yTile;
    public float longtitude;
    public float latitude;
    public int zoom;
    public List<MapObj> mapObjs;
    public struct MapObj
    {
        public string type;
        public List<Vector2> verticies;
        public MapObj(string type, List<Vector2> vecs)
        {
            this.type = type;
            this.verticies = vecs;
        }

    }
    public MapBoundary mapBoundary = new MapBoundary();
    public struct MapBoundary
    {
        public float Up;
        public float Down;
        public float Right;
        public float Left;
        public MapBoundary(float zero = 0)
        {
            this.Up = zero;
            this.Down = zero;
            this.Right = zero;
            this.Left = zero;
        }

        public float Height
        {
            get
            {
                return Up - Down;
            }
        }

        public float Width
        {
            get
            {
                return Right - Left;
            }
        }
    }
    public GameObject plane;

    public MapTile(float lon, float lat, int zoom)
    {
        this.latitude = lat;
        this.longtitude = lon;
        this.zoom = zoom;
        WorldToTilePos();
        TileToWorldPos();
        this.mapObjs = new List<MapObj>();
        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(Vector3.right, -90);
    }

    public MapTile(int xTile, int yTile, int zoom)
    {
        this.xTile = xTile;
        this.yTile = yTile;
        this.zoom = zoom;
        TileToWorldPos();
        this.mapObjs = new List<MapObj>();
        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.Rotate(Vector3.right, -90);
    }

    public void WorldToTilePos() //經緯度轉地圖格編號
    {
        xTile = (int)Mathf.Floor((longtitude + 180.0f) / 360.0f * (1 << zoom));
        yTile = (int)Mathf.Floor(((1.0f - Mathf.Log(Mathf.Tan(latitude * Mathf.PI / 180.0f) + 1.0f / Mathf.Cos(latitude * Mathf.PI / 180.0f)) / Mathf.PI) / 2.0f * (1 << zoom)));
    }

    public void TileToWorldPos() //地圖格編號轉經緯度
    {
        float n = Mathf.PI - ((2.0f * Mathf.PI * yTile) / Mathf.Pow(2.0f, zoom));
        longtitude = (float)((xTile / Mathf.Pow(2.0f, zoom) * 360.0) - 180.0);
        latitude = (float)(180.0 / Mathf.PI * Mathf.Atan((Mathf.Exp(n) - Mathf.Exp(-n)) / 2));
    }

    public void UpdateBound(Vector2 vec)
    {
        if (vec.x > mapBoundary.Right)
        {
            mapBoundary.Right = vec.x;
        }
        if (vec.x < mapBoundary.Left)
        {
            mapBoundary.Left = vec.x;
        }
        if (vec.y > mapBoundary.Up)
        {
            mapBoundary.Up = vec.y;
        }
        if (vec.y < mapBoundary.Down)
        {
            mapBoundary.Down = vec.y;
        }
    }

    public void AddMapObj(string type, List<Vector2> vecs)
    {
        mapObjs.Add(new MapObj(type, vecs));
    }

    public void Normalize()
    {
        float times = Mathf.Pow(2, zoom) / 30; //除30試試出來的@@;
        mapBoundary.Up = mapBoundary.Down = (latitude - MapProcessor.latOrigin) * times;
        mapBoundary.Left = mapBoundary.Right = (longtitude - MapProcessor.lonOrigin) * times;
        foreach (MapObj mo in mapObjs)
        {
            for (int i = 0; i < mo.verticies.Count; i++)
            {
                Vector2 temp = mo.verticies[i];

                temp.x = (temp.x - MapProcessor.lonOrigin) * times;
                temp.y = (temp.y - MapProcessor.latOrigin) * times;
                UpdateBound(temp);
                //Debug.Log(temp);
                mo.verticies[i] = temp;
            }
        }
        plane.transform.localScale = new Vector3(mapBoundary.Width / 10, 1, mapBoundary.Height / 10);
        plane.transform.position = new Vector3((longtitude - MapProcessor.lonOrigin) * times + 0.5f * mapBoundary.Width, (latitude - MapProcessor.latOrigin) * times - 0.5f * mapBoundary.Height, 0);
        plane.GetComponent<Renderer>().material = Resources.Load<Material>("plane");
    }
}
