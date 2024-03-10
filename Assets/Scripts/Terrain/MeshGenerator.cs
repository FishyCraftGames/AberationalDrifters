using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public Mesh mesh;

    Vector3[] vertecies;
    int[] triangles;
    Vector2[] uvs;
    Color[] vertexColor;

    public Texture2D hMap = Resources.Load("Heightmap") as Texture2D;

    public int xSize;
    public int zSize;

    public Transform start;
    public Transform end;

    void Start()
    {

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        try
        {
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
        catch { }

        CreateShape();
        UpdateMesh();
    }

    private void Update()
    {
        //CreateShape();
        //UpdateMesh();
    }

    private void CreateShape()
    {
        vertecies = new Vector3[(xSize + 1) * (zSize + 1)];

        Texture2D heightMap = GenerateRoads(hMap);

        for(int i = 0, z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++)
            {
                //swapOut With Noise
                //float y = hMap.GetPixel(z, x).grayscale * 20;
                float y = 0;

                vertecies[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6];

        int vert = 0;
        int tris = 0;

        for(int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }

        uvs = new Vector2[vertecies.Length];
        vertexColor = new Color[vertecies.Length];

        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                uvs[i] = new Vector2((float)x/ xSize, (float)z/ zSize);
                vertexColor[i] = heightMap.GetPixel(z, x);
                i++;
            }
        }

    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertecies;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.colors = vertexColor;

        mesh.RecalculateBounds();
    }

    Texture2D GenerateRoads(Texture2D a)
    {
        Texture2D flowMap = new Texture2D(a.width, a.height);
        List<Vector2> frontiers = new List<Vector2>();
        //frontiers.Add(new Vector2(Mathf.RoundToInt(start.transform.position.x), Mathf.RoundToInt(start.transform.position.z)));
        frontiers.Add(new Vector2(10, 10));
        flowMap.filterMode = FilterMode.Point;
        flowMap.wrapMode = TextureWrapMode.Clamp;

        var fillColorArray = flowMap.GetPixels();
        Color fillColor = Color.black;

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] *= fillColor;
        }

        flowMap.SetPixels(fillColorArray);

        List<Color> colors = new List<Color>();
        colors.Add(Color.green);
        colors.Add(Color.red);
        colors.Add(Color.blue);
        colors.Add(Color.yellow);

        while (frontiers.Count > 0)
        {
            //check neighbours
            Vector2 current = frontiers[0];
            float from = a.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y)).grayscale;
            current += new Vector2(0, 1);
            if (current.x <= flowMap.width || current.y <= flowMap.height)
            {
                float up = a.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y)).grayscale;
                float diff = Mathf.Abs(from - up) * 20 / 1;
                if (diff <= 1)
                {
                    if (!colors.Contains(flowMap.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y))))
                    {
                        frontiers.Add(current);
                    }
                    flowMap.SetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Color.green);
                }
                else
                {
                    flowMap.SetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Color.magenta);
                }
            }

            current += new Vector2(1, -1);
            float right = a.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y)).grayscale;
            if (current.x <= flowMap.width || current.y <= flowMap.height)
            {
                float diff = Mathf.Abs(from - right) / 1;
                if (diff <= 1)
                {
                    if (!colors.Contains(flowMap.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y))))
                    {
                        frontiers.Add(current);
                    }
                    flowMap.SetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Color.red);
                }
                else
                {
                    flowMap.SetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Color.magenta);
                }
            }

            current += new Vector2(-1, -1);
            float down = a.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y)).grayscale;
            if (current.x <= flowMap.width || current.y <= flowMap.height)
            {
                float diff = Mathf.Abs(from - down) / 1;
                if (diff <= 1)
                {
                    if (!colors.Contains(flowMap.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y))))
                    {
                        frontiers.Add(current);
                    }
                    flowMap.SetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Color.blue);
                }
                else
                {
                    flowMap.SetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Color.magenta);
                }
            }

            current += new Vector2(-1, 1);
            float left = a.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y)).grayscale;
            if (current.x <= flowMap.width || current.y <= flowMap.height)
            {
                float diff = Mathf.Abs(from - left) / 1;
                if (diff <= 1)
                {
                    if (!colors.Contains(flowMap.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y))))
                    {
                        frontiers.Add(current);
                    }
                    flowMap.SetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Color.yellow);
                }
                else
                {
                    flowMap.SetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Color.magenta);
                }
            }

            //remove frontier
            frontiers.Remove(frontiers[0]);
        }

        Texture2D path = new Texture2D(flowMap.width, flowMap.height);
        Vector2 target = new Vector2(70, 5);

        fillColorArray = path.GetPixels();

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] *= fillColor;
        }

        path.SetPixels(fillColorArray);

        int b = 0;
        while (target != new Vector2(10, 10) && b < 500)
        {
            Color c = flowMap.GetPixel(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y));
            if (!colors.Contains(c))
                break;

            path.SetPixel(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), Color.white);

            if(c == Color.green)
            {
                target -= new Vector2(0, -1);
            }
            if (c == Color.red)
            {
                target -= new Vector2(-1, 0);
            }
            if (c == Color.blue)
            {
                target -= new Vector2(0, 1);
            }
            if (c == Color.yellow)
            {
                target -= new Vector2(1, 0);
            }

            b++;
        }

        //for debugging to visualize the path
        //return flowMap; 
        
        //still just for debugging to see the path
        return path;
    }
}
