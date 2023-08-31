using System.Collections.Generic;
using UnityEngine;


// these are the different types of materials that can be mined
public class Minerals
{
    // list of all the minerals
    public Dictionary<string, Item> MineralsList = new Dictionary<string, Item>();
    public Dictionary<string, Item> OresList = new Dictionary<string, Item>();

    Dictionary<int, List<Item>> NormalRockMineralZones = new Dictionary<int, List<Item>>();
    Dictionary<int, List<Item>> BigRockMineralZones = new Dictionary<int, List<Item>>();

    Dictionary<int, List<Item>> MineralGroups = new Dictionary<int, List<Item>>();

    // Start is called before the first frame update
    void Start()
    {
        SetUp();
    }

    Item SetupOneItem(string name, string desc, float density, float sellValue, float buyValue, bool stackable, int maxStack, Item.ItemType itemType, Color color, int level)
    {
        Item newItem = new Item();
        // get the sprint with that name
        Sprite sprite = Resources.Load<Sprite>("Textures/MineralSprites/" + name);
        newItem.SetUpItem(name, desc, density, sellValue, buyValue, stackable, maxStack, itemType, color, level, sprite);
        return newItem;
    }

    public Item GetMineralByName(string name)
    {
        if (MineralsList.ContainsKey(name))
        {
            return MineralsList[name];
        }
        else if (OresList.ContainsKey(name))
        {
            return OresList[name];
        }
        else
        {
            Debug.Log("Mineral " + name + " not found");
            return null;
        }
    }

    public void SetUp()
    {
        // light armor plating group 1 // white color
        MineralsList.Add("Titanium", SetupOneItem("Titanium", "refined tier 1 light armor materials", 4.5f, 2f, -1f, true, 99, Item.ItemType.Mineral, Color.white, 1));
        // heavy armor plating group 1 // grey metalic color
        MineralsList.Add("Iron", SetupOneItem("Iron", "refined tier 1 heavy armor materials", 7.8f, 2f, -1f, true, 99, Item.ItemType.Mineral, new Color(.63f, .61f, .58f, 1f), 1));
        // energy parts group 1 // dark grey color
        MineralsList.Add("Silicon", SetupOneItem("Silicon", "refined tier 1 computer materials", 2.3f, 2f, -1f, true, 99, Item.ItemType.Mineral, new Color(.25f, .25f, .25f, 1f), 1));
        // energy parts group 1 // dark blue color
        MineralsList.Add("Cobalt", SetupOneItem("Cobalt", "refined tier 1 energy materials", 8.9f, 2f, -1f, true, 99, Item.ItemType.Mineral, new Color(0f, .278f, .67f, 1f), 1));
        // weapons parts group 1 // blue and silver color
        MineralsList.Add("Magnesium", SetupOneItem("Magnesium", "refined tier 1 weapons materials", 1.7f, 2f, -1f, true, 99, Item.ItemType.Mineral, new Color(.75f, .76f, .76f, 1f), 1));
        // fuel group 1 // green color
        MineralsList.Add("Uranium", SetupOneItem("Uranium", "refined tier 1 fuel", 8.7f, 2f, -1f, true, 99, Item.ItemType.Fuel, Color.green, 1));


        // light armor plating group 2 // orange metalic color
        MineralsList.Add("Argonide", SetupOneItem("Argonide", "refined tier 2 light armor materials", 15.8f, 2f, -1f, true, 99, Item.ItemType.Mineral, new Color(.63f, .61f, .58f, 1f), 2));
        // heavy armor plating group 2 // brown and blocky
        MineralsList.Add("Tungsten", SetupOneItem("Tungsten", "refined tier 2 heavy armor materials", 19.3f, 2f, -1f, true, 99, Item.ItemType.Mineral, Color.black, 2));
        // computer parts group 2 // shiny silver color
        MineralsList.Add("Platinum", SetupOneItem("Platinum", "refined tier 2 computer materials", 6.4f, 2f, -1f, true, 99, Item.ItemType.Mineral, Color.gray, 2));
        // energy parts group 2 // redish pink
        MineralsList.Add("Rhodonide", SetupOneItem("Rhodonide", "refined tier 2 energy materials", 8.9f, 2f, -1f, true, 99, Item.ItemType.Mineral, Color.red, 2));
        // weapons parts group 2 // glowing green color
        MineralsList.Add("Nuclarium", SetupOneItem("Nuclarium", "refined tier 2 weapons materials", 13.5f, 2f, -1f, true, 99, Item.ItemType.Mineral, Color.green, 2));        
        // fuel group 2 // bright purple color, glowing
        MineralsList.Add("Pentolium", SetupOneItem("Pentolite", "refined tier 2 fuel", 15.1f, 2f, -1f, true, 99, Item.ItemType.Fuel, new Color(1f, .5f, 1f, 1f), 2));

        // light armor plating group 3. Should be glowing white
        MineralsList.Add("Phasium", SetupOneItem("Phasium", "refined tier 3 light armor materials", 5.4f, 2f, -1f, true, 99, Item.ItemType.Mineral, new Color(1f, 1f, 1f, 0.5f), 3));
        // heavy armor plating group 3 // should be greyish white
        MineralsList.Add("Lunarite", SetupOneItem("Lunarite", "refined tier 3 heavy armor materials", 19.3f, 2f, -1f, true, 99, Item.ItemType.Mineral, Color.white, 3));
        // computer parts group 3 // shiny red color
        MineralsList.Add("Rubelline", SetupOneItem("Rubelline", "refined tier 3 computer materials", 5.1f, 2f, -1f, true, 99, Item.ItemType.Mineral, Color.red, 3));
        // energy parts group 3 // yellow color
        MineralsList.Add("Solarium", SetupOneItem("Solarium", "refined tier 3 energy materials", 25.1f, 2f, -1f, true, 99, Item.ItemType.Mineral, Color.yellow, 3));
        // weapons parts group 3 // dark purple color
        MineralsList.Add("Gravitite", SetupOneItem("Gravitite", "refined tier 3 weapons materials", 40.1f, 2f, -1f, true, 99, Item.ItemType.Mineral, new Color(.29f,0, .5f, 1f), 3));
        // fuel group 3 // lime color
        MineralsList.Add("Exlite", SetupOneItem("Exlite", "refined tier 3 fuel", 30.7f, 2f, -1f, true, 99, Item.ItemType.Fuel, new Color(.75f, .93f, .38f, 1f), 3));

        for (int i = 0; i < 4; i++)
        {
            MineralGroups.Add(i, new List<Item>());
        }
        
        foreach (Item mineral in MineralsList.Values)
        {
            if (!MineralGroups.ContainsKey(mineral.getLevel()))
                MineralGroups.Add(mineral.getLevel(), new List<Item>());
            
            MineralGroups[mineral.getLevel()].Add(mineral);
        }

        // create ores
        OresList.Add("Stone", SetupOneItem("Stone", "rocks", 2.6f, 2f, -1f, true, 99, Item.ItemType.Mineral, new Color(0.2f, .2f, .2f, 1), 0));
        OresList.Add("Ice", SetupOneItem("Ice", "tier 1 fuel", .9f, 2f, -1f, true, 99, Item.ItemType.Fuel, new Color(.758f, .845f, .992f, 1f), 0));

        // for every mineral, create an ore
        foreach (Item mineral in MineralsList.Values)
        {
            OresList.Add(mineral.getName() + " Ore", SetupOneItem(mineral.getName() + " Ore", "un"+ mineral.getDescription(), mineral.getDensity(), mineral.getSellValue()/2, -1f, true, 999, Item.ItemType.Ore, mineral.getColor(), mineral.getLevel()));
        }

        foreach (Item ore in OresList.Values)
        {
            if (!MineralGroups.ContainsKey(ore.getLevel()))
                MineralGroups.Add(ore.getLevel(), new List<Item>());

            MineralGroups[ore.getLevel()].Add(ore);
        }

        // add groups 0 - 7 to normal and big rock mineral groups
        for (int i = 0; i < 7; i++)
        {
            NormalRockMineralZones.Add(i, new List<Item>());
            BigRockMineralZones.Add(i, new List<Item>());
        }

        foreach (Item ore in OresList.Values)
        {
            if (ore.getName() == "Ice")
            {
                // add to small rock groups
                NormalRockMineralZones[1].Add(ore);
                NormalRockMineralZones[1].Add(ore);
                BigRockMineralZones[1].Add(ore);
            }
            // group 1 ores
            if (ore.getName() == "Titanium Ore" || ore.getName() == "Iron Ore" || ore.getName() == "Silicon Ore" || ore.getName() == "Cobalt Ore" || ore.getName() == "Magnesium Ore")
            {
                NormalRockMineralZones[1].Add(ore);
                NormalRockMineralZones[2].Add(ore);
                BigRockMineralZones[2].Add(ore);
                NormalRockMineralZones[3].Add(ore);
                BigRockMineralZones[3].Add(ore);
                BigRockMineralZones[4].Add(ore);
            }
            // group 2 ores
            if (ore.getName() == "Platinum Ore")
            {
                NormalRockMineralZones[3].Add(ore);
                NormalRockMineralZones[4].Add(ore);
                BigRockMineralZones[4].Add(ore);
                BigRockMineralZones[5].Add(ore);
                BigRockMineralZones[6].Add(ore);
            }
            // group 3 ores
            if (ore.getName() == "Phasium Ore" || ore.getName() == "Lunarite Ore" || ore.getName() == "Rubelline Ore" || ore.getName() == "Solarium Ore" || ore.getName() == "Nuclarium Ore")
            {
                NormalRockMineralZones[5].Add(ore);
                NormalRockMineralZones[6].Add(ore);
                BigRockMineralZones[6].Add(ore);
            }
        }
    }

    public List<Item> GetMineralsInGroup(int group)
    {
        return MineralGroups[group];
    }

    public int CalculateZone(Vector3 WorldPos)
    {
        // get distance from 0,0,0
        float distance = Vector3.Distance(Vector3.zero, WorldPos);
        if (distance < 1000)
        {
            return 0;
        }
        if (distance < 10000)
        {
            return 1;
        }
        if (distance < 100000)
        {
            return 2;
        }
        if (distance < 200000)
        {
            return 3;
        }
        if (distance < 300000)
        {
            return 4;
        }
        if (distance < 2000000)
        {
            return 0;
        }
        if (distance < 3000000)
        {// figure out where H is for the warp stuff
            return 5;
        }
        return 0;
    }

    public Item GetMineralTypeFromPos(Vector3 WorldPos, bool isBigAsteroid)
    {
        int zone = CalculateZone(WorldPos);

        // 90% chance of stone
        if (Random.Range(0, 100) < 90)
        {
            return OresList["Stone"];
        }

        // if its a big asteroid, get a mineral from the big rock zones
        if (isBigAsteroid)
        {
            // if there are no minerals in this zone, return stone
            if (BigRockMineralZones[zone].Count == 0)
            {
                return OresList["Stone"];
            }
            return BigRockMineralZones[zone][Random.Range(0, BigRockMineralZones[zone].Count)];
        }
        // if its a small asteroid, get a mineral from the small rock zones
        else
        {
            // if there are no minerals in this zone, return stone
            if (NormalRockMineralZones[zone].Count == 0)
            {
                return OresList["Stone"];
            }
            int rando = Random.Range(0, NormalRockMineralZones[zone].Count);
            return NormalRockMineralZones[zone][rando];
        }
    }

}
