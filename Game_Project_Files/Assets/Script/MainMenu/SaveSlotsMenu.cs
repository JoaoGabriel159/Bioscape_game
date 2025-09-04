using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Localization.Plugins.XLIFF.V20;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveSlotsMenu : Menu
{
    [Header("Menu Navigation")]
    [SerializeField] private MainMenu mainMenu;

    [Header("Menu Buttons")]
    [SerializeField] private Button backButton;

    [Header("Confirmation Popup")]
    [SerializeField] private ConfirmationPopupMenu confirmationPopupMenu;
    [SerializeField] private LocalizedString confirmationText;

    private SaveSlot[] saveSlots;

    private bool isLoadingGame = false;

    private void Awake() 
    {
        saveSlots = this.GetComponentsInChildren<SaveSlot>();
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot) 
    {
        // disable all buttons
        DisableMenuButtons();
        // case - loading game
        if (isLoadingGame) 
        {
            DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
            DataPersistenceManager.instance.LoadGame();
            SaveGameAndLoadScene();
        }
        // case - new game, but the save slot has data
        else if (saveSlot.hasData) 
        {
            
            confirmationPopupMenu.ActivateMenu(
                confirmationText,
                // function to execute if we select 'yes'
                () => {

                    DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
                    DataPersistenceManager.instance.NewGame();
                    SceneSwapManager.instance._updatedCoords = DataPersistenceManager.instance.GetPlayerPosition();
                    SaveGameAndLoadScene();
                    PlayerPrefs.DeleteAll();
                },
                // function to execute if we select 'cancel'
                () => {
                    this.ActivateMenu(isLoadingGame);
                }
            );
        }
        // case - new game, and the save slot has no data
        else 
        {
            Debug.Log("TEST New Game clicked on empty save slot: " + saveSlot.GetProfileId());
            DataPersistenceManager.instance.ChangeSelectedProfileId(saveSlot.GetProfileId());
            DataPersistenceManager.instance.NewGame();
            SceneSwapManager.instance._updatedCoords = DataPersistenceManager.instance.GetPlayerPosition();
            
            SaveGameAndLoadScene();
        }
    }

    private void SaveGameAndLoadScene() 
    {
        // save the game anytime before loading a new scene
        
        DataPersistenceManager.instance.SaveGame();

        // load the scene
        // SceneManager.LoadSceneAsync("SampleScene");
        string sceneToLoad = DataPersistenceManager.instance.GetDataSceneName();

        SceneSwapManager.SwapSceneFromDoorUse(sceneToLoad, DataPersistenceManager.instance.GetPlayerPosition());
        SceneManager.LoadSceneAsync(sceneToLoad);
    }

    // public void OnClearClicked(SaveSlot saveSlot) 
    // {
    //     DisableMenuButtons();

    //     confirmationPopupMenu.ActivateMenu(
    //         "Are you sure you want to delete this saved data?",
    //         // function to execute if we select 'yes'
    //         () => {
    //             DataPersistenceManager.instance.DeleteProfileData(saveSlot.GetProfileId());
    //             ActivateMenu(isLoadingGame);
    //         },
    //         // function to execute if we select 'cancel'
    //         () => {
    //             ActivateMenu(isLoadingGame);
    //         }
    //     );
    // }

    public void OnBackClicked() 
    {
        // mainMenu.ActivateMenu();
        mainMenu.EnableMenuButtons();
        this.DeactivateMenu();
    }

    public void ActivateMenu(bool isLoadingGame) 
    {
        // set this menu to be active
        this.gameObject.SetActive(true);

        // set mode
        this.isLoadingGame = isLoadingGame;

        // load all of the profiles that exist
        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.instance.GetAllProfilesGameData();

        // ensure the back button is enabled when we activate the menu
        backButton.interactable = true;

        // loop through each save slot in the UI and set the content appropriately
        GameObject firstSelected = backButton.gameObject;
        foreach (SaveSlot saveSlot in saveSlots) 
        {
            GameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.GetProfileId(), out profileData);
            saveSlot.SetData(profileData);
            if (profileData == null && isLoadingGame) 
            {
                saveSlot.SetInteractable(false);
            }
            else 
            {
                saveSlot.SetInteractable(true);
                if (firstSelected.Equals(backButton.gameObject))
                {
                    firstSelected = saveSlot.gameObject;
                }
            }
        }

        // set the first selected button
        Button firstSelectedButton = firstSelected.GetComponent<Button>();
        this.SetFirstSelected(firstSelectedButton);
    }

    public void DeactivateMenu() 
    {
        this.gameObject.SetActive(false);
    }

    private void DisableMenuButtons() 
    {
        foreach (SaveSlot saveSlot in saveSlots) 
        {
            saveSlot.SetInteractable(false);
        }
        backButton.interactable = false;
    }
}
