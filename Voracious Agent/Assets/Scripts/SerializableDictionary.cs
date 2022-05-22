using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

[System.Serializable]
public class SerializableDictionary
{
    Dictionary<string, GameObject> dictionary;
    public List<string> keys;
    public List<GameObject> objects;
    public GameObject this[string key]
    {
        get { return dictionary[key]; }
    }
    public void Initialize()
    {
        if (keys.Count != objects.Count)
            throw new System.Exception("Lists of keys and gameobjects must be same length.");

        dictionary = new Dictionary<string, GameObject>();
        for (int i = 0; i < keys.Count; ++i)
        {
            dictionary.Add(keys[i], objects[i]);
        }
    }
    public SerializableDictionary(in SerializableDictionary copy)
    {
        keys = new List<string>();
        objects = new List<GameObject>();
        for (int i = 0; i < copy.keys.Count; ++i)
        {
            keys.Add(copy.keys[i]);
            objects.Add(copy.objects[i]);
        }
        Initialize();
    }
}