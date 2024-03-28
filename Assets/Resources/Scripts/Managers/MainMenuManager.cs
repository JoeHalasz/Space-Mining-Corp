using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MainMenuManager : MonoBehaviour
{

    GameObject mainMenuCanvas;
    WorldManager worldManager;
    GameObject newGameGroup;
    GameObject loadGameGroup;
    GameObject settingsGroup;
    GameObject selectedLoadRow;
    GameObject loadGameSelectionRow;
    GameObject loadGameScrollContent;

    void Start()
    {
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        worldManager = GetComponent<WorldManager>();
        newGameGroup = GameObject.Find("NewGameGroup");
        loadGameGroup = GameObject.Find("LoadGameGroup");
        settingsGroup = GameObject.Find("SettingsGroup");
        loadGameSelectionRow = GameObject.Find("LoadGameSelectionRow");
        loadGameScrollContent = GameObject.Find("LoadGameScrollContent");
        newGameGroup.SetActive(false);
        loadGameGroup.SetActive(false);
        settingsGroup.SetActive(false);
    }


    public void OnPressNewGame()
    {
        newGameGroup.SetActive(true);
        loadGameGroup.SetActive(false);
        settingsGroup.SetActive(false);
    }

    public void onNewGameConfirm()
    {
        // get the textmeshpro from newGameGroup.transform.Find("NameInput")
        string name = newGameGroup.transform.Find("NameInput").GetComponent<TMPro.TMP_InputField>().text;
        string seedStr = newGameGroup.transform.Find("SeedInput").GetComponent<TMPro.TMP_InputField>().text;
        if (seedStr == null || seedStr == "")
        {
            seedStr = "-1";
        }
        int seed = int.Parse(seedStr);
        // create the world
        if (name == "" || name == " ")
        {
            // TODO figure out what to do
            Debug.Log("Invalid name for new world [" + name + "]");
        }
        else
        {
            Debug.Log("Creating new world with name: " + name + " and seed: " + seed);
            newGameGroup.transform.Find("NameInput").GetComponent<TMPro.TMP_InputField>().text = "";
            newGameGroup.transform.Find("SeedInput").GetComponent<TMPro.TMP_InputField>().text = "";
            worldManager.createNewWorld(name, seed);
            newGameGroup.SetActive(false);
            loadGameGroup.SetActive(false);
            settingsGroup.SetActive(false);
        }
    }


    public void OnSelectSaveForLoading()
    {
        selectedLoadRow = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
    }

    public void onLoadGameConfirm()
    {
        // get the child named SaveName under selectedLoadRow
        string saveName = selectedLoadRow.transform.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text;
        #if UNITY_EDITOR
            Debug.Log("Loading save: " + saveName);
        #endif
        worldManager.Load(saveName);
        newGameGroup.SetActive(false);
        loadGameGroup.SetActive(false);
        settingsGroup.SetActive(false);
    }

    public void OnDeleteSavePress()
    {
        selectedLoadRow = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        // TODO show the confirmation
    }

    public void OnDeleteConfirm()
    {
        // delete the save
    }

    public void OnPressLoadGame()
    {
        newGameGroup.SetActive(false);
        loadGameGroup.SetActive(true);
        settingsGroup.SetActive(false);
        if (!Directory.Exists("data"))
        {
            Directory.CreateDirectory("data");
        }
        if (!Directory.Exists("data/saves"))
        {
            Directory.CreateDirectory("data/saves");
        }
        string[] files = Directory.GetFiles("data/saves");

        // delete all of the children of the LoadGameScrollContent except for the loadGameSelectionRow
        foreach (Transform child in loadGameScrollContent.transform)
        {
            if (child.gameObject != loadGameSelectionRow)
                GameObject.Destroy(child.gameObject);
        }
        loadGameSelectionRow.SetActive(false);

        int i = 0;
        foreach (string file in files)
        {
            if (file.EndsWith(".meta"))
            {
                string saveData = File.ReadAllText(file);
                // make a copy of the loadGameSelectionRow and copy in the correct data
                GameObject newLoadGameSelectionRow = Instantiate(loadGameSelectionRow, loadGameScrollContent.transform);
                newLoadGameSelectionRow.SetActive(true);
                newLoadGameSelectionRow.transform.SetParent(loadGameScrollContent.transform);
                newLoadGameSelectionRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -i++ * 70);
                string saveName = saveData.Split(';')[0];
                string saveDateTime = saveData.Split(';')[1];
                newLoadGameSelectionRow.transform.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text = saveName;
                newLoadGameSelectionRow.transform.Find("DateAndTimeSaved").GetComponent<TMPro.TextMeshProUGUI>().text = saveDateTime;
            }
        }
    }

    public void OnPressSettings()
    {
        newGameGroup.SetActive(false);
        loadGameGroup.SetActive(false);
        settingsGroup.SetActive(true);
    }

    public void OnPressExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
