using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class World : IXmlSerializable
{

    // A two-dimensional array to hold our tile data.
    Tile[,] tiles;

    public List<Character> characters;
    public List<Furniture> furnitures;
    public List<Room>      rooms;
    

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;

    Dictionary<string, Furniture> furniturePrototypes;

    // The tile width of the world.
    public int Width { get; protected set; }

    // The tile height of the world
    public int Height { get; protected set; }

    Action<Furniture> cbFurnitureCreated;
    Action<Tile> cbTileChanged;
    Action<Character> cbCharacterCreated;

    public JobQueue jobQueue;

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// </summary>
    /// <param name="width">Width in tiles.</param>
    /// <param name="height">Height in tiles.</param>
    public World(int width, int height)
    {
        SetupWorld(width, height);

        // Create one character
        CreateCharacter(tiles[Width / 2, Height / 2]);
    }

    void SetupWorld(int width, int height)
    {
        jobQueue = new JobQueue();

        Width = width;
        Height = height;

        tiles = new Tile[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
            }
        }

        CreateFurniturePrototypes();

        characters = new List<Character>();
        furnitures = new List<Furniture>();
        rooms = new List<Room>();

    }

    public Character CreateCharacter(Tile t)
    {
        Character c = new Character(t);

        characters.Add(c);

        if (cbCharacterCreated != null)
            cbCharacterCreated(c);

        return c;
    }

    void CreateFurniturePrototypes()
    {
        // This will be replaced by a function that reads all of our furniture data
        // from a text file in the future.

        furniturePrototypes = new Dictionary<string, Furniture>();

        furniturePrototypes.Add("Wall", 
            new Furniture(
                             "Wall",
                              0, // Impassable
                              1, // Width
                              1, // Height
                              true // Links to neighbours
                          )
            );

        furniturePrototypes.Add("Door",
            new Furniture(
                             "Door",
                              1, // movementCost
                              1, // Width
                              1, // Height
                              false // Links to neighbours
                          )
            );

        furniturePrototypes["Door"].furnParams["DoorStatus"] = 0;
        furniturePrototypes["Door"].furnParams["IsOpening"] = 0;
        furniturePrototypes["Door"].updateActions += FurnitureActions.Door_UpdateAction;
        furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;
    }

    /// <summary>
	/// A function for testing out the system
	/// </summary>
    public void GenerateWorld()
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {   
                    tiles[x, y].Type = TileType.Grass;
            }
        }

        Debug.Log("World created with: " + Width * Height + " tiles");
    }

    /// <summary>
	/// Gets the tile data at x and y.
	/// </summary>
	/// <returns>The <see cref="Tile"/>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
    public Tile GetTileAt(int x, int y)
    {
        if (x >= Width || x < 0 || y >= Height || y < 0)
        {
            //Debug.LogError("Tile (" + x + ", " + y + ") is out of range.");
            return null;
        }
        return tiles[x, y];
    }

    public Furniture PlaceFurniture(string furnitureType, Tile t)
    {
        if (furniturePrototypes.ContainsKey(furnitureType) == false)
        {
            Debug.LogError("FurniturePrototypes doesn't contain a proto for key: " + furnitureType);
            return null;
        }
        Furniture furn = Furniture.PlaceInstance(furniturePrototypes[furnitureType], t);

        if (furn == null)
        {
            return null;
        }

        furnitures.Add(furn);

        if (cbFurnitureCreated != null)
        {
            cbFurnitureCreated(furn);
            InvalidateTileGraph();
        }

        return furn;
    }

    public void RegisterFurnitureCreated(Action<Furniture> callbackfunc)
    {
        cbFurnitureCreated += callbackfunc;
    }

    public void UnregisterFurnitureCreated(Action<Furniture> callbackfunc)
    {
        cbFurnitureCreated -= callbackfunc;
    }

    public void RegisterCharacterCreated(Action<Character> callbackfunc)
    {
        cbCharacterCreated += callbackfunc;
    }

    public void UnregisterCharacterCreated(Action<Character> callbackfunc)
    {
        cbCharacterCreated -= callbackfunc;
    }

    public void RegisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged += callbackfunc;
    }

    public void UnregisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged -= callbackfunc;
    }

    // Gets called whenever ANY tile changes.
    public void OnTileChanged(Tile t)
    {
        if (cbTileChanged == null)
            return;
        cbTileChanged(t);

        InvalidateTileGraph();
    }

    public bool IsFurniturePlacementValid(string furnitureType, Tile t)
    {
        return furniturePrototypes[furnitureType].funcPositionValidation(t);
    }

    public Furniture GetFurniturePrototype(string furnitureType)
    {
        if (furniturePrototypes.ContainsKey(furnitureType) == false)
        { 
            Debug.LogError("No Furniture with Type: " + furnitureType);
            return null;
        }
        return furniturePrototypes[furnitureType];
    }

    public void Update(float deltaTime)
    {
        foreach (Character c in characters)
        {
            c.Update(deltaTime);
        }

        foreach (Furniture f in furnitures)
        {
            f.Update(deltaTime);
        }
    }

    // This should be called whenever a change to the world
    // means that our old pahtfinding info is invalid
    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    ///                 SAVING & LOADING
    /// 
    /////////////////////////////////////////////////////////////////////////////////////////////////////////

    public World() {}

    public XmlSchema GetSchema() { return null; }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        // Save all tiles data.
        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y< Height; y++)
            {
                writer.WriteStartElement("Tile");
                tiles[x, y].WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        writer.WriteEndElement();

        // Save all furniture data.
        writer.WriteStartElement("Furnitures");
        foreach (Furniture furn in furnitures)
        { 
            writer.WriteStartElement("Furniture");
            furn.WriteXml(writer);
            writer.WriteEndElement();   
        }
        writer.WriteEndElement();

        // Save all characters data.
        writer.WriteStartElement("Characters");
        foreach (Character c in characters)
        {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

    }

    public void ReadXml(XmlReader reader)
    {
        
        // Load info here
        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(Width, Height);
        
        while(reader.Read())
        {
            switch (reader.Name)
            {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Furnitures":
                    ReadXml_Furnitures(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
            }


        }
    }

    void ReadXml_Tiles(XmlReader reader)
    {
        if (reader.ReadToDescendant("Tile"))
        {
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                tiles[x, y].ReadXml(reader);

            } while (reader.ReadToNextSibling("Tile"));
        }
    }

    void ReadXml_Furnitures(XmlReader reader)
    {

        if (reader.ReadToDescendant("Furniture"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Furniture furn = PlaceFurniture(reader.GetAttribute("furnitureType"), tiles[x, y]);
                furn.ReadXml(reader);

            } while (reader.ReadToNextSibling("Furniture"));
        }
    }

    void ReadXml_Characters(XmlReader reader)
    {

        if (reader.ReadToDescendant("Character"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Character c = CreateCharacter(tiles[x, y]);
                c.ReadXml(reader);

            } while (reader.ReadToNextSibling("Character"));
        }
    }
}
