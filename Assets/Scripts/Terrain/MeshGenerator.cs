using System.Collections;
using System.Collections.Generic;
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
    public int h;

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
                float y = heightMap.GetPixel(z, x).grayscale * 20;
                //float y = 0;

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

        GetComponent<MeshCollider>().sharedMesh = mesh;

        mesh.RecalculateBounds();
    }

    Texture2D GenerateRoads(Texture2D a)
    {
        Vector2 startPos = new Vector2(40, 101);
        Vector2 target = new Vector2(175, 216);

        Texture2D flowMap = new Texture2D(a.width, a.height, TextureFormat.RGB24, false);

        //List<Vector2> frontiers = new List<Vector2>();
        List<Vector3> frontiers = new List<Vector3>();
        frontiers.Add(new Vector3(startPos.x, startPos.y, 0));
        flowMap.filterMode = FilterMode.Point;
        flowMap.wrapMode = TextureWrapMode.Clamp;

        Dictionary<Vector2, int> costMap = new Dictionary<Vector2, int>();
        costMap.Add(startPos, 0);

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

        int g = 0;
        while (frontiers.Count > 0)
        {
            //check neighbours
            int lowestCostIndex = GetLowestCost(frontiers);
            Vector3 fr = frontiers[lowestCostIndex];
            Vector2 current = new Vector2(fr.x, fr.y);
            float from = a.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y)).grayscale;

            //remove current frontier
            frontiers.RemoveAt(lowestCostIndex);

            List<Vector2> neighbors = new List<Vector2>();
            neighbors.Add(current + new Vector2(0, 1));
            neighbors.Add(current + new Vector2(1, 0));
            neighbors.Add(current + new Vector2(0, -1));
            neighbors.Add(current + new Vector2(-1, 0));
            if ((current.x + current.y) % 2 == 0) neighbors.Reverse();

            foreach (var neighbor in neighbors)
            {
                if(neighbor.x >= 0 && neighbor.x < flowMap.width && neighbor.y >= 0 && neighbor.y < flowMap.height)
                {
                    float to = a.GetPixel(Mathf.RoundToInt(neighbor.x), Mathf.RoundToInt(neighbor.y)).grayscale;
                    float diff = Mathf.Abs(from - to) * 40 / 1f;

                    if (diff <= 1)
                    {
                        int newCost = costMap[current] + Mathf.RoundToInt(diff*100);
                        if (!costMap.ContainsKey(neighbor))
                        {
                            costMap.Add(neighbor, newCost);
                        }

                        if (!frontiers.Contains(neighbor) && flowMap.GetPixel(Mathf.RoundToInt(neighbor.x), Mathf.RoundToInt(neighbor.y)) == Color.black)
                        {
                            Debug.Log(newCost);
                            frontiers.Add(new Vector3(neighbor.x, neighbor.y, newCost));
                            Color color = GetDirectionColor(neighbor - current);
                            flowMap.SetPixel(Mathf.RoundToInt(neighbor.x), Mathf.RoundToInt(neighbor.y), color);
                        }
                    }
                    else if (flowMap.GetPixel(Mathf.RoundToInt(neighbor.x), Mathf.RoundToInt(neighbor.y)) == Color.black)
                    {
                        flowMap.SetPixel(Mathf.RoundToInt(neighbor.x), Mathf.RoundToInt(neighbor.y), Color.white);
                    }
                }
            }

            g++;
        }

        Texture2D path = new Texture2D(flowMap.width, flowMap.height);

        fillColorArray = path.GetPixels();
        fillColor = new Vector4(0, 0, 0, 0);

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] *= fillColor;
        }

        path.SetPixels(fillColorArray);

        int b = 0;
        while (target != startPos && b < 5000)
        {
            Color c = flowMap.GetPixel(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y));
            if (!colors.Contains(c))
                break;

            //path.SetPixel(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), Color.white);
            path = DrawCircle(target, 4, a ,path);

            if(c == Color.green)
            {
                target += new Vector2(0, -1);
            }
            else if (c == Color.red)
            {
                target += new Vector2(-1, 0);
            }
            else if (c == Color.blue)
            {
                target += new Vector2(0, 1);
            }
            else if (c == Color.yellow)
            {
                target += new Vector2(1, 0);
            }

            b++;
        }

        path = BlurTexture(path, 3);

        SaveTexture(flowMap);
        SaveTexture(path);

        Texture2D result = new Texture2D(path.width, path.height, TextureFormat.RGB24, false);

        int x = path.width;
        int y = path.height;
        while (x > 0)
        {
            y = path.height;
            while(y > 0)
            {
                float alpha = path.GetPixel(x, y).a;

                Color col = path.GetPixel(x, y);
                col *= alpha;
                alpha = 1f - alpha;
                col = col + a.GetPixel(x, y)*alpha;
                result.SetPixel(x, y, col);

                y--;
            }
            x--;
        }

        //for debugging to visualize the path
        //return flowMap; 
        
        //still just for debugging to see the path
        return result;
    }

    Texture2D DrawCircle(Vector2 coordinates, int radius, Texture2D texInput, Texture2D texOutput)
    {
        int x = Mathf.RoundToInt(coordinates.x);
        int y = Mathf.RoundToInt(coordinates.y);
        Color color = texInput.GetPixel(x, y);
        float rSquared = radius * radius;

        for (int u = x - radius; u < x + radius + 1; u++)
            for (int v = y - radius; v < y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) < rSquared)
                    texOutput.SetPixel(u, v, color);

        return texOutput;
    }

    Texture2D BlurTexture(Texture2D image, int blurSize)
    {
        Texture2D blurred = new Texture2D(image.width, image.height);
     
        // look at every pixel in the blur rectangle
        for (int xx = 0; xx < image.width; xx++)
        {
            for (int yy = 0; yy < image.height; yy++)
            {
                float avgR = 0, avgG = 0, avgB = 0, avgA = 0;
                int blurPixelCount = 0;
     
                // average the color of the red, green and blue for each pixel in the
                // blur size while making sure you don't go outside the image bounds
                for (int x = xx; (x < xx + blurSize && x < image.width); x++)
                {
                    for (int y = yy; (y < yy + blurSize && y < image.height); y++)
                    {
                        Color pixel = image.GetPixel(x, y);
     
                        avgR += pixel.r;
                        avgG += pixel.g;
                        avgB += pixel.b;
                        avgA += pixel.a;
     
                        blurPixelCount++;
                    }
                }
     
                avgR = avgR / blurPixelCount;
                avgG = avgG / blurPixelCount;
                avgB = avgB / blurPixelCount;
                avgA = avgA / blurPixelCount;
     
                // now that we know the average for the blur size, set each pixel to that color
                for (int x = xx; x < xx + blurSize && x < image.width; x++)
                    for (int y = yy; y < yy + blurSize && y < image.height; y++)
                        blurred.SetPixel(x, y, new Color(avgR, avgG, avgB, avgA));
            }
        }
        blurred.Apply();
        return blurred;
    }

    Color GetDirectionColor(Vector2 dir)
    {
        if(dir == new Vector2(0, 1))
        {
            return Color.green;
        }
        else if(dir == new Vector2(1, 0))
        {
            return Color.red;
        }
        else if (dir == new Vector2(0, -1))
        {
            return Color.blue;
        }
        else
        {
            return Color.yellow;
        }
    }

    int GetLowestCost(List<Vector3> l)
    {
        float HighestCost = 0;
        int index = 0;
        for(int i = 0; i < l.Count; i++)
        {
            if (l[i].z <= HighestCost)
            {
                index = i;
                HighestCost = l[i].z;
            }
        }
        return index;
    }

    private void SaveTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/RenderOutput";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        System.IO.File.WriteAllBytes(dirPath + "/R_" + Random.Range(0, 100000) + ".png", bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
