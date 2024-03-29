using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenInventoryUI : MonoBehaviour
{

    GameObject inventoryUILeft;
    GameObject inventoryUIRight;
    GameObject inventoryUILeftBackground;
    GameObject inventoryUIRightBackground;

    GameObject currentInventoryUI = null;
    GameObject currentInventoryUIBackground = null;

    GameObject inventoryTilePrefab;
    GameObject inventoryBackgroundTilePrefab;

    public bool inventoryOpen = false;

    int backgroundSize;
    int iconSize;
    int diff;
    int spaceBetween;
    public int numRows;
    public int numCols;
    int leftOver;
    int iconCurrentlyOn = -1;

    bool IsOnPlayer = false;

    List<GameObject> BackgroundTiles = new List<GameObject>();
    List<GameObject> Icons = new List<GameObject>();

    public GameObject heldItemSprite = null;
    public ItemPair heldItem = null;

    Inventory inventory;

    int lastPressedPos = 0;

    GameObject player;
    ItemManager itemManager;
    bool first = true;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inventory = GetComponent<Inventory>();
        inventoryUILeft = GameObject.Find("LeftInventory");
        inventoryUIRight = GameObject.Find("RightInventory");
        inventoryUILeftBackground = GameObject.Find("LeftInventoryBackground");
        inventoryUIRightBackground = GameObject.Find("RightInventoryBackground");
        itemManager = GameObject.Find("WorldManager").GetComponent<ItemManager>();

        if (this.gameObject.tag == "Player")
            IsOnPlayer = true;

        backgroundSize = 80;
        iconSize = 60;
        diff = backgroundSize - iconSize;
        spaceBetween = backgroundSize - diff / 2;

        inventoryTilePrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryTile");
        inventoryBackgroundTilePrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryBackgroundTile");
    }

    public GameObject LastCaller = null;

    public void ShowInventory(GameObject caller, bool isLeft)
    {
        currentInventoryUI = isLeft ? inventoryUILeft : inventoryUIRight;
        currentInventoryUIBackground = isLeft ? inventoryUILeftBackground : inventoryUIRightBackground;

        if (currentInventoryUI != null)
        {
            currentInventoryUI.SetActive(true);
            currentInventoryUIBackground.SetActive(true);
            // get the numRows and numCols and extra from the inventory script
            numRows = inventory.numRows;
            numCols = inventory.numCols;
            leftOver = inventory.leftOver;

            LastCaller = caller;
            // set lastCallers lastCaller to this game object
            if (LastCaller.GetComponent<OpenInventoryUI>() != null)
            {
                LastCaller.GetComponent<OpenInventoryUI>().LastCaller = this.gameObject;
            }
            inventoryOpen = true;
            iconCurrentlyOn = -1;
            GenerateInventoryUI(isLeft);
        }
    }

    public void HideInventory(bool isLeft)
    {
        inventoryUILeft.SetActive(false);
        inventoryUIRight.SetActive(false);
        inventoryUILeftBackground.SetActive(false);
        inventoryUIRightBackground.SetActive(false);

        currentInventoryUI = null;
        // if the LastCaller has an inventoryui then set it as not active
        if (LastCaller != null)
        {
            if (LastCaller.GetComponent<OpenInventoryUI>() != null)
            {
                LastCaller.GetComponent<OpenInventoryUI>().LastCaller = null;
            }
        }
        LastCaller = null;
        inventoryOpen = false;
        iconCurrentlyOn = -1;
    }

    int mouseHoldThreshhold = 4;

    public void ShowOrHideUI(InputAction.CallbackContext context)
    {
        Debug.Log("here");
        // if the player presses tab and the inventory is not open, open it else close it
        if (IsOnPlayer)
        {
            Debug.Log("here2");
            if (!inventoryUILeft.activeSelf && !GetComponent<UIManager>().getUIOpen())
            {
                Debug.Log("here3");
                if (GetComponent<UIManager>().openAnyUI(this.gameObject, false, true, new List<GameObject> { inventoryUILeft, inventoryUIRight, inventoryUILeftBackground, inventoryUIRightBackground }))
                {
                    Debug.Log("Showing");
                    ShowInventory(this.gameObject, true);
                }
            }
            else if (inventoryUILeft.activeSelf)
            {
                Debug.Log("here4");
                if (GetComponent<UIManager>().OpenOrCloseInventory(this.gameObject))
                {
                    Debug.Log("Hiding");
                    HideInventory(true);
                }
            }   
        }
        
    }

    public void OnDrag(int invSpot)
    {
        lastPressedPos = invSpot;
        // Then pick up the item at that spot
        heldItem = inventory.getItemAtPos(invSpot);
        if (heldItem != null)
        {
            inventory.removeItem(invSpot);
            UpdateInventory();
            // update the sprite
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 1;
            heldItemSprite.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
            heldItemSprite.GetComponent<UnityEngine.UI.Image>().sprite = itemManager.getSprite(heldItem.item.getName());
        }
    }

    public void OnDrop(int invSpot)
    {
        // if we are holding something then place it here
        bool fromOtherInv = false;
        Inventory otherInventory = null;
        OpenInventoryUI otherInventoryUI = null;
        if (LastCaller != null && (heldItem == null || heldItem.amount == 0))
        {
            otherInventory = LastCaller.GetComponent<Inventory>();
            otherInventoryUI = LastCaller.GetComponent<OpenInventoryUI>();
            heldItem = otherInventoryUI.heldItem;
            fromOtherInv = true;
        }
        if (heldItem != null)
        {
            bool skip = false;
            bool skipPlaceHere = false;
            // if there is something in invSpot then move that item to where this one was 
            ItemPair temp = inventory.getItemAtPos(invSpot);
            if (temp != null)
            {
                // if its the same item then add the amounts together and put the remainder back
                if (temp.item.getName() == heldItem.item.getName())
                {
                    float total = temp.amount + heldItem.amount;
                    float leftOver = total - temp.item.getMaxStack();
                    if (leftOver > 0)
                    {
                        temp.amount = temp.item.getMaxStack();
                        heldItem.amount = leftOver;
                        if (fromOtherInv)
                        {
                            otherInventory.addItem(heldItem.item, heldItem.amount, otherInventoryUI.lastPressedPos);
                        }
                        else
                        {
                            inventory.addItem(heldItem.item, heldItem.amount, lastPressedPos);
                        }
                    }
                    else
                    {
                        temp.amount = total;
                    }
                }
                else
                {
                    inventory.addItem(heldItem.item, heldItem.amount, invSpot);
                    if (fromOtherInv)
                    {
                        otherInventory.addItem(temp.item, temp.amount, otherInventoryUI.lastPressedPos);
                    }
                    else
                    {
                        inventory.addItem(temp.item, temp.amount, lastPressedPos);
                    }
                }
            }
            else
            {
                inventory.addItem(heldItem.item, heldItem.amount, invSpot);
            }
            UpdateInventory();
            heldItem = null;
            heldItemSprite.GetComponent<UnityEngine.UI.Image>().sprite = null;
            heldItemSprite.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
            if (fromOtherInv)
            {
                otherInventoryUI.heldItem = null;
                otherInventoryUI.heldItemSprite.GetComponent<UnityEngine.UI.Image>().sprite = null;
                otherInventoryUI.heldItemSprite.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
                otherInventoryUI.UpdateInventory();
            }

        }
    }

    public void OnPointerDown(int invSlot)
    {
        if (LastCaller == null) return;
        Inventory otherInventory = LastCaller.GetComponent<Inventory>();
        OpenInventoryUI otherInventoryUI = LastCaller.GetComponent<OpenInventoryUI>();
        Inventory thisInventory = GetComponent<Inventory>();
        // if shift is pressed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (LastCaller != null)
            {
                // this means there are 2 inventories open and we are moving an item from one to the other
                ItemPair item = thisInventory.getItemAtPos(invSlot);
                if (item != null)
                {
                    // remove the item from this inventory
                    thisInventory.removeItem(invSlot);
                    // add the item to the other inventory
                    ItemPair itemLeftOver = otherInventory.addItem(item.item, item.amount, -1);
                    while (itemLeftOver != null)
                    {
                        float amount = itemLeftOver.amount;
                        itemLeftOver = otherInventory.addItem(itemLeftOver.item, itemLeftOver.amount, -1);
                        if (amount == itemLeftOver.amount)
                        {
                            thisInventory.addItem(itemLeftOver.item, itemLeftOver.amount, -1);
                            break;
                        }
                    }
                    // update the UIs
                    otherInventory.inventoryUIScript.UpdateInventory();
                }
            }
        }
    }

    public void OnPointerEnter(int invSpot)
    {
        // if there is an item here then highlight it 
        iconCurrentlyOn = invSpot;
        ItemPair item = inventory.getItemAtPos(invSpot);
        if (item != null)
        {
            // make the image in the inventoryUI brighter
            Icons[invSpot].GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
        }
    }

    public void OnPointerExit(int invSpot)
    {
        iconCurrentlyOn = -1;
        // if there is an item here then unhighlight it 
        ItemPair item = inventory.getItemAtPos(invSpot);
        if (item != null)
        {
            // make the image in the inventoryUI lew bright
            Icons[invSpot].GetComponent<UnityEngine.UI.Image>().color = new Color(.88f, .88f, .88f, 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (first)
        {
            inventoryUILeft.SetActive(false);
            inventoryUIRight.SetActive(false);
            inventoryUILeftBackground.SetActive(false);
            inventoryUIRightBackground.SetActive(false);
            first = false;
        }
        if (inventoryOpen)
        {
            if (heldItem != null && heldItemSprite != null)
            {
                // if the mouse isnt down then put the item back
                if (!Input.GetMouseButton(0))
                {
                    GetComponent<Inventory>().addItem(heldItem.item, heldItem.amount, lastPressedPos);
                    heldItem = null;
                    if (heldItemSprite != null)
                    {
                        heldItemSprite.GetComponent<UnityEngine.UI.Image>().sprite = null;
                        heldItemSprite.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
                    }
                    UpdateInventory();
                }
                else
                {
                    // set its position to the mouse position
                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = 1;
                    heldItemSprite.transform.position = mousePos;
                }
            }
        }
        else
        {
            // make sure if we are holding something it goes back to where it came from
            if (heldItem != null)
            {
                // put the item back in its original spot
                GetComponent<Inventory>().addItem(heldItem.item, heldItem.amount, lastPressedPos);
                heldItem = null;
                UpdateInventory();
            }
        }
    }

    int getFlatPos(int row, int col)
    {
        return col * numRows + row;
    }

    void GenerateInventoryUI(bool isLeft)
    {
        // delete all children of the inventoryUI
        foreach (Transform child in currentInventoryUI.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        foreach (Transform child in currentInventoryUIBackground.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        BackgroundTiles.Clear();
        Icons.Clear();

        int total = 0;

        for (int i = 0; i < numCols + 1; i++)
        {
            for (int j = 0; j < numRows; j++)
            {
                if (total >= inventory.GetTotalInvSlots())
                    break;

                // spawn an inventory tile prefab at the correct position
                GameObject tile = Instantiate(inventoryBackgroundTilePrefab, currentInventoryUIBackground.transform);
                // set its parent
                tile.transform.SetParent(currentInventoryUIBackground.transform, true);
                tile.transform.localPosition = new Vector3(j * spaceBetween, -i * spaceBetween, 0);
                // set the width and height to size
                tile.GetComponent<RectTransform>().sizeDelta = new Vector2(backgroundSize, backgroundSize);
                BackgroundTiles.Add(tile);
                // make another tile in the same spot for the item sprites
                GameObject icon = Instantiate(inventoryTilePrefab, currentInventoryUI.transform);
                icon.GetComponent<HandleUIInput>().invSlot = total;
                icon.GetComponent<HandleUIInput>().openInventoryUI = this;
                // set its parent
                icon.transform.SetParent(currentInventoryUI.transform, true);
                icon.transform.localPosition = new Vector3(j * spaceBetween, -i * spaceBetween, 0);
                // set the width and height to size
                icon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);
                // set the sprint to null for now
                icon.GetComponent<UnityEngine.UI.Image>().sprite = null;
                icon.GetComponent<UnityEngine.UI.Image>().color = new Color(.88f, .88f, .88f, 1);
                // add to the list of icons
                Icons.Add(icon);
                total++;
            }
        }
        // make a sprite for the held item
        heldItemSprite = Instantiate(inventoryTilePrefab, currentInventoryUI.transform);
        Destroy(heldItemSprite.GetComponent<HandleUIInput>());
        Destroy(heldItemSprite.GetComponent<UnityEngine.EventSystems.EventTrigger>());
        heldItemSprite.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
        heldItemSprite.transform.SetParent(currentInventoryUI.transform, true);
        heldItemSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);
        heldItemSprite.GetComponent<UnityEngine.UI.Image>().sprite = null;
        heldItemSprite.name = "HeldItemSprite";
        UpdateInventory();
    }

    // this should be called by the inventory script when the inventory is updated
    public void UpdateInventory()
    {
        List<ItemPair> allItems = GetComponent<Inventory>().getAllItems();
        for (int i = 0; i < Mathf.Min(allItems.Count, Icons.Count); i++)
        {
            if (allItems[i] == null)
            {
                Icons[i].GetComponent<UnityEngine.UI.Image>().sprite = null;
                Icons[i].GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
            }
            else
            {
                Icons[i].GetComponent<UnityEngine.UI.Image>().color = new Color(.88f, .88f, .88f, 1);
                if (i == iconCurrentlyOn)
                {
                    Icons[i].GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
                }
                if (itemManager.getSprite(allItems[i].item.getName()) == null)
                {
                    Debug.Log("here1");
                }
                Icons[i].GetComponent<UnityEngine.UI.Image>().sprite = itemManager.getSprite(allItems[i].item.getName());
            }
        }
    }
}
