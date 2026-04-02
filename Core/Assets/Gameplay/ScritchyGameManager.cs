using System;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScritchyGameManager : MonoBehaviour
{
    public enum GameState
    {
        START_RUN,
        DAY_JOB,
        SHOP,
        SCRATCHING,
        EVALUATE,
        PRESTIGE,
        ENDING
    }

    [Serializable]
    public class TicketDefinition
    {
        public string id = "Basic";
        public int price = 10;
        public int minReward = 0;
        public int maxReward = 20;
        [Range(0f, 1f)] public float jackpotChance = 0.02f;
        public int jackpotReward = 100;
        [Range(0f, 1f)] public float bustChance;
        public int bustPenalty;
    }

    [Serializable]
    public class PerkProgress
    {
        public int startingMoneyLevel;
        public int dayJobIncomeLevel;
        public int baseLuckLevel;
    }

    [Serializable]
    public class GadgetDefinition
    {
        public string id = "Gadget";
        public int unlockAtMoney = 0;
        public int baseCost = 100;
        [Min(1)] public int maxLevel = 1;
        [Range(1f, 5f)] public float costMultiplier = 1.5f;
        public bool startsOwned;
    }

    [Serializable] public class IntEvent : UnityEvent<int> { }
    [Serializable] public class StateEvent : UnityEvent<GameState> { }

    [Header("Run Config")]
    [SerializeField] int baseStartingMoney = 1;
    [SerializeField] int baseDayJobIncome = 1;
    [SerializeField] float baseLuck = 0f;
    [SerializeField] int endingTargetMoney = 1000000;
    [SerializeField] int minMoneyToOpenShop = 10;

    [Header("Upgrades / Automation")]
    [SerializeField] int luckUpgradeBaseCost = 30;
    [SerializeField] float luckPerUpgrade = 0.01f;
    [SerializeField] int scratchSpeedUpgradeBaseCost = 40;
    [SerializeField] float scratchSpeedPerUpgrade = 0.1f;
    [SerializeField] int scratchBotBaseCost = 300;
    [SerializeField] float baseScratchInterval = 2f;

    [Header("Prestige")]
    [SerializeField] int prestigeDivisor = 100;
    [SerializeField] int perkCostStartingMoney = 1;
    [SerializeField] int perkCostDayJobIncome = 1;
    [SerializeField] int perkCostBaseLuck = 1;
    [SerializeField] int perkStepStartingMoney = 1;
    [SerializeField] int perkStepDayJobIncome = 1;
    [SerializeField] float perkStepBaseLuck = 0.01f;

    [Header("Tickets")]
    [SerializeField] List<TicketDefinition> ticketCatalog = new List<TicketDefinition>();

    [Header("Gadgets")]
    [SerializeField] List<GadgetDefinition> gadgetCatalog = new List<GadgetDefinition>();

    [Header("Events")]
    [SerializeField] IntEvent onMoneyChanged;
    [SerializeField] IntEvent onJackPointsChanged;
    [SerializeField] StateEvent onStateChanged;

    public GameState State { get; private set; }
    public int Money { get; private set; }
    public int JackPoints { get; private set; }
    public int CurrentLuckUpgradeLevel { get; private set; }
    public int CurrentScratchSpeedUpgradeLevel { get; private set; }
    public int CurrentScratchBotLevel { get; private set; }
    public int CurrentTicketCount => ticketInventory.Count;
    public int GadgetCount => gadgetCatalog.Count;
    public int EndingTargetMoney => endingTargetMoney;

    public PerkProgress Perks = new PerkProgress();

    readonly List<TicketDefinition> ticketInventory = new List<TicketDefinition>();
    readonly List<int> gadgetLevels = new List<int>();
    readonly List<bool> gadgetOwned = new List<bool>();
    float botTimer;

    void Start()
    {
        ChangeState(GameState.START_RUN);
        StartRun();
    }

    void Update()
    {
        if (State != GameState.SHOP || CurrentScratchBotLevel <= 0 || ticketInventory.Count <= 0)
            return;

        botTimer += Time.deltaTime;
        if (botTimer >= GetCurrentScratchInterval())
        {
            botTimer = 0f;
            ScratchAllTickets();
        }
    }

    public void StartRun()
    {
        EnsureDefaultGadgets();
        InitializeGadgets();

        Money = Mathf.Max(0, baseStartingMoney + Perks.startingMoneyLevel * perkStepStartingMoney);
        CurrentLuckUpgradeLevel = 0;
        CurrentScratchSpeedUpgradeLevel = 0;
        CurrentScratchBotLevel = 0;
        ticketInventory.Clear();
        botTimer = 0f;

        BroadcastMoney();

        if (Money >= minMoneyToOpenShop)
            ChangeState(GameState.SHOP);
        else
            ChangeState(GameState.DAY_JOB);
    }

    public void DoDayJobAction()
    {
        if (State != GameState.DAY_JOB) return;

        int earned = GetCurrentDayJobIncome();
        Money += Mathf.Max(1, earned);
        BroadcastMoney();

        if (Money >= minMoneyToOpenShop)
            ChangeState(GameState.SHOP);
    }

    public void OpenShop()
    {
        if (State == GameState.DAY_JOB || State == GameState.EVALUATE)
            ChangeState(GameState.SHOP);
    }

    public bool BuyTicketByIndex(int index)
    {
        if (State != GameState.SHOP) return false;
        if (index < 0 || index >= ticketCatalog.Count) return false;

        TicketDefinition ticket = ticketCatalog[index];
        if (ticket == null || Money < ticket.price) return false;

        Money -= ticket.price;
        ticketInventory.Add(ticket);
        BroadcastMoney();
        ChangeState(GameState.SCRATCHING);
        return true;
    }

    public void ScratchAllTickets()
    {
        if (State != GameState.SCRATCHING && State != GameState.SHOP) return;
        if (ticketInventory.Count == 0)
        {
            ChangeState(GameState.EVALUATE);
            EvaluateRunState();
            return;
        }

        ChangeState(GameState.SCRATCHING);

        float luckBonus = baseLuck + Perks.baseLuckLevel * perkStepBaseLuck + CurrentLuckUpgradeLevel * luckPerUpgrade;
        for (int i = 0; i < ticketInventory.Count; i++)
        {
            TicketDefinition ticket = ticketInventory[i];
            int reward = UnityEngine.Random.Range(ticket.minReward, ticket.maxReward + 1);

            float jackpotChance = Mathf.Clamp01(ticket.jackpotChance + luckBonus);
            if (UnityEngine.Random.value <= jackpotChance)
                reward += Mathf.Max(0, ticket.jackpotReward);

            if (ticket.bustPenalty > 0 && UnityEngine.Random.value <= Mathf.Clamp01(ticket.bustChance))
                reward -= ticket.bustPenalty;

            Money += reward;
        }

        ticketInventory.Clear();
        BroadcastMoney();

        ChangeState(GameState.EVALUATE);
        EvaluateRunState();
    }

    public void EvaluateRunState()
    {
        if (TryTriggerEnding())
            return;

        if (Money <= 0)
        {
            ChangeState(GameState.PRESTIGE);
            return;
        }

        if (Money < minMoneyToOpenShop)
            ChangeState(GameState.DAY_JOB);
        else
            ChangeState(GameState.SHOP);
    }

    public void PrestigeAndRestartRun()
    {
        if (State != GameState.PRESTIGE) return;

        int gained = Mathf.Max(1, Mathf.Abs(Money) / Mathf.Max(1, prestigeDivisor));
        JackPoints += gained;
        onJackPointsChanged.Invoke(JackPoints);

        ChangeState(GameState.START_RUN);
        StartRun();
    }

    public bool BuyPerkStartingMoney()
    {
        if (JackPoints < perkCostStartingMoney) return false;
        JackPoints -= perkCostStartingMoney;
        Perks.startingMoneyLevel++;
        onJackPointsChanged.Invoke(JackPoints);
        return true;
    }

    public bool BuyPerkDayJobIncome()
    {
        if (JackPoints < perkCostDayJobIncome) return false;
        JackPoints -= perkCostDayJobIncome;
        Perks.dayJobIncomeLevel++;
        onJackPointsChanged.Invoke(JackPoints);
        return true;
    }

    public bool BuyPerkBaseLuck()
    {
        if (JackPoints < perkCostBaseLuck) return false;
        JackPoints -= perkCostBaseLuck;
        Perks.baseLuckLevel++;
        onJackPointsChanged.Invoke(JackPoints);
        return true;
    }

    public bool BuyLuckUpgrade()
    {
        if (State != GameState.SHOP) return false;
        int cost = GetLuckUpgradeCost();
        if (Money < cost) return false;
        Money -= cost;
        CurrentLuckUpgradeLevel++;
        BroadcastMoney();
        return true;
    }

    public bool BuyScratchSpeedUpgrade()
    {
        if (State != GameState.SHOP) return false;
        int cost = GetScratchSpeedUpgradeCost();
        if (Money < cost) return false;
        Money -= cost;
        CurrentScratchSpeedUpgradeLevel++;
        BroadcastMoney();
        return true;
    }

    public bool BuyScratchBot()
    {
        if (State != GameState.SHOP) return false;
        int cost = GetScratchBotCost();
        if (Money < cost) return false;
        Money -= cost;
        CurrentScratchBotLevel++;
        BroadcastMoney();
        return true;
    }

    public int GetLuckUpgradeCost()
    {
        return luckUpgradeBaseCost * (CurrentLuckUpgradeLevel + 1);
    }

    public int GetScratchSpeedUpgradeCost()
    {
        return scratchSpeedUpgradeBaseCost * (CurrentScratchSpeedUpgradeLevel + 1);
    }

    public int GetScratchBotCost()
    {
        return scratchBotBaseCost * (CurrentScratchBotLevel + 1);
    }

    public int GetCurrentDayJobIncome()
    {
        return Mathf.Max(1, baseDayJobIncome + Perks.dayJobIncomeLevel * perkStepDayJobIncome);
    }

    public float GetEndingProgress01()
    {
        return Mathf.Clamp01((float)Money / Mathf.Max(1, endingTargetMoney));
    }

    public bool BuyOrUpgradeGadgetByIndex(int index)
    {
        if (State != GameState.SHOP) return false;
        if (!IsValidGadgetIndex(index)) return false;
        if (!IsGadgetUnlocked(index)) return false;
        if (IsGadgetMaxLevel(index)) return false;

        int cost = GetGadgetUpgradeCost(index);
        if (Money < cost) return false;

        Money -= cost;
        if (!gadgetOwned[index])
        {
            gadgetOwned[index] = true;
            gadgetLevels[index] = Mathf.Max(1, gadgetLevels[index]);
        }
        else
        {
            gadgetLevels[index]++;
        }

        BroadcastMoney();
        return true;
    }

    public bool IsGadgetUnlocked(int index)
    {
        if (!IsValidGadgetIndex(index)) return false;

        if (gadgetOwned[index]) return true;
        return Money >= Mathf.Max(0, gadgetCatalog[index].unlockAtMoney);
    }

    public bool IsGadgetOwned(int index)
    {
        if (!IsValidGadgetIndex(index)) return false;
        return gadgetOwned[index];
    }

    public int GetGadgetLevel(int index)
    {
        if (!IsValidGadgetIndex(index)) return 0;
        return gadgetLevels[index];
    }

    public int GetGadgetMaxLevel(int index)
    {
        if (!IsValidGadgetIndex(index)) return 1;
        return Mathf.Max(1, gadgetCatalog[index].maxLevel);
    }

    public bool IsGadgetMaxLevel(int index)
    {
        if (!IsValidGadgetIndex(index)) return true;
        return gadgetOwned[index] && gadgetLevels[index] >= GetGadgetMaxLevel(index);
    }

    public int GetGadgetUpgradeCost(int index)
    {
        if (!IsValidGadgetIndex(index)) return int.MaxValue;

        GadgetDefinition gadget = gadgetCatalog[index];
        int levelForCost = Mathf.Max(0, gadgetLevels[index]);
        float scaled = gadget.baseCost * Mathf.Pow(gadget.costMultiplier, levelForCost);
        return Mathf.Max(1, Mathf.RoundToInt(scaled));
    }

    public string GetGadgetId(int index)
    {
        if (!IsValidGadgetIndex(index)) return string.Empty;
        return gadgetCatalog[index].id;
    }

    bool TryTriggerEnding()
    {
        if (Money >= endingTargetMoney)
        {
            ChangeState(GameState.ENDING);
            return true;
        }

        return false;
    }

    float GetCurrentScratchInterval()
    {
        float speedBonus = CurrentScratchSpeedUpgradeLevel * scratchSpeedPerUpgrade;
        return Mathf.Max(0.15f, baseScratchInterval - speedBonus);
    }

    void BroadcastMoney()
    {
        onMoneyChanged.Invoke(Money);
    }

    void InitializeGadgets()
    {
        gadgetLevels.Clear();
        gadgetOwned.Clear();

        for (int i = 0; i < gadgetCatalog.Count; i++)
        {
            GadgetDefinition gadget = gadgetCatalog[i];
            bool startsOwned = gadget != null && gadget.startsOwned;

            gadgetOwned.Add(startsOwned);
            gadgetLevels.Add(startsOwned ? 1 : 0);
        }
    }

    bool IsValidGadgetIndex(int index)
    {
        return index >= 0 && index < gadgetCatalog.Count &&
               index < gadgetOwned.Count && index < gadgetLevels.Count;
    }

    public int GetGadgetUnlockMoney(int index)
    {
        if (!IsValidGadgetIndex(index)) return 0;
        return Mathf.Max(0, gadgetCatalog[index].unlockAtMoney);
    }

    void EnsureDefaultGadgets()
    {
        if (gadgetCatalog != null && gadgetCatalog.Count > 0) return;

        if (gadgetCatalog == null)
            gadgetCatalog = new List<GadgetDefinition>();

        gadgetCatalog.Add(new GadgetDefinition
        {
            id = "Trash Can",
            unlockAtMoney = 0,
            baseCost = 120,
            maxLevel = 1,
            costMultiplier = 1.5f,
            startsOwned = false
        });

        gadgetCatalog.Add(new GadgetDefinition
        {
            id = "Scratch Bot",
            unlockAtMoney = 50,
            baseCost = 250,
            maxLevel = 6,
            costMultiplier = 1.45f,
            startsOwned = false
        });

        gadgetCatalog.Add(new GadgetDefinition
        {
            id = "Fan",
            unlockAtMoney = 120,
            baseCost = 200,
            maxLevel = 4,
            costMultiplier = 1.4f,
            startsOwned = false
        });

        gadgetCatalog.Add(new GadgetDefinition
        {
            id = "Mystery Gadget",
            unlockAtMoney = 400,
            baseCost = 600,
            maxLevel = 3,
            costMultiplier = 1.6f,
            startsOwned = false
        });
    }

    void ChangeState(GameState newState)
    {
        State = newState;
        onStateChanged.Invoke(State);
    }
}
