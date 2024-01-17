using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Demo_Project.SceneManager;

namespace Demo_Project
{
    public class SideMenu : MonoBehaviour
    {
        // Menu state related items
        public bool isMenuOpen = false;
        const float openPositon = -263;
        const float closePosition = -726;
        public float menuMoveSpeed = 5;
        string menuState = "";


        //Upper menu related items
        int currentSelectedItem = 0;
        int totalNumberOfItems = 0;


        // Lower menu related items
        public string selectedItem = "";
        public GameObject selectedItemObject = null;

        // Camera related items

        float cameraOriginalSize = 2.78f;
        float cameraOriginalPosition = 0;
        float cameraShrinkSize = 5.0f;
        float cameraShrinkPosition = -2.5f;

        GameObject itemNameObject = null;
        bool moveText = false;
        public float textPopupMoveSpeed = .3f;
        public bool burstDelay = false;

        float timeSinceLastFrame = 0;

        // Start is called before the first frame update
        void Start()
        {
            SceneManager.listOfMenuObjects.Add(this.gameObject);
            itemNameObject = transform.parent.Find("Item_Name").gameObject;

            SelectBurst();
            AdjustSelectionArrow();
            UpdateItemRatio();
            UpdateItemName();
            InitialOpenMenu();

        }

        void InitialOpenMenu()
        {
            //Open Menu
            transform.Find("Main Menu Image").GetComponent<RectTransform>().localPosition = new Vector3(openPositon, 241, 0);
            transform.Find("Main Menu Image").transform.Find("Arrow").GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            // Adjust the camera
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().orthographicSize = cameraShrinkSize;
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().transform.position = new Vector3(cameraShrinkPosition, 0, -10);
            isMenuOpen = true;

        }

        // Controls the click action of the open and close button.
        public void MenuClickCheck()
        {
            if (!isMenuOpen && menuState == "")
            {
                menuState = "opening";
            }
            else
            {
                menuState = "closing";
            }

        }

        // Click of the next button
        public void NextClick()
        {
            currentSelectedItem++;
            if (currentSelectedItem >= totalNumberOfItems)
            {
                currentSelectedItem = 0;
            }
            UpdateItemName();
            UpdateItemRatio();
            SendBulletInformationToPlayer();
            burstDelay = true;

            for (int i = 0; i < SceneManager.listOfArms.Count; i++)
            {
                SceneManager.listOfArms[i].GetComponent<Player>().RemoveChargeObject();
            }
            NewCheck();
        }

        // Click of the previous button
        public void PrevClick()
        {
            currentSelectedItem--;
            if (currentSelectedItem < 0)
            {
                currentSelectedItem = totalNumberOfItems - 1;
            }
            UpdateItemName();
            UpdateItemRatio();
            SendBulletInformationToPlayer();
            burstDelay = true;
            for (int i = 0; i < SceneManager.listOfArms.Count; i++)
            {
                SceneManager.listOfArms[i].GetComponent<Player>().RemoveChargeObject();
            }
            NewCheck();
        }

        // Select Burst from the menu
        public void SelectBurst()
        {
            if (selectedItem != "burst")
            {
                selectedItem = "burst";
                currentSelectedItem = 0;
                totalNumberOfItems = transform.Find("LowerSection").transform.Find("Burst").transform.childCount;
                if (totalNumberOfItems > 0)
                {
                    UpdateItemName();
                }
                UpdateItemRatio();
                AdjustSelectionArrow();
                HidePlayerAndTarget();
                SendBulletInformationToPlayer();
                burstDelay = true;
                NewCheck();
                CheckLeftClickText();

            }
        }

        // Select Loop from the menu
        public void SelectLoop()
        {
            if (selectedItem != "loop")
            {
                selectedItem = "loop";
                currentSelectedItem = 0;
                totalNumberOfItems = transform.Find("LowerSection").transform.Find("Loops").transform.childCount;
                if (totalNumberOfItems > 0)
                {
                    UpdateItemName();
                }
                UpdateItemRatio();
                AdjustSelectionArrow();
                HidePlayerAndTarget();
                SendBulletInformationToPlayer();
                NewCheck();
                CheckLeftClickText();

                for (int i = 0; i < SceneManager.listOfArms.Count; i++)
                {
                    SceneManager.listOfArms[i].GetComponent<Player>().BurstManualRemoval();
                }
            }
        }

        // Select Projectile from the menu
        public void SelectProjectile()
        {
            if (selectedItem != "projectile")
            {
                selectedItem = "projectile";
                currentSelectedItem = 0;
                totalNumberOfItems = transform.Find("LowerSection").transform.Find("Projectiles").transform.childCount;
                if (totalNumberOfItems > 0)
                {
                    UpdateItemName();
                }
                UpdateItemRatio();
                AdjustSelectionArrow();
                SendBulletInformationToPlayer();

                ShowPlayerAndTarget();
                NewCheck();
                CheckLeftClickText();
                for (int i = 0; i < SceneManager.listOfArms.Count; i++)
                {
                    SceneManager.listOfArms[i].GetComponent<Player>().BurstManualRemoval();
                }
            }
        }

        // Sends information about the selected object to the player
        public void SendBulletInformationToPlayer()
        {
            for (int i = 0; i < SceneManager.listOfArms.Count; i++)
            {
                if (selectedItem == "loop")
                {

                    SceneManager.listOfArms[i].GetComponent<Player>().ReceiveLoopInformation(selectedItemObject);
                }

                if (selectedItem == "burst")
                {

                    SceneManager.listOfArms[i].GetComponent<Player>().PlayBurst();
                }
            }
        }

        //Moves the selection arrow to the selected item
        public void AdjustSelectionArrow()
        {
            GameObject arrowSelection = transform.Find("Main Menu Image").transform.Find("LowerSection").transform.Find("Selection_Arrow").gameObject;
            Vector3 arrowPosition = arrowSelection.GetComponent<RectTransform>().localPosition;
            if (selectedItem == "burst")
            {
                arrowPosition.y = 33;
                arrowSelection.GetComponent<RectTransform>().localPosition = arrowPosition;
            }

            else if (selectedItem == "loop")
            {
                arrowPosition.y = -60;
                arrowSelection.GetComponent<RectTransform>().localPosition = arrowPosition;
            }
            else if (selectedItem == "projectile")
            {
                arrowPosition.y = -150;
                arrowSelection.GetComponent<RectTransform>().localPosition = arrowPosition;
            }
        }

        // Update the items name in the menu
        void UpdateItemName()
        {

            Transform parentItem = null;
            if (selectedItem == "projectile")
            {

                parentItem = transform.Find("LowerSection").transform.Find("Projectiles");
            }

            else if (selectedItem == "burst")
            {
                parentItem = transform.Find("LowerSection").transform.Find("Burst");
            }

            else if (selectedItem == "loop")
            {
                parentItem = transform.Find("LowerSection").transform.Find("Loops");
            }
            transform.Find("Main Menu Image").transform.Find("UpperSection").transform.Find("Selection_text").GetComponent<TMP_Text>().text = parentItem.transform.GetChild(currentSelectedItem).name;
            itemNameObject.GetComponent<TMP_Text>().text = parentItem.transform.GetChild(currentSelectedItem).name;
            SetSelectedItem(parentItem.transform.GetChild(currentSelectedItem).gameObject);
            ResetItemNameText();
        }

        void ResetItemNameText()
        {
            itemNameObject.GetComponent<RectTransform>().localPosition = new Vector3(620, -585, 0);
            moveText = true;
        }

        // Sets the selected item
        void SetSelectedItem(GameObject tempGameObject)
        {
            selectedItemObject = tempGameObject;
        }

        // Updates the item ratio
        void UpdateItemRatio()
        {
            int tempCurrentSelected = currentSelectedItem + 1;
            transform.Find("Main Menu Image").transform.Find("UpperSection").transform.Find("Ratio").GetComponent<TMP_Text>().text = tempCurrentSelected.ToString() + " / " + totalNumberOfItems.ToString();
        }



        // Controls the menu's movements.
        void UpdateMenuState()
        {
            if (menuState == "opening")
            {
                Vector3 tempPosition = transform.Find("Main Menu Image").GetComponent<RectTransform>().localPosition;
                tempPosition.x += menuMoveSpeed * timeSinceLastFrame;
                transform.Find("Main Menu Image").GetComponent<RectTransform>().localPosition = tempPosition;
                MoveCamera();
                if (tempPosition.x >= openPositon)
                {
                    isMenuOpen = true;
                    menuState = "";
                    transform.Find("Main Menu Image").GetComponent<RectTransform>().localPosition = new Vector3(openPositon, 241, 0);
                    transform.Find("Main Menu Image").transform.Find("Arrow").GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                }

            }

            if (menuState == "closing")
            {
                Vector3 tempPosition = transform.Find("Main Menu Image").GetComponent<RectTransform>().localPosition;
                tempPosition.x -= menuMoveSpeed * timeSinceLastFrame;
                transform.Find("Main Menu Image").GetComponent<RectTransform>().localPosition = tempPosition;
                MoveCamera();
                if (tempPosition.x <= closePosition)
                {
                    isMenuOpen = false;
                    menuState = "";
                    transform.Find("Main Menu Image").GetComponent<RectTransform>().localPosition = new Vector3(closePosition, 241, 0);
                    transform.Find("Main Menu Image").transform.Find("Arrow").GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
                }


            }
        }

        void ScrollWheelAndButtonCheck()
        {
            if (Input.mouseScrollDelta.y < 0 || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.W))
            {
                NextClick();
            }

            if (Input.mouseScrollDelta.y > 0 || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S))
            {
                PrevClick();
            }
        }

        // Hides the player and target from the screen
        void HidePlayerAndTarget()
        {
            for (int i = 0; i < SceneManager.listOfArms.Count; i++)
            {
                SceneManager.listOfArms[i].GetComponent<SpriteRenderer>().enabled = false;
                SceneManager.listOfArms[i].GetComponent<Player>().RemoveChargeObject();
            }

            for (int i = 0; i < SceneManager.listOfHeads.Count; i++)
            {
                SceneManager.listOfHeads[i].GetComponent<SpriteRenderer>().enabled = false;
            }

            for (int i = 0; i < SceneManager.listOfBodies.Count; i++)
            {
                SceneManager.listOfBodies[i].GetComponent<SpriteRenderer>().enabled = false;
            }

            for (int i = 0; i < SceneManager.listOfTargets.Count; i++)
            {
                SceneManager.listOfTargets[i].GetComponent<SpriteRenderer>().enabled = false;
            }

            
        }

        // Displays the player and target on the screen
        void ShowPlayerAndTarget()
        {
            for (int i = 0; i < SceneManager.listOfArms.Count; i++)
            {
                SceneManager.listOfArms[i].GetComponent<SpriteRenderer>().enabled = true;
            }
            for (int i = 0; i < SceneManager.listOfHeads.Count; i++)
            {
                SceneManager.listOfHeads[i].GetComponent<SpriteRenderer>().enabled = true;
            }
            for (int i = 0; i < SceneManager.listOfBodies.Count; i++)
            {
                SceneManager.listOfBodies[i].GetComponent<SpriteRenderer>().enabled = true;
            }
            for (int i = 0; i < SceneManager.listOfTargets.Count; i++)
            {
                SceneManager.listOfTargets[i].GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        void MoveCamera()
        {
            if (menuState == "opening")
            {
                GameObject.FindWithTag("MainCamera").GetComponent<Camera>().orthographicSize += .05f * timeSinceLastFrame;
                GameObject.FindWithTag("MainCamera").GetComponent<Camera>().transform.Translate(-.05f * timeSinceLastFrame, 0, 0);

                if (GameObject.FindWithTag("MainCamera").GetComponent<Camera>().orthographicSize > cameraShrinkSize)
                {
                    GameObject.FindWithTag("MainCamera").GetComponent<Camera>().orthographicSize = cameraShrinkSize;
                }

                if (GameObject.FindWithTag("MainCamera").GetComponent<Camera>().transform.position.x < cameraShrinkPosition)
                {
                    GameObject.FindWithTag("MainCamera").GetComponent<Camera>().transform.position = new Vector3(cameraShrinkPosition, 0, -10);
                }
            }

            if (menuState == "closing")
            {
                GameObject.FindWithTag("MainCamera").GetComponent<Camera>().orthographicSize -= .05f * timeSinceLastFrame;
                GameObject.FindWithTag("MainCamera").GetComponent<Camera>().transform.Translate(.05f * timeSinceLastFrame, 0, 0);

                if (GameObject.FindWithTag("MainCamera").GetComponent<Camera>().orthographicSize < cameraOriginalSize)
                {
                    GameObject.FindWithTag("MainCamera").GetComponent<Camera>().orthographicSize = cameraOriginalSize;
                }

                if (GameObject.FindWithTag("MainCamera").GetComponent<Camera>().transform.position.x > cameraOriginalPosition)
                {
                    GameObject.FindWithTag("MainCamera").GetComponent<Camera>().transform.position = new Vector3(cameraOriginalPosition, 0, -10);
                }
            }

        }

        void UpdateItemNameTextPosition()
        {
            if (moveText)
            {
                itemNameObject.GetComponent<RectTransform>().Translate(0, textPopupMoveSpeed * timeSinceLastFrame, 0);

                if (itemNameObject.GetComponent<RectTransform>().localPosition.y >= -495)
                {
                    moveText = false;
                    itemNameObject.GetComponent<RectTransform>().localPosition = new Vector3(itemNameObject.GetComponent<RectTransform>().localPosition.x, -495, itemNameObject.GetComponent<RectTransform>().localPosition.z);
                }
            }
        }

        void NewCheck()
        {

            if (selectedItemObject.GetComponent<NewStatus>() != null && selectedItemObject.GetComponent<NewStatus>().isNew)
            {
                transform.Find("Main Menu Image").transform.Find("NewImage").GetComponent<Image>().enabled = true;
            }
            else
            {
                transform.Find("Main Menu Image").transform.Find("NewImage").GetComponent<Image>().enabled = false;
            }


        }

        public void CheckLeftClickText()
        {
            if (selectedItem == "loop")
            {
                transform.Find("Main Menu Image").transform.Find("LeftClickText").GetComponent<TMP_Text>().enabled = false;
            }
            else
            {
                transform.Find("Main Menu Image").transform.Find("LeftClickText").GetComponent<TMP_Text>().enabled = true;
            }
        }

        public void OpenCartoonCoffeeWebsite()
        {
            Application.OpenURL("http://cartooncoffeegames.com/");
        }

        // Update is called once per frame
        void Update()
        {
            timeSinceLastFrame = Time.deltaTime / .001666f;
            burstDelay = false;
            UpdateMenuState();
            UpdateItemNameTextPosition();
            ScrollWheelAndButtonCheck();

        }
    }
}