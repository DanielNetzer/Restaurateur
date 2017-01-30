using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

// Furniture are things like walls, doors and funrniture

public class Furniture : IXmlSerializable
{

    // Dictionary of Strings/Furniture Paramaters.
    public Dictionary<string, float> furnParams;

    // Array of all Dynamic furnitures actions thats needs updated every tick.
    public Action<Furniture, float> updateActions;

    public Func<Furniture, ENTERABILITY> IsEnterable;

    public void Update(float deltaTime)
    {
        if (updateActions != null)
        {
            updateActions(this, deltaTime);
        }
    }

    // Base tile.
    public Tile tile { get; protected set; }

    // sort of an ID for the furniture to help us render the graphics
    public string furnitureType { get; protected set; }

    // Furniture dimensions
    int width;
    int height;

    public bool linksToNeighbour { get; protected set; }

    public Action<Furniture> cbOnChanged;

    public Func<Tile, bool> funcPositionValidation;

    // A multiplier that will reduce the movement speed according to enviromental changes (Walls, Furnitures, Rough tiles).
    // SPECIAL : if movementCost = 0, tile is impassible.
    public float movementCost { get; protected set; }

    public Furniture() { /* Used for serialization mostly */
        furnParams = new Dictionary<string, float>();
    }

    // Copy constructur
    protected Furniture(Furniture other)
    {
        this.furnitureType = other.furnitureType;
        this.movementCost = other.movementCost;
        this.width = other.width;
        this.height = other.height;
        this.linksToNeighbour = other.linksToNeighbour;

        this.furnParams = new Dictionary<string, float>(other.furnParams);
        this.IsEnterable = other.IsEnterable;

        if(other.updateActions != null)
        {
            this.updateActions = (Action<Furniture, float>)other.updateActions.Clone();
        }
    }

    virtual public Furniture Clone()
    {
        return new Furniture(this);
    }

    // Create fully parameterezied Furniture
    public Furniture (string furnitureType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false)
    {
        this.furnitureType = furnitureType;
        this.movementCost = movementCost;
        this.width = width;
        this.height = height;
        this.linksToNeighbour = linksToNeighbour;

        furnParams = new Dictionary<string, float>();

        this.funcPositionValidation = this.IsValidPosition;
    }

    static public Furniture PlaceInstance(Furniture proto, Tile tile)
    {

        if (proto.funcPositionValidation(tile) == false)
        {
            Debug.LogError("PlaceInstance -- Position Validity Function returned FALSE.");
            return null;
        }

        Furniture furn = proto.Clone();

        furn.tile = tile;

        if(tile.PlaceFurniture(furn) == false)
        {
            // For some reason, we weren't able to place our furniture in this tile.
            return null;
        }

        if(furn.linksToNeighbour)
        {
            // this furniture have neighbours links itself to its neighbours,
            // so we should inform our neighbours that they have a new graphic.
            // just trigger their OnChangedCallback.
            Tile t;
            int x = tile.X;
            int y = tile.Y;

            t = tile.world.GetTileAt(x, y + 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.furnitureType == furn.furnitureType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = tile.world.GetTileAt(x+1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.furnitureType == furn.furnitureType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = tile.world.GetTileAt(x, y - 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.furnitureType == furn.furnitureType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }

            t = tile.world.GetTileAt(x-1, y);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.furnitureType == furn.furnitureType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
        }

        return furn;
    }

    public void RegisterOnChangedCallback(Action<Furniture> callbackfunc)
    {
        cbOnChanged += callbackfunc;
    }

    public void unRegisterOnChangedCallback(Action<Furniture> callbackfunc)
    {
        cbOnChanged -= callbackfunc;
    }

    public bool IsValidPosition(Tile t)
    {
        // Check if tileType is ground (floor, grass...)
        // Check tile doesnt have a furn on him already(wall, sofa, chair....)
        if ((t.Type != TileType.Floor && t.Type != TileType.Grass) || t.furniture != null)
        {
            return false;
        }

        return true;
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///
    ///                 SAVING & LOADING
    /// 
    /////////////////////////////////////////////////////////////////////////////////////////////////////////

    public XmlSchema GetSchema() { return null; }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("furnitureType", furnitureType);

        foreach (string k in furnParams.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("Name", k);
            writer.WriteAttributeString("Value", furnParams[k].ToString());
            writer.WriteEndElement();
        }
        
    }

    public void ReadXml(XmlReader reader)
    {
        if(reader.ReadToDescendant("Param"))
        {
            do
            {
                string k = reader.GetAttribute("Name");
                float v = float.Parse(reader.GetAttribute("Value"));
                furnParams[k] = v;
            } while (reader.ReadToNextSibling("Param"));
        }
    }
}
