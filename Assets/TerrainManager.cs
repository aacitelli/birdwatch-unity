using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField]
    private int mapWidthInTiles, mapDepthInTiles;

    [SerializeField]
    private GameObject tilePrefab;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    void GenerateMap()
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int) tileSize.x;
        int tileDepth = (int) tileSize.z;

        // Instantiate a tile at the given position
        for (int xIndex = 0; xIndex < mapWidthInTiles; xIndex++)
        {
            for (int zIndex = 0; zIndex < mapDepthInTiles; zIndex++)
            {
                // Calculate position 
                Vector3 pos = new Vector3(this.gameObject.transform.position.x + xIndex * tileWidth, 
                    this.gameObject.transform.position.y, 
                    this.gameObject.transform.position.z + zIndex * tileDepth);

                // Instantiate new tile GameObject 
                // Syntax: Instantiate(<prefab>, <parent transform>, <rotation>)
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity) as GameObject;
            }
        }
    }
}