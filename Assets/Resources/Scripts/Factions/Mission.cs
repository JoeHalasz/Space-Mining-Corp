using System.Collections.Generic;
using UnityEngine;


public class Mission
{
    string Name;
    public string GetName() { return Name; }
    string Description;
    public string GetDescription() { return Description; }
    int Level;
    public int GetLevel() { return Level; }
    float CreditsReward;
    public float GetCreditsReward() { return (int)CreditsReward; }
    float ReputationReward;
    public float GetReputationReward() { return (int)ReputationReward; }
    List<ItemPair> ItemRewards;
    public List<ItemPair> GetItemRewards() { return ItemRewards; }

    List<ItemPair> Goal;
    public List<ItemPair> GetGoal() { return Goal; }

    public void SetUpMission(string name, string description, int level, float creditsReward, float reputationReward, List<ItemPair> ItemRewards, List<ItemPair> goal)
    {
        this.Name = name;
        this.Description = description;
        this.Level = level;
        this.CreditsReward = creditsReward;
        this.ReputationReward = reputationReward;
        this.ItemRewards = ItemRewards;
        this.Goal = goal;
    }

    public void SetUpMission(Mission other)
    {
        this.Name = other.Name;
        this.Description = other.Description;
        this.Level = other.Level;
        this.CreditsReward = other.CreditsReward;
        this.ReputationReward = other.ReputationReward;
        ItemRewards = new List<ItemPair>();
        foreach (ItemPair item in other.ItemRewards)
        {
            ItemRewards.Add(item.GetCopy());
        }
        Goal = new List<ItemPair>();
        foreach (ItemPair item in other.Goal)
        {
            Goal.Add(item.GetCopy());
        }
    }

    // this function assumes that the other mission has only one goal and one reward
    public void CombineMission(Mission other)
    {
        this.CreditsReward += other.CreditsReward;
        this.ReputationReward += other.ReputationReward;
        // if this.name contains other.name then combine the missions
        if (this.Name.Contains(other.Name))
        {
            if (other.Goal.Count > 0)
            {
                bool found = false;
                for (int i = 0; i < this.Goal.Count; i++)
                {
                    if (this.Goal[i].item.getName() == other.Goal[0].item.getName())
                    {
                        found = true;
                        this.Goal[i].amount += other.Goal[0].amount;
                        break;
                    }
                }
                if (!found)
                    this.Goal.Add(other.Goal[0]);
                
            }

            if (other.ItemRewards.Count > 0)
            {
                bool found = false;
                for (int i = 0; i < this.ItemRewards.Count; i++)
                {
                    if (this.ItemRewards[i].item.getName() == other.ItemRewards[0].item.getName())
                    {
                        found = true;
                        this.ItemRewards[i].amount += other.ItemRewards[0].amount;
                        break;
                    }
                }
                if (!found)
                    this.ItemRewards.Add(other.ItemRewards[0]);
            }
                            
        }
        else
        {
            // add the other mission to the end of this one
            this.Name += ", " + other.Name;
            // remove all the words "and" from this.Description
            this.Description = this.Description.Replace("and", "");
            this.Description += ", and " + other.Description;
            this.Level = Mathf.Max(this.Level, other.Level);
            foreach (ItemPair reward in other.ItemRewards)
            {
                this.ItemRewards.Add(reward);
            }
            foreach (ItemPair goal in other.Goal)
            {
                this.Goal.Add(goal);
            }
        }
    }

    public void PrintMission()
    {
        // print out everything about the mission
        Debug.Log("Name: " + this.Name);
        Debug.Log("Description: " + this.Description);
        Debug.Log("Level: " + this.Level);
        Debug.Log("CreditsReward: " + this.CreditsReward);
        Debug.Log("ReputationReward: " + this.ReputationReward);
        Debug.Log("Goal: ");
        foreach (ItemPair itemPair in this.Goal)
        {
            Debug.Log("Item: " + itemPair.item.getName() + " Amount: " + itemPair.amount);
        }
    }

    public void RandomlyChangeAmount()
    {
        float newCreditsReward = 0;
        float newRepReward = 0;
        // change the amount of the goal
        foreach (ItemPair itemPair in this.Goal)
        {
            // randomly change the amount of the item by 10%
            int newAmount = (int)(itemPair.amount * Random.Range(.5f, 2f));
            itemPair.amount = newAmount;
            newCreditsReward += newAmount * itemPair.item.getSellValue() * Random.Range(.9f, 1.1f);
            newRepReward += newAmount * itemPair.item.getSellValue() * Random.Range(.9f, 1.1f) * .1f;
        }
        this.CreditsReward = newCreditsReward;
        this.ReputationReward = newRepReward;
    }

    private bool CheckFinished(Inventory playerInventory, Inventory shipInventory)
    {
        bool notDone = false;
        // return true if there are enough of every item in the inventorys
        foreach (ItemPair itemPair in Goal)
        {
            float total = playerInventory.GetAmountOfItem(itemPair.item);
            if (shipInventory != null)
                total += shipInventory.GetAmountOfItem(itemPair.item);
            
            if (total < itemPair.amount)
            {
                Debug.Log("Not enough of item: " + itemPair.item.getName());
                Debug.Log("You have " + total + " and you need " + itemPair.amount);
                notDone = true;
            }
        }
        if (notDone)
            return false;
        
        return true;
    }


    // will return false if the player the items in their inventory
    public bool HandInMission(Inventory playerInventory, Inventory shipInventory)
    {
        if (CheckFinished(playerInventory, shipInventory))
        {
            // remove the goal items from the player inventory
            foreach (ItemPair itemPair in Goal)
            {
                float playerItemAmount = playerInventory.GetAmountOfItem(itemPair.item);
                // take as much as is needed from the player, and then the rest from the ship
                if (playerItemAmount >= itemPair.amount)
                {
                    playerInventory.removeItemAmount(itemPair.item, itemPair.amount);
                }
                else if (shipInventory != null)
                {
                    playerInventory.removeItemAmount(itemPair.item, playerItemAmount);
                    shipInventory.removeItemAmount(itemPair.item, itemPair.amount - playerItemAmount);
                }
                else
                {
                    Debug.Log("This should never happen in Mission.cs");
                }
            }
            return true;
        }
        return false;
    }


}
