using System;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScritchyGameplayUI : MonoBehaviour
{
    enum ShopTab
    {
        Tickets,
        Gadgets
    }

    [Serializable]
    public class TicketButtonBinding
    {
        public Button button;
        public int ticketIndex;
    }

    [Serializable]
    public class GadgetButtonBinding
    {
        public Button button;
        public int gadgetIndex;
        public TMP_Text nameText;
        public TMP_Text statusText;
        public TMP_Text costText;
    }

    [Header("References")]
    [SerializeField] ScritchyGameManager gameManager;

    [Header("Panels")]
    [SerializeField] GameObject hudPanel;
    [SerializeField] GameObject dayJobPanel;
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject ticketsTabContent;
    [SerializeField] GameObject gadgetsTabContent;
    [SerializeField] GameObject prestigePanel;
    [SerializeField] GameObject endingPanel;

    [Header("Shop Tabs")]
    [SerializeField] Button ticketsTabButton;
    [SerializeField] Button gadgetsTabButton;
    [SerializeField] Image ticketsTabHighlight;
    [SerializeField] Image gadgetsTabHighlight;
    [SerializeField] Color activeTabColor = Color.white;
    [SerializeField] Color inactiveTabColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("HUD Text")]
    [SerializeField] TMP_Text stateText;
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text jackPointsText;
    [SerializeField] TMP_Text ticketsText;
    [SerializeField] TMP_Text upgradesText;

    [Header("HUD Extra (Optional)")]
    [SerializeField] bool autoBindHudByName = true;
    [SerializeField] TMP_Text topMoneyText;
    [SerializeField] TMP_Text goalProgressText;
    [SerializeField] Slider goalProgressSlider;
    [SerializeField] TMP_Text dayJobInfoText;

    [Header("Action Buttons")]
    [SerializeField] Button dayJobButton;
    [SerializeField] Button openShopButton;
    [SerializeField] Button scratchAllButton;
    [SerializeField] Button buyLuckUpgradeButton;
    [SerializeField] Button buyScratchSpeedButton;
    [SerializeField] Button buyScratchBotButton;
    [SerializeField] Button prestigeRestartRunButton;

    [Header("Upgrade Cost Text")]
    [SerializeField] TMP_Text luckUpgradeCostText;
    [SerializeField] TMP_Text scratchSpeedUpgradeCostText;
    [SerializeField] TMP_Text scratchBotCostText;

    [Header("Perk Buttons")]
    [SerializeField] Button buyPerkStartingMoneyButton;
    [SerializeField] Button buyPerkDayJobIncomeButton;
    [SerializeField] Button buyPerkBaseLuckButton;

    [Header("Ticket Buttons")]
    [SerializeField] List<TicketButtonBinding> ticketButtons = new List<TicketButtonBinding>();

    [Header("Gadget Buttons")]
    [SerializeField] List<GadgetButtonBinding> gadgetButtons = new List<GadgetButtonBinding>();

    ShopTab currentTab = ShopTab.Tickets;

    void Awake()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<ScritchyGameManager>();

        if (autoBindHudByName)
            AutoBindHudFields();

        BindButtons();
    }

    void Start()
    {
        RefreshAll();
    }

    void Update()
    {
        RefreshAll();
    }

    void BindButtons()
    {
        if (ticketsTabButton != null) ticketsTabButton.onClick.AddListener(ShowTicketsTab);
        if (gadgetsTabButton != null) gadgetsTabButton.onClick.AddListener(ShowGadgetsTab);

        if (dayJobButton != null) dayJobButton.onClick.AddListener(() => gameManager?.DoDayJobAction());
        if (openShopButton != null) openShopButton.onClick.AddListener(() => gameManager?.OpenShop());
        if (scratchAllButton != null) scratchAllButton.onClick.AddListener(() => gameManager?.ScratchAllTickets());
        if (buyLuckUpgradeButton != null) buyLuckUpgradeButton.onClick.AddListener(() => gameManager?.BuyLuckUpgrade());
        if (buyScratchSpeedButton != null) buyScratchSpeedButton.onClick.AddListener(() => gameManager?.BuyScratchSpeedUpgrade());
        if (buyScratchBotButton != null) buyScratchBotButton.onClick.AddListener(() => gameManager?.BuyScratchBot());
        if (prestigeRestartRunButton != null) prestigeRestartRunButton.onClick.AddListener(() => gameManager?.PrestigeAndRestartRun());

        if (buyPerkStartingMoneyButton != null) buyPerkStartingMoneyButton.onClick.AddListener(() => gameManager?.BuyPerkStartingMoney());
        if (buyPerkDayJobIncomeButton != null) buyPerkDayJobIncomeButton.onClick.AddListener(() => gameManager?.BuyPerkDayJobIncome());
        if (buyPerkBaseLuckButton != null) buyPerkBaseLuckButton.onClick.AddListener(() => gameManager?.BuyPerkBaseLuck());

        for (int i = 0; i < ticketButtons.Count; i++)
        {
            TicketButtonBinding binding = ticketButtons[i];
            if (binding == null || binding.button == null) continue;

            int ticketIndex = binding.ticketIndex;
            binding.button.onClick.AddListener(() => gameManager?.BuyTicketByIndex(ticketIndex));
        }

        for (int i = 0; i < gadgetButtons.Count; i++)
        {
            GadgetButtonBinding binding = gadgetButtons[i];
            if (binding == null || binding.button == null) continue;

            int gadgetIndex = binding.gadgetIndex;
            binding.button.onClick.AddListener(() => gameManager?.BuyOrUpgradeGadgetByIndex(gadgetIndex));
        }
    }

    void ShowTicketsTab()
    {
        currentTab = ShopTab.Tickets;
    }

    void ShowGadgetsTab()
    {
        currentTab = ShopTab.Gadgets;
    }

    void RefreshAll()
    {
        if (gameManager == null) return;

        ScritchyGameManager.GameState state = gameManager.State;

        if (hudPanel != null) hudPanel.SetActive(true);
        if (dayJobPanel != null) dayJobPanel.SetActive(state == ScritchyGameManager.GameState.DAY_JOB);
        bool inShopFlow = state == ScritchyGameManager.GameState.SHOP || state == ScritchyGameManager.GameState.SCRATCHING || state == ScritchyGameManager.GameState.EVALUATE;
        if (shopPanel != null) shopPanel.SetActive(inShopFlow);
        if (prestigePanel != null) prestigePanel.SetActive(state == ScritchyGameManager.GameState.PRESTIGE);
        if (endingPanel != null) endingPanel.SetActive(state == ScritchyGameManager.GameState.ENDING);

        if (ticketsTabContent != null) ticketsTabContent.SetActive(inShopFlow && currentTab == ShopTab.Tickets);
        if (gadgetsTabContent != null) gadgetsTabContent.SetActive(inShopFlow && currentTab == ShopTab.Gadgets);

        if (ticketsTabHighlight != null) ticketsTabHighlight.color = currentTab == ShopTab.Tickets ? activeTabColor : inactiveTabColor;
        if (gadgetsTabHighlight != null) gadgetsTabHighlight.color = currentTab == ShopTab.Gadgets ? activeTabColor : inactiveTabColor;

        if (stateText != null) stateText.text = "State: " + state;
        if (moneyText != null) moneyText.text = "$ " + FormatNumber(gameManager.Money);
        if (topMoneyText != null) topMoneyText.text = "$ " + FormatNumber(gameManager.Money);
        if (jackPointsText != null) jackPointsText.text = "Jack Points: " + FormatNumber(gameManager.JackPoints);
        if (ticketsText != null) ticketsText.text = "Tickets: " + FormatNumber(gameManager.CurrentTicketCount);

        if (goalProgressText != null)
            goalProgressText.text = FormatNumber(gameManager.Money) + " / " + FormatNumber(gameManager.EndingTargetMoney);

        if (goalProgressSlider != null)
            goalProgressSlider.value = gameManager.GetEndingProgress01();

        if (dayJobInfoText != null)
            dayJobInfoText.text = "Day Job\n$" + FormatNumber(gameManager.GetCurrentDayJobIncome()) + "\nLv " + (gameManager.Perks.dayJobIncomeLevel + 1);

        if (upgradesText != null)
        {
            upgradesText.text =
                "Luck Lv: " + gameManager.CurrentLuckUpgradeLevel +
                " | Speed Lv: " + gameManager.CurrentScratchSpeedUpgradeLevel +
                " | Bot Lv: " + gameManager.CurrentScratchBotLevel;
        }

        if (luckUpgradeCostText != null)
            luckUpgradeCostText.text = "$" + FormatNumber(gameManager.GetLuckUpgradeCost()) + " (Lv " + gameManager.CurrentLuckUpgradeLevel + ")";

        if (scratchSpeedUpgradeCostText != null)
            scratchSpeedUpgradeCostText.text = "$" + FormatNumber(gameManager.GetScratchSpeedUpgradeCost()) + " (Lv " + gameManager.CurrentScratchSpeedUpgradeLevel + ")";

        if (scratchBotCostText != null)
            scratchBotCostText.text = "$" + FormatNumber(gameManager.GetScratchBotCost()) + " (Lv " + gameManager.CurrentScratchBotLevel + ")";

        if (dayJobButton != null) dayJobButton.interactable = state == ScritchyGameManager.GameState.DAY_JOB;
        if (openShopButton != null) openShopButton.interactable = state == ScritchyGameManager.GameState.DAY_JOB || state == ScritchyGameManager.GameState.EVALUATE;
        if (scratchAllButton != null) scratchAllButton.interactable = state == ScritchyGameManager.GameState.SHOP || state == ScritchyGameManager.GameState.SCRATCHING;

        bool inShop = state == ScritchyGameManager.GameState.SHOP;
        if (buyLuckUpgradeButton != null) buyLuckUpgradeButton.interactable = inShop;
        if (buyScratchSpeedButton != null) buyScratchSpeedButton.interactable = inShop;
        if (buyScratchBotButton != null) buyScratchBotButton.interactable = inShop;

        bool inPrestige = state == ScritchyGameManager.GameState.PRESTIGE;
        if (buyPerkStartingMoneyButton != null) buyPerkStartingMoneyButton.interactable = inPrestige;
        if (buyPerkDayJobIncomeButton != null) buyPerkDayJobIncomeButton.interactable = inPrestige;
        if (buyPerkBaseLuckButton != null) buyPerkBaseLuckButton.interactable = inPrestige;
        if (prestigeRestartRunButton != null) prestigeRestartRunButton.interactable = inPrestige;

        RefreshGadgets(inShop);
    }

    void RefreshGadgets(bool inShop)
    {
        if (gameManager == null || gadgetButtons == null) return;

        for (int i = 0; i < gadgetButtons.Count; i++)
        {
            GadgetButtonBinding binding = gadgetButtons[i];
            if (binding == null) continue;

            int index = binding.gadgetIndex;
            bool unlocked = gameManager.IsGadgetUnlocked(index);
            bool owned = gameManager.IsGadgetOwned(index);
            bool maxed = gameManager.IsGadgetMaxLevel(index);
            int level = gameManager.GetGadgetLevel(index);
            int maxLevel = gameManager.GetGadgetMaxLevel(index);

            if (binding.nameText != null)
                binding.nameText.text = gameManager.GetGadgetId(index);

            if (binding.costText != null)
                binding.costText.text = maxed ? "MAX" : ("$" + FormatNumber(gameManager.GetGadgetUpgradeCost(index)));

            if (binding.statusText != null)
            {
                if (!unlocked)
                    binding.statusText.text = "Locked ($" + FormatNumber(gameManager.GetGadgetUnlockMoney(index)) + ")";
                else if (!owned)
                    binding.statusText.text = "Buy";
                else
                    binding.statusText.text = "Lv " + level + "/" + maxLevel;
            }

            if (binding.button != null)
            {
                bool canAfford = gameManager.Money >= gameManager.GetGadgetUpgradeCost(index);
                binding.button.interactable = inShop && unlocked && !maxed && canAfford;
            }
        }
    }

    string FormatNumber(int value)
    {
        return value.ToString("N0");
    }

    void AutoBindHudFields()
    {
        if (moneyText == null) moneyText = FindTmpByName("MoneyText");
        if (topMoneyText == null) topMoneyText = FindTmpByName("TopMoneyText");
        if (goalProgressText == null) goalProgressText = FindTmpByName("GoalProgressText");
        if (dayJobInfoText == null) dayJobInfoText = FindTmpByName("DayJobInfoText");
        if (goalProgressSlider == null) goalProgressSlider = FindSliderByName("GoalProgressSlider");
    }

    TMP_Text FindTmpByName(string objectName)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name == objectName)
                return texts[i];
        }

        return null;
    }

    Slider FindSliderByName(string objectName)
    {
        Slider[] sliders = GetComponentsInChildren<Slider>(true);
        for (int i = 0; i < sliders.Length; i++)
        {
            if (sliders[i] != null && sliders[i].name == objectName)
                return sliders[i];
        }

        return null;
    }
}
