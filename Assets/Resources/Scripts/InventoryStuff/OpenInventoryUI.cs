using System.Collections.Generic;
using UnityEngine;

public class OpenInventoryUI : MonoBehaviour
{

    [SerializeField]
    public GameObject inventoryUI;

    GameObject inventoryTilePrefab;
    GameObject inventoryBackgroundTilePrefab;

    public bool inventoryOpen = false;

    PlayerMovement playerMovement;

    int backgroundSize;
    int iconSize;
    int diff;
    int spaceBetween;
    public int numRows = 6;
    public int numCols = 7;

    bool IsOnPlayer = false;

    List<GameObject> BackgroundTiles = new List<GameObject>();
    List<GameObject> Icons = new List<GameObject>();

    public GameObject heldItemSprite = null;
    public ItemPair heldItem = null;

    int lastPressedPos = 0;

    // Start is called before the first frame update
    void Start()
    {

        if (this.gameObject.tag == "Player")
            IsOnPlayer = true;

        backgroundSize = 80;
        iconSize = 60;
        diff = backgroundSize - iconSize;
        spaceBetween = backgroundSize - diff / 2;

        inventoryTilePrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryTile");
        inventoryBackgroundTilePrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryBackgroundTile");

        playerMovement = GetComponent<PlayerMovement>();
        if (inventoryUI == null)
            Debug.LogError("Inventory UI is null in OpenInventoryUI script on " + this.gameObject);
        else
            GenerateInventoryUI();

    }

    public GameObject LastCaller = null;

    public void ShowInventory(GameObject caller)
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(true);
            // lock the players movement
            if (playerMovement != null)
                playerMovement.LockPlayerInputs(caller);
            LastCaller = caller;
            // set lastCallers lastCaller to this game object
            if (LastCaller.GetComponent<OpenInventoryUI>() != null)
            {
                LastCaller.GetComponent<OpenInventoryUI>().LastCaller = this.gameObject;
            }
            // set the players velocity to 0
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            // show the mouse
            Cursor.visible = true;
            // move the mouse to the center of the screen
            Cursor.lockState = CursorLockMode.None;
            inventoryOpen = true;
        }
    }

    public void HideInventory()
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(false);
            // if the LastCaller has an inventoryui then set it as not active
            if (LastCaller != null)
            {
                if (LastCaller.GetComponent<OpenInventoryUI>() != null)
                {
                    LastCaller.GetComponent<OpenInventoryUI>().inventoryUI.SetActive(false);
                    LastCaller.GetComponent<OpenInventoryUI>().LastCaller = null;
                }
            }
            LastCaller = null;
            // unlock the players movement
            playerMovement.UnlockPlayerInputs();
            // hide the mouse
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            inventoryOpen = false;
        }
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

    // Update is called once per frame
    void Update()
    {
        // if the player presses tab and the inventory is not open, open it else close it
        if (IsOnPlayer && Input.GetKeyDown(KeyCode.Tab) && !inventoryUI.activeSelf && !GetComponent<UIManager>().UIOpen)
        {
            ShowInventory(this.gameObject);
            GetComponent<UIManager>().UIOpen = true;
        }
        else if (IsOnPlayer && Input.GetKeyDown(KeyCode.Tab) && inventoryUI.activeSelf)
        {
            HideInventory();
            GetComponent<UIManager>().UIOpen = false;
        }

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

                float xMinCheck = inventoryUI.transform.position.x - iconSize / 2;

                float xMaxCheck = xMinCheck + numRows * spaceBetween - diff / 2;

                float yMaxCheck = inventoryUI.transform.position.y + iconSize / 2;
                float yMinCheck = yMaxCheck - numCols * spaceBetween + diff / 2;

                bool InSquare = false;
                int xSquare = -1;
                int ySquare = -1;

                if (x >= xMinCheck && x <= xMaxCheck && y >= yMinCheck && y <= yMaxCheck)
                {
                    x -= xMinCheck;
                    y -= yMinCheck;
                    x /= spaceBetween;
                    y /= spaceBetween;
                    y = numCols - y;
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
                    Debug.Log(xSquare + " " + ySquare + " " + pos);
                    bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
                    Inventory thisInventory = GetComponent<Inventory>();
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
            if (heldItem != null)
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


    // sets the number of rows and columns based on the number of slots in the inventory
    // only changes for the otherInventory
    public void SetNumSlots(int numSlots)
    {
        numCols = 0;
        while (numSlots > 0)
        {
            numSlots -= numRows;
            numCols++;
        }
        GenerateInventoryUI();
    }


    void GenerateInventoryUI()
    {
        // delete all children of the inventoryUI
        foreach (Transform child in inventoryUI.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        BackgroundTiles.Clear();
        Icons.Clear();

        if (inventoryBackgroundTilePrefab == null)
            inventoryBackgroundTilePrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryBackgroundTile");
        if (inventoryTilePrefab == null)
            inventoryTilePrefab = Resources.Load<GameObject>("Prefabs/UI/InventoryTile");
        
        for (int i = 0; i < numCols; i++)
        {
            for (int j = 0; j < numRows; j++)
            {
                // spawn an inventory tile prefab at the correct position
                GameObject tile = Instantiate(inventoryBackgroundTilePrefab, inventoryUI.transform);
                // set its parent
                tile.transform.SetParent(inventoryUI.transform, true);
                tile.transform.localPosition = new Vector3(j * spaceBetween, -i * spaceBetween, 0);
                // set the width and height to size
                tile.GetComponent<RectTransform>().sizeDelta = new Vector2(backgroundSize, backgroundSize);
                BackgroundTiles.Add(tile);
                // make another tile in the same spot for the item sprites
                GameObject icon = Instantiate(inventoryTilePrefab, inventoryUI.transform);
                // set its parent
                icon.transform.SetParent(inventoryUI.transform, true);
                icon.transform.localPosition = new Vector3(j * spaceBetween, -i * spaceBetween, 0);
                // set the width and height to size
                icon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);
                // set the sprint to null for now
                icon.GetComponent<UnityEngine.UI.Image>().sprite = null;
                // add to the list of icons
                Icons.Add(icon);
            }
        }
        // make a sprite for the held item
        heldItemSprite = Instantiate(inventoryTilePrefab, inventoryUI.transform);
        heldItemSprite.transform.SetParent(inventoryUI.transform, true);
        heldItemSprite.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);
        heldItemSprite.GetComponent<UnityEngine.UI.Image>().sprite = null;
        heldItemSprite.name = "HeldItemSprite";
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
        if (heldItem == null)
        {
            heldItemSprite.GetComponent<UnityEngine.UI.Image>().sprite = null;
            heldItemSprite.GetComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0);
        }

    }
}
