using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Quest
{
    public QUEST_TYPE questType;
    public List<int> amount;
    public List<int> reward;
    public QuestStatus status;
    public Quest(QUEST_TYPE questType, List<int> amount, List<int> reward)
    {
        this.questType = questType;
        this.amount = amount;
        this.reward = reward;
        //status = new QuestStatus(reward.Count);
    }
}
//[Serializable]
//public class QuestList
//{
//    public List<Quest> questList;
//    public QuestList(List<Quest> questList)
//    {
//        this.questList = questList;
//    }
//}
[Serializable]
public class QuestStatus
{
    public int progress;
    public List<bool> status;
    public QuestStatus(int rewardCount)
    {
        progress = 0;
        status = new List<bool>();
        for (int i = 0; i < rewardCount; i++)
            status.Add(false);
    }
    public QuestStatus(int progress, List<bool> status)
    {
        this.progress = progress;
        this.status = status;
    }
}
//[Serializable]
//public class QuestStatusList
//{
//    public List<QuestStatus> questStatusList;
//}
public enum QUEST_TYPE
{
    COMPLETE_X_LEVELS,
    MATCH_X_TILES,
    USE_X_UNDOS,
    USE_X_HINT,
    USE_X_SHUFFLE,
    //USE_X_ADDSLOT,
    CLAIM_X_DAILY,
    WATCH_X_ADS,
    TOP_DAILY_RANK,
}
public class QuestManager : MonoBehaviour
{
    public static QuestManager instance;
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
            Destroy(gameObject);
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public bool isInit;

    public static List<Quest> allTimeQuests = new()
    {
        new(QUEST_TYPE.COMPLETE_X_LEVELS,
            new(){ 10,25,50,100,150,200,300,400,500,600,700,800,900,1000 },
            new(){ 25,50,75,100,100,100,100,100,100,100,100,100,100,100 }),
        new(QUEST_TYPE.MATCH_X_TILES,
            new(){ 100,300,500,1000,2000,3000,4000,5000 },
            new(){ 25, 50, 75, 100, 100, 100, 100, 100 }),
        new(QUEST_TYPE.CLAIM_X_DAILY,
            new(){ 3,  5,  7,  10, 15, 20, 25, 30, 40, 50 },
            new(){ 100,100,100,100,100,100,100,100,100,100 }),
        new(QUEST_TYPE.USE_X_UNDOS,
            new(){ 5, 10,15,20, 30, 40, 50, 60, 70, 80, 90, 100 },
            new(){ 25,50,75,100,100,100,100,100,100,100,100,100 }),
        new(QUEST_TYPE.USE_X_HINT,
            new(){ 5, 10,15,20, 30, 40, 50, 60, 70, 80, 90, 100 },
            new(){ 25,50,75,100,100,100,100,100,100,100,100,100 }),
        new(QUEST_TYPE.USE_X_SHUFFLE,
            new(){ 5, 10,15,20, 30, 40, 50, 60, 70, 80, 90, 100 },
            new(){ 25,50,75,100,100,100,100,100,100,100,100,100 }),
    };
    public static List<Quest> dailyQuests = new()
    {
        new(QUEST_TYPE.COMPLETE_X_LEVELS, new(){3}, new(){30}),
        new(QUEST_TYPE.MATCH_X_TILES, new(){150}, new(){30}),
        new(QUEST_TYPE.CLAIM_X_DAILY, new(){1}, new(){20}),
        new(QUEST_TYPE.WATCH_X_ADS, new(){1}, new(){50}),
        new(QUEST_TYPE.USE_X_UNDOS, new(){2}, new(){30}),
        new(QUEST_TYPE.USE_X_HINT, new(){2}, new(){30}),
        new(QUEST_TYPE.USE_X_SHUFFLE, new(){2}, new(){30}),
    };
    //private void Start()
    //{
    //    Init();
    //}
    public void Init()
    {
        GetAllTimeQuestStatus();
        GetDailyQuestStatus();
        isInit = true;
        if (HomeScript.instance)
            HomeScript.instance.QuestNotif();
    }

    //"0_00000000000000|0_00000000|0_000000000000|3_000000000000|3_000000000000|0_0000000000"
    //format: progress_status|progress_status|progress_status|progress_status|progress_status
    const string allTimeQuestKey = "quest_all_time";
    const string dailyQuestKey = "quest_daily";
    public static void GetAllTimeQuestStatus()
    {
    NoKey:
        if (PlayerPrefs.HasKey(allTimeQuestKey))
        {
            try
            {
                string key = PlayerPrefs.GetString(allTimeQuestKey);
                string[] statuses = key.Split("|");
                for (int i = 0; i < allTimeQuests.Count; i++)
                {
                    if (i >= statuses.Length)
                    {
                        allTimeQuests[i].status = new QuestStatus(allTimeQuests[i].reward.Count);
                        continue;
                    }
                    string[] value = statuses[i].Split("_");
                    int progress = int.Parse(value[0]);
                    List<bool> status = new();
                    foreach (char c in value[1])
                        status.Add(c != '0');
                    if (status.Count < allTimeQuests[i].reward.Count)
                        for (int j = 0; j < allTimeQuests[i].reward.Count - status.Count; j++)
                            status.Add(false);
                    allTimeQuests[i].status = new QuestStatus(progress, status);
                }
            }
            catch
            {
                PlayerPrefs.DeleteKey(allTimeQuestKey);
                goto NoKey;
            }
        }
        else
        {
            foreach (var quest in allTimeQuests)
            {
                quest.status = new QuestStatus(quest.reward.Count);
            }
        }
    }
    public static void SetAllTimeQuestStatus()
    {
        List<string> statuses = new();
        foreach (Quest quest in allTimeQuests)
        {
            string key = "";
            foreach (bool index in quest.status.status)
                key += index ? '1' : '0';
            statuses.Add(quest.status.progress + "_" + key);
        }
        PlayerPrefs.SetString(allTimeQuestKey, string.Join("|", statuses));
        if (HomeScript.instance)
            HomeScript.instance.QuestNotif();
    }

    public static void GetDailyQuestStatus()
    {
    NoKey:
        if (PlayerPrefs.HasKey(dailyQuestKey))
        {
            try
            {
                string key = PlayerPrefs.GetString(dailyQuestKey);
                string[] statuses = key.Split("|");
                for (int i = 0; i < dailyQuests.Count; i++)
                {
                    if (i >= statuses.Length)
                    {
                        dailyQuests[i].status = new QuestStatus(dailyQuests[i].reward.Count);
                        continue;
                    }
                    string[] value = statuses[i].Split("_");
                    int progress = int.Parse(value[0]);
                    List<bool> status = new();
                    foreach (char c in value[1])
                        status.Add(c != '0');
                    if (status.Count < dailyQuests[i].reward.Count)
                        for (int j = 0; j < dailyQuests[i].reward.Count - status.Count; j++)
                            status.Add(false);
                    dailyQuests[i].status = new QuestStatus(progress, status);
                }
            }
            catch
            {
                PlayerPrefs.DeleteKey(dailyQuestKey);
                goto NoKey;
            }
        }
        else
        {
            foreach (var quest in dailyQuests)
            {
                quest.status = new QuestStatus(quest.reward.Count);
            }
        }
    }
    public static void SetDailyQuestStatus()
    {
        List<string> statuses = new();
        foreach (Quest quest in dailyQuests)
        {
            string key = "";
            foreach (bool index in quest.status.status)
                key += index ? '1' : '0';
            statuses.Add(quest.status.progress + "_" + key);
        }
        PlayerPrefs.SetString(dailyQuestKey, string.Join("|", statuses));
        if (HomeScript.instance)
            HomeScript.instance.QuestNotif();
    }
    public static void DailyReset()
    {
        PlayerPrefs.DeleteKey(dailyQuestKey);
        if (instance.isInit)
            GetDailyQuestStatus();
    }
    public static void QuestComplete(QUEST_TYPE questType, int amount)
    {
        if (allTimeQuests.Exists(x => x.questType == questType))
        {
            allTimeQuests.Find(x => x.questType == questType).status.progress += amount;
            SetAllTimeQuestStatus();
        }
        if (dailyQuests.Exists(x => x.questType == questType))
        {
            dailyQuests.Find(x => x.questType == questType).status.progress += amount;
            SetDailyQuestStatus();
        }
    }
    public static void QuestProgressSet(QUEST_TYPE questType, int progress)
    {
        if (allTimeQuests.Exists(x => x.questType == questType))
        {
            allTimeQuests.Find(x => x.questType == questType).status.progress = progress;
            SetAllTimeQuestStatus();
        }
        if (dailyQuests.Exists(x => x.questType == questType))
        {
            dailyQuests.Find(x => x.questType == questType).status.progress = progress;
            SetDailyQuestStatus();
        }
    }
    public static bool GetAllTimeClaimAble()
    {
        for (int i = 0; i < allTimeQuests.Count; i++)
        {
            for (int j = 0; j < allTimeQuests[i].amount.Count; j++)
            {
                if (allTimeQuests[i].questType != QUEST_TYPE.TOP_DAILY_RANK)
                {
                    if (!allTimeQuests[i].status.status[j] && allTimeQuests[i].amount[j] <= allTimeQuests[i].status.progress)
                        return true;
                }
                else
                {
                    if (!allTimeQuests[i].status.status[j] && allTimeQuests[i].status.progress > 0 && allTimeQuests[i].amount[j] >= allTimeQuests[i].status.progress)
                        return true;
                }
            }
        }
        return false;
    }
    public static bool GetDailyClaimAble()
    {
        for (int i = 0; i < dailyQuests.Count; i++)
        {
            //Debug.Log(dailyQuests[i].status.status.Count);
            for (int j = 0; j < dailyQuests[i].amount.Count; j++)
            {
                if (dailyQuests[i].questType != QUEST_TYPE.TOP_DAILY_RANK)
                {
                    if (!dailyQuests[i].status.status[j] && dailyQuests[i].amount[j] <= dailyQuests[i].status.progress)
                        return true;
                }
                else
                {
                    if (!dailyQuests[i].status.status[j] && dailyQuests[i].status.progress > 0 && dailyQuests[i].amount[j] >= dailyQuests[i].status.progress)
                        return true;
                }
            }
        }
        return false;
    }
}
