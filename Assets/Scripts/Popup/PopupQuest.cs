using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupQuest : Popup
{
    public Button btClose;
    [Header("AllTime")]
    public Toggle toggleAllTime;
    public TextMeshProUGUI txAllTime;
    public GameObject allTimeGO;
    public Transform allTimeQuestParent;
    public GameObject allTimeNotif;
    [Header("Daily")]
    public Toggle toggleDaily;
    public TextMeshProUGUI txDaily;
    public GameObject dailyGO;
    public Transform dailyQuestParent;
    public GameObject dailyNotif;
    [Space]
    public Sprite[] buttonSprites;
    public QuestObject questObject;
    public List<QuestObject> questObjectList;

    public void Notif()
    {
        dailyNotif.SetActive(QuestManager.GetDailyClaimAble());
        allTimeNotif.SetActive(QuestManager.GetAllTimeClaimAble());
    }
    public override void SetupTweenCallback()
    {
        SetupTweenCallback(() =>
        {
            btClose.onClick.AddListener(() => Close());
            toggleDaily.onValueChanged.AddListener((value) =>
            {
                if (value)
                    ChangeTab();
            });
            toggleAllTime.onValueChanged.AddListener((value) =>
            {
                if (value)
                    ChangeTab();
            });
            toggleAllTime.interactable = true;
            toggleDaily.interactable = true;
            //btAllTime.onClick.AddListener(() => AllTimeTab());
            //btDaily.onClick.AddListener(() => DailyTab());
        });
    }
    void ChangeTab()
    {
        bool daily = toggleDaily.isOn;
        bool allTime = toggleAllTime.isOn;
        dailyGO.SetActive(daily);
        allTimeGO.SetActive(allTime);
        txDaily.color = daily ? Color.white : new Color(1, 1, 1, 0.5f);
        txAllTime.color = allTime ? Color.white : new Color(1, 1, 1, 0.5f);
    }
    //void AllTimeTab()
    //{
    //    allTimeGO.SetActive(true);
    //    btAllTime.image.color = Color.white;
    //    btAllTime.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
    //    //btAllTime.image.sprite = buttonSprites[0];

    //    dailyGO.SetActive(false);
    //    btDaily.image.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    //    btDaily.GetComponentInChildren<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.5f);
    //    //btDaily.image.sprite = buttonSprites[1];
    //}
    //void DailyTab()
    //{
    //    dailyGO.SetActive(true);
    //    btDaily.image.color = Color.white;
    //    btDaily.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
    //    //btDaily.image.sprite = buttonSprites[0];

    //    allTimeGO.SetActive(false);
    //    btAllTime.image.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    //    btAllTime.GetComponentInChildren<TextMeshProUGUI>().color = new Color(1, 1, 1, 0.5f);
    //    //btAllTime.image.sprite = buttonSprites[1];
    //}

    public override void Open()
    {
        //fadeIgnore = new();
        Notif();
        btClose.onClick.RemoveAllListeners();
        toggleAllTime.interactable = false;
        toggleDaily.interactable = false;
        toggleAllTime.onValueChanged.RemoveAllListeners();
        toggleDaily.onValueChanged.RemoveAllListeners();
        questObjectList = new();
        allTimeQuestParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        dailyQuestParent.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        QuestSetup(QuestManager.allTimeQuests, allTimeQuestParent, 0);
        QuestSetup(QuestManager.dailyQuests, dailyQuestParent, 1);

        // Reset cả 2 toggle trước
        toggleDaily.SetIsOnWithoutNotify(false);
        toggleAllTime.SetIsOnWithoutNotify(false);

        // Rồi mới set đúng tab
        if (QuestManager.GetDailyClaimAble())
            toggleDaily.SetIsOnWithoutNotify(true);
        else if (QuestManager.GetAllTimeClaimAble())
            toggleAllTime.SetIsOnWithoutNotify(true);
        else
            toggleDaily.SetIsOnWithoutNotify(true);
        ChangeTab();

        base.Open();
    }
    void QuestSetup(List<Quest> questList, Transform parent, int questTimer)
    {
        List<QuestObject> list = new();
        for (int i = 0; i < questList.Count; i++)
        {
            for (int questIndex = 0; questIndex < questList[i].amount.Count; questIndex++)
            {
                string title = questList[i].questType switch
                {
                    QUEST_TYPE.COMPLETE_X_LEVELS => "Complete " + questList[i].amount[questIndex] + " levels",
                    QUEST_TYPE.MATCH_X_TILES => "Match " + questList[i].amount[questIndex] + " tiles",
                    QUEST_TYPE.USE_X_UNDOS => "Use undo " + questList[i].amount[questIndex] + " times",
                    QUEST_TYPE.USE_X_HINT => "Use hint " + questList[i].amount[questIndex] + " times",
                    QUEST_TYPE.USE_X_SHUFFLE => "Use shuffle " + questList[i].amount[questIndex] + " times",
                    QUEST_TYPE.CLAIM_X_DAILY => "Claim daily " + questList[i].amount[questIndex] + " times",
                    QUEST_TYPE.WATCH_X_ADS => "Watch rewarded ads " + questList[i].amount[questIndex] + " times",
                    QUEST_TYPE.TOP_DAILY_RANK => "Reach top " + questList[i].amount[questIndex] + " challenge",
                    _ => ""
                };
                QuestObject quest = PoolManager.instance.GetObject<QuestObject>(questObject, parent);
                quest.txTitle.text = title;
                quest.txRewards[0].text = $"{questList[i].reward[questIndex]}";
                quest.txRewards[1].text = $"{questList[i].reward[questIndex]}";
                quest.SetQuestType(questList[i].questType);
                quest.questTimer = questTimer;
                quest.questIndex = questIndex;
                quest.goldsGain = questList[i].reward[questIndex];
                if (questList[i].questType != QUEST_TYPE.TOP_DAILY_RANK)
                {
                    quest.progressFill.fillAmount = (float)questList[i].status.progress / questList[i].amount[questIndex];
                    quest.txProgress.text = $"{questList[i].status.progress}/{questList[i].amount[questIndex]}";
                    quest.SetButton(questList[i].status.progress >= questList[i].amount[questIndex], questList[i].status.status[questIndex]);
                }
                else
                {
                    quest.progressFill.fillAmount = questList[i].status.progress > 0 && questList[i].status.progress <= questList[i].amount[questIndex] ? 1 : 0;
                    if (questList[i].status.progress > 0)
                        quest.txProgress.text = "Best: Top " + questList[i].status.progress;
                    else
                        quest.txProgress.text = "";
                    quest.SetButton(questList[i].status.progress > 0 && questList[i].status.progress <= questList[i].amount[questIndex], questList[i].status.status[questIndex]);
                }
                questObjectList.Add(quest);
                list.Add(quest);
                //fadeIgnore.Add(quest.gameObject);
            }
        }
        int childIndex = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].statusInt == 1)
                list[i].transform.SetSiblingIndex(childIndex++);
            else if (list[i].statusInt == 2)
                list[i].transform.SetSiblingIndex(list.Count - 1);
        }
    }
    public override void Close()
    {
        base.Close();
        foreach (var quest in questObjectList)
            PoolManager.instance.ReleaseObject(quest);
        questObjectList.Clear();
        //fadeIgnore.Clear();
    }
}
