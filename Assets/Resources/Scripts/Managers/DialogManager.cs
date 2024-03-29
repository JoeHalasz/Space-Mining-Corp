using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// TODO control f { and } in this and fill in with the correct things

public class DialogManager : MonoBehaviour
{

    public struct Dialog
    {
        public string text;
        public int voiceNumber;
        public float displayTime;
        public bool displayMissions;

        public Dialog(string text, int voiceNumber, float displayTime, bool displayMissions = false)
        {
            this.text = text;
            this.voiceNumber = voiceNumber;
            this.displayTime = displayTime;
            this.displayMissions = displayMissions;
        }
    }

    GameObject worldManagerObject;
    WorldManager worldManager;
    PlayerStats playerStats;
    GameObject player;

    int currentState = 0;
    bool playerChoice = false;
    bool playerShouldChooseNext = false;

    // text mesh pro game object for the dialog box
    GameObject dialogBox;
    List<GameObject> dialogOptions = new List<GameObject>();

    // current unity sound playing
    AudioSource currentVoiceLine;

    void Start()
    {
        GameObject worldManagerObject = GameObject.Find("WorldManager");
        worldManager = worldManagerObject.GetComponent<WorldManager>();
        player = GameObject.Find("Player");
        playerStats = player.GetComponent<PlayerStats>();
        // get the dialogBox gameobject
        dialogBox = GameObject.Find("DialogBox");
        dialogOptions.Add(GameObject.Find("Option1").transform.GetChild(0).gameObject);
        dialogOptions.Add(GameObject.Find("Option2").transform.GetChild(0).gameObject);
        dialogOptions.Add(GameObject.Find("Option3").transform.GetChild(0).gameObject);
        dialogOptions.Add(GameObject.Find("Option4").transform.GetChild(0).gameObject);
        dialogBox.SetActive(false);
        for (int i = 0; i < dialogOptions.Count; i++)
        {
            dialogOptions[i].transform.parent.gameObject.SetActive(false);
        }
    }

    void endDialog()
    {
        currentState = 0;
    }

    // coroutine to set the dialog box inactive after a certain amount of time that takes in the amount of time to wait and what the text used to be 
    IEnumerator setDialogBoxInactive(float time, string text)
    {
        yield return new WaitForSeconds(time);
        if (dialogBox.GetComponent<TextMeshProUGUI>().text == text)
        {
            dialogBox.SetActive(false);
        }
    }


    public bool interactedWithFaction(FactionManager faction, GameObject voicedBy)
    {
        List<Dialog> textToDisplay = findWhatToDisplay(faction);
        if (textToDisplay.Count > 1)
        {
            // then its the player choosing a response
            // set the dialog box to inactive
            dialogBox.SetActive(false);
            for (int i = 0; i < textToDisplay.Count; i++)
            {
                dialogOptions[i].GetComponent<TextMeshProUGUI>().text = textToDisplay[i].text;
                // set the parent to active
                dialogOptions[i].transform.parent.gameObject.SetActive(true);
            }
        }
        else if (textToDisplay.Count == 1)
        {
            // its an NPC talking
            // set the dialog options to inactive
            for (int i = 0; i < dialogOptions.Count; i++)
            {
                dialogOptions[i].SetActive(false);
            }
            dialogBox.GetComponent<TextMeshProUGUI>().text = textToDisplay[0].text;
            dialogBox.SetActive(true);
            // set the dialogBox as inactive in displayTime seconds if the text is still the same then
            StartCoroutine(setDialogBoxInactive(textToDisplay[0].displayTime, textToDisplay[0].text));
            playVoiceLine(faction, voicedBy, textToDisplay[0]);
            if (textToDisplay[0].displayMissions)
            {
                return true;
            }
        }
        else
        {
            Debug.LogError("Could not find dialog for faction " + faction.getFactionName() + " in state " + currentState);
        }
        return false;
    }

    void playVoiceLine(FactionManager faction, GameObject voicedBy, Dialog dialog)
    {
        // stop playing the currentVoiceLine if its still playing
        if (currentVoiceLine != null)
        {
            currentVoiceLine.Stop();
        }
        // get the voice line from the faction and the current state
        currentVoiceLine = getVoiceLine(faction, voicedBy, dialog.voiceNumber);
        if (currentVoiceLine != null)
        {
            currentVoiceLine.Play();
        }
        else
        {
            Debug.Log("Could not find voice line for faction " + faction.getFactionName() + " in state " + currentState);
        }
    }

    AudioSource getVoiceLine(FactionManager faction, GameObject voicedBy, int voiceNumber)
    {
        // find the audio file for that voice line in the Voicelines folder in Resources
        string path = "Voicelines/" + faction.getFactionName() + "/" + voiceNumber + "/" + currentState;
        // if that path exists
        if (Resources.Load<AudioSource>(path) == null)
        {
            return null;
        }
        AudioSource voiceLine = Resources.Load<AudioSource>(path);
        // set its position to voicedBy
        voiceLine.transform.position = voicedBy.transform.position;
        // attach the voiceLine to the voicedBy gameobject so that the sound follows that NPC
        voiceLine.transform.parent = voicedBy.transform;
        return voiceLine;
    }

    List<Dialog> findWhatToDisplay(FactionManager faction)
    {
        if (playerShouldChooseNext)
        {
            playerShouldChooseNext = false;
            return getPlayerChoice(faction);
        }
        else
        {
            List<Dialog> textToDisplay = new List<Dialog>();
            textToDisplay.Add(getDialog(faction));
            return textToDisplay;
        }
    }

    List<Dialog> getPlayerChoice(FactionManager faction)
    {
        List<Dialog> choiceOptions = new List<Dialog>();
        switch (faction.getFactionName())
        {
            case ("Leader"):
                switch (currentState)
                {
                    case (2):
                        choiceOptions.Add(new Dialog("To fix the warp gate so that this outpost can be recconected with earth", -1, 10)); // -1 means its the player 
                        choiceOptions.Add(new Dialog("To figure out what happened to the warp gate and insure it doesn't happen again", -1, 10));
                        choiceOptions.Add(new Dialog("I love exploration", -1, 10));
                        break;
                    case (3):
                        choiceOptions.Add(new Dialog("We should fix the warp gate and destroy them. My ship is equipped with shields and weapons", -1, 10)); // -1 means its the player 
                        choiceOptions.Add(new Dialog("We should fix the communications array and tell earth what’s going on", -1, 10));
                        break;
                    case (4):
                        choiceOptions.Add(new Dialog("Yes", -1, 10));
                        choiceOptions.Add(new Dialog("Whats in it for me?", -1, 10));
                        break;
                    case (1012):
                        choiceOptions.Add(new Dialog("Why did the {evil faction name} attack?", 2, 10));
                        break;
                }
                if (faction.GetPlayerReputation() >= .01f && currentState == 0)
                {
                    choiceOptions.Add(new Dialog("What happened to the warp gate?", -1, 10)); // this will be state 1001
                    choiceOptions.Add(new Dialog("Who are the {evil faction name}?", -1, 10)); // this will be state 1011 // TODO fix this
                    choiceOptions.Add(new Dialog("Why do you need resources?", -1, 10)); // this will be state 1021
                }

                break;

        }

        return choiceOptions;
    }

    Dialog introDialog()
    {
        switch (currentState - 1)
        {
            case (0):
                return new Dialog("Bot M12B1, your current mission is to fix the warp gate over planet Navin 3", 0, 10); // earth voice
            case (1):
                return new Dialog("You are attempt 13 at this. Using new Shield technology, your ship should be more durable then those before", 0, 10);
            case (2):
                return new Dialog("Good luck to you and your crew RR12", 0, 10);
            case (3):
                return new Dialog("We're in the middle of an asteroid field!", 101, 10); // random new model robot voices are in the 100s
            case (4):
                return new Dialog("I don't think the warp gate was aligned correctly", 102, 10);
            case (5):
                return new Dialog("Ship slowing down", 1, 10); // ship AI voice
            case (6):
                return new Dialog("Ship shield taking damage", 1, 10);
            case (7):
                return new Dialog("If the shield doesn't hold we'll end up like one of those other ships!", 103, 10);
            case (8):
                return new Dialog("Shield integrety at 50%", 1, 10); // TODO spelling check
            case (9):
                return new Dialog("We're almost stopped but theres a huge asteroid ahead of us!", 101, 10);
            case (10):
                return new Dialog("Shield down", 1, 10);
            case (11):
                return new Dialog("Look the bot rebuilder is working!", 51, 10); // old bot voices start at 50
            case (12):
                return new Dialog("Wow that bot looks really different! must be a newer model", 52, 10);
            case (13):
                return new Dialog("We haven't seen a ship in centuries, it must be here to help us!", 53, 10);
            case (14):
                return new Dialog("Welcome to Navin! we were able to get just enough power to your ship to start up the rebuilder.", 51, 10);
            case (15):
                return new Dialog("I dont think it will make more than 1 bot at a time, but at least its working.", 52, 10);
            case (16):
                endDialog();
                worldManager.nextWorldState();
                return new Dialog("Go talk to {leader name}. She will know how you can help best", 51, 10); // TODO put the leader name
        }
        return new Dialog();
    }
    Dialog getDialog(FactionManager faction)
    {
        currentState++;
        if (worldManager.worldState == WorldState.Intro)
        {
            return introDialog();
        }
        switch (faction.getFactionName())
        {
            case ("Leader"):
                if (currentState > 1000)
                {
                    switch (currentState - 1)
                    {
                        case (1001):
                            playerShouldChooseNext = true;
                            endDialog();
                            return new Dialog("After the {evil faction name} came through the warp gate, they destroyed all but one of our ships.\nWe used that ship to slam into the warp gate and disable it, so that they couldn’t send another attack fleet", 2, 10);
                        case (1011):
                            playerShouldChooseNext = true;
                            return new Dialog("We don’t know. All we know is that they came through the warp gate and constantly transmitted a message that said “food” over and over.", 2, 10);
                        case (1021):
                            endDialog();
                            if (worldManager.worldState == WorldState.BeforeBeam1)
                            {
                                return new Dialog("We need to fix the stations power supply. Without that we can do very much. We have been surviving out here off of the minerals in the asteroid, but we have to be extremely careful not to destroy anything because of how limited our supplies are.\n There aren't enough supplies to build ships to go mine other asteroids, so you and your ship are our only hope.", 2, 10);
                            }
                            else if (worldManager.worldState == WorldState.BeforeBeam2)
                            {
                                return new Dialog("We need to fix the warp gate so that we can defeat the {enemy faction} and get back home safely", 2, 10);
                            }
                            else if (worldManager.worldState == WorldState.BeforeStationDestroyed)
                            {
                                return new Dialog("We need resources to be able to destroy the {enemy faction} base, and set the warp gate to send us back to earth", 2, 10);
                            }
                            else
                            {
                                return new Dialog("The station can always use more resources. Just ask the faction leaders", 2, 10);
                            }
                        case (1012):
                            endDialog();
                            return new Dialog("Based on their only message, they probably needed resources, and somehow figured out how to jump to any warp gate. Scary.", 2, 10);

                    }
                }
                if (faction.GetPlayerReputation() < .01f) // first time interaction
                {
                    switch (currentState - 1)
                    {
                        case (0): // 2 is the leader voice
                            return new Dialog("Welcome to Navin. We have seen other attempts to get here, but only because their ships slam into asteroids and explode.\nWhat ever warp gate you used to get here is not aligned correctly. Your ships shield tech is the only reason your ship was in enough of a working condition for us to fix it.", 2, 10);
                        case (1):
                            playerShouldChooseNext = true;
                            return new Dialog("Anyway, why are you here?", 2, 10);
                        // state will now be 2 if first choice was made, 12 if second, and 22 if third.
                        case (2):
                            playerShouldChooseNext = true;
                            return new Dialog("Centuries ago we destroyed that warp gate because of the war. The {evil faction name} jumped through the gate from {some other system} and tried to destroy our station.\nWe were able to fight them off but they destroyed our comms array and all but one of our ships. We used that ship to destroy the warp gate to ensure no more of them could come through, but that trapped us here, and with no ships", 2, 10);
                        case (12):
                            currentState -= 10;
                            playerShouldChooseNext = true;
                            return new Dialog("Great to hear. You can help us by using your ship to go out and mine the resources we need to get the stations power source back up and running.\nWe haven’t had a working ship since the {evil faction name} came through the warp gate and destroyed everything", 2, 10);
                        case (22):
                            currentState -= 20;
                            playerShouldChooseNext = true;
                            return new Dialog("Great to hear. You can help us by using your ship to go out and mine the resources we need to get the stations power source back up and running.\nWe haven’t had a working ship since the {evil faction name} came through the warp gate and destroyed everything", 2, 10);
                        case (13):
                            currentState -= 10;
                            return new Dialog("the comms array can’t be fixed, we don’t have the blueprints for it and we would have to fully rebuild it. There’s nothing left at all.\nLet’s focus on what we have to do first, we need {resources for the first mission}. can you handle flying out and mining what we need?", 2, 10); // TODO update this
                        case (3):
                            return new Dialog("Let’s focus on what we have to do first, we need {resources for the first mission}. can you handle flying out and mining what we need?", 2, 10); // TODO update this
                        case (4):
                            endDialog();
                            return new Dialog("Great. Let me know if you need anything else.", 2, 10);
                        case (14):
                            endDialog();
                            return new Dialog("Thats funny. Let me know if you need anything else.", 2, 10);
                        default:
                            break;
                    }
                }
                else if (faction.GetPlayerReputation() < 1)
                {
                    switch (currentState - 1)
                    {
                        case (0):
                            faction.SetPlayerReputation(1);
                            return new Dialog("Well done! This is extremely exciting, we haven’t had hope in such a long time. I’ll start the preparations to get the energy core going again. \nIn the mean time talk to the other bots around, I’m sure all the factions could use some resources.", 2, 10);
                    }
                }
                else if (faction.GetPlayerReputation() == 1)
                {
                    return new Dialog("Come back to me in a little while, im trying to get the energy core ready. Go to the other faction leaders.", 2, 10);
                }
                else if (faction.GetPlayerReputation() < 2)
                {
                    switch (currentState - 1)
                    {
                        case (0):
                            return new Dialog("Ok, I think i've gotten the core ready to turn back on. Thanks to you this station may actually light up again. I left the best part for you. Press that button to start the beams", 2, 10);
                        case (1):
                            faction.SetPlayerReputation(2);
                            return new Dialog("Beautiful isnt it? The station looks amazing at night. Funny cause its always night here.\nAnyway... great job so far but theres more to get done. Cobalt and Uranium aren't going to be enough to get the warp gate going.\nWe are going to need more advanced materials that are further from the station. Ask the leader of the energy faction for more info.", 2, 10);
                    }
                }
                else if (faction.GetPlayerReputation() == 2)
                {
                    return new Dialog("Come back to me once you've help the station along a little more. Im still prepairing the core to accept the new fuel.", 2, 10);
                }
                else if (faction.GetPlayerReputation() < 3)
                {
                    return new Dialog("I pointed the warp gate at the {enemy faction name}’s main base. They will have no idea our warp gate is up until you step through it.\nDestroy all the ships you see, and their comms array.\nMake sure that you have fixed your shield before going through the warp gate, and make sure you have plenty of weapons. For the first time you go through, I recommend leaving your mining laser behind. You will want lots of weapons just incase", 2, 10);
                }
                else if (faction.GetPlayerReputation() == 3)
                {
                    return new Dialog("Im so happy to see you back in once piece. If your ship needs repairs just ask the resource faction leader. He can help.\nWe need to figure out the tech they used to jump here. Its our only way to get back to earths warp gate with it miss aligned the way it is.\nWhen you are ready, jump through the gate again and explore their base. Find the tech we need.", 2, 10);
                }
                else if (faction.GetPlayerReputation() < 4)
                {
                    return new Dialog("You have it! Great. Let me see it.\n... ... ...\n ... ... ...\nThis is worrying. I mean... I have the data we need to get us home, but it looks like the station is sending out a distress beacon.\nWe are goig to have to destroy their station somehow.\nI know the energy core can create enough power to destroy it, but not with the fuel we have here.\n I need you to travel through the gate again and find some more powerful fuel. Be very carful out there... the further away from that station you go, the bigger the ships will get.", 2, 10);
                }
                else if (faction.GetPlayerReputation() == 5)
                {
                    switch (currentState - 1)
                    {
                        case (0):
                            return new Dialog("Ok, looks like we have enough high tier fuel stocked up to destroy their station. Hit the button when you are ready", 2, 10);
                        case (1):
                            return new Dialog("Wow that was bright, that definitely did the job... just to make sure you should probably go through and check.", 2, 10);
                    }
                }
                else if (faction.GetPlayerReputation() < 6)
                {
                    switch (currentState - 1)
                    {
                        case (0):
                            return new Dialog("Well? Did it work? Are we safe?", 2, 10);
                        case (1):
                            faction.SetPlayerReputation(6);
                            return new Dialog("Im so glad. The warp gate should now be ready to bring you back to earth so you can complete your mission.\nThank you so much for everything you did for this station.\n We finally have enough resources to start building ships and be useful to earth again.\n Please come back any time to help us get more resources.", 2, 10);
                    }
                }
                else if (faction.GetPlayerReputation() >= 6)
                {
                    return new Dialog("Thanks again for everything you did. Im excited to finally get to fly a ship again.\nThis station will always be open to you as your home away from home.", 2, 10);
                }
                break;
            case ("Resource Faction"):
                // 3 is resource faction leader voice
                if (faction.GetPlayerReputation() < .01f)
                {
                    switch (currentState - 1)
                    {
                        case (0):
                            return new Dialog("mmm... an outsider... I dont trust outsiders.", 3, 10);
                        case (1):
                            return new Dialog("My faction ensures that resources go to the correct places.\nThe day we run out of resources from that asteroid is the day we all start to perish... But you can change that.", 3, 10);
                        case (2):
                            faction.AddPlayerReputation(.02f);
                            return new Dialog("Im going to need resources of all kinds. Hopefully you can save us.", 3, 10);
                    }
                }
                else if (faction.GetPlayerReputation() < 1)
                {
                    return new Dialog("I need resources. Can you find any of these?", 3, 10, true);
                }
                else if (faction.GetPlayerReputation() == 1)
                {
                    faction.AddPlayerReputation(.02f);
                    return new Dialog("Ok, I guess you aren’t too bad outsider.\nYou've given me enough resources to fix almost everything on the station, we should be able to start focusing on making upgrades to the station, and your ship of course", 3, 10);
                }
                else if (faction.GetPlayerReputation() > 1)
                {
                    // TODO let the player choose if they want to get shup upgrades or missions
                    if (faction.GetPlayerReputation() > 2 && Random.Range(0, 100) <= 10)
                    {
                        return new Dialog("Upgrades upgrades upgrades...", 3, 10, true);
                    }
                    return new Dialog("What kind of upgrades were you thinking of?", 3, 10, true);
                }
                break;
            case ("Energy Faction"):
                // TODO add these
                // 4 is energy faction leader
                currentState = 0;
                if (faction.GetPlayerReputation() < .01f)
                {
                    faction.AddPlayerReputation(.02f);
                    return new Dialog("Hi! its great to meet you! My faction deals with all the energy on the station! I always had hope, but its great to finally see that all that hope standing right infront of me!", 4, 10);
                }
                if (faction.GetPlayerReputation() < 2.5)
                {
                    return new Dialog("Hi! im looking for some materials to make energy and fuel, can you get me any of that?", 4, 10, true);
                }
                if (faction.GetPlayerReputation() > 2)
                {
                    return new Dialog("We've got enough of everything you can find around here... Unless you can somehow find something else I can burn", 4, 10);
                }
                break;
            case ("Weapons Faction"):
                // 5 is weapons faction leader
                currentState = 0;
                if (faction.GetPlayerReputation() < .01f)
                {
                    faction.AddPlayerReputation(.02f);
                    return new Dialog("Hel..lo, my name is.... I am a bot. I make ... something...", 5, 10);
                }
                if (faction.GetPlayerReputation() < 1)
                {
                    return new Dialog("I neeeeeeeed.... something. We pans!", 5, 10, true);
                }
                if (faction.GetPlayerReputation() > 1)
                {
                    return new Dialog("Leeeeeader says you neeeeeeeed.... (whispers)weapons. I make those... good. Pew pew", 5, 10, true);
                }
                if (faction.GetPlayerReputation() > 2)
                {
                    return new Dialog("mmmmmm.... kill... wearprons...", 5, 10, true);
                }
                break;
            case ("EnemyFaction"):
                currentState = 0;
                return new Dialog("Food.", -2, 10); // no voice
                break;
            default:
                break;
        }
        return new Dialog();
    }
}
