using UnityEngine;
using System.Collections.Generic;


public class TileSpriteController : MonoBehaviour {

    public Sprite concreteSprite;
    public Sprite grassSprite;
    public Sprite woodSprite;

    Dictionary<Tile, GameObject> tileGameObjectMap;

    World world
    {
        get { return WorldController.Instance.world; }
    }

	// Use this for initialization
	void Start () {

        // Instantiate our dictionary that tracks which GamObject is rendering which Tile data.
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Create a GameObject for each of our tiles, so they show visually. (and redunt reduntantly)
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile tile_data = world.GetTileAt(x, y);

                // Creating a new GameObject and adding it to our scene.
                GameObject tile_go = new GameObject();

                // Add our tile/GO pair to the dictionary.
                tileGameObjectMap.Add(tile_data, tile_go);

                tile_go.name = "Tile_" + x + "_" + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);
                tile_go.transform.SetParent(this.transform, true);

                // Adding a sprite renderer component,
                // with a default sprite (grassSprite).
                SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer>();
                sr.sprite = grassSprite;
                sr.sortingLayerName = "Tiles";

                OnTileChanged(tile_data);
            }
        }

        // Register our callbacks so that our GameObjects update accordingly.
        world.RegisterTileChanged(OnTileChanged);

	}

    // This function should be called automatically whenever a tile's type gets changed.
    void OnTileChanged(Tile tile_data)
    {

        if(tileGameObjectMap.ContainsKey(tile_data) == false)
        {
            Debug.LogError("doesn't contain the tile_data");
            return;
        }

        GameObject tile_go = tileGameObjectMap[tile_data];

        if (tile_go == null)
        {
            Debug.LogError("tileGamObjectMap doesn't contain the tile_go for " + tile_data);
            return;
        }

        if (tile_data.Type == TileType.Floor)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = concreteSprite;
        }
        else if (tile_data.Type == TileType.Grass)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = grassSprite;
        }
        else
        {
            Debug.LogError("OnTileChanged - Unrecognized tile type!");
        }
    }
}
