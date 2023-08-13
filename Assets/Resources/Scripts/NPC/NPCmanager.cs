using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NPCmanager : MonoBehaviour
{

    [SerializeField]
    GameObject missionUI;
    GameObject MissionsUIFolder;

    [SerializeField]
    GameObject playerMissionUI;
    GameObject PlayerMissionsUIFolder;


    PlayerMovement playerMovement;
    MissionManager playerMissionManager;
    GameObject player;

    List<GameObject> shownMissions = new List<GameObject>();
    List<GameObject> playerMissions = new List<GameObject>();

    public FactionManager factionManager;

    string factionName;

    void Start()
    {
        player = GameObject.Find("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        playerMissionManager = player.GetComponent<MissionManager>();
        factionManager = transform.parent.GetComponent<FactionManager>();
        MissionsUIFolder = missionUI.transform.Find("Missions").gameObject;
        PlayerMissionsUIFolder = playerMissionUI.transform.Find("Missions").gameObject;
        factionName = transform.parent.name;
    }

    public void RefreshMissions()
    {
        hideAllMissions();
        showAllMissions();
    }


    public void InteractWithPlayer(GameObject player)
    {
        if (missionUI.activeSelf) // close inv
        {
            missionUI.SetActive(false);
            playerMissionUI.SetActive(false);
            playerMovement.UnlockPlayerMovement();
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            player.GetComponent<UIManager>().UIOpen = false;
            hideAllMissions();
        }
        else // open inv
        {
            if (!player.GetComponent<UIManager>().UIOpen)
            {
                missionUI.SetActive(true);
                playerMissionUI.SetActive(true);
                playerMovement.LockPlayerInputs(this.gameObject);
                player.GetComponent<Rigidbody>().velocity = Vector3.zero;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                player.GetComponent<UIManager>().UIOpen = true;
                showAllMissions();
            }
        }
    }

    void MissionAcceptButtonOnClick(int missionNum)
    {
        Debug.Log("Giving mission to player");
        // move this mission to the players missions
        if (playerMissionManager.AddMission(factionManager.GetCurrentMissions()[missionNum]))
        {
            // delete this mission from the factions missions
            factionManager.RemoveMission(factionManager.GetCurrentMissions()[missionNum]);
            // redraw all the missions
            RefreshMissions();
        }
    }

    void MissionHandInOnClick(int missionNum)
    {
        // make sure the player can hand in the mission
        if (playerMissionManager.HandInMission(playerMissionManager.GetMissions()[missionNum], factionManager))
        {
            Debug.Log("Handing in mission");
            // redraw all the missions
            RefreshMissions();
        }
    }

    GameObject showOneMission(Mission mission, float yPos, int missionNum, GameObject missionPrefab, Transform parent, string buttonName)
    {
        GameObject newMission = Instantiate(missionPrefab, missionUI.transform);
        // set the missions parent
        newMission.transform.SetParent(parent);
        // set the yPos of the missionUI
        newMission.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, yPos - (200 * missionNum));
        // set the mission name
        newMission.transform.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = mission.GetName();
        // set the mission description
        if (mission.getIsMainMission())
            newMission.transform.Find("Description").GetComponent<TMPro.TextMeshProUGUI>().text = mission.GetDescription();
        else
            newMission.transform.Find("Description").GetComponent<TMPro.TextMeshProUGUI>().text = factionName + " needs some " + mission.GetDescription();
        newMission.transform.Find("CreditsReward").GetComponent<TMPro.TextMeshProUGUI>().text = "Credits: " + mission.GetCreditsReward();
        newMission.transform.Find("ReputationReward").GetComponent<TMPro.TextMeshProUGUI>().text = "Reputation: " + mission.GetReputationReward();

        // set button text to the button name
        newMission.transform.Find("Button").GetComponentInChildren<TMPro.TextMeshProUGUI>().text = buttonName;

        if (buttonName == "Accept") // then this is a faction mission
        {
            // set the button on click event to MissionAcceptButtonOnClick(missionNum)
            newMission.transform.Find("Button").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => MissionAcceptButtonOnClick(missionNum));
        }
        else // this is a player mission
        {
            newMission.transform.Find("Button").GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => MissionHandInOnClick(missionNum));
        }

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
            image.GetComponent<UnityEngine.UI.Image>().sprite = itemPair.item.getSprite();
            image.Find("OreAmount").GetComponent<TMPro.TextMeshProUGUI>().text = "" + itemPair.getAmount();
        }

        return newMission;
    }

    void showAllMissions()
    {
        // get all the missions from the faction manager
        List<Mission> missions = factionManager.GetCurrentMissions();
        // get the Quest Prefab 
        GameObject missionPrefab = Resources.Load<GameObject>("Prefabs/UI/MissionUI");
        // for each mission in the list make new mission prefab and add it to the list of shown missions

        float containerSize = 200 * missions.Count;

        float yPos = containerSize / 6;
        // set MissionsUIFolder.transform y size to containerSize
        MissionsUIFolder.GetComponent<RectTransform>().offsetMin = new Vector2(MissionsUIFolder.GetComponent<RectTransform>().offsetMin.x, -1*containerSize/3);
        MissionsUIFolder.GetComponent<RectTransform>().offsetMax = new Vector2(MissionsUIFolder.GetComponent<RectTransform>().offsetMax.x, 0);

        int i = 0;
        foreach (Mission mission in missions)
        {
            shownMissions.Add(showOneMission(mission, yPos, i++, missionPrefab, MissionsUIFolder.transform, "Accept"));
        }


        i = 0;
        List<Mission> playerMissions = player.GetComponent<MissionManager>().GetMissions();   
        containerSize = 200 * playerMissions.Count;

        yPos = containerSize / 6;

        // set PlayerMissionsUIFolder.transform y size to containerSize
        PlayerMissionsUIFolder.GetComponent<RectTransform>().offsetMin = new Vector2(PlayerMissionsUIFolder.GetComponent<RectTransform>().offsetMin.x, -1*containerSize/3);
        PlayerMissionsUIFolder.GetComponent<RectTransform>().offsetMax = new Vector2(PlayerMissionsUIFolder.GetComponent<RectTransform>().offsetMax.x, 0);

        foreach (Mission mission in playerMissions)
        {
            shownMissions.Add(showOneMission(mission, yPos, i++, missionPrefab, PlayerMissionsUIFolder.transform, "Complete"));
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
    
}
