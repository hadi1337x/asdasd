using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Tilemaps;


public class WorldGeneration : MonoBehaviour
{
    public class WorldItem
    {
        public int foreground = 0;
        public int background = 0;
    }
    public static WorldGeneration Instance { get; private set; }


    [SerializeField] int width, height;
    [SerializeField] public Tilemap Foreground;
    [SerializeField] Item[] items;

    private List<WorldItem> WorldData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        WorldData = Connection.Packet.meta;
        RenderMap(WorldData, Foreground);
    }

    public void RenderMap(List<WorldItem> worldItem, Tilemap Foreground)
    {
        for (int i = 0; i < worldItem.Count; i++)
        {
            int x = i % width;
            int y = i / width;

            WorldItem WorldItem = worldItem[i];
            if (WorldItem != null && WorldItem.foreground != 0)
            {
                Item item = items.FirstOrDefault(it => it.id == WorldItem.foreground);
                Foreground.SetTile(new Vector3Int(x, y, 0), item.tile);
            }

            // Set the background tile using the background value
            //backgroundTilemap.SetTile(new Vector3Int(x, y, 0), tileTypes[item.background]);
        }
        /*for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Foreground.SetTile(new Vector3Int(x, y, 0), Dirt);
            }
        }*/
        Connection.Packet.RequestPlayerSpawn();
    }

}