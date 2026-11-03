using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public partial class RankCard : PoolObjectUI
{
    public bool isTop3;
    [Header("Rank")]
    public TextMeshProUGUI txRank;
    public Image imgRankIcon;
    public Sprite[] topRankSprites = new Sprite[3];

    [Header("Player")]
    public Image imgAvatar;
    public Image imgFrame;
    [FormerlySerializedAs("imgFlag")] public Image imgGift;
    public TextMeshProUGUI txDisplayName;
    public TextMeshProUGUI txScore;

    [Header("Gift")]
    public Sprite[] rankGiftSprites = new Sprite[10];

    Sprite defaultRankIconSprite;
    Color defaultRankIconColor = Color.white;
    bool defaultRankIconCached;
    bool isDailyChallengeCard;

    public override void OnValidate()
    {
        ResetValues();
        base.OnValidate();
    }
    void ResetValues()
    {
        CacheDefaultVisuals();

        if (imgAvatar)
            imgAvatar.color = new Color(0, 0, 0, 0);
        if (imgFrame)
            imgFrame.color = new Color(0, 0, 0, 0);
        if (imgGift)
            imgGift.color = new Color(0, 0, 0, 0);
        if (imgRankIcon)
        {
            imgRankIcon.sprite = defaultRankIconSprite;
            imgRankIcon.color = defaultRankIconColor;
        }
        if (txDisplayName)
            txDisplayName.text = "";
        if (txScore)
            txScore.text = "";
        if (txRank)
            txRank.text = "";
    }
    public override void Inactive()
    {
        base.Inactive();
        ResetValues();
    }
    public void CardSetup(int rank, int avatar, string displayName, int frame, int flag, int score, bool dailyChallenge = false)
    {
        ApplyTileSkyStyle();
        isDailyChallengeCard = dailyChallenge;
        CacheDefaultVisuals();
        SetRank(rank);
        SetGift(dailyChallenge ? rank : -1);

        if (!dailyChallenge)
        {
            txScore.text = score.ToString();
        }
        else
        {
            TimeSpan ts = TimeSpan.FromSeconds(score);
            txScore.text = $"{Mathf.Floor((float)ts.TotalMinutes):00}:{ts:ss}";
        }
        imgAvatar.sprite = UnityAuthentication.instance.GetAvatarSprite(avatar);
        imgAvatar.color = Color.white;
        txDisplayName.text = displayName;
        imgFrame.sprite = UnityAuthentication.instance.GetFrameSprite(frame);
        imgFrame.color = Color.white;
    }
    public void UpdateInfo(int avatar, string displayName, int frame, int flag)
    {
        imgAvatar.sprite = UnityAuthentication.instance.GetAvatarSprite(avatar);
        txDisplayName.text = displayName;
        imgFrame.sprite = UnityAuthentication.instance.GetFrameSprite(frame);
    }
    public void UpdateRank(int rank)
    {
        CacheDefaultVisuals();
        SetRank(rank);
        SetGift(isDailyChallengeCard ? rank : -1);
    }

    void CacheDefaultVisuals()
    {
        if (defaultRankIconCached || !imgRankIcon)
            return;

        defaultRankIconSprite = imgRankIcon.sprite;
        defaultRankIconColor = imgRankIcon.color;
        defaultRankIconCached = true;
    }

    void SetRank(int rank)
    {
        bool hasTopIcon = rank >= 0 && rank < 3 && imgRankIcon && rank < topRankSprites.Length && topRankSprites[rank];

        if (imgRankIcon)
        {
            imgRankIcon.sprite = hasTopIcon ? topRankSprites[rank] : defaultRankIconSprite;
            imgRankIcon.color = hasTopIcon ? Color.white : defaultRankIconColor;
            imgRankIcon.type = hasTopIcon || !tileSkyStyled ? Image.Type.Simple : Image.Type.Sliced;
            imgRankIcon.preserveAspect = true;
        }

        if (!txRank)
            return;

        txRank.text = hasTopIcon ? "" : rank > -1 ? (rank + 1).ToString() : "_";
    }

    void SetGift(int rank)
    {
        if (!imgGift)
            return;

        bool hasGift = rank >= 0 && rank < rankGiftSprites.Length && rankGiftSprites[rank];
        imgGift.sprite = hasGift ? rankGiftSprites[rank] : null;
        imgGift.type = Image.Type.Simple;
        imgGift.preserveAspect = true;
        imgGift.color = hasGift ? Color.white : new Color(0, 0, 0, 0);
    }
}
