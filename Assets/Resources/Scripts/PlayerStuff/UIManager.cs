using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    bool UIOpen = false;
    GameObject missionPrefab;
    GameObject PlayerMissionsUIFolder;

    [SerializeField]
    GameObject playerMissionUI;
    [SerializeField]
    GameObject leftInventory;
    [SerializeField]
    GameObject rightInventory;

    PlayerMovement playerMovement;

    List<GameObject> shownMissions = new List<GameObject>();

    WorldManager worldManager;
    ItemManager itemManager;


    void Start()
    {
        worldManager = GameObject.Find("WorldManager").GetComponent<WorldManager>();
        itemManager = GameObject.Find("WorldManager").GetComponent<ItemManager>();
        missionPrefab = Resources.Load<GameObject>("Prefabs/UI/MissionUI");
        PlayerMissionsUIFolder = playerMissionUI.transform.Find("Missions").gameObject;
        playerMovement = GetComponent<PlayerMovement>();
    }

    public bool getUIOpen() { return UIOpen; }

    bool updateState = false;

    public void OnMissionsMenu(InputAction.CallbackContext context)
    {
        if (context.started)
            updateState = true;
    }

    void Update()
    {
        if (updateState)
        {
            OpenOrCloseMissionsMenu();
            updateState = false;
        }
    }

    bool doneByThis = false;

    public void closeAnyUI()
    {
        playerMovement.UnlockPlayerMovement();
        playerMovement.UnlockPlayerInputs();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        UIOpen = false;
    }

    public void openAnyUI(GameObject caller)
    {
        if (worldManager.inGame)
        {
            playerMovement.LockPlayerInputs(caller);
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            UIOpen = true;
        }
    }

    public void OpenOrCloseMissionsMenu()
    {

        if (playerMissionUI.activeSelf && doneByThis) // close inv
        {
            playerMissionUI.SetActive(false);
            closeAnyUI();
            hideAllMissions();
            doneByThis = false;
        }
        else // open inv
        {
            if (!UIOpen)
            {
                playerMissionUI.SetActive(true);
                openAnyUI(this.gameObject);
                showPlayerMissions();
                doneByThis = true;
            }
        }
    }

    public void OpenOrCloseInventory(GameObject caller)
    {
        if (UIOpen)
        {
            // close the inv
            leftInventory.SetActive(false);
            rightInventory.SetActive(false);
            closeAnyUI();
        }
        else
        {
            leftInventory.SetActive(true);
            rightInventory.SetActive(true);
            openAnyUI(caller);
        }
    }

    void hideAllMissions()
    {
        foreach (GameObject mission in shownMissions)
        {
            Destroy(mission);
        }
        shownMissions.Clear();
    }

    void showPlayerMissions()
    {
        int i = 0;
        List<Mission> playerMissions = GetComponent<MissionManager>().GetMissions();
        float containerSize = 200 * playerMissions.Count;

        float yPos = containerSize / 6;

        // set PlayerMissionsUIFolder.transform y size to containerSize
        PlayerMissionsUIFolder.GetComponent<RectTransform>().offsetMin = new Vector2(PlayerMissionsUIFolder.GetComponent<RectTransform>().offsetMin.x, -1 * containerSize / 3);
        PlayerMissionsUIFolder.GetComponent<RectTransform>().offsetMax = new Vector2(PlayerMissionsUIFolder.GetComponent<RectTransform>().offsetMax.x, 0);

        foreach (Mission mission in playerMissions)
        {
            shownMissions.Add(showOneMission(mission, yPos, i++, missionPrefab, PlayerMissionsUIFolder.transform, "Complete"));
        }
    }

    public GameObject showOneMission(Mission mission, float yPos, int missionNum, GameObject missionPrefab, Transform parent, string buttonName)
    {
        GameObject newMission = Instantiate(missionPrefab, playerMissionUI.transform);
        // set the missions parent
        newMission.transform.SetParent(parent);
        // set the yPos of the missionUI
        newMission.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos - (200 * missionNum));
        // set the mission name
        newMission.transform.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = mission.GetName();
        // set the mission description
        newMission.transform.Find("Description").GetComponent<TMPro.TextMeshProUGUI>().text = mission.GetDescription();
        newMission.transform.Find("CreditsReward").GetComponent<TMPro.TextMeshProUGUI>().text = "Credits: " + mission.GetCreditsReward();
        newMission.transform.Find("ReputationReward").GetComponent<TMPro.TextMeshProUGUI>().text = "Reputation: " + mission.GetReputationReward();

        GameObject button = newMission.transform.Find("Button").gameObject;
        button.SetActive(false);

        int x = 1;
        for (x = 1; x < 4; x++)
        {
            Transform image = newMission.transform.Find("Image" + x);
            image.GetComponent<UnityEngine.UI.Image>().sprite = null;
            image.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
            image.Find("OreAmount").GetComponent<TMPro.TextMeshProUGUI>().text = "";
        }
        x = 1;

        foreach (ItemPair itemPair in mission.GetGoal())
        {
            Transform image = newMission.transform.Find("Image" + x++);
            image.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
            image.GetComponent<UnityEngine.UI.Image>().sprite = itemManager.getSprite(itemPair.item.getName());
            image.Find("OreAmount").GetComponent<TMPro.TextMeshProUGUI>().text = "" + itemPair.getAmount();
        }

        return newMission;
    }

}
