using UnityEngine;

public class SoundController : MonoBehaviour {

    float soundCooldown = 0f;

    World world
    {
        get { return WorldController.Instance.world; }
    }

	// Use this for initialization
	void Start () {

        world.RegisterFurnitureCreated ( OnFurnitureCreated );

        world.RegisterTileChanged ( OnTileChanged );

    }
	
    void Update()
    {
        soundCooldown -= Time.deltaTime;
    }

    void OnTileChanged (Tile t)
    {
        if (soundCooldown > 0)
            return;

        AudioClip ac = Resources.Load<AudioClip>("Sound/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(ac, new Vector3(t.X, t.Y, 0));

        soundCooldown = 0.1f;
    }

    void OnFurnitureCreated ( Furniture furn )
    {
        if (soundCooldown > 0)
            return;

        AudioClip ac = Resources.Load<AudioClip>("Sound/Wall_OnCreated");
        AudioSource.PlayClipAtPoint(ac, new Vector3(furn.tile.X, furn.tile.Y, 0));

        soundCooldown = 0.1f;
    }
}
