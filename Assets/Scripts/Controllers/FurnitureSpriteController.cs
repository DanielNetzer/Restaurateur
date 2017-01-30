using UnityEngine;
using System.Collections.Generic;

public class FurnitureSpriteController : MonoBehaviour {

    Dictionary<string, Sprite> furnitureSprites;
    Dictionary<Furniture, GameObject> furnitureGameObjectMap;

    World world
    {
        get { return WorldController.Instance.world; }
    }

	// Use this for initialization
	void Start () {

        LoadSprites();

        // Instantiate our dictionary that tracks which GamObject is rendering which Tile data.
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        // Register our callbacks so that our GameObjects update accordingly.
        world.RegisterFurnitureCreated(OnFurnitureCreated);

        // Go through all the furniture and call the Oncreated event manually.
        foreach(Furniture furn in world.furnitures)
        {
            OnFurnitureCreated(furn);
        }
	}

    void LoadSprites()
    {
        furnitureSprites = new Dictionary<string, Sprite>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Furniture/");

        foreach (Sprite s in sprites)
        {
            furnitureSprites[s.name] = s;
        }
    }

    public void OnFurnitureCreated(Furniture furn)
    {
        // Debug.LogError("OnFurnitureCreated");
        // FIXME : does not consider multi-tile Furnitures nor rotated Furnitures.

        // Creating a new GameObject and adding it to our scene.
        GameObject furn_go = new GameObject();

        furnitureGameObjectMap.Add(furn, furn_go);

        furn_go.name = furn.furnitureType +"_" + furn.tile.X + "_" + furn.tile.Y;
        furn_go.transform.position = new Vector3(furn.tile.X, furn.tile.Y, 0);
        furn_go.transform.SetParent(this.transform, true);

        // Door rotation logic
        if (furn.furnitureType == "Door")
        {
            // Default door sprite is for east and west walls,
            // check to see if we have walls to the north and south, and if
            // so then rotate this GO by 90 degrees.
            Tile northTile = world.GetTileAt(furn.tile.X, furn.tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.tile.X, furn.tile.Y - 1);

            if (northTile != null && southTile != null
                && northTile.furniture != null &&
                southTile.furniture != null &&
                northTile.furniture.furnitureType == "Wall"
                && southTile.furniture.furnitureType == "Wall")
            {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
                furn_go.transform.Translate(1f, 0, 0, Space.World); // ugly HACK to compensate on the bottom left pivot for sprites.
            }
        }

        // Adding a sprite renderer component, sprites will be added later.
        SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furn);
        sr.sortingLayerName = "Furnitures";

        furn.RegisterOnChangedCallback(OnFurnitureChanged);
    }

    void OnFurnitureChanged (Furniture furn)
    {
        
        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureChanged -- Trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];
        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
    }

    public Sprite GetSpriteForFurniture(Furniture furn)
    {
        string spriteName = furn.furnitureType;

        if ( furn.linksToNeighbour == false )
        {
            // if this is a DOOR, lets check DoorStatus and update the status.
            if (furn.furnitureType == "Door")
            {
                if (furn.furnParams["DoorStatus"] < 0.1f)
                {
                    // Door is closed.
                    spriteName = "Door";
                }
                else if (furn.furnParams["DoorStatus"] < 0.5f)
                {
                    // Door is a bit open.
                    spriteName = "Door_Status1";
                }
                else if (furn.furnParams["DoorStatus"] < 0.9f)
                {
                    // Door is a lot open.
                    spriteName = "Door_Status2";
                }
                else
                {
                    // Door is fully open.
                    spriteName = "Door_Status3";
                }
            }

            return furnitureSprites[spriteName];
        }

        spriteName = furn.furnitureType + "_";

        // Check for neighbours North, East, South, West (Walls graphics logic)
        int x = furn.tile.X;
        int y = furn.tile.Y;
        Tile t;

        t = world.GetTileAt(x, y + 1);
        if (t!= null && t.furniture != null && t.furniture.furnitureType == furn.furnitureType)
        {
            spriteName += "N";
        }

        t = world.GetTileAt(x+1, y);
        if (t != null && t.furniture != null && t.furniture.furnitureType == furn.furnitureType)
        {
            spriteName += "E";
        }

        t = world.GetTileAt(x, y - 1);
        if (t!= null && t.furniture != null && t.furniture.furnitureType == furn.furnitureType)
        {
            spriteName += "S";
        }

        t = world.GetTileAt(x-1, y);
        if (t != null && t.furniture != null && t.furniture.furnitureType == furn.furnitureType)
        {
            spriteName += "W";
        }

        if (furnitureSprites.ContainsKey(spriteName) == false)
        {
            Debug.LogError("GetSpriteForFurniture - No Sprite with name: " + spriteName);
            return null;
        }

        return furnitureSprites[spriteName];
    }

    public Sprite GetSpriteForFurniture(string furnitureType)
    {
        if (furnitureSprites.ContainsKey(furnitureType))
        {
            return furnitureSprites[furnitureType];
        }

        if (furnitureSprites.ContainsKey(furnitureType+"_"))
        {
            return furnitureSprites[furnitureType+"_"];
        }

        Debug.LogError("GetSpriteForFurniture - No Sprite with name: " + furnitureType);
        return null;
    }
}
