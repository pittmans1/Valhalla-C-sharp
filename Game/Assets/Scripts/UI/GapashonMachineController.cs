using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GashaponMachineController : MonoBehaviour
{
    public static GashaponMachineController Instance { get; private set; }

    [Header("UI Pop-Up Referencing Components")]
    [SerializeField] private GameObject rewardPopUpPanel;
    [SerializeField] private Image rewardIconDisplay;
    [SerializeField] private TextMeshProUGUI rewardNameText;

    [Header("Interactive Core Controls")]
    [SerializeField] private Button crankLeverButton;
    [SerializeField] private Button closeRewardButton;

    [Header("Content Databases (Drop Pools)")]
    [SerializeField] private List<CatProfileData> unlockableCatBreeds = new List<CatProfileData>();
    [SerializeField] private List<Sprite> unlockableHumanOutfitsPool = new List<Sprite>();

    [Header("Audio Tracks")]
    [SerializeField] private AudioClip leverCrankSound;
    [SerializeField] private AudioClip capsulePopOpenSound;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Setup initial display configurations safely
        if (rewardPopUpPanel != null) rewardPopUpPanel.SetActive(false);

        // Bind interactive button click execution loops cleanly
        if (crankLeverButton != null) crankLeverButton.onClick.AddListener(OnCrankLeverPressed);
        if (closeRewardButton != null) closeRewardButton.onClick.AddListener(CloseRewardWindow);
    }

    /// <summary>
    /// Handles rolling for random unlocked content tracking matrices.
    /// </summary>
    public void OnCrankLeverPressed()
    {
        // 1. Play physics lever sounds
        if (AudioManagerHub.Instance != null && leverCrankSound != null)
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(leverCrankSound, Vector3.zero);

        crankLeverButton.interactable = false; // Lock out rapid multi-clicking exploit states
        StartCoroutine(ExecuteCapsuleRollingSequenceAnimation());
    }

    private IEnumerator ExecuteCapsuleRollingSequenceAnimation()
    {
        // Pacing delay window for machine mechanics
        yield return new WaitForSeconds(1.2f);

        if (AudioManagerHub.Instance != null && capsulePopOpenSound != null)
            AudioManagerHub.Instance.PlaySpatialExplosiveSFX(capsulePopOpenSound, Vector3.zero);

        bool isRollingForCat = Random.Range(0, 2) == 0;

        if (isRollingForCat && unlockableCatBreeds.Count > 0)
        {
            int randIndex = Random.Range(0, unlockableCatBreeds.Count);
            CatProfileData wonCat = unlockableCatBreeds[randIndex];

            // DYNAMIC INJECTION: Hot-swaps your text token frame directly with the won profile data string
            if (rewardNameText != null) 
            {
                rewardNameText.text = $"UNLOCKED\n[ {wonCat.displayBreedName.ToUpper()} ]";
            }
            
            if (rewardIconDisplay != null) rewardIconDisplay.sprite = wonCat.characterSelectionIcon;
        }
        else if (unlockableHumanOutfitsPool.Count > 0)
        {
            int randIndex = Random.Range(0, unlockableHumanOutfitsPool.Count);
            Sprite wonOutfit = unlockableHumanOutfitsPool[randIndex];

            if (rewardNameText != null) 
            {
                rewardNameText.text = "UNLOCKED\n[ NEW OUTFIT SKIN ]";
            }
            
            if (rewardIconDisplay != null) rewardIconDisplay.sprite = wonOutfit;
        }

        if (rewardPopUpPanel != null) rewardPopUpPanel.SetActive(true);
    }


    private void CloseRewardWindow()
    {
        if (rewardPopUpPanel != null) rewardPopUpPanel.SetActive(false);
        crankLeverButton.interactable = true; // Safe release click lockout constraints
    }

    private void OnDestroy()
    {
        if (crankLeverButton != null) crankLeverButton.onClick.RemoveListener(OnCrankLeverPressed);
        if (closeRewardButton != null) closeRewardButton.onClick.RemoveListener(CloseRewardWindow);
    }
}
