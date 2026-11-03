using System;
using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
            Destroy(gameObject);
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public DateTime currentTime;
    public DateTime today;
    public bool connected;
    //public Calendar myCal;
    //CalendarWeekRule myCWR;
    //DayOfWeek myFirstDOW;
    private void Start()
    {
        GetTime(GameConfig.GetTimeNow());
    }
    public void GetTime(DateTime time)
    {
        Application.targetFrameRate = 120;
        GameConfig.InitVibrate();
        currentTime = time;
        Today();
        connected = true;
        BattlePassManager.instance.GetBattlePassTimer();
        //StartCoroutine(WaitRemoteConfig());
        //BlackFridayScript.instance.SetupBlackFriday();
        //LunarNewYearScript.instance.SetupLunarNewYear();
    }
    //public IEnumerator WaitRemoteConfig()
    //{
    //    EventManager.instance.GetEventTimer();
    //}
    void Today()
    {
        today = currentTime.Date;
        Debug.Log("today reset " + today);
        if (GameConfig.GetMonthIndex(today) != GameConfig.GetCurrentMonth())
        {
            NewMonth();
        }
        if (GameConfig.GetWeekIndex(today) != GameConfig.GetCurrentWeek())
        {
            NewWeek();
        }
        if (GameConfig.GetDateIndex(today) != GameConfig.GetCurrentDate())
        {
            NewDate();
        }
    }
    public TimeSpan TimeToNextDay()
    {
        return currentTime.Date.AddDays(1) - currentTime;
    }
    public (bool isPiggyBank, TimeSpan timeLeft) TimeEndPiggyBank()
    {
        if (GameConfig.IsPiggyBankBought())
            return (false, TimeSpan.MinValue);
        var (startTime, endTime) = GameConfig.GetPiggyBankStartEndTime();
        return (currentTime >= startTime && currentTime < endTime, endTime - currentTime);
    }
    void NewDate()
    {
        GameConfig.SetCurrentDate(today);
        GameConfig.ResetFreeCoins();
        //GameConfig.ResetDailyBonus();
        //GameConfig.ResetDailyGift();
        if (!GameConfig.IsDailyLogin())
            GameConfig.SetDailyLogin(0);
    }
    void NewWeek()
    {
        GameConfig.SetCurrentWeek(today);
    }
    void NewMonth()
    {
        GameConfig.SetCurrentMonth(today);
    }
    private void Update()
    {
        if (connected)
        {
            currentTime = currentTime.AddSeconds(Time.unscaledDeltaTime);
            if (today != currentTime.Date)
                Today();
        }
    }
}
