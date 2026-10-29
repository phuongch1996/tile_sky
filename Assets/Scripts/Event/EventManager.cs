using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

public enum EventType
{
    none,
    collect_event_1,
    combo_event_1,
    streak_event_1,
}
public class EventManager : MonoBehaviour
{
    public static EventManager instance;
    public const int UnlockLevel = 15;
    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
    const string eventStartingDateKey = "event_starting_";
    const string eventEndingDateKey = "event_ending_";
    public DateTime startingDate;
    public DateTime endingDate;
    //0:undo 1:hint 2:shuffle 4:gold
    #region gold_tile
    public static readonly List<EventPrize> eventCollectPrizes = new()
    {
        new(10, 50, 4),
        new(20, 100,4),
        new(35, 1,  0),
        new(55, 150,4),
        new(80, 1,  2),
        new(105,150,4),
        new(135,1,  1),
        new(165,200,4),
        new(200,1,  2),
        new(235,150,4),
        new(275,200,4),
        new(315,1,  0,2),
        new(365,200,4),
        new(415,2,  1),
        new(465,200,4),
        new(515,2,  2),
        new(565,250,4),
        new(615,300,4),
        new(665,300,4),
        new(715,500,4),
    };
    const string goldTileCountKey = "gold_tile_count";
    public static int GetGoldTileCount()
    {
        return PlayerPrefs.GetInt(goldTileCountKey, 0);
    }
    public static bool AddGoldTileCount(int add)
    {
        int goldTileCount = GetGoldTileCount();
        PlayerPrefs.SetInt(goldTileCountKey, goldTileCount + add);
        return add > 0 && GetEventPrizeClaimed() < eventCollectPrizes.Count;
    }
    public static void ResetEvent()
    {
        PlayerPrefs.DeleteKey(goldTileCountKey);
        PlayerPrefs.DeleteKey(eventPrizeClaimedKey);
        PlayerPrefs.DeleteKey(eventLevelKey);
        PlayerPrefs.DeleteKey(eventTutorialKey);
    }
    #endregion

    //0:undo 1:hint 2:shuffle 4:gold
    #region streak
    public static readonly List<EventPrize> eventStreakPrizes = new()
    {
        new(3, 150,4),
        new(5, 250,4),
        new(7, 300,4),
        new(10,500,4),
        new(15,500,4),
    };
    const string currentStreakKey = "streak_current";
    const string highestStreakKey = "streak_highest";
    const string streakAdditiveKey = "streak_additive";
    public static int GetCurrentStreak()
    {
        return PlayerPrefs.GetInt(currentStreakKey, 0);
    }
    public static void SetCurrentStreak(int streak)
    {
        PlayerPrefs.SetInt(currentStreakKey, streak);
    }
    public static int GetHighestStreak()
    {
        return PlayerPrefs.GetInt(highestStreakKey, 0);
    }
    public static void SetHighestStreak(int streak)
    {
        PlayerPrefs.SetInt(highestStreakKey, streak);
    }
    public static int GetStreakAdditive()
    {
        return PlayerPrefs.GetInt(streakAdditiveKey, 0) * eventStreakPrizes[^1].collect;
    }
    public static void SetStreakAdditive(int streak)
    {
        PlayerPrefs.SetInt(streakAdditiveKey, streak);
    }
    public static void SteakReset()
    {
        PlayerPrefs.DeleteKey(currentStreakKey);
        PlayerPrefs.DeleteKey(highestStreakKey);
        PlayerPrefs.DeleteKey(streakAdditiveKey);
        PlayerPrefs.DeleteKey(eventPrizeClaimedKey);
        PlayerPrefs.DeleteKey(eventLevelKey);
        PlayerPrefs.DeleteKey(eventTutorialKey);
    }
    public static void StreakAdditive()
    {
        int steakAdditive = PlayerPrefs.GetInt(streakAdditiveKey, 0);
        SetStreakAdditive(steakAdditive + 1);
        PlayerPrefs.DeleteKey(eventPrizeClaimedKey);
    }
    #endregion

    //0:undo 1:hint 2:shuffle 4:gold
    #region combo
    public static readonly List<EventPrize> eventComboPrizes = new()
    {
        new(10,  50, 4),
        new(60,  100,4),
        new(160, 1,  0),
        new(280, 150,4),
        new(430, 1,  2),
        new(610, 150,4),
        new(820, 1,  1),
        new(1070,200,4),
        new(1370,1,  2),
        new(1720,150,4),
        new(2070,200,4),
        new(2470,1,  0,2),
        new(2870,200,4),
        new(3270,2,  1),
        new(3720,200,4),
        new(4170,2,  2),
        new(4620,250,4),
        new(5120,300,4),
        new(5620,300,4),
        new(6120,500,4),
    };

    const string comboCountKey = "combo_count";
    public static int GetComboCount()
    {
        return PlayerPrefs.GetInt(comboCountKey, 0);
    }
    public static bool AddComboCount(int add)
    {
        int comboCount = GetComboCount();
        PlayerPrefs.SetInt(comboCountKey, comboCount + add);
        return add > 0 && GetEventPrizeClaimed() < eventComboPrizes.Count;
    }
    public static void ResetComboEvent()
    {
        PlayerPrefs.DeleteKey(comboCountKey);
        PlayerPrefs.DeleteKey(eventPrizeClaimedKey);
        PlayerPrefs.DeleteKey(eventLevelKey);
        PlayerPrefs.DeleteKey(eventTutorialKey);
    }
    #endregion

    const string eventTutorialKey = "tutorial_event";
    public static bool GetEventTutorial()
    {
        return PlayerPrefs.GetInt(eventTutorialKey, 0) == 0;
    }
    public static void SetEventTutorial()
    {
        PlayerPrefs.SetInt(eventTutorialKey, 1);
    }

    const string eventPrizeClaimedKey = "event_prize_claimed";
    public static int GetEventPrizeClaimed()
    {
        return PlayerPrefs.GetInt(eventPrizeClaimedKey, 0);
    }
    public static void SetEventPrizeClaimed(int eventPrizeClaimed)
    {
        PlayerPrefs.SetInt(eventPrizeClaimedKey, eventPrizeClaimed);
    }
    public static void ClaimCurrentPrize()
    {
        int currentPrize = GetEventPrizeClaimed();
        if (eventType == EventType.collect_event_1 || eventType == EventType.combo_event_1)
        {
            SetEventPrizeClaimed(currentPrize + 1);
        }
        if (eventType == EventType.streak_event_1)
        {
            if (currentPrize + 1 < eventStreakPrizes.Count)
            {
                SetEventPrizeClaimed(currentPrize + 1);
            }
            else
            {
                StreakAdditive();
            }
        }
    }

    const string eventLevelKey = "event_level";
    public static int GetEventCurrentLevel()
    {
        return PlayerPrefs.GetInt(eventLevelKey, 0);
    }
    public static void SetEventCurrentLevel(int level)
    {
        PlayerPrefs.SetInt(eventLevelKey, level);
    }
    public static void ResetEventLevel()
    {
        PlayerPrefs.DeleteKey(eventLevelKey);
    }
    const string eventIndexKey = "event_index";
    public static int GetEventIndex()
    {
        return PlayerPrefs.GetInt(eventIndexKey, 0);
    }
    public static void SetEventIndex(int index)
    {
        PlayerPrefs.SetInt(eventIndexKey, index);
    }
    public static EventType eventType;
    public bool isEvent;
    public bool isEventTimeGet;
    //public bool isCurrentLevelEvent;
    public bool IsCurrentLevelEvent()
    {
        if (isEvent && eventType == EventType.collect_event_1 && GameConfig.level >= UnlockLevel)
        {
            return true;
            //if (GameFunc.cheatEnabled)
            //    return true;
            //if (Config.level == Config.GetCurrentLevel() && Config.level > GetEventCurrentLevel())
            //{
            //    SetEventCurrentLevel(Config.level);
            //    return true;
            //}
        }
        return false;
    }
    public bool IsSteakEvent()
    {
        tempStreak = null;
        if (isEvent && eventType == EventType.streak_event_1 && GameConfig.level >= UnlockLevel)
        {
            if (GameConfig.level == GameConfig.GetCurrentLevel() && GameConfig.level > GetEventCurrentLevel())
            {
                //SetEventCurrentLevel(Config.level);
                return true;
            }
            //else
            //    SetCurrentStreak(0);
            //SetCurrentStreak(GetStreakAdditive());
        }
        return false;
    }
    public bool IsComboEvent()
    {
        if (isEvent && eventType == EventType.combo_event_1 && GameConfig.level >= UnlockLevel)
        {
            return true;
            //if (GameFunc.cheatEnabled)
            //    return true;
            //if (Config.level == Config.GetCurrentLevel() && Config.level > GetEventCurrentLevel())
            //{
            //    SetEventCurrentLevel(Config.level);
            //    return true;
            //}
        }
        return false;
    }
    public bool StreakWin()
    {
        if (IsSteakEvent())
        {
            SetEventCurrentLevel(GameConfig.level);
            int currentStreak = GetCurrentStreak();
            int highestStreak = GetHighestStreak();
            int streak = currentStreak + 1;
            if (streak > highestStreak)
                SetHighestStreak(streak);
            SetCurrentStreak(streak);

            return GetEventPrizeClaimed() < eventStreakPrizes.Count;
        }
        return false;
    }
    int? tempStreak;
    public void StreakLose()
    {
        if (IsSteakEvent())
        {
            //SetEventCurrentLevel(Config.level);
            tempStreak = GetCurrentStreak();
            SetCurrentStreak(GetStreakAdditive());
        }
    }
    public void StreakRevive()
    {
        //if (IsSteakEvent())
        //{
        if (tempStreak.HasValue)
        {
            //SetEventCurrentLevel(Config.level - 1);
            SetCurrentStreak(tempStreak.Value);
            tempStreak = null;
        }
        //}
    }
    public bool IsPrizeClaimable()
    {
        try
        {
            //Debug.Log($"{GetHighestStreak()}/{eventStreakPrizes[GetEventPrizeClaimed()].collect}");
            if (eventType == EventType.collect_event_1)
                return GetGoldTileCount() >= eventCollectPrizes[GetEventPrizeClaimed()].collect;
            else if (eventType == EventType.combo_event_1)
                return GetComboCount() >= eventComboPrizes[GetEventPrizeClaimed()].collect;
            else if (eventType == EventType.streak_event_1)
                return GetHighestStreak() >= eventStreakPrizes[GetEventPrizeClaimed()].collect + GetStreakAdditive();
            //else return false;
        }
        catch
        {
            return false;
        }
        return false;
    }
    public bool pendingEvent;
    public void GetEventTimer()
    {
        //if (Config.GetCurrentLevel() < 15)
        //{
        //    pendingEvent = true;
        //    return;
        //}
        //pendingEvent = false;
    GetEventTimer:
        if (!PlayerPrefs.HasKey(eventStartingDateKey))
        {
            //if (TimeManager.instance.currentTime < eventDateLimit[0] && GetEventIndex() <= 0)
            if (GetEventIndex() % 3 == 0)
            {
                startingDate = TimeManager.instance.currentTime;
                //endingDate = TimeManager.instance.currentTime.AddDays(7);
                endingDate = TimeManager.instance.currentTime.AddDays(Common.config_event_dates);
                PlayerPrefs.SetString(eventStartingDateKey, startingDate.ToString("o"));
                PlayerPrefs.SetString(eventEndingDateKey, endingDate.ToString("o"));
                ResetEvent();
                SetEventIndex(1);
                eventType = EventType.collect_event_1;
                isEvent = true;
            }
            //else if (TimeManager.instance.currentTime < eventDateLimit[1])
            else if (GetEventIndex() % 3 == 1)
            {
                startingDate = TimeManager.instance.currentTime;
                //endingDate = TimeManager.instance.currentTime.AddDays(7);
                endingDate = TimeManager.instance.currentTime.AddDays(Common.config_event_dates);
                PlayerPrefs.SetString(eventStartingDateKey, startingDate.ToString("o"));
                PlayerPrefs.SetString(eventEndingDateKey, endingDate.ToString("o"));
                ResetComboEvent();
                SetEventIndex(2);
                eventType = EventType.combo_event_1;
                isEvent = true;
            }
            //else if (TimeManager.instance.currentTime < eventDateLimit[1])
            else if (GetEventIndex() % 3 == 2)
            {
                startingDate = TimeManager.instance.currentTime;
                //endingDate = TimeManager.instance.currentTime.AddDays(7);
                endingDate = TimeManager.instance.currentTime.AddDays(Common.config_event_dates);
                PlayerPrefs.SetString(eventStartingDateKey, startingDate.ToString("o"));
                PlayerPrefs.SetString(eventEndingDateKey, endingDate.ToString("o"));
                SteakReset();
                SetEventIndex(3);
                eventType = EventType.streak_event_1;
                isEvent = true;
            }
            else
            {
                eventType = EventType.none;
                isEvent = false;
            }
        }
        else
        {
            try
            {
                if (!DateTime.TryParse(PlayerPrefs.GetString(eventStartingDateKey), null, DateTimeStyles.RoundtripKind, out startingDate))
                {
                    startingDate = DateTime.Parse(PlayerPrefs.GetString(eventStartingDateKey));
                }
                if (!DateTime.TryParse(PlayerPrefs.GetString(eventEndingDateKey), null, DateTimeStyles.RoundtripKind, out endingDate))
                {
                    endingDate = DateTime.Parse(PlayerPrefs.GetString(eventEndingDateKey));
                }
            }
            catch
            {
                //startingDate = TimeManager.instance.currentTime;
                //endingDate = TimeManager.instance.currentTime.AddDays(7);
                //PlayerPrefs.SetString(eventStartingDateKey, startingDate.ToString("o"));
                //PlayerPrefs.SetString(eventEndingDateKey, endingDate.ToString("o"));
                startingDate = DateTime.MinValue;
                endingDate = DateTime.MinValue;
            }
            isEvent = TimeManager.instance.currentTime < endingDate && TimeManager.instance.currentTime >= startingDate;
            int eventIndex = GetEventIndex();
            //if (eventIndex == 1)
            //{
            //    eventType = EventType.collect_event_1;
            //}
            //else if 
            eventType = eventIndex switch
            {
                1 => EventType.collect_event_1,
                2 => EventType.combo_event_1,
                3 => EventType.streak_event_1,
                _ => EventType.none
            };
            //if (startingDate < eventDateLimit[0])
            //{
            //    SetEventIndex(1);
            //    eventType = EventType.collect_event_1;
            //}
            //else if
            if (!isEvent)
            {
                EndEvent();
                goto GetEventTimer;
            }
        }
        isEventTimeGet = true;
    }
    public void EndEvent()
    {
        eventType = EventType.none;
        PlayerPrefs.DeleteKey(eventStartingDateKey);
        PlayerPrefs.DeleteKey(eventEndingDateKey);
    }
    public TimeSpan GetEventTimeLeft()
    {
        return endingDate - TimeManager.instance.currentTime;
    }
    public bool IsEventTimeLeft()
    {
        return GetEventTimeLeft().TotalSeconds > 0;
    }
}
[Serializable]
public class EventPrize
{
    public int collect;
    public int prizeAmount;
    public int[] prizeType;
    public EventPrize(params int[] parameters)
    {
        collect = parameters[0];
        prizeAmount = parameters[1];
        prizeType = parameters.Skip(2).ToArray();
    }
}
