using UnityEngine;
using System.Collections.Generic;

public class CharacterSpriteController : MonoBehaviour {

    Dictionary<string, Sprite> characterSprites;
    Dictionary<Character, GameObject> characterGameObjectMap;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    // Use this for initialization
    void Start () {

        LoadSprites();

        // Instantiate our dictionary that tracks which GamObject is rendering which Tile data.
        characterGameObjectMap = new Dictionary<Character, GameObject>();

        // Register our callbacks so that our GameObjects update accordingly.
        world.RegisterCharacterCreated(OnCharacterCreated);

        // check for pre-existing characters, which wont do the callback.
        foreach (Character c in world.characters)
        {
            OnCharacterCreated(c);
        }
    }

    void LoadSprites()
    {
        characterSprites = new Dictionary<string, Sprite>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Character/");

        foreach (Sprite s in sprites)
        {
            characterSprites[s.name] = s;
        }
    }

    public void OnCharacterCreated(Character character)
    {

        // Creating a new GameObject and adding it to our scene.
        GameObject c_go = new GameObject();

        characterGameObjectMap.Add(character, c_go);

        c_go.name = "Character";
        c_go.transform.position = new Vector3(character.X, character.Y, 0);
        c_go.transform.SetParent(this.transform, true);

        // Adding a sprite renderer component, sprites will be added later.
        SpriteRenderer sr = c_go.AddComponent<SpriteRenderer>();
        sr.sprite = characterSprites["MainGuyFrontIdle"];
        sr.sortingLayerName = "Characters";

        character.RegisterOnChangedCallback(OnCharacterChanged);
    }

    void OnCharacterChanged(Character character)
    {

        if (characterGameObjectMap.ContainsKey(character) == false)
        {
            Debug.LogError("OnCharacterChanged -- Trying to change visuals for character not in our map.");
            return;
        }

        GameObject c_go = characterGameObjectMap[character];
        //c_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
        c_go.transform.position = new Vector3(character.X, character.Y, 0);
    }

}
