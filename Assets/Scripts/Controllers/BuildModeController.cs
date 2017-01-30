using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeController : MonoBehaviour
{
    private World World
    {
        get { return WorldController.Instance.world; }
    }

    private bool buildModeIsFurniture = false;

    private TileType buildModeTile = TileType.Floor;
    private string buildModeFurnitureType;

    public void SetMode_BuildFloor()
    {
        buildModeIsFurniture = false;
        buildModeTile = TileType.Floor;
    }

    public void SetMode_BuildFurniture(string furnitureType)
    {
        buildModeIsFurniture = true;
        buildModeFurnitureType = furnitureType;
    }

    public void SetMode_Bulldoze()
    {
        buildModeIsFurniture = false;
        this.buildModeTile = TileType.Grass;
    }

    public void DoBuild(Tile t)
    {
        if (this.buildModeIsFurniture)
        {
            string furnitureType = this.buildModeFurnitureType;

            // Can we build the furniture in the selected tile?
            // Run the ValidPlacement function
            if (this.World.IsFurniturePlacementValid(furnitureType, t) &&
                t.pendingFurnitureJob == null
                )
            {
                // This tile is valid for this furniture
                // Create the Furniture construction job and enqueue it.
                Job j = new Job(t,furnitureType, (theJob) => {
                    this.World.PlaceFurniture(furnitureType, theJob.tile);
                    t.pendingFurnitureJob = null;
                }
                );

                t.pendingFurnitureJob = j;
                j.RegisterJobCancelCallback((theJob) => { theJob.tile.pendingFurnitureJob = null; });

                // Add the job to the queue
                this.World.jobQueue.Enqueue(j);
        }
        else
        {
            t.Type = this.buildModeTile;
        }
    }
}