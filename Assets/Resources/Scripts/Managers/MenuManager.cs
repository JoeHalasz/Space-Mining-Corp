using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MenuManager : MonoBehaviour
{

    GameObject mainMenuCanvas;
    GameObject escapeMenuCanvas;
    WorldManager worldManager;
    GameObject newGameGroup;
    GameObject loadGameGroup;
    GameObject settingsGroup;
    GameObject deleteSaveConfirmationGroup;
    GameObject escapeMenuGroup;
    GameObject selectedLoadRow;
    GameObject loadGameSelectionRow;
    GameObject loadGameScrollContent;
    bool fromEscapeMenu = false;

    void Start()
    {
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        worldManager = GetComponent<WorldManager>();
        newGameGroup = GameObject.Find("NewGameGroup");
        loadGameGroup = GameObject.Find("LoadGameGroup");
        settingsGroup = GameObject.Find("SettingsGroup");
        deleteSaveConfirmationGroup = GameObject.Find("DeleteSaveConfirmationGroup");
        escapeMenuGroup = GameObject.Find("EscapeMenuGroup");
        loadGameSelectionRow = GameObject.Find("LoadGameSelectionRow");
        loadGameScrollContent = GameObject.Find("LoadGameScrollContent");
        newGameGroup.SetActive(false);
        loadGameGroup.SetActive(false);
        settingsGroup.SetActive(false);
        deleteSaveConfirmationGroup.SetActive(false);
    }

    public void turnAllGroupsOff()
    {
        newGameGroup.SetActive(false);
        loadGameGroup.SetActive(false);
        settingsGroup.SetActive(false);
        deleteSaveConfirmationGroup.SetActive(false);
        escapeMenuGroup.SetActive(false);
    }


    public void OnPressNewGame()
    {
        turnAllGroupsOff();
        newGameGroup.SetActive(true);
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
            turnAllGroupsOff();
        }
    }

    public void OnPressLoadGameFromEscapeMenu()
    {
        fromEscapeMenu = true;
        mainMenuCanvas.SetActive(true);
        // set all children to inactive
        foreach (Transform child in mainMenuCanvas.transform)
        {
            child.gameObject.SetActive(false);
        }
        OnPressLoadGame();
    }

    public void OnPressSettingsFromEscapeMenu()
    {
        fromEscapeMenu = true;
        mainMenuCanvas.SetActive(true);
        // set all children to inactive
        foreach (Transform child in mainMenuCanvas.transform)
        {
            child.gameObject.SetActive(false);
        }
        OnPressSettings();
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
        turnAllGroupsOff();
    }

    public void OnDeleteSavePress()
    {
        selectedLoadRow = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.parent.gameObject;
        deleteSaveConfirmationGroup.SetActive(true);
        loadGameGroup.SetActive(false);
    }

    public void OnDeleteSaveConfirm()
    {
        // delete the save
        string saveName = selectedLoadRow.transform.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text;
        string[] files = Directory.GetFiles("data/saves");
        foreach (string file in files)
        {
            if (file.EndsWith(".meta"))
            {
                string saveData = File.ReadAllText(file);
                if (saveData.Split(';')[0] == saveName)
                {
                    File.Delete(file);
                    // delete the file with the same name but without the .meta and with the .dat extension
                    File.Delete(file.Replace(".meta", ".dat"));
                    deleteSaveConfirmationGroup.SetActive(false);
                    break;
                }
            }
        }
        OnPressLoadGame();
    }

    public void OnDeleteSaveCancel()
    {
        deleteSaveConfirmationGroup.SetActive(false);
        loadGameGroup.SetActive(true);
    }

    public void OnPressLoadGame()
    {
        turnAllGroupsOff();
        loadGameGroup.SetActive(true);

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
        turnAllGroupsOff();
        settingsGroup.SetActive(true);
    }

    public void OnPressMainMenu()
    {
        turnAllGroupsOff();
        worldManager.backToMainMenu();
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
