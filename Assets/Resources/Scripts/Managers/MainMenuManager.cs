using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

    GameObject mainMenuCanvas;
    WorldManager worldManager;
    GameObject newGameGroup;
    GameObject loadGameGroup;
    GameObject settingsGroup;
    GameObject selectedLoadRow;

    void Start()
    {
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        worldManager = GetComponent<WorldManager>();
        newGameGroup = GameObject.Find("NewGameGroup");
        loadGameGroup = GameObject.Find("LoadGameGroup");
        settingsGroup = GameObject.Find("SettingsGroup");
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
        // TODO use the selected load row
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
