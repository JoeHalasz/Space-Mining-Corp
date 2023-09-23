using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenInventoryUI : MonoBehaviour
{

    GameObject inventoryUILeft;
    GameObject inventoryUIRight;
    GameObject currentInventoryUI = null;

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

    bool IsOnPlayer = false;

    List<GameObject> BackgroundTiles = new List<GameObject>();
    List<GameObject> Icons = new List<GameObject>();

    public GameObject heldItemSprite = null;
    public ItemPair heldItem = null;

    Inventory inventory;

    int lastPressedPos = 0;

    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        inventory = GetComponent<Inventory>();
        inventoryUILeft = GameObject.Find("LeftInventory");
        inventoryUIRight = GameObject.Find("RightInventory");

        if (inventoryBackgroundTilePrefab == null)
            inventoryBackgroundTilePrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryBackgroundTile");
        if (inventoryTilePrefab == null)
            inventoryTilePrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryTile");

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
        if (currentInventoryUI != null)
        {
            currentInventoryUI.SetActive(true);
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
            GenerateInventoryUI(isLeft);
        }
    }

    public void HideInventory(bool isLeft)
    {
        inventoryUILeft.SetActive(false);
        inventoryUIRight.SetActive(false);
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
    }

    int mouseDownFrames = 0;

    void FixedUpdate()
    {
        if (Input.GetMouseButton(0))
        {
            mouseDownFrames += 1;
        }
    }

    int mouseHoldThreshhold = 4;

    public void ShowOrHideUI(InputAction.CallbackContext context)
    {
        // if the player presses tab and the inventory is not open, open it else close it
        if (IsOnPlayer && !inventoryUILeft.activeSelf && !GetComponent<UIManager>().getUIOpen())
        {
            ShowInventory(this.gameObject, true);
            GetComponent<UIManager>().openAnyUI(this.gameObject);
        }
        else if (IsOnPlayer && inventoryUILeft.activeSelf)
        {
            HideInventory(true);
            GetComponent<UIManager>().OpenOrCloseInventory(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (inventoryOpen)
        {
            if (Input.GetMouseButtonDown(0) || (mouseDownFrames > mouseHoldThreshhold && !Input.GetMouseButton(0) && (heldItem != null || (LastCaller != null && LastCaller.GetComponent<OpenInventoryUI>().heldItem != null))))
            {
                // get the mouse pos
                Vector3 mousePos = Input.mousePosition;
                // figure out which inventory square is under that mouse pos
                // get the x and y of the mouse pos
                float x = mousePos.x;
                float y = mousePos.y;

                int localNumCols = numCols;
                if (leftOver != 0)
                    localNumCols++;

                float xMinCheck = currentInventoryUI.transform.position.x - iconSize / 2;

                float xMaxCheck = xMinCheck + numRows * spaceBetween - diff / 2;

                float yMaxCheck = currentInventoryUI.transform.position.y + iconSize / 2;
                float yMinCheck = yMaxCheck - localNumCols * spaceBetween + diff / 2;

                bool InSquare = false;
                int xSquare = -1;
                int ySquare = -1;

                if (x >= xMinCheck && x <= xMaxCheck && y >= yMinCheck && y <= yMaxCheck)
                {
                    x -= xMinCheck;
                    y -= yMinCheck;
                    x /= spaceBetween;
                    y /= spaceBetween;
                    y = localNumCols - y;
                    if (x - (int)x < .85f && y - (int)y > .15f)
                    {
                        InSquare = true;
                        xSquare = (int)x;
                        ySquare = (int)y;
                    }
                }
                if (InSquare)
                {
                    int pos = getFlatPos(xSquare, ySquare);
                    // make sure pos isnt more than the inv size
                    if (pos > inventory.GetTotalInvSlots())
                        return;

                    bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
                    Inventory thisInventory = GetComponent<Inventory>();
                    if (LastCaller == null) return;
                    Inventory otherInventory = LastCaller.GetComponent<Inventory>();
                    OpenInventoryUI otherInventoryUI = LastCaller.GetComponent<OpenInventoryUI>();
                    if (LastCaller != null && shiftPressed)
                    {
                        // this means there are 2 inventories open and we are moving an item from one to the other
                        
                        ItemPair item = thisInventory.getItemAtPos(pos);
                        if (item != null)
                        {
                            // remove the item from this inventory
                            thisInventory.removeItem(pos);
                            // add the item to the other inventory
                            ItemPair itemLeftOver = otherInventory.addItem(item.item, item.amount, -1);
                            if (itemLeftOver != null)
                            {
                                thisInventory.addItem(itemLeftOver.item, itemLeftOver.amount, pos);
                            }
                            // update the UIs
                            otherInventory.inventoryUIScript.UpdateInventory();
                        }
                    }
                    else
                    {
                        // if the heldItem is not null and LastCaller is null or LastCaller.heldItem is not null
                        ItemPair otherHeldItem = null;
                        GameObject otherHeldItemSprite = null;
                        
                        if (LastCaller != null) {
                            otherHeldItem = otherInventoryUI.heldItem;
                            otherHeldItemSprite = otherInventoryUI.heldItemSprite;
                        }
                        
                        if (heldItem != null || otherHeldItem != null)
                        {
                            // we are placing something
                            if (heldItem != null)
                            {
                                // we are moving something from this inv to this inv
                                if (thisInventory.getItemAtPos(pos) == null || thisInventory.getItemAtPos(pos).item.getName() != heldItem.item.getName())
                                {
                                    ItemPair temp = thisInventory.getItemAtPos(pos);

                                    // remove temp from the inventory
                                    thisInventory.removeItem(pos);
                                    // add this the heldItem to the inventory
                                    ItemPair leftOver = thisInventory.addItem(heldItem.item, heldItem.amount, pos);

                                    // make temp heldItem
                                    heldItem = temp;

                                }
                                else
                                {
                                    // they have the same name, add as much as you can to it
                                    ItemPair itemLeftOver = thisInventory.addItem(heldItem.item, heldItem.amount, pos);
                                    if (itemLeftOver == null)
                                    {
                                        heldItem = null;
                                    }
                                    else
                                    {
                                        heldItem = itemLeftOver;
                                    }
                                }
                            }
                            else // otherHeldItem != null
                            {
                                // we are moving something from the other inv to this inv
                                if (thisInventory.getItemAtPos(pos) == null || thisInventory.getItemAtPos(pos).item.getName() != otherHeldItem.item.getName())
                                {
                                    ItemPair temp = thisInventory.getItemAtPos(pos);

                                    // remove temp from the inventory
                                    thisInventory.removeItem(pos);
                                    // add this the heldItem to the inventory
                                    thisInventory.addItem(otherHeldItem.item, otherHeldItem.amount, pos);
                                    // make temp heldItem
                                    otherHeldItem = temp;
                                   
                                    otherInventoryUI.heldItem = temp;
                                    otherInventoryUI.UpdateInventory();
                                }
                                else
                                {
                                    // they have the same name, add as much as you can to it
                                    ItemPair itemLeftOver = thisInventory.addItem(otherInventoryUI.heldItem.item, otherInventoryUI.heldItem.amount, pos);
                                    if (itemLeftOver == null)
                                    {
                                        // move the sprite back to the original position
                                        otherInventoryUI.heldItem = null;
                                        otherInventoryUI.UpdateInventory();
                                    }
                                    else
                                    {
                                        otherInventoryUI.heldItem = itemLeftOver;
                                    }
                                }
                            }
                        }
                        else
                        {   
                            // we are picking something up from this inventory
                            ItemPair item = thisInventory.getItemAtPos(pos);
                            // if theres something at that pos
                            if (item != null)
                            {
                                // set the heldItem to that item
                                heldItem = item;
                                // remove from this inv
                                thisInventory.removeItem(pos);
                            }
                        }
                    }
                    lastPressedPos = pos;
                    UpdateInventory();
                }
            }
            if (heldItem != null && heldItemSprite != null)
            {
                // set its position to the mouse position
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = 1;
                heldItemSprite.transform.position = mousePos;
                heldItemSprite.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
                heldItemSprite.GetComponent<UnityEngine.UI.Image>().sprite = heldItem.item.getSprite();
            }
            if (!Input.GetMouseButton(0))
            {
                mouseDownFrames = 0;
            }
        }
        else
        {
            // make sure if we are holding something it goes back to where it came from
            if (heldItem != null)
            {
                Debug.Log("Resetting because inv was closed");
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
        BackgroundTiles.Clear();
        Icons.Clear();

        int total = 0;
        
        for (int i = 0; i < numCols+1; i++)
        {
            for (int j = 0; j < numRows; j++)
            {
                if (total >= inventory.GetTotalInvSlots())
                    break;
                
                // spawn an inventory tile prefab at the correct position
                GameObject tile = Instantiate(inventoryBackgroundTilePrefab, currentInventoryUI.transform);
                // set its parent
                tile.transform.SetParent(currentInventoryUI.transform, true);
                tile.transform.localPosition = new Vector3(j * spaceBetween, -i * spaceBetween, 0);
                // set the width and height to size
                tile.GetComponent<RectTransform>().sizeDelta = new Vector2(backgroundSize, backgroundSize);
                BackgroundTiles.Add(tile);
                // make another tile in the same spot for the item sprites
                GameObject icon = Instantiate(inventoryTilePrefab, currentInventoryUI.transform);
                // set its parent
                icon.transform.SetParent(currentInventoryUI.transform, true);
                icon.transform.localPosition = new Vector3(j * spaceBetween, -i * spaceBetween, 0);
                // set the width and height to size
                icon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);
                // set the sprint to null for now
                icon.GetComponent<UnityEngine.UI.Image>().sprite = null;
                // add to the list of icons
                Icons.Add(icon);
                total++;
            }
        }
        // make a sprite for the held item
        heldItemSprite = Instantiate(inventoryTilePrefab, currentInventoryUI.transform);
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
                Icons[i].GetComponent<UnityEngine.UI.Image>().color = new Color(1, 1, 1, 1);
                Icons[i].GetComponent<UnityEngine.UI.Image>().sprite = allItems[i].item.getSprite();
            }
        }
        if (heldItemSprite != null)
        {
            heldItemSprite.GetComponent<UnityEngine.UI.Image>().sprite = null;
            heldItemSprite.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
        }

    }
}
