using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Purchasing;

public enum GAME_STATE
{
    START,
    PLAYING,
    PAUSE,
    UNDO,
    SHUFFLE,
    SUGGEST,
    LOSE,
    LOSE_POPUP,
    WIN,
    WIN_POPUP,
    TUTORIAL,
    STICKY,
    UP2,
    RAINBOW
}
public enum ITEMTILE_STATE
{
    START,
    HIDDEN,
    TOUCHABLE,
    HOVERING,
    //MOVING,
    SLOTTED,
    DONE,
}
public enum GAME_MODE
{
    normal,
    //timeCD,
    //hardxx,
    tile_random,
    tile_double,
    tile_freeze,
    //tile_lock_slot,
    //tile_snail,
    tile_gold,
    //tile_blast,
    tile_2up,
    tile_rainbow,
    tile_fog,
    tile_color,
    // Keep all previous enum values unchanged: they are stored in progress saves.
    tile_missslot = 9,
    tile_poison = 10,
    tile_bomb = 11,
    tile_tray = 12,
    tile_winning = 13,
    tile_gold_2 = 14,
}
public class GameConfig
{
    public const string homeScene = "HomeScene";
    public const string gameScene = "GameScene";

    public static bool isUMP;
    public static bool isGameLoaded;

    public static bool isNewGame;
    public static int level;
    public static bool isExtraSlot;
    public static bool isTesting;
    public static int reviveCount;
    public static int loseCount;
    public static int boosterCount;
    public static bool isChallengeLevel;

    public const int unlockDailyLogin = 5;
    public const int unlockFreeGold = 8;
    public const int unlockPiggyBank = 22;
    public const int unlockRank = 26;
    //public static bool IsDailyChallenge => false;
    public static bool IsDailyChallenge => UnityAuthentication.IsLeaderboard() && UnityAuthentication.instance
         && GetCurrentDate() == UnityAuthentication.instance.dailyChallengeDateIndex
         && GetCurrentLevel() >= Common.config_unlock_dailychallenge;

    public const string startLevelKey = "start_level";

    public static GAME_MODE gameMode = GAME_MODE.normal;
    public static int Handicap()
    {
        return Mathf.Clamp(loseCount / 3 + boosterCount / 2, 0, 2);
    }

    const string levelKey = "level";
    public static int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt(levelKey, 1);
    }
    public static void SetCurrentLevel(int level)
    {
        PlayerPrefs.SetInt(levelKey, level);
    }

    const string goldKey = "gold";
    public static Action<int, bool> OnGoldChange;
    public static int GetGold()
    {
        return PlayerPrefs.GetInt(goldKey, 0);
    }
    public static void AddGold(int add)
    {
        int gold = GetGold();
        gold += add;
        PlayerPrefs.SetInt(goldKey, gold);
        OnGoldChange(gold, true);
    }
    public static bool UseGold(int use)
    {
        int gold = GetGold();
        if (use > gold)
            return false;
        gold -= use;
        PlayerPrefs.SetInt(goldKey, gold);
        OnGoldChange(gold, false);
        return true;
    }
    //0:undo 1:hint 2:shuffle
    const string powerUpsKey = "powerups";
    public static int[] powerPrices = new int[] { 80, 120, 100, 200 };
    public static int[] GetPowerUps()
    {
        List<int> powerUps = new();
        string key = PlayerPrefs.GetString(powerUpsKey, "2|2|2|2");
        string[] value = key.Split("|");
        foreach (string v in value)
        {
            powerUps.Add(int.Parse(v));
        }
        return powerUps.ToArray();
    }
    public static Action OnPowerUpsChange;
    public static int NormalizePowerUpPrizeType(int power)
    {
        return power == 3 ? 2 : power;
    }
    public static void SetPowerUps(int[] powerUps)
    {
        PlayerPrefs.SetString(powerUpsKey, string.Join("|", powerUps));
        OnPowerUpsChange?.Invoke();
    }
    public static void AddPowerUps(int amount, params int[] powers)
    {
        var powerUps = GetPowerUps();
        foreach (var power in powers)
        {
            int normalizedPower = NormalizePowerUpPrizeType(power);
            if (normalizedPower < 0 || normalizedPower >= powerUps.Length)
                continue;
            powerUps[normalizedPower] += amount;
        }
        SetPowerUps(powerUps);
    }
    public static void AddAllPowerUps(int amount)
    {
        AddPowerUps(amount, 0, 1, 2);
    }
    public static void AddPowerUndo(int amount)
    {
        AddPowerUps(amount, 0);
    }
    public static void AddPowerHint(int amount)
    {
        AddPowerUps(amount, 1);
    }
    public static void AddPowerShuffle(int amount)
    {
        AddPowerUps(amount, 2);
    }
    public static void AddPowerAddSlot(int amount)
    {
    }
    public static bool UsePowers(int power)
    {
        var powerUps = GetPowerUps();
        if (powerUps[power] > 0)
        {
            powerUps[power]--;
            SetPowerUps(powerUps);
            return true;
        }
        else
            return false;
    }

    #region background
    const string bgTypeKey = "bg_type";
    public static int GetBGType()
    {
        return PlayerPrefs.GetInt(bgTypeKey, 0);
    }
    public static void SetBGType(int bg)
    {
        PlayerPrefs.SetInt(bgTypeKey, bg);
    }
    public static int bgTypeDefault = 0;
    const string bgTypeDefaultKey = "bg_type_default";
    public static Action OnEventChange = delegate { };
    public static int GetBGTypeDefault()
    {
        return bgTypeDefault;
    }
    public static void SetBgTypeDefault()
    {
        int _default = PlayerPrefs.GetInt(bgTypeDefaultKey, 0);
        if (_default != bgTypeDefault)
        {
            PlayerPrefs.SetInt(bgTypeDefaultKey, bgTypeDefault);
            OnEventChange?.Invoke();
            DeleteBGNotif();
            if (GetBGType() >= 0 && bgTypeDefault != GetBGType())
            {
                SetDefaultBg();
            }
        }
    }
    public static void SetDefaultBg()
    {
        SetBGType(bgTypeDefault);
        if (bgTypeDefault != 0)
        {
            SetCurrentBackground(0);
        }
        else
        {
            var bg = GetSelectableBackground();
            SetCurrentBackground(bg[^1]);
        }
    }
    const string currentBackgroundKey = "current_background";
    public static int GetCurrentBackground()
    {
        return PlayerPrefs.GetInt(currentBackgroundKey, 0);
    }
    public static void SetCurrentBackground(int bg)
    {
        PlayerPrefs.SetInt(currentBackgroundKey, bg);
    }
    public static List<int> GetSelectableBackground()
    {
        List<int> backgroundUnlock = new();
        int currentLevel = CHEAT_ENABLED ? LevelManager.instance.levelsSO.LevelCount() : GetCurrentLevel();
        //int currentLevel = GetCurrentLevel();
        int range = GetBGLevelRange(currentLevel).rangeIndex;
        for (int i = 0; i <= range; i++)
        {
            backgroundUnlock.Add(i);
        }
        return backgroundUnlock;
    }

    public static (int start, int end, int rangeIndex) GetBGLevelRange(int level)
    {
        if (level <= 0)
            return (0, 0, 0);
        int rangeIndex = -1;
        int starting;
        int ending = 0;
        int offset = 10;
        do
        {
            starting = ending + 1;
            ending += offset;
            rangeIndex++;
        } while (!(level >= starting && level <= ending));
        return (starting, ending, rangeIndex);
    }
    public static int GetUnlockBGLevel(int index)
    {
        int starting;
        int ending = 0;
        int offset = 10;
        int rangeIndex = -1;
        do
        {
            starting = ending + 1;
            ending += offset;
            rangeIndex++;
        } while (rangeIndex < index);
        return starting;
    }
    public static (int start, int end, int rangeIndex) GetBonusLevelRange(int level)
    {
        if (level <= 0)
            return (0, 0, 0);
        int rangeIndex = -1;
        int starting;
        int ending = 0;
        int offset = 4;
        do
        {
            starting = ending + 1;
            ending += offset++;
            rangeIndex++;
        } while (!(level >= starting && level <= ending));
        return (starting, ending, rangeIndex);
    }
    const string bgNotifKey = "bg_notif";
    public static bool GetBGNotif()
    {
        return PlayerPrefs.GetInt(bgNotifKey, 0) == 0;
    }
    public static void DoneBGNotif()
    {
        PlayerPrefs.SetInt(bgNotifKey, 1);
    }
    public static void DeleteBGNotif()
    {
        PlayerPrefs.DeleteKey(bgNotifKey);
    }
    #endregion

    #region settings

    public static double bgm;
    public static bool bgmToggle;
    public static double sfx;
    public static bool sfxToggle;
    public static bool vib;
    public static Action OnSoundChange = delegate { };
    public static void GetSoundSettings()
    {
        bgm = GetMusic();
        bgmToggle = bgm > 0;
        sfx = GetAudio();
        sfxToggle = sfx > 0;
        vib = GetVibrate();
        OnSoundChange?.Invoke();
    }
    public static double GetAudio()
    {
        return GetDouble("audio", 0.8);
    }
    public static void SetAudio(double audio)
    {
        SetDouble("audio", audio);
        GetSoundSettings();
    }
    public static void SetAudioToggle(bool isOn)
    {
        SetAudio(isOn ? 0.8 : 0);
    }
    public static double GetMusic()
    {
        return GetDouble("music", 0.8);// > 0 ? 0.8:0;
    }
    public static void SetMusic(double bgm)
    {
        SetDouble("music", bgm);
        GetSoundSettings();
    }
    public static void SetMusicToggle(bool isOn)
    {
        SetMusic(isOn ? 0.8 : 0);
    }
    public static int DefaultVibrate()
    {
        if (SystemInfo.systemMemorySize < 4096)
            return 0;
        return 1;
    }
    public static bool GetVibrate()
    {
        return PlayerPrefs.GetInt("vib", DefaultVibrate()) == 1;
    }
    public static void SetVibrate(bool vib)
    {
        PlayerPrefs.SetInt("vib", vib ? 1 : 0);
        GetSoundSettings();
    }
    public static void InitVibrate()
    {
        Vibration.Init();
    }
    public static void VibratePop()
    {
        Vibration.VibratePop();
        //Handheld.Vibrate();
    }
    #endregion

    #region piggybank

    public static int piggyMin = 0;
    public static int piggyMax1 = 1000;
    public static int piggyMax2 = 2000;
    public static int piggyAdd = 0;
    const string piggyBankKey = "piggybank";
    const string piggyBankStartTimeKey = "piggybank_starttime";
    const string piggyBankEndTimeKey = "piggybank_endtime";
    const string piggyBankLevelsUnlockKey = "piggybank_levelsunlock";
    const string piggyBankBoughtKey = "piggybank_bought";
    public static int GetPiggyBank()
    {
        return PlayerPrefs.GetInt(piggyBankKey, piggyMin);
    }
    public static void SetPiggyBank(int piggy)
    {
        PlayerPrefs.SetInt(piggyBankKey, piggy);
    }
    public static bool IsPiggyBankBought()
    {
        return PlayerPrefs.GetInt(piggyBankBoughtKey, 0) != 0;
    }
    public static void BoughtPiggyBank()
    {
        PlayerPrefs.SetInt(piggyBankBoughtKey, 1);
    }
    public static (DateTime startTime, DateTime endTime) GetPiggyBankStartEndTime()
    {
        if (!PlayerPrefs.HasKey(piggyBankStartTimeKey))
        {
            SetPiggyBankStartEndTime(TimeManager.instance.currentTime);
        }
        try
        {
            if (DateTime.TryParse(PlayerPrefs.GetString(piggyBankStartTimeKey, DateTime.MinValue.ToString("o")), null, DateTimeStyles.RoundtripKind, out DateTime startTime) &&
                DateTime.TryParse(PlayerPrefs.GetString(piggyBankEndTimeKey, DateTime.MinValue.ToString("o")), null, DateTimeStyles.RoundtripKind, out DateTime endTime))
                return (startTime, endTime);
            return (DateTime.Parse(PlayerPrefs.GetString(piggyBankStartTimeKey, DateTime.MinValue.ToString())), DateTime.Parse(PlayerPrefs.GetString(piggyBankEndTimeKey, DateTime.MinValue.ToString())));
        }
        catch
        {
            return (DateTime.MinValue, DateTime.MinValue);
        }
    }
    public static void SetPiggyBankStartEndTime(DateTime startTime)
    {
        PlayerPrefs.SetString(piggyBankStartTimeKey, startTime.ToString("o"));
        PlayerPrefs.SetString(piggyBankEndTimeKey, startTime.AddDays(2).ToString("o"));
    }
    public static int GetPiggyBankLevelsUnlock()
    {
        return PlayerPrefs.GetInt(piggyBankLevelsUnlockKey, 1);
    }
    public static void SetPiggyBankLevelsUnlock()
    {
        int levelsUnlock = GetPiggyBankLevelsUnlock();
        if (levelsUnlock > 0)
        {
            levelsUnlock--;
            if (levelsUnlock == 0)
            {
                PlayerPrefs.DeleteKey(piggyBankLevelsUnlockKey);
                SetPiggyBankStartEndTime(TimeManager.instance.currentTime.AddDays(2));
                ResetPiggyBank();
            }
            else
                PlayerPrefs.SetInt(piggyBankLevelsUnlockKey, levelsUnlock);
        }
    }
    public static void AddPiggyBank(int add)
    {
        if (GetPiggyBank() == piggyMax2)
            return;
        int bank = GetPiggyBank() + add;
        PlayerPrefs.SetInt(piggyBankKey, Mathf.Min(bank, piggyMax2));
        piggyAdd = 0;
    }
    public static void ResetPiggyBank()
    {
        PlayerPrefs.SetInt(piggyBankKey, piggyMin);
    }
    public static bool IsPiggySmash()
    {
        return GetPiggyBank() >= piggyMax1;
    }
    #endregion

    #region freecoin

    const string freeCoinsKey = "freecoin";
    public static int freeCoinsMaxDaily = 5;
    public static void ResetFreeCoins()
    {
        PlayerPrefs.SetInt(freeCoinsKey, freeCoinsMaxDaily);
    }
    public static int GetFreeCoins()
    {
        return PlayerPrefs.GetInt(freeCoinsKey, freeCoinsMaxDaily);
    }
    public static void UseFreeCoins()
    {
        int key = GetFreeCoins();
        if (key > 0)
            PlayerPrefs.SetInt(freeCoinsKey, key - 1);
    }
    #endregion

    #region dailylogin

    const string dailyLoginDateKey = "daily_login_date";
    const string dailyLoginDateAdsKey = "daily_login_date_ads";
    const string dailyLoginKey = "daily_login";
    const string dailyLoginTotalKey = "daily_login_total";
    public static int GetDailyLoginDate()
    {
        return PlayerPrefs.GetInt(dailyLoginDateKey, 0);
    }
    public static void SetDailyLoginDate(int dateIndex)
    {
        PlayerPrefs.SetInt(dailyLoginDateKey, dateIndex);
    }
    public static int GetDailyLoginDateAds()
    {
        return PlayerPrefs.GetInt(dailyLoginDateAdsKey, 0);
    }
    public static void SetDailyLoginDateAds(int dateIndex)
    {
        PlayerPrefs.SetInt(dailyLoginDateAdsKey, dateIndex);
    }
    public static int GetDailyLogin()
    {
        return PlayerPrefs.GetInt(dailyLoginKey, 0);
    }
    public static void SetDailyLogin(int value)
    {
        PlayerPrefs.SetInt(dailyLoginKey, value);
    }
    public static int GetDailyLoginTotal()
    {
        return PlayerPrefs.GetInt(dailyLoginTotalKey, 0);
    }
    public static void SetDailyLoginTotal()
    {
        int loginTotal = GetDailyLoginTotal();
        PlayerPrefs.SetInt(dailyLoginTotalKey, loginTotal + 1);
    }
    public static bool IsDailyLogin()
    {
        return GetDailyLogin() < 8;
    }
    public static bool IsDailyLoginAvailable()
    {
        int currentDate = GetCurrentDate();
        int dailyLogin = GetDailyLogin();
        return currentDate > GetDailyLoginDate() && dailyLogin < 8;
    }
    #endregion

    #region time

    const string dateKey = "date";
    const string weekKey = "week";
    const string monthKey = "month";
    public static int GetDateIndex(DateTime date)
    {
        return date.Year * 10000 + date.Month * 100 + date.Day;
    }
    public static int[] GetIndexDate(int index)
    {
        int[] datetime = new int[3];
        datetime[0] = index / 10000;
        datetime[1] = (index - datetime[0] * 10000) / 100;
        datetime[2] = (index - datetime[0] * 10000 - datetime[1] * 100);
        //Debug.Log(String.Format("{0} {1} {2}", datetime[0],datetime[1],datetime[2]));
        return datetime;
    }
    public static int GetWeekIndex(DateTime date)
    {
        CultureInfo myCI = new("en-US");
        Calendar myCal = myCI.Calendar;
        CalendarWeekRule myCWR = myCI.DateTimeFormat.CalendarWeekRule;
        DayOfWeek myFirstDOW = myCI.DateTimeFormat.FirstDayOfWeek;
        return date.Year * 10000 + myCal.GetWeekOfYear(date, myCWR, myFirstDOW);
    }
    public static int GetMonthIndex(DateTime date)
    {
        return date.Year * 100 + date.Month;
    }
    public static DateTime GetStartWeek(DateTime date)
    {
        return date.AddDays((1 - (int)date.DayOfWeek) % 7);
    }
    public static DateTime GetEndWeek(DateTime date)
    {
        return GetStartWeek(date).AddDays(6);// date.AddDays(6 - (int)date.DayOfWeek);
    }
    public static int GetCurrentDate()
    {
        return PlayerPrefs.GetInt(dateKey, 0);
    }
    public static void SetCurrentDate(DateTime date)
    {
        PlayerPrefs.SetInt(dateKey, GetDateIndex(date));
    }
    public static int GetCurrentWeek()
    {
        return PlayerPrefs.GetInt(weekKey, 0);
    }
    public static void SetCurrentWeek(DateTime date)
    {
        PlayerPrefs.SetInt(weekKey, GetWeekIndex(date));
    }
    public static int GetCurrentMonth()
    {
        return PlayerPrefs.GetInt(monthKey, 0);
    }
    public static void SetCurrentMonth(DateTime date)
    {
        PlayerPrefs.SetInt(monthKey, GetMonthIndex(date));
    }
    public static bool IsRatingDate()
    {
        return !GetRated() && GetTimeNow().Date > GetRatingDate().AddDays(1);
    }
    public static DateTime GetRatingDate()
    {
        int[] date = GetIndexDate(PlayerPrefs.GetInt("rating_date", 19700101));
        return new DateTime(date[0], date[1], date[2]);
    }
    public static void SetRatingDate(DateTime date)
    {
        PlayerPrefs.SetInt("rating_date", GetDateIndex(date));
    }
    public static bool GetRated()
    {
        return PlayerPrefs.GetInt("rated", 0) == 1;
    }
    public static void SetRated(bool rated)
    {
        PlayerPrefs.SetInt("rated", rated ? 1 : 0);
    }
    public static DateTime GetTimeNow(bool utc = true)
    {
        if (utc)
            return DateTime.UtcNow;
        else
            return DateTime.Now;
    }
    #endregion

    #region player
    const string displayNameKey = "display_name";
    //public static string defaultName = "player";
    public static string GetDisplayName()
    {
        return PlayerPrefs.GetString(displayNameKey, GetDefaultName());
    }
    public static (bool notDefault, bool newName) SetDisplayName(string value)
    {
        if (string.IsNullOrEmpty(value.Trim()))
        {
            PlayerPrefs.DeleteKey(displayNameKey);
            return (false, false);
        }
        else
        {
            if (string.Equals(value.Trim(), GetDefaultName(), StringComparison.Ordinal))
            {
                PlayerPrefs.DeleteKey(displayNameKey);
                return (false, false);
            }
            string oldName = GetDisplayName();
            PlayerPrefs.SetString(displayNameKey, value);
            return (true, !string.Equals(value.Trim(), oldName));
        }
    }
    public static string GetUserId()
    {
        return UnityAuthentication.GetPlayerId();
    }
    const string playerNameDefaultKey = "player_name_default";
    public static string GetDefaultName()
    {
        string numberStr;
        try
        {
            string num = Regex.Replace(GetUserId(), "[^0-9]", "");
            numberStr = "#" + num.Substring(0, Mathf.Min(6, num.Length));
        }
        catch
        {
            numberStr = "#000000";
        }
        if (PlayerPrefs.HasKey(playerNameDefaultKey))
            return PlayerPrefs.GetString(playerNameDefaultKey);// + numberStr;
        return numberStr;
    }
    public static Action OnDefaultNameChange = delegate { };
    public static void SetDefaultName(string name)
    {
        if (PlayerPrefs.GetString(playerNameDefaultKey) == name)
            return;
        PlayerPrefs.SetString(playerNameDefaultKey, name);
        OnDefaultNameChange?.Invoke();
    }
    const string playerAvatarDefaultKey = "player_avatar_default";
    public static int GetDefaultPlayerAvatar()
    {
        if (!PlayerPrefs.HasKey(playerAvatarDefaultKey))
        {
            PlayerPrefs.SetInt(playerAvatarDefaultKey, UnityEngine.Random.Range(0, 13));
        }
        return PlayerPrefs.GetInt(playerAvatarDefaultKey);
    }
    const string playerAvatarKey = "player_avatar";
    public static int GetPlayerAvatar()
    {
        return PlayerPrefs.GetInt(playerAvatarKey, GetDefaultPlayerAvatar());
    }
    public static bool SetPlayerAvatar(int avatar)
    {
        int oldAvatar = GetPlayerAvatar();
        PlayerPrefs.SetInt(playerAvatarKey, avatar);
        return avatar != oldAvatar;
    }
    const string playerFrameKey = "player_frame";
    public static int GetPlayerFrame()
    {
        return PlayerPrefs.GetInt(playerFrameKey, 0);
    }
    public static bool SetPlayerFrame(int frame)
    {
        int oldFrame = GetPlayerFrame();
        PlayerPrefs.SetInt(playerFrameKey, frame);
        return frame != oldFrame;
    }
    static int? defaultFlag;
    static int DeviceLanguage()
    {
        if (defaultFlag != null)
            return defaultFlag.Value;
        SystemLanguage deviceLanguage = Application.systemLanguage;
        Debug.Log("Device language is: " + deviceLanguage);
        // Example of checking for a specific language
        defaultFlag = deviceLanguage switch
        {
            SystemLanguage.Vietnamese => 0,
            SystemLanguage.Hindi => 1,
            SystemLanguage.Japanese => 3,
            SystemLanguage.Korean => 4,
            SystemLanguage.Portuguese => 5,
            SystemLanguage.Indonesian => 7,
            SystemLanguage.Turkish => 8,
            SystemLanguage.German => 9,
            SystemLanguage.Spanish => 10,
            SystemLanguage.Chinese or SystemLanguage.ChineseSimplified or SystemLanguage.ChineseTraditional => 13,
            SystemLanguage.Italian => 14,
            SystemLanguage.French => 15,
            SystemLanguage.Thai => 16,
            _ => 2,
        };
        return defaultFlag.Value;
    }
    const string playerFlagKey = "player_flag";
    public static int GetPlayerFlag()
    {
        return PlayerPrefs.GetInt(playerFlagKey, DeviceLanguage());
    }
    public static bool SetPlayerFlag(int flag)
    {
        int oldFlag = GetPlayerFlag();
        PlayerPrefs.SetInt(playerFlagKey, flag);
        return flag != oldFlag;
    }

    //const string changeNameBonus = "change_name_bonus";
    //public static bool IsChangeNameBonus()
    //{
    //    return PlayerPrefs.GetInt(changeNameBonus, 0) == 0;
    //}
    //public static void ChangeNameBonus()
    //{
    //    PlayerPrefs.SetInt(changeNameBonus, 1);
    //}

    #endregion

    public static string GetLocalizedPrice(string productId)
    {
        if (!IAPManager.PurchasingEnabled) return "Unavailable";
        //Product product = CodelessIAPStoreListener.Instance.StoreController.products.WithID(productId);
        try
        {
            Product product = CodelessIAPStoreListener.Instance.GetProduct(productId);
            //Product product = UnityIAPServices.DefaultProduct().GetProducts()//m_Controller.products.WithID(productId);
            if (product != null)
            {
#if UNITY_EDITOR
                ProductMetadata metadata = product.metadata;
#elif UNITY_ANDROID
            ProductMetadata metadata = GetGoogleProductMetadataExtension.GetGoogleProductMetadata(product.metadata);
#elif UNITY_IOS
            ProductMetadata metadata = GetAppleProductMetadataExtension.GetAppleProductMetadata(product.metadata);            
#endif
                if (metadata != null)
                {
                    return metadata.localizedPriceString;// + metadata.isoCurrencyCode;
                }
            }
        }
        catch
        {
            return "$$$";
        }
        return "$$$";
    }
    public static int CompareHierarchy(Transform t1, Transform t2)
    {
        if (t1 == t2) return 0;

        // Get the full chain of parents for both objects
        var path1 = GetPath(t1);
        var path2 = GetPath(t2);

        // Compare the paths level by level starting from the root
        int maxLength = Mathf.Min(path1.Count, path2.Count);
        for (int i = 0; i < maxLength; i++)
        {
            if (path1[i] != path2[i])
            {
                // Return the comparison of sibling indices at the first point they differ
                return path1[i].GetSiblingIndex().CompareTo(path2[i].GetSiblingIndex());
            }
        }

        // If paths are identical up to the shortest path's length, 
        // the shorter path (the parent) comes before the longer path (the child).
        return path1.Count.CompareTo(path2.Count);
    }

    private static List<Transform> GetPath(Transform t)
    {
        var path = new List<Transform>();
        while (t != null)
        {
            path.Add(t);
            t = t.parent;
        }
        path.Reverse(); // Now root is at index 0
        return path;
    }
    public static int GetTileSet()
    {
        return Common.config_tile_set;
    }

    #region utils

    public static void SetDouble(string key, double value)
    {
        PlayerPrefs.SetString(key, GameConfig.DoubleToString(value));
    }

    public static double GetDouble(string key, double defaultValue)
    {
        string defaultValue2 = GameConfig.DoubleToString(defaultValue);
        return GameConfig.StringToDouble(PlayerPrefs.GetString(key, defaultValue2));
    }

    public static double GetDouble(string key)
    {
        return GameConfig.GetDouble(key, 0.0);
    }

    private static string DoubleToString(double target)
    {
        return target.ToString("R");
    }

    private static double StringToDouble(string target)
    {
        if (string.IsNullOrEmpty(target))
        {
            return 0.0;
        }
        return double.Parse(target);
    }

    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, (!value) ? 0 : 1);
    }

    public static bool GetBool(string key, bool defaultValue = false)
    {
        int defaultValue2 = (!defaultValue) ? 0 : 1;
        return PlayerPrefs.GetInt(key, defaultValue2) == 1;
    }

    public static double GetCurrentTime()
    {
        return DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
    }

    public static double GetCurrentTimeInDays()
    {
        return DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalDays;
    }

    public static double GetCurrentTimeInMills()
    {
        return DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
    }

    public static T GetRandom<T>(params T[] arr)
    {
        return arr[UnityEngine.Random.Range(0, arr.Length)];
    }
    public static List<int> GetRandomCountFromRange(int count, int start, int end)
    {
        List<int> range = new();
        for (int i = start; i <= end; i++)
            range.Add(i);
        return GetRandomCount(count, range.ToArray());
    }
    public static List<T> GetRandomCount<T>(int count, params T[] arr)
    {
        List<T> list = new();
        if (count >= arr.Length)
        {
            list.AddRange(arr);
            return list;
        }
        List<T> parameters = arr.ToList();
        for (int i = 0; i < count; i++)
        {
            T rnd = GetRandom(parameters.ToArray());
            parameters.Remove(rnd);
            list.Add(rnd);
        }
        return list;
    }
    public static List<int> GetRandomCountFromTotal(int count, int start, int end)
    {
        List<int> range = new();
        for (int i = start; i <= end; i++)
            range.Add(i);
        return GetRandomCount(count, range.ToArray());
    }

    public static List<int> GetRandomCountFromTotalExclude(int count, int start, int end, List<int> exclude)
    {
        List<int> range = new();
        for (int i = start; i <= end; i++)
        {
            if (exclude.Contains(i))
                continue;
            range.Add(i);
        }
        return GetRandomCount(count, range.ToArray());
    }
    public static List<Vector3Int[]> GetRandomCountV3(int count, params Vector3Int[][] arr)
    {
        List<Vector3Int[]> list = new();
        if (count >= arr.Length)
        {
            list.AddRange(arr);
            return list;
        }
        List<Vector3Int[]> parameters = arr.ToList();
        for (int i = 0; i < count; i++)
        {
            Vector3Int[] rnd = GetRandom(parameters.ToArray());
            Debug.Log("double random " + rnd[0] + rnd[1]);
            parameters.RemoveAll(v => v.Contains(rnd[0]) || v.Contains(rnd[1]));
            list.Add(rnd);
            if (parameters.Count == 0)
                return list;
        }
        return list;
    }

    public static bool IsActionAvailable(string action, int time, bool availableFirstTime = true)
    {
        if (!PlayerPrefs.HasKey(action + "_time"))
        {
            if (!availableFirstTime)
            {
                GameConfig.SetActionTime(action);
            }
            return availableFirstTime;
        }
        int num = (int)(GameConfig.GetCurrentTime() - GameConfig.GetActionTime(action));
        return num >= time;
    }

    public static double GetActionDeltaTime(string action)
    {
        if (GameConfig.GetActionTime(action) == 0.0)
        {
            return 0.0;
        }
        return GameConfig.GetCurrentTime() - GameConfig.GetActionTime(action);
    }

    public static void SetActionTime(string action)
    {
        GameConfig.SetDouble(action + "_time", GameConfig.GetCurrentTime());
    }
    public static void DeleteActionTime(string action)
    {
        PlayerPrefs.DeleteKey(action + "_time");
    }

    public static void SetActionTime(string action, double time)
    {
        GameConfig.SetDouble(action + "_time", time);
    }

    public static double GetActionTime(string action)
    {
        return GameConfig.GetDouble(action + "_time");
    }

    public static bool IsiOSVersionAtLeast14()
    {
#if UNITY_IOS && !UNITY_EDITOR
        string version = UnityEngine.iOS.Device.systemVersion;
        // iOS "14.2" split
        int major = 0;
        if (!string.IsNullOrEmpty(version))
        {
            string[] parts = version.Split('.');
            if (parts.Length > 0)
                int.TryParse(parts[0], out major);
        }
        return major >= 14;
#else
        return false; // not iOS return false
#endif
    }
    #endregion

    #region ads

    public static bool disableCommonAd = false;
#if UNITY_STANDALONE_WIN
    public const bool CHEAT_ENABLED = true;
#else //if UNITY_ANDROID
    public const bool CHEAT_ENABLED = false;
#endif
    public static void SetDisableCommonAd(bool value)
    {
        disableCommonAd = value;
    }
    public static bool IsDisableCommonAd()
    {
        Debug.Log(disableCommonAd);
        return disableCommonAd;
    }
    public static void SetRemoveAds(bool value)
    {
        GameConfig.SetBool("remove_ads", value);
    }

    public static bool IsAdsRemoved()
    {
        return GameConfig.GetBool("remove_ads", false);
    }
    public static void ShowAppOpenAd()
    {
        if (GameConfig.IsDisableCommonAd() || GameConfig.IsAdsRemoved() || CHEAT_ENABLED)
        {
            return;
        }
        if (GameConfig.GetCurrentLevel() < Common.ads_aoa_start_level)
            return;
        if (IsActionAvailable("ads_aoa", Common.ads_aoa_duration, true))
        {
            if (Common.ads_aoa_type == 1 && AdmobAd.instance.IsAppOpenAdAvailable)
            {
                AdmobAd.instance.ShowAppOpenAd();
                GameConfig.SetActionTime("ads_aoa");
            }
            else if (Common.ads_aoa_type == 2 && MaxAd.instance.IsAOAReady())
            {
                MaxAd.instance.ShowAOAIfReady();
                GameConfig.SetActionTime("ads_aoa");
            }
            else
            {
                Debug.Log("aoa not ready");
            }
        }
        else
        {
            Debug.Log("aoa capping");
        }
    }
    public static bool isMaxBanner;
    public static bool isMrec;
    public static void SetBannerType()
    {
        //run once
        if (Common.ads_banner_type == 2)
            isMaxBanner = true;
        else
            isMaxBanner = false;
    }
    public static void ShowCollapseBannerAd()
    {
        if (GameConfig.IsAdsRemoved() || CHEAT_ENABLED)
        {
            return;
        }
        if (!Common.ads_collapse_show || GameConfig.GetCurrentLevel() < Common.ads_collapse_start_level || isMaxBanner)
            return;
        if (IsActionAvailable("ads_collaps", Common.ads_collapse_duration, true))
        {
            Debug.Log("Showing collapse banner");
            HideMrec();
            AdmobAd.instance.LoadBannerAd(true);
            GameConfig.SetActionTime("ads_collaps");
        }
        else
        {
            Debug.Log("collapse capping");
        }
    }
    public static void ShowBannerAd()
    {
        if (GameConfig.IsAdsRemoved() || CHEAT_ENABLED)
        {
            return;
        }
        HideMrec();
        if (!Common.ads_banner_show)
            return;
        if (!isMaxBanner)
        {
            Debug.Log("Show admob banner");
            AdmobAd.instance.ShowBannerAd();
        }
        else
        {
            Debug.Log("Show max banner");
            MaxAd.instance.ShowBanner();
        }
    }
    public static void HideBannerAd()
    {
        if (AdmobAd.instance != null)
            AdmobAd.instance.HideBannerAd();
        if (MaxAd.instance != null)
            MaxAd.instance.HideBanner();
    }
    public static void DestroyBannerAd()
    {
        if (AdmobAd.instance != null)
            AdmobAd.instance.DestroyBannerAd();
        if (MaxAd.instance != null)
            MaxAd.instance.DestroyBanner();
    }
    public static void ShowMrec()
    {
        if (GameConfig.IsAdsRemoved() || CHEAT_ENABLED)
        {
            return;
        }
        if (!Common.ads_mrec_show)
            return;
        isMrec = true;
        MaxAd.instance.ShowMrec();
    }
    public static void HideMrec()
    {
        isMrec = false;
        if (MaxAd.instance != null)
            MaxAd.instance.HideMrec();
    }
    public static void DestroyMrec()
    {
        isMrec = false;
        if (MaxAd.instance != null)
            MaxAd.instance.DestroyMrec();
    }
    public static int levelInterCounter = 0;
    public static bool notFirstInter;
    public static int Inter_duration
    {
        get
        {
            if (notFirstInter)
                return Common.ads_inter_duration;
            else
                return Common.ads_inter_first_show;
        }
    }
    public static bool InterStartLevel()
    {
        //if (levelInterCounter < Common.ads_inter_level_range)
        levelInterCounter++;
        if (Common.ads_inter_shown_on_level_start && levelInterCounter >= Common.ads_inter_level_range)
            return true;
        return false;
    }
    public static bool InterEndLevel()
    {
        if (Common.ads_inter_shown_on_level_end && levelInterCounter >= Common.ads_inter_level_range)
            return true;
        return false;
    }
    public static void ShowInterstitialAd(bool adBreak = false)
    {
        if (GameConfig.IsAdsRemoved() || CHEAT_ENABLED)
        {
            return;
        }
        if (GameConfig.GetCurrentLevel() < Common.ads_inter_start_level)
            return;
        if (adBreak)
        {
            notFirstInter = true;
            if (Common.ads_inter_type == 1 && AdmobAd.instance.IsInterstitialAdBreakAvailable)
            {
                SetDisableCommonAd(true);
                Debug.Log("Showing interstitial ad break.");
                AdmobAd.instance.ShowInterstitialAdAdBreak();
                GameConfig.SetActionTime("ads_inter");
            }
            else if (Common.ads_inter_type == 2 && MaxAd.instance.IsInterstitialAdBreakAvailable())
            {
                SetDisableCommonAd(true);
                Debug.Log("Showing interstitial ad break.");
                MaxAd.instance.ShowInterstitialAdBreak();
                GameConfig.SetActionTime("ads_inter");
            }
        }
        else
        {
            if (GameConfig.IsActionAvailable("ads_inter", Inter_duration, true))
            {
                notFirstInter = true;
                levelInterCounter = 0;
                if (Common.ads_inter_type == 1 && AdmobAd.instance.IsInterstitialAvailable)
                {
                    SetDisableCommonAd(true);
                    Debug.Log("Showing interstitial ad.");
                    AdmobAd.instance.ShowInterstitialAd();
                    GameConfig.SetActionTime("ads_inter");
                }
                else if (Common.ads_inter_type == 2 && MaxAd.instance.IsInterstitialAvailable())
                {
                    SetDisableCommonAd(true);
                    Debug.Log("Showing interstitial ad.");
                    MaxAd.instance.ShowInterstitial();
                    GameConfig.SetActionTime("ads_inter");
                }
            }
            else
            {
                Debug.Log("inter capping");
            }
        }
    }
    public static bool InterAvailable(bool adBreak = false) //for adbreak
    {
        if (GameConfig.IsAdsRemoved() || CHEAT_ENABLED)
        {
            return false;
        }
        if (GameConfig.GetCurrentLevel() < Common.ads_inter_start_level)
            return false;
        bool interAvail = false;
        if (Common.ads_inter_type == 1)
            interAvail = adBreak ? AdmobAd.instance.IsInterstitialAdBreakAvailable : AdmobAd.instance.IsInterstitialAvailable;
        else if (Common.ads_inter_type == 2)
            interAvail = adBreak ? MaxAd.instance.IsInterstitialAdBreakAvailable() : MaxAd.instance.IsInterstitialAvailable();
        return interAvail && GameConfig.IsActionAvailable("ads_inter", adBreak ? 0 : Inter_duration, true);
    }
    public static bool IsRewardedAdAvailable()
    {
        if (CHEAT_ENABLED)
            return true;

        if (Common.ads_reward_type == 1)
            return AdmobAd.instance != null && AdmobAd.instance.IsRewardedAdAvailable;
        if (Common.ads_reward_type == 2)
            return MaxAd.instance != null && MaxAd.instance.IsRewardedAdAvailable();

        return false;
    }
    public static void ShowRewardedAd(int rewardType)
    {
        if (CHEAT_ENABLED)
        {
            AdmobAd.instance.TYPE_REWARD = rewardType;
            AdmobAd.instance.GetReward();
            return;
        }
        if (Common.ads_reward_type == 1)
        {
            if (AdmobAd.instance.IsRewardedAdAvailable)
            {
                SetDisableCommonAd(true);
                Debug.Log("Showing rewarded ad.");
                AdmobAd.instance.TYPE_REWARD = rewardType;
                AdmobAd.instance.ShowRewardedAd();
                if (Common.rewarded_reset_capping_time)
                    GameConfig.SetActionTime("ads_inter");
            }
            else
                ToastScript.instance.Toast(1);
        }
        else if (Common.ads_reward_type == 2)
        {
            if (MaxAd.instance.IsRewardedAdAvailable())
            {
                SetDisableCommonAd(true);
                Debug.Log("Showing rewarded ad.");
                AdmobAd.instance.TYPE_REWARD = rewardType;
                MaxAd.instance.ShowRewardedAd();
                if (Common.rewarded_reset_capping_time)
                    GameConfig.SetActionTime("ads_inter");
            }
            else
                ToastScript.instance.Toast(1);
        }
    }
    #endregion
}
