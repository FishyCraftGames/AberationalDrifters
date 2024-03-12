using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Windows;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public Mesh mesh;

    Vector3[] vertecies;
    int[] triangles;
    Vector2[] uvs;
    Color[] vertexColor;

    public Texture2D hMap = Resources.Load("Heightmap") as Texture2D;
    public Texture2D fMap = Resources.Load("Fallofmap") as Texture2D;

    public int mapChunkSize = 241;

    public Transform start;
    public Transform end;
    public int h;

    void Start()
    {

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        CreateShape();
        UpdateMesh();

        gameObject.AddComponent<MeshCollider>();
    }

    private void Update()
    {
        //CreateShape();
        //UpdateMesh();
    }

    private void CreateShape()
    {
        vertecies = new Vector3[(mapChunkSize + 1) * (mapChunkSize + 1)];

        Texture2D heightMap = GenerateRoads(GenerateHeightMap(130,  Random.Range(0, 100000),5, 0.5f, 2));

        for(int i = 0, z = 0; z <= mapChunkSize; z ++)
        {
            for(int x = 0; x <= mapChunkSize; x ++)
            {
                //swapOut With Noise
                float y = heightMap.GetPixel(z, x).r * 40;
                //float y = 0;

                vertecies[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[mapChunkSize * mapChunkSize * 6];

        int vert = 0;
        int tris = 0;

        for(int z = 0; z < mapChunkSize; z++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                triangles[tris + 0] = vert;
                triangles[tris + 1] = vert + mapChunkSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + mapChunkSize + 1;
                triangles[tris + 5] = vert + mapChunkSize + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }

        uvs = new Vector2[vertecies.Length];
        vertexColor = new Color[vertecies.Length];

        for (int i = 0, z = 0; z <= mapChunkSize; z++)
        {
            for (int x = 0; x <= mapChunkSize; x++)
            {
                uvs[i] = new Vector2((float)x/ mapChunkSize, (float)z/ mapChunkSize);
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

    Texture2D Errode(Texture2D input, int iterrations)
    {
        for(int i = 0; i < iterrations; i++)
        {
            float posX = Random.Range(0f, input.width - 1f);
            float posY = Random.Range(0f, input.width - 1f);
            float dirX = 0;
            float dirY = 0;
            float speed = 1;
            float water = 1;
            float sediment = 0;

            for(int l = 0; l < 20; l++)
            {
                int nodeX = (int)posX;
                int nodeY = (int)posY;

                HeightAndGradient heightAndGradient = CalculateHeightAndGradient(input, input.width, posX, posY);

                dirX = (dirX * 0.5f - heightAndGradient.gradientX * (1 - 0.5f));
                dirY = (dirY * 0.5f - heightAndGradient.gradientY * (1 - 0.5f));

                float len = Mathf.Sqrt(dirX * dirX + dirY * dirY);
                if (len != 0)
                {
                    dirX /= len;
                    dirY /= len;
                }
                posX += dirX;
                posY += dirY;

                if ((dirX == 0 && dirY == 0) || posX < 0 || posX >= input.width - 1 || posY < 0 || posY >= input.height - 1)
                {
                    break;
                }

                float newHeight = CalculateHeightAndGradient(input, input.width, posX, posY).height;
                float deltaHeight = newHeight - heightAndGradient.height;

                float sedimentCapacity = Mathf.Max(-deltaHeight * speed * water * 4, .01f);

                if (sediment > sedimentCapacity || deltaHeight > 0)
                {
                    float amountToDeposit = (deltaHeight > 0) ? Mathf.Min(deltaHeight, sediment) : (sediment - sedimentCapacity) * .3f;
                    sediment -= amountToDeposit;

                    input = AddPixel(input, nodeX, nodeY, amountToDeposit);
                }
                else
                {
                    float amountToErode = Mathf.Min((sedimentCapacity - sediment) * .3f, -deltaHeight) / 5;

                    input = AddPixel(input, nodeX, nodeY, -amountToErode);
                    input = AddPixel(input, nodeX - 1, nodeY + 1, -amountToErode);
                    input = AddPixel(input, nodeX + 1, nodeY + 1, -amountToErode);
                    input = AddPixel(input, nodeX - 1, nodeY - 1, -amountToErode);
                    input = AddPixel(input, nodeX + 1, nodeY - 1, -amountToErode);
                    sediment += amountToErode*4;
                }

                speed = Mathf.Sqrt(speed * speed + deltaHeight * 4);
                water *= (1 - .01f);

            }

        }

        return input;
    }

    Texture2D AddPixel(Texture2D tex, int x, int y, float ammount)
    {
        tex.SetPixel(x, y, Color.white * (tex.GetPixel(x, y).r + ammount));
        return tex;
    }

    HeightAndGradient CalculateHeightAndGradient(Texture2D nodes, int mapSize, float posX, float posY)
    {
        int coordX = (int)posX;
        int coordY = (int)posY;

        // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
        float x = posX - coordX;
        float y = posY - coordY;

        // Calculate heights of the four nodes of the droplet's cell
        float heightNW = nodes.GetPixel(coordX - 1, coordY + 1).r;
        float heightNE = nodes.GetPixel(coordX + 1, coordY + 1).r;
        float heightSW = nodes.GetPixel(coordX - 1, coordY - 1).r;
        float heightSE = nodes.GetPixel(coordX + 1, coordY - 1).r;

        // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
        float gradientX = (heightNE - heightNW) * (1 - y) + (heightSE - heightSW) * y;
        float gradientY = (heightSW - heightNW) * (1 - x) + (heightSE - heightNE) * x;

        // Calculate height with bilinear interpolation of the heights of the nodes of the cell
        float height = heightNW * (1 - x) * (1 - y) + heightNE * x * (1 - y) + heightSW * (1 - x) * y + heightSE * x * y;

        return new HeightAndGradient() { height = height, gradientX = gradientX, gradientY = gradientY };
    }

    Texture2D GenerateHeightMap(float scale, int seed, int octaves, float persistance, float lacunarity)
    {
        Texture2D hm = new Texture2D(mapChunkSize, mapChunkSize, TextureFormat.R16, false);
        float[,] noiseMap = new float[mapChunkSize, mapChunkSize];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; i ++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for(int y = 0; y < mapChunkSize; y++)
        {
            for(int x = 0; x < mapChunkSize; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHieght = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency + octaveOffsets[i].x;
                    float sampleY = y / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHieght += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                
                if(noiseHieght > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHieght;
                }
                else if(noiseHieght < minNoiseHeight)
                {
                    minNoiseHeight = noiseHieght;
                }
                noiseMap[x, y] = noiseHieght;
            }
        }

        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                Color col = Color.white * noiseMap[x, y];
                col = Color.white * Mathf.Max(col.grayscale, fMap.GetPixel(x, y).grayscale);
                hm.SetPixel(x, y, col);
            }
        }



        hm.Apply();
        return hm;
    }

    Texture2D GenerateRoads(Texture2D a)
    {
        Texture2D bluredA = BlurTexture(a, 10);
        Texture2D result = new Texture2D(a.width, a.height, TextureFormat.RGB24, false);
        bool hasFoundRoad = false;

        while (!hasFoundRoad)
        {
            Vector2 startPos = new Vector2(Random.Range(25, 75), Random.Range(25, 75));
            Vector2 target = new Vector2(Random.Range(160, 220), Random.Range(160, 220));

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
                float from = a.GetPixel(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y)).r;

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
                    if (neighbor.x >= 0 && neighbor.x < flowMap.width && neighbor.y >= 0 && neighbor.y < flowMap.height)
                    {
                        float to = a.GetPixel(Mathf.RoundToInt(neighbor.x), Mathf.RoundToInt(neighbor.y)).r;
                        float diff = Mathf.Abs(from - to) * 40 / 1f;

                        if (diff <= 0.35)
                        {
                            int newCost = costMap[current] + Mathf.RoundToInt(diff * diff * 100);
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

            int xp = path.width;
            int yp = path.height;
            while (xp > 0)
            {
                yp = path.height;
                while (yp > 0)
                {
                    Color col = Color.white * a.GetPixel(xp, yp).r;
                    col.a = 0;
                    path.SetPixel(xp, yp, col);

                    yp--;
                }
                xp--;
            }

            int b = 0;
            int n = 0;
            hasFoundRoad = true;
            while (target != startPos && n < 100)
            {
                if (n >= 98)
                {
                    hasFoundRoad = false;
                }

                Color c = flowMap.GetPixel(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y));
                if (!colors.Contains(c))
                {
                    target = GenerateTarget(startPos);
                    b = 0;

                    n++;
                }

                //path.SetPixel(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y), Color.white);
                path = DrawCircle(target, 8, bluredA, path);

                if (c == Color.green)
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

                if (b >= 1000)
                {
                    target = GenerateTarget(startPos);
                    b = 0;

                    n++;
                }
                b++;
            }

            if (hasFoundRoad)
            {
                path = BlurTexture(path, 8);

                //SaveTexture(flowMap);
                //SaveTexture(path);

                int x = path.width;
                int y = path.height;
                while (x > 0)
                {
                    y = path.height;
                    while (y > 0)
                    {
                        float alpha = path.GetPixel(x, y).a;

                        Color col = path.GetPixel(x, y);
                        col *= alpha;
                        alpha = 1f - alpha;
                        col = col + Color.white * a.GetPixel(x, y).r * alpha;
                        result.SetPixel(x, y, col);

                        y--;
                    }
                    x--;
                }

                //for debugging to visualize the path
                //return flowMap; 

                //still just for debugging to see the path

                int xr = path.width;
                int yr = path.height;
                while (xr > 0)
                {
                    yr = path.height;
                    while (yr > 0)
                    {
                        Color col = Color.black;
                        col.r = result.GetPixel(xr, yr).grayscale;
                        col.g = path.GetPixel(xr, yr).a;
                        result.SetPixel(xr, yr, col);

                        yr--;
                    }
                    xr--;
                }

                //SaveTexture(result);
            }
        }

        return result;
    }

    Vector2 GenerateTarget(Vector2 start)
    {
        Vector2 a = new Vector2(Random.Range(160, 220), Random.Range(160, 220));

        int b = 0;
        while (Vector2.Distance(start, a) < 100 && b < 1000)
        {
            a = new Vector2(Random.Range(160, 220), Random.Range(160, 220));
            b++;
        }

        return a;
    }

    Texture2D DrawCircle(Vector2 coordinates, int radius, Texture2D texInput, Texture2D texOutput)
    {
        int x = Mathf.RoundToInt(coordinates.x);
        int y = Mathf.RoundToInt(coordinates.y);
        Color color = Color.white * texInput.GetPixel(x, y).r;
        color.a = 1;
        float rSquared = radius * radius;

        for (int u = x - radius; u <= x + radius + 1; u++)
            for (int v = y - radius; v <= y + radius + 1; v++)
                if ((x - u) * (x - u) + (y - v) * (y - v) <= rSquared)
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
        var dirPath = Application.dataPath + "/Art/Map";
        if (!System.IO.Directory.Exists(dirPath))
        {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        System.IO.File.WriteAllBytes(dirPath + "/map" + Random.Range(0,1000) + ".png", bytes);
        Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}

struct HeightAndGradient
{
    public float height;
    public float gradientX;
    public float gradientY;
}