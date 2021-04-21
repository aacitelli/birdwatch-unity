using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// Selectively shade chunks 
[System.Serializable]
public class TerrainType {
	public string name;
	public float height;
	public Color color;
}

// Generate chunks 
public class ChunkGen : MonoBehaviour
{
    // Edit different "biomes" from the editor 
    [SerializeField]
    private TerrainType[] terrainTypes;

    [SerializeField]
    Noise noise;

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    [SerializeField]
    private float mapScale;

    void Start()
    {
        GenerateChunk();
    }

    [SerializeField]
    private Wave[] waves;

    void GenerateChunk()
    {
        // Get a list of the square vertices 
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int chunkDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int chunkWidth = chunkDepth;

        // Convert from shared vertices to non-shared vertices; 
        // If we use shared vertices, there's just no way to 
        UpdateTriangles();

        // Offset from overall world position based on coordinates
        float offsetX = -this.gameObject.transform.position.x;
        float offsetZ = -this.gameObject.transform.position.z;
        Dictionary<Vector2, float> heightMap = this.noise.GenerateNoiseMap(chunkDepth, chunkWidth, this.mapScale, offsetX, offsetZ, waves);

        // Update vertex heights
        UpdateMeshVertices(heightMap);

        // Vertices 
    }

    // Converts the vertex representation of the mesh to non-shared vertices 
    void UpdateTriangles()
    {
        // Utility method
        Vector3 CopyVector3(Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        // Three vertices for each end vertex
        int width = (int) Mathf.Sqrt(this.meshFilter.mesh.vertices.Length);
        int newVerticesPos = 0;
        Vector3[] newVertices = new Vector3[(int) Mathf.Pow(width, 2) * 6];

        // Iterate through each vertex
        for (int row = 0; row < width - 1; row++)
        {
            for (int col = 0; col < width - 1; col++)
            {
                // Grab references to the vertices that will matter for us here 
                // Note that for loop conditions will stop us from hitting a bounds check 
                Vector3 currentVertex = this.meshFilter.mesh.vertices[row * width + col];
                // print("currentVertex: " + currentVertex);
                Vector3 rightVertex = this.meshFilter.mesh.vertices[row * width + (col + 1)];
                // print("rightVertex: " + rightVertex);
                Vector3 downVertex = this.meshFilter.mesh.vertices[(row + 1) * width + col];
                // print("downVertex: " + downVertex);
                Vector3 downRightVertex = this.meshFilter.mesh.vertices[(row + 1) * width + (col + 1)];
                // print("downRightVertex: " + downRightVertex);

                // End representation (assuming we are getting vertices for down-right box) should be:
                // First triangle: this vertex, vertex one to the right, vertex one to the right and one down
                newVertices[newVerticesPos] = CopyVector3(currentVertex); 
                newVertices[newVerticesPos + 1] = CopyVector3(rightVertex);
                newVertices[newVerticesPos + 2] = CopyVector3(downRightVertex);

                // Second triangle: this vertex, vertex one to the bottom, vertex one to the right and one down 
                newVertices[newVerticesPos + 3] = CopyVector3(currentVertex); 
                newVertices[newVerticesPos + 4] = CopyVector3(downVertex);
                newVertices[newVerticesPos + 5] = CopyVector3(downRightVertex);

                // Switch to calculating for next vertex
                newVerticesPos += 6;
            }
        }

        // Triangle vertices are easy; we know each triplet of vertices represents a triangle now
        int[] newTriangles = new int[newVertices.Length];
        for (int i = 0; i < newTriangles.Length; i++)
        {
            newTriangles[i] = i;
        }

        this.meshFilter.mesh.vertices = newVertices;
        this.meshFilter.mesh.triangles = newTriangles;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        this.meshFilter.mesh.uv = uvs;
    }

    /*
    private Texture2D BuildTexture(float[,] heightMap)
    {
        int chunkDepth = heightMap.GetLength(0);
        int chunkWidth = heightMap.GetLength(1);
        // print("Depth: " + chunkDepth);
        // print("Width: " + chunkWidth);

        // Get a color for each vertex based on its noise value, basically 
        Color[] colorMap = new Color[chunkDepth * chunkWidth];
        for (int zIndex = 0; zIndex < chunkDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < chunkWidth; xIndex++)
            {
                // Get height 
                int colorIndex = zIndex * chunkWidth + xIndex;
                float height = heightMap[zIndex, xIndex];

                // Instead of lerping from white to black, set color based on heightmap value 
                TerrainType terrainType = ChooseTerrainType(height);
                colorMap[colorIndex] = terrainType.color;
            }
        }

        // Set a Texture2D to the color map we just generated 
        Texture2D tileTexture = new Texture2D(chunkWidth, chunkDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        // byte[] bytes = tileTexture.EncodeToPNG();
        // var dirPath = "C:\\Users\\aacit\\Downloads";
        // File.WriteAllBytes(dirPath + "Image" + ".png", bytes);
        // print("Writing to " + dirPath + "Image" + ".png");

        return tileTexture;
    }
    */    

    // How "vertical" we want our map to be. Lower values will result in less extreme highs and lows and will generally make slopes smoother.
    [SerializeField]
    private float heightMultiplier; 

    // A useful thing Unity adds that lets us essentially do a height distribution rather than relying entirely on noise ourselves
    [SerializeField]
    private AnimationCurve heightCurve;

    // Iterates through all vertices in a given Chunk and sets their heights based on their noise values 
    private void UpdateMeshVertices(Dictionary<Vector2, float> heightMap)
    {
        int tileDepth = (int) Mathf.Sqrt(heightMap.Count);
        int tileWidth = tileDepth;
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;

        // Access data from our mesh and update heights accordingly 
        int vertexIndex = 0;
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileDepth; xIndex++)
            {
                float height = heightMap[new Vector2(xIndex, zIndex)];
                Vector3 vertex = meshVertices[vertexIndex];
                meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);
                vertexIndex++;
            }
        }

        // Update actual mesh properties; basically "apply" the heights to the mesh 
        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();
        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }

    // Helper method that returns which color should be used for a given vertex height (technically a noise value, but it's basically the same thing)
    TerrainType ChooseTerrainType(float height)
    {
        foreach (TerrainType terrainType in terrainTypes)
        {
            // Triggers on the first one where we qualify the condition
            // For instance, we have water below .3, then lowlands below like .5, so if I feed in .4, it won't get in
            // here for water but it will get in here for lowlands 
            if (height < terrainType.height)
            {
                return terrainType;
            }
        }

        // If we didn't hit one, choose the highest one 
        return terrainTypes[terrainTypes.Length - 1]; 
    }
}