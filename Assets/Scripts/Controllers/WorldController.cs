using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class WorldController : MonoBehaviour {

    public static WorldController Instance { get; protected set; }

    static bool loadWorld = false;

    // The world and tile data
    public World world { get; protected set; }

	// Use this for initialization
	void OnEnable () {

        if (Instance != null)
        {
            Debug.LogError("There shouldent be more then one instance of world controller!");
        }
        Instance = this;

        if(loadWorld)
        {
            loadWorld = false;
            CreateWorldFromSaveFile();
        } else
        {
            CreateEmptyWorld();
        }
        
	}

    void Update()
    {
        // TODO: Add pause/unpause, speed controls, etc.
        world.Update(Time.deltaTime);
    }

    /// <summary>
	/// Gets the tile at the unity-space coordinates
	/// </summary>
	/// <returns>The tile at world coordinate.</returns>
	/// <param name="coord">Unity World-Space coordinates.</param>
	public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.FloorToInt(coord.x);
        int y = Mathf.FloorToInt(coord.y);

        return world.GetTileAt(x, y);
    }

    void CreateEmptyWorld()
    {
        // Creating an empty world.
        world = new World(100, 100);

        // Center the Camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);

        // Shake things up, for testing.
        world.GenerateWorld();
    }

    void CreateWorldFromSaveFile()
    {
        // Creating an empty world from our save file data.
        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        world = (World)serializer.Deserialize(reader);
        reader.Close();

        // Center the Camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);

    }

    public void NewWorld()
    {
        // Debug.Log("NewWorld button was clicked.");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveWorld()
    {
        // Debug.Log("Save button was clicked.");

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, world);
        writer.Close();

        // Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());

    }

    public void LoadWorld()
    {
        // Debug.Log("Load button was clicked.");

        // Reload the scene and purge all old refferences.
        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
