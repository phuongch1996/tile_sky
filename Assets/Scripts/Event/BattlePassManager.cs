using Beebyte.Obfuscator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public class BattlePassManager : MonoBehaviour
{
    public static BattlePassManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public static string bpStartingDateKey = "bp_starting_";
    public static string bpEndingDateKey = "bp_ending_";
    public DateTime startingDate;
    public DateTime endingDate;

    //0:undo 1:hint 2:shuffle 4:gold
    public static readonly List<BattlePass> battlePass = new()
    {
        new BattlePass(0,  50,  200, 300,  4),
        new BattlePass(2,  100, 200, 300,  4),
        new BattlePass(4,  1,   2,   3,    1),
        new BattlePass(6,  100, 200, 300,  4),
        new BattlePass(8,  1,   2,   3,    2),
        new BattlePass(10, 100, 200, 300,  4),
        new BattlePass(12, 1,   1,   2,    0,2),
        new BattlePass(15, 150, 300, 450,  4),
        new BattlePass(18, 2,   3,   5,    1),
        new BattlePass(21, 150, 300, 450,  4),
        new BattlePass(24, 2,   3,   5,    2),
        new BattlePass(27, 200, 300, 500,  4),
        new BattlePass(30, 300, 500, 1000, 4),
        new BattlePass(33, 1,   1,   2,    0,2),
        new BattlePass(36, 200, 300, 500,  4),
        new BattlePass(39, 2,   3,   5,    1),
        new BattlePass(42, 2,   3,   5,    2),
        new BattlePass(47, 1,   1,   2,    0,2),
        new BattlePass(50, 500, 1000,2000, 4),
        new BattlePass(55, 200, 300, 500,  4),
        new BattlePass(60, 2,   3,   5,    1),
        new BattlePass(65, 2,   3,   5,    2),
        new BattlePass(70, 200, 300, 500,  4),
        new BattlePass(75, 1,   2,   3,    0,1,2),
        new BattlePass(80, 300, 500, 1000, 4),
        new BattlePass(85, 300, 500, 1000, 4),
        new BattlePass(90, 300, 500, 1000, 4),
        new BattlePass(95, 1,   2,   3,    0,1,2),
        new BattlePass(100,500, 1000,3000, 4),
    };

    public static bool IsBattlePassVIP()
    {
        return PlayerPrefs.GetInt("bp_vip", 0) != 0;
    }
    public static void BuyBattlePassVIP()
    {
        PlayerPrefs.SetInt("bp_vip", 1);
        HomeScript.instance.battlePassPopup.BPProgress();
    }
    public static string bpDataKey = "bp_data";
    public static BattlePassClaimData GetBPData()
    {
        if (PlayerPrefs.HasKey(bpDataKey))
        {
            return JsonConvert.DeserializeObject<BattlePassClaimData>(PlayerPrefs.GetString(bpDataKey));
        }
        else
            return new();
    }
    public static void SetBPData(BattlePassClaimData bpData)
    {
        bpData.freeClaimed.Sort();
        PlayerPrefs.SetString(bpDataKey, JsonConvert.SerializeObject(bpData));
    }
    public BattlePassClaimData bpData;
    public void GetData()
    {
        bpData = GetBPData();
    }
    public void UpdateData()
    {
        SetBPData(bpData);
    }

    public static string bpWinCountKey = "bp_win_count";
    public static int GetBPWinCount()
    {
        return PlayerPrefs.GetInt(bpWinCountKey, 0);
    }
    public static void AddBPWinCount()
    {
        if (GameConfig.GetCurrentLevel() < Common.config_unlock_battlepass)
            return;
        int bpWinCount = GetBPWinCount();
        PlayerPrefs.SetInt(bpWinCountKey, bpWinCount + 1);
    }
    public static (int index, float winFill) GetBPWinFill(int winCount)
    {
        var bp = battlePass.Find(x => x.winAmount > winCount);
        int index;
        if (bp != null)
            index = battlePass.IndexOf(bp);
        else
            index = battlePass.Count;
        if (index < 1)
            return (0, 0);
        int passWin = battlePass[index - 1].winAmount;
        return (index, index < battlePass.Count ? ((float)(winCount - passWin) / (bp.winAmount - passWin)) : 0);
    }
    public bool Claimable()
    {
        int winCount = GetBPWinCount();
        var bp = battlePass.Find(x => x.winAmount > winCount);
        int index;
        if (bp != null)
            index = battlePass.IndexOf(bp);
        else
            index = battlePass.Count;
        return bpData.freeClaimed.Count < index || (IsBattlePassVIP() && bpData.vipClaimed.Count < index);
    }
    public static readonly string bpIndexKey = "bp_index";
    public static int GetBPIndex()
    {
        return PlayerPrefs.GetInt(bpIndexKey, 0);
    }
    public static void SetBPIndex(int index)
    {
        PlayerPrefs.SetInt(bpIndexKey, index);
    }
    public bool isBattlePass;
    public bool isBattlePassTimerGet;
    public void GetBattlePassTimer()
    {
        if (!PlayerPrefs.HasKey(bpStartingDateKey))
        {
            if (GetBPIndex() == 0)
            {
                startingDate = TimeManager.instance.currentTime;
                endingDate = TimeManager.instance.currentTime.AddDays(30);
                PlayerPrefs.SetString(bpStartingDateKey, startingDate.ToString("o"));
                PlayerPrefs.SetString(bpEndingDateKey, endingDate.ToString("o"));
                ResetBattlePass();
                SetBPIndex(1);
                GetData();
                isBattlePass = true;
            }
        }
        else
        {
            try
            {
                if (!DateTime.TryParse(PlayerPrefs.GetString(bpStartingDateKey), null, DateTimeStyles.RoundtripKind, out startingDate))
                {
                    startingDate = DateTime.Parse(PlayerPrefs.GetString(bpStartingDateKey));
                }
                if (!DateTime.TryParse(PlayerPrefs.GetString(bpEndingDateKey), null, DateTimeStyles.RoundtripKind, out endingDate))
                {
                    endingDate = DateTime.Parse(PlayerPrefs.GetString(bpEndingDateKey));
                }
            }
            catch
            {
                startingDate = DateTime.MinValue;
                endingDate = DateTime.MinValue;
            }
            isBattlePass = TimeManager.instance.currentTime < endingDate && TimeManager.instance.currentTime >= startingDate;
            if (!isBattlePass)
            {
                SetBPIndex(2);
                EndBattlePass();
            }
            else
            {
                GetData();
            }
        }
        isBattlePassTimerGet = true;
    }
    void EndBattlePass()
    {
        //isBattlePass = false;
        PlayerPrefs.DeleteKey(bpStartingDateKey);
        PlayerPrefs.DeleteKey(bpEndingDateKey);
    }
    static void ResetBattlePass()
    {
        PlayerPrefs.DeleteKey(bpDataKey);
        PlayerPrefs.DeleteKey(bpWinCountKey);
    }
    public TimeSpan TimeLeftBattlePass()
    {
        if (isBattlePass)
        {
            TimeSpan ts = endingDate - TimeManager.instance.currentTime;
            if (ts.Ticks < 0)
                isBattlePass = false;
            return ts;
        }
        return new TimeSpan(-1);
    }
}

[Serializable]
public class BPPrizes
{
    public int prizeAmount;
    public int[] prizeTypes;
    public BPPrizes(int prizeAmount, int[] prizeTypes)
    {
        this.prizeAmount = prizeAmount;
        this.prizeTypes = prizeTypes;
    }
}
public class BattlePass
{
    public int winAmount;
    public BPPrizes[] bpPrizes;
    //public BattlePass(int winAmount, int[] prizeTypes, int freeAmount, int adsAmount, int vipAmount)
    public BattlePass(params int[] parameters)
    {
        winAmount = parameters[0];
        int freeAmount = parameters[1];
        int adsAmount = parameters[2];
        int vipAmount = parameters[3];
        int[] prizeTypes = parameters.Skip(4).ToArray();
        bpPrizes = new BPPrizes[]
        {
            new BPPrizes(freeAmount, prizeTypes),
            new BPPrizes(adsAmount, prizeTypes),
            new BPPrizes(vipAmount, prizeTypes),
        };
    }
}
[Skip]
[Serializable]
public class BattlePassClaimData
{
    public List<int> freeClaimed;
    public List<int> adsClaimed;
    public List<int> vipClaimed;
    public BattlePassClaimData()
    {
        freeClaimed = new();
        adsClaimed = new();
        vipClaimed = new();
    }
    public void ClaimFree(int index)
    {
        freeClaimed.Add(index);
        freeClaimed.Sort();
    }
    public void ClaimAds(int index)
    {
        adsClaimed.Add(index);
        adsClaimed.Sort();
    }
    public void ClaimVip(int index)
    {
        vipClaimed.Add(index);
        vipClaimed.Sort();
    }
}
