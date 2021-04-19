using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;

    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private GameObject player; 

    [SerializeField]
    private int generateRadius;

    // Holds references to chunks we've generated so that we don't regenerate them 
    private Dictionary<Vector2, GameObject> chunks = new Dictionary<Vector2, GameObject>();

    void Update() 
    {
        GenerateChunks();
    }

    void GenerateChunks()
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int) tileSize.x;
        int tileDepth = (int) tileSize.z;

        // Instantiate a tile at the given position
        int xStart, xEnd, zStart, zEnd;
        xStart = ((int) player.transform.position.x / tileWidth) - generateRadius; 
        xEnd = ((int) player.transform.position.x / tileWidth) + generateRadius;
        for (int xIndex = xStart; xIndex < xEnd; xIndex++)
        {
            zStart = ((int) player.transform.position.z / tileDepth) - generateRadius; 
            zEnd = ((int) player.transform.position.z / tileDepth) + generateRadius;
            for (int zIndex = zStart; zIndex < zEnd; zIndex++)
            {
                Vector2 pos = new Vector2(xIndex, zIndex);
                if (!chunks.ContainsKey(pos))
                {
                    // Calculate position 
                    Vector3 chunkPos = new Vector3(this.gameObject.transform.position.x + xIndex * tileWidth, 
                        this.gameObject.transform.position.y, 
                        this.gameObject.transform.position.z + zIndex * tileDepth);

                    // Instantiate new tile GameObject 
                    // Syntax: Instantiate(<prefab>, <parent transform>, <rotation>)
                    GameObject tile = Instantiate(tilePrefab, chunkPos, Quaternion.identity) as GameObject;
                    chunks[pos] = tile;
                }                
            }
        }
    }
}