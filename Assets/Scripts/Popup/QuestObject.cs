using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestObject : PoolObjectUI
{
    public TextMeshProUGUI txTitle;
    public Image progressFill;
    public TextMeshProUGUI txProgress;
    public TextMeshProUGUI[] txRewards;
    public Button btClaim;
    public GameObject claimedGO;
    public GameObject notDoneGO;
    public GameObject[] doneGO;
    public GameObject[] objectiveImgs;

    public QUEST_TYPE questType;
    public int questTimer;
    public int questIndex;
    public int goldsGain;
    public int statusInt;
    public override void Inactive()
    {
        base.Inactive();
    }
    public void SetQuestType(QUEST_TYPE _qyestType)
    {
        questType = _qyestType;
        foreach (var item in objectiveImgs)
            item.SetActive(false);
        objectiveImgs[(int)_qyestType].SetActive(true);
    }
    public void SetButton(bool completed, bool status)
    {
        btClaim.onClick.RemoveAllListeners();
        if (completed)
        {
            if (status)
            {
                btClaim.interactable = false;
                notDoneGO.SetActive(false);
                doneGO[0].SetActive(true);
                doneGO[1].SetActive(true);
                claimedGO.SetActive(true);
                statusInt = 2;
            }
            else
            {
                btClaim.interactable = true;
                notDoneGO.SetActive(false);
                doneGO[0].SetActive(true);
                doneGO[1].SetActive(true);
                claimedGO.SetActive(false);
                btClaim.onClick.AddListener(() => ClaimQuestReward());
                statusInt = 1;
            }
        }
        else
        {
            btClaim.interactable = false;
            notDoneGO.SetActive(true);
            doneGO[0].SetActive(false);
            doneGO[1].SetActive(false);
            claimedGO.SetActive(false);
            statusInt = 0;
        }
    }
    void ClaimQuestReward()
    {
        btClaim.interactable = false;
        notDoneGO.SetActive(false);
        doneGO[0].SetActive(true);
        doneGO[1].SetActive(true);
        claimedGO.SetActive(true);
        //statusInt = 2;
        switch (questTimer)
        {
            case 0://alltime
                QuestManager.allTimeQuests.Find(x => x.questType == questType).status.status[questIndex] = true;
                QuestManager.SetAllTimeQuestStatus();
                break;
            case 1://daily
                QuestManager.dailyQuests.Find(x => x.questType == questType).status.status[questIndex] = true;
                QuestManager.SetDailyQuestStatus();
                break;
        }
        HomeScript.instance.TopBarUpdateGolds(txRewards[0].transform, goldsGain, goldsGain / 5);
        if (HomeScript.instance)
        {
            HomeScript.instance.questPopup.Notif();
            HomeScript.instance.QuestNotif();
        }
    }
}
