using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MenuManager : MonoBehaviour
{

    GameObject mainMenuCanvas;
    GameObject escapeMenuCanvas;
    WorldManager worldManager;
    GameObject mainMenuGroup;
    GameObject newGameGroup;
    GameObject loadGameGroup;
    GameObject settingsGroup;
    GameObject deleteSaveConfirmationGroup;
    GameObject escapeMenuGroup;
    GameObject saveGameGroup;
    GameObject overrightSaveConfirmationGroup;
    GameObject backButton;
    GameObject selectedLoadRow;
    GameObject loadGameSelectionRow;
    GameObject loadGameScrollContent;
    GameObject saveGameSelectionRow;
    GameObject saveGameScrollContent;
    bool fromEscapeMenu = false;

    string tryingToSaveTo = null;

    void Start()
    {
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        worldManager = GetComponent<WorldManager>();
        mainMenuGroup = GameObject.Find("MainMenuGroup");
        newGameGroup = GameObject.Find("NewGameGroup");
        loadGameGroup = GameObject.Find("LoadGameGroup");
        settingsGroup = GameObject.Find("SettingsGroup");
        deleteSaveConfirmationGroup = GameObject.Find("DeleteSaveConfirmationGroup");
        escapeMenuGroup = GameObject.Find("EscapeMenuGroup");
        saveGameGroup = GameObject.Find("SaveGameGroup");
        overrightSaveConfirmationGroup = GameObject.Find("OverrightSaveConfirmationGroup");
        backButton = GameObject.Find("Back button");
        loadGameSelectionRow = GameObject.Find("LoadGameSelectionRow");
        loadGameScrollContent = GameObject.Find("LoadGameScrollContent");
        saveGameSelectionRow = GameObject.Find("SaveGameSelectionRow");
        saveGameScrollContent = GameObject.Find("SaveGameScrollContent");
        newGameGroup.SetActive(false);
        loadGameGroup.SetActive(false);
        settingsGroup.SetActive(false);
        deleteSaveConfirmationGroup.SetActive(false);
    }

    /* ------------------ Helper functions ------------------*/

    public void turnAllGroupsOff()
    {
        newGameGroup.SetActive(false);
        loadGameGroup.SetActive(false);
        settingsGroup.SetActive(false);
        deleteSaveConfirmationGroup.SetActive(false);
        escapeMenuGroup.SetActive(false);
        saveGameGroup.SetActive(false);
        overrightSaveConfirmationGroup.SetActive(false);
        backButton.SetActive(false);
    }

    bool checkSaveExists(string name)
    {
        string[] files = Directory.GetFiles("data/saves");
        foreach (string file in files)
        {
            if (file.EndsWith(".meta"))
            {
                string saveData = File.ReadAllText(file);
                if (saveData.Split(';')[0] == name)
                {
                    return true;
                }
            }
        }
        return false;
    }

    string[] getSaveFiles()
    {
        if (!Directory.Exists("data"))
        {
            Directory.CreateDirectory("data");
        }
        if (!Directory.Exists("data/saves"))
        {
            Directory.CreateDirectory("data/saves");
        }
        // sort the save files by date and time (meta data after the ; in the file)
        string[] files = Directory.GetFiles("data/saves");
        List<string> filesList = new List<string>();
        foreach (string file in files)
        {
            if (file.EndsWith(".meta"))
            {
                filesList.Add(file);
            }
        }
        files = filesList.ToArray();
        System.Array.Sort(files, (x, y) => File.GetLastWriteTime(y).CompareTo(File.GetLastWriteTime(x)));
        return files;
    }

    /* ------------------ Main menu buttons ------------------ */

    public void OnPressNewGame()
    {
        turnAllGroupsOff();
        newGameGroup.SetActive(true);
        // clear the fields
        newGameGroup.transform.Find("ErrorText").GetComponent<TMPro.TextMeshProUGUI>().text = "";
        newGameGroup.transform.Find("NameInput").GetComponent<TMPro.TMP_InputField>().text = "";
        newGameGroup.transform.Find("SeedInput").GetComponent<TMPro.TMP_InputField>().text = "";
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
            List<char> invalidChars = worldManager.createNewWorld(name, seed);
            if (invalidChars.Count == 0)
            {
                newGameGroup.transform.Find("ErrorText").GetComponent<TMPro.TextMeshProUGUI>().text = "";
                newGameGroup.transform.Find("NameInput").GetComponent<TMPro.TMP_InputField>().text = "";
                newGameGroup.transform.Find("SeedInput").GetComponent<TMPro.TMP_InputField>().text = "";
                turnAllGroupsOff();
            }
            else
            {
                Debug.LogError("Could not create new world");
                newGameGroup.transform.Find("ErrorText").GetComponent<TMPro.TextMeshProUGUI>().text = "Name cannot contain: " + string.Join("", invalidChars);
            }
        }
    }

    public void OnPressLoadGame()
    {
        turnAllGroupsOff();
        loadGameGroup.SetActive(true);

        string[] files = getSaveFiles();

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
                newLoadGameSelectionRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, loadGameSelectionRow.GetComponent<RectTransform>().anchoredPosition.y + (-i++ * 70));
                string saveName = saveData.Split(';')[0];
                string saveDateTime = saveData.Split(';')[1];
                newLoadGameSelectionRow.transform.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text = saveName;
                newLoadGameSelectionRow.transform.Find("DateAndTimeSaved").GetComponent<TMPro.TextMeshProUGUI>().text = saveDateTime;
            }
        }
    }

    public void OnSelectRow()
    {
        selectedLoadRow = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
    }

    public void onLoadGameConfirm()
    {
        // get the child named SaveName under selectedLoadRow
        if (selectedLoadRow == null)
        {
            return;
        }
        string saveName = selectedLoadRow.transform.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text;
#if UNITY_EDITOR
            Debug.Log("Loading save: " + saveName);
#endif
        turnAllGroupsOff();
        worldManager.Load(saveName);
        worldManager.backToGame();
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
        turnAllGroupsOff();
        loadGameGroup.SetActive(true);
    }

    public void OnPressSettings()
    {
        turnAllGroupsOff();
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

    /* ------------------ Escape menu buttons ------------------ */

    public void onBackButtonEscapeMenu()
    {
        turnAllGroupsOff();
        escapeMenuGroup.SetActive(true);
    }

    public void OnPressSaveGame()
    {
        turnAllGroupsOff();
        saveGameGroup.SetActive(true);
        backButton.SetActive(true);
        saveGameGroup.transform.Find("ErrorText").GetComponent<TMPro.TextMeshProUGUI>().text = "";
        saveGameGroup.transform.Find("NameInput").GetComponent<TMPro.TMP_InputField>().text = "";

        string[] files = getSaveFiles();

        // delete all of the children of the SaveGameScrollContent except for the saveGameSelectionRow
        foreach (Transform child in saveGameScrollContent.transform)
        {
            if (child.gameObject != saveGameSelectionRow)
                GameObject.Destroy(child.gameObject);
        }
        saveGameSelectionRow.SetActive(false);

        int i = 0;
        foreach (string file in files)
        {
            if (file.EndsWith(".meta"))
            {
                string saveData = File.ReadAllText(file);
                // make a copy of the saveGameSelectionRow and copy in the correct data
                GameObject newSaveGameSelectionRow = Instantiate(saveGameSelectionRow, saveGameScrollContent.transform);
                newSaveGameSelectionRow.SetActive(true);
                newSaveGameSelectionRow.transform.SetParent(saveGameScrollContent.transform);
                // get saveGameSelectionRows y pos
                newSaveGameSelectionRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, saveGameSelectionRow.GetComponent<RectTransform>().anchoredPosition.y + (-i++ * 70));
                string saveName = saveData.Split(';')[0];
                string saveDateTime = saveData.Split(';')[1];
                newSaveGameSelectionRow.transform.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text = saveName;
                newSaveGameSelectionRow.transform.Find("DateAndTimeSaved").GetComponent<TMPro.TextMeshProUGUI>().text = saveDateTime;
            }
        }
    }

    public void useSaveNameInputField()
    {
        string text = saveGameGroup.transform.Find("NameInput").GetComponent<TMPro.TMP_InputField>().text;
        if (text != null && text != "" && text != " " && !string.IsNullOrWhiteSpace(text))
        {
            selectedLoadRow = saveGameGroup.transform.Find("NameInput").gameObject;
        }
        else
        {
            selectedLoadRow = null;
        }
    }

    public void onPressSaveGameConfirm()
    {
        string saveName;
        if (selectedLoadRow == null)
        {
            return;
        }
        if (selectedLoadRow.name == "NameInput")
        {
            // use the text from the input field
            saveName = saveGameGroup.transform.Find("NameInput").GetComponent<TMPro.TMP_InputField>().text;
        }
        else
        {
            // use the selected row
            saveName = selectedLoadRow.transform.Find("SaveName").GetComponent<TMPro.TextMeshProUGUI>().text;
        }
        if (checkSaveExists(saveName))
        {
            tryingToSaveTo = saveName;
            turnAllGroupsOff();
            overrightSaveConfirmationGroup.SetActive(true);
        }
        else
        {
            tryingToSaveTo = saveName;
            onOverrightSaveConfirm();
        }
    }

    public void onOverrightSaveConfirm()
    {
        List<char> invalidChars = worldManager.Save(tryingToSaveTo);
        if (invalidChars.Count == 0)
        {
            saveGameGroup.transform.Find("ErrorText").GetComponent<TMPro.TextMeshProUGUI>().text = "";
            saveGameGroup.transform.Find("NameInput").GetComponent<TMPro.TMP_InputField>().text = "";
            turnAllGroupsOff();
            worldManager.backToGame();
        }
        else
        {
            Debug.LogError("Could not save game");
            turnAllGroupsOff();
            saveGameGroup.SetActive(true);
            saveGameGroup.transform.Find("ErrorText").GetComponent<TMPro.TextMeshProUGUI>().text = "Name cannot contain " + string.Join("", invalidChars);
        }
    }

    public void onOverrightSaveCancel()
    {
        turnAllGroupsOff();
        saveGameGroup.SetActive(true);
    }

    public void OnSaveGameCancel()
    {
        turnAllGroupsOff();
        saveGameGroup.SetActive(true);
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
        backButton.SetActive(true);
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
        backButton.SetActive(true);
    }

    public void OnPressMainMenu()
    {
        turnAllGroupsOff();
        worldManager.backToMainMenu();
        mainMenuCanvas.SetActive(true);
        mainMenuGroup.SetActive(true);
    }
}