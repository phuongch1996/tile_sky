/// <summary>
/// Official Google demo ad units, configured locally without a remote backend.
/// https://developers.google.com/admob/unity/test-ads
/// MAX remains installed, but cannot request ads without a new project configuration.
/// </summary>
public static class GoogleTestAds
{
    public static bool MaxEnabled => false;

#if UNITY_IOS
    public const string AppOpen = "ca-app-pub-3940256099942544/5575463023";
    public const string Banner = "ca-app-pub-3940256099942544/2435281174";
    public const string Interstitial = "ca-app-pub-3940256099942544/4411468910";
    public const string Rewarded = "ca-app-pub-3940256099942544/1712485313";
    public const string Native = "ca-app-pub-3940256099942544/3986624511";
#else
    public const string AppOpen = "ca-app-pub-3940256099942544/9257395921";
    public const string Banner = "ca-app-pub-3940256099942544/9214589741";
    public const string Interstitial = "ca-app-pub-3940256099942544/1033173712";
    public const string Rewarded = "ca-app-pub-3940256099942544/5224354917";
    public const string Native = "ca-app-pub-3940256099942544/2247696110";
#endif
}
