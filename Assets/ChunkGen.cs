using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGen : MonoBehaviour
{
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

    void GenerateChunk()
    {
        // Split up chunk into coordinates, basically 
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int chunkDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int chunkWidth = chunkDepth;

        // Offset from overall world position based on coordinates
        float[,] heightMap = this.noise.GenerateNoiseMap(chunkDepth, chunkWidth, this.mapScale);

        // Generate heightmap using the noise
        Texture2D chunkTexture = BuildTexture(heightMap);
        this.meshRenderer.material.mainTexture = chunkTexture;
    }

    private Texture2D BuildTexture(float[,] heightMap)
    {
        int chunkDepth = heightMap.GetLength(0);
        int chunkWidth = heightMap.GetLength(1);

        // Get a color for each vertex based on its noise value, basically 
        Color[] colorMap = new Color[chunkDepth * chunkWidth];
        for (int zIndex = 0; zIndex < chunkDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < chunkWidth; xIndex++)
            {
                // Get height 
                int colorIndex = zIndex * chunkWidth + xIndex;
                float height = heightMap[zIndex, xIndex];

                // Scale proportionally to height 
                colorMap[colorIndex] = Color.Lerp(Color.black, Color.white, height);
            }
        }

        // Set a Texture2D to the color map we just generated 
        Texture2D tileTexture = new Texture2D(chunkWidth, chunkDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }
}