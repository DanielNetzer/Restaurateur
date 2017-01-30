using UnityEngine;
using System.Collections;

public static class FurnitureActions {

	public static void Door_UpdateAction(Furniture furn, float deltaTime)
    {
        if (furn.furnParams["IsOpening"] >= 1)
        {
            furn.furnParams["DoorStatus"] += deltaTime * 4;
            if (furn.furnParams["DoorStatus"] >= 1)
            {
                furn.furnParams["IsOpening"] = 0;
            }
        }
        else
        {
            furn.furnParams["DoorStatus"] -= deltaTime * 4;
        }

        furn.furnParams["DoorStatus"] = Mathf.Clamp01(furn.furnParams["DoorStatus"]);
        furn.cbOnChanged(furn);
    }

    public static ENTERABILITY Door_IsEnterable(Furniture furn)
    {
        furn.furnParams["IsOpening"] = 1;

        if (furn.furnParams["DoorStatus"] >=1 )
        {
            return ENTERABILITY.Yes;
        }

        return ENTERABILITY.Soon;
    }
}
