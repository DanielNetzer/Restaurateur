using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

// TileType is the base type of the tile. In some tile-based games, that might be
// the terrain type. For us, we only need to differentiate between empty space
// and floor (a.k.a. the station structure/scaffold). Walls/Doors/etc... will be
// Furniture sitting on top of the floor.
public enum TileType { Grass, Floor, Water };
public enum ENTERABILITY { Yes, Never, Soon };

public class Tile : IXmlSerializable
{

    private TileType _type = TileType.Grass;

    // The function we callback any time our type changes
    Action<Tile> cbTileChanged;

    public TileType Type
    {
        get { return _type; }
        set
        {
            TileType oldType = _type;
            _type = value;
            // Call the callback and let things know we've changed.

            if (cbTileChanged != null && oldType != _type)
                cbTileChanged(this);
        }
    }

    public float movementCost
    {
        get
        {
            if (Type == TileType.Water)
                return 0;
            if (furniture == null)
                return 1;

            return 1 * furniture.movementCost;
        }
    }

    // Inventory is something like a drill or a stack of metal sitting on the floor
    // Inventory looseObject;

    // Furniture is something like a wall, door, or sofa.
    public Furniture furniture { get; protected set; }

    // set pending jobs for a specific tile
    public Job pendingFurnitureJob;

    // We need to know the context in which we exist. Probably. Maybe.
    public World world { get; protected set; }
    public int X { get; protected set; }
    public int Y { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class.
    /// </summary>
    /// <param name="world">A World instance.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile(World world, int x, int y)
    {
        this.world = world;
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Register a function to be called back when our tile type changes.
    /// </summary>
    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged += callback;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged -= callback;
    }

    public bool PlaceFurniture(Furniture objInstance)
    {
        if (objInstance == null)
        {
            // We are uninstalling whatever was here before.
            furniture = null;
            return true;
        }

        // ObjInstance isnt' null
        if (furniture != null)
        {
            // Debug.LogError("Trying to assign an installed furniture to a tile that already has one!");
            return false;
        }

        // At this point everything's fine!

        furniture = objInstance;
        return true;
    }

    public bool IsNeighbour(Tile tile, bool diagOkay = false)
    {

        return Mathf.Abs(this.X - tile.X) + Mathf.Abs(this.Y - tile.Y) == 1 || // Check hori/vert adj.
            (diagOkay && (Mathf.Abs(this.X - tile.X) == 1 && Mathf.Abs(this.Y - tile.Y) == 1)); // Check diag adj.g

    }

    public Tile[] GetNeighbours(bool diagOkay = false)
    {
        Tile[] ns;

        if (diagOkay == false)
        {
            ns = new Tile[4]; // Tile Order : N E S W
        }
        else
        {
            ns = new Tile[8]; // Tile Order : N E S W NE SE SW NW
        }

        Tile n;

        n = world.GetTileAt(X, Y + 1);
        //if (n == null) Debug.LogError("Saving NULL Coordinates aswell as neighbours.");
        ns[0] = n; // N
        n = world.GetTileAt(X + 1, Y);
        //if (n == null) Debug.LogError("Saving NULL Coordinates aswell as neighbours.");
        ns[1] = n; // E
        n = world.GetTileAt(X, Y - 1);
        //if (n == null) Debug.LogError("Saving NULL Coordinates aswell as neighbours.");
        ns[2] = n; // S
        n = world.GetTileAt(X - 1, Y);
        //if (n == null) Debug.LogError("Saving NULL Coordinates aswell as neighbours.");
        ns[3] = n; // W

        if (diagOkay == true)
        {
            n = world.GetTileAt(X + 1, Y + 1);
            ns[4] = n;  // NE
            n = world.GetTileAt(X + 1, Y - 1);
            ns[5] = n;  // SE
            n = world.GetTileAt(X - 1, Y - 1);
            ns[6] = n;  // SW
            n = world.GetTileAt(X - 1, Y + 1);
            ns[7] = n;  // NW
        }

        return ns;
    }

    public ENTERABILITY IsEnterable()
    {
        if (movementCost == 0)
            return ENTERABILITY.Never;

        if (furniture != null && furniture.IsEnterable != null)
        {
            return furniture.IsEnterable(furniture);
        }

        return ENTERABILITY.Yes;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    ///                 SAVING & LOADING
    /// 
    /////////////////////////////////////////////////////////////////////////////////////////////////////////

    public XmlSchema GetSchema() { return null; }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader)
    {
        Type = (TileType)int.Parse(reader.GetAttribute("Type"));
    }
}
