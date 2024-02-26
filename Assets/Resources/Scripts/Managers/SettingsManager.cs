using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
class Settings // for now just settings that dont have to do with unity quality 
{
    public float renderDistance; // TODO add connection to unity generation script. Will have to reload to get this to work. 

    // TODO add keybinds
    // TODO add unity quality settings
    // TODO add sound settings

}

public class SettingsManager : MonoBehaviour
{
    // should save and load to a file in the data folder
    Settings settings;
    void Save()
    {
        if (!Directory.Exists("data"))
        {
            Directory.CreateDirectory("data");
        }
        // save a file under data/settings.dat
        string filePath = "data/settings.dat";
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FileStream file = File.Create(filePath);
        if (File.Exists(filePath))
        {
#if UNITY_EDITOR
                Debug.Log("File created successfully");
#endif
        }
        else
        {
#if UNITY_EDITOR
                Debug.Log("File creation failed");
#endif
            return;
        }
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, settings);
        file.Close();
    }

    void Load()
    {
        // load a file under saves/{name}.dat
        string filePath = "data/settings.dat";
        if (File.Exists(filePath))
        {
#if UNITY_EDITOR
                Debug.Log("File exists");
#endif
        }
        else
        {
#if UNITY_EDITOR
                Debug.Log("File does not exist");
#endif
            return;
        }
        FileStream file = File.Open(filePath, FileMode.Open);
        // load the seed, name and name2
        BinaryFormatter bf = new BinaryFormatter();
        settings = (Settings)bf.Deserialize(file);
        file.Close();
    }
}
