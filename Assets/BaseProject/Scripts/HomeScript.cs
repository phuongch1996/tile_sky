using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScript : MonoBehaviour
{
	public static HomeScript instance;

	public GameObject canvasBlocker;

	public CanvasBG canvasBG;

	public TopBarScript topBarScript;

	public void OnAddGold()
	{
		GameConfig.AddGold(100000);
		PlayerPrefs.Save();
	}
	public TMP_InputField levelInput;

	private void OnEnable()
	{
		SetupTestControls();
	}

	private void SetupTestControls()
	{
		if (levelInput == null)
			return;

		levelInput.gameObject.SetActive(true);
		var inputRect = (RectTransform)levelInput.transform;
		inputRect.localScale = Vector3.one;
		inputRect.anchorMin = inputRect.anchorMax = new Vector2(0.5f, 0.5f);
		inputRect.anchoredPosition = new Vector2(0, -40);
		inputRect.sizeDelta = new Vector2(440, 100);
		inputRect.SetAsLastSibling();
		levelInput.textComponent.fontSize = 40;
		if (levelInput.placeholder is TMP_Text placeholder)
		{
			placeholder.text = "Enter level";
			placeholder.fontSize = 36;
		}

		var goldTransform = inputRect.Find("AddGoldButton");
		Button goldButton;
		if (goldTransform == null)
		{
			// Older scenes already loaded in the Editor may not contain the new button.
			var template = inputRect.GetComponentInChildren<Button>(true);
			if (template == null)
				return;
			goldButton = Instantiate(template, inputRect);
			goldButton.name = "AddGoldButton";
		}
		else
		{
			goldButton = goldTransform.GetComponent<Button>();
		}
		goldButton.onClick = new Button.ButtonClickedEvent();
		goldButton.onClick.AddListener(OnAddGold);
		goldButton.GetComponentInChildren<TMP_Text>(true).text = "+100000 gold";

		int index = 0;
		foreach (var button in inputRect.GetComponentsInChildren<Button>(true))
		{
			button.gameObject.SetActive(true);
			var buttonRect = (RectTransform)button.transform;
			buttonRect.localScale = Vector3.one;
			buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
			buttonRect.anchoredPosition = new Vector2((index % 3 - 1) * 320, -120 - index / 3 * 100);
			buttonRect.sizeDelta = new Vector2(300, 85);
			var label = button.GetComponentInChildren<TMP_Text>(true);
			if (label != null)
			{
				label.enableAutoSizing = true;
				label.fontSizeMin = 22;
				label.fontSizeMax = 34;
			}
			index++;
		}
	}

	public Button btPlay;

	public TextMeshProUGUI txPlayButton;

	public Button btChallengeLevel;

	public TextMeshProUGUI txChallengeLevel;

	public Button btSettings;

	public PopupSettings settingsPopup;

	public PopupRateUs rateUsPopup;

	public Button btShop;

	public PopupShop shopPopup;

	public Button btDailyLogin;

	public PopupDailyLogin dailyLoginPopup;

	public GameObject dailyLoginNotif;

	public TextMeshProUGUI txDailyLoginTimer;

	public Button btEvent;

	public TextMeshProUGUI txBtEvent;

	public GameObject eventNotif;

	public GameObject[] eventIcons;

	public TextMeshProUGUI txEventTimer;

	public PopupCollectionEvent collectionEventPopup;

	public PopupStreakEvent streakEventPopup;

	public PopupComboEvent comboEventPopup;

	public PopupEventTutorial collectionTutorialPopup;

	public PopupEventTutorial comboTutorialPopup;

	public Button btBattlePass;

	public PopupBattlePass battlePassPopup;

	public GameObject battlePassNotif;

	public TextMeshProUGUI txBattlePassTimer;

	public PopupBonus bonusPopup;

	public Button btMap;

	public PopupMap mapPopup;

	public PopupMapPreview mapPreviewPopup;

	public Button btFreeGold;

	public PopupFreeGold freeGoldPopup;

	public Button btPiggyBank;

	public TextMeshProUGUI txPiggyValue;

	public GameObject piggyNotif;

	public TextMeshProUGUI txPiggyTimer;

	public PopupPiggyBank piggyBankPopup;

	public Button btQuest;

	public GameObject questNotif;

	public PopupQuest questPopup;

	public Button btRank;

	public PopupRank rankPopup;

	public PopupChallengeInfo challengeInfoPopup;

	public PopupChallengeReward challengeRewardPopup;

	public Button btEditPlayer;

	public PopupEditPlayer editPlayerPopup;

	public Image imgFrame;

	public Image imgAvatar;

	public Image imgFlag;

	public TextMeshProUGUI txDisplayName;

	public void AdmobInspector()
	{
		AdmobAd.instance.OpenAdInspector();
	}

	public void MaxInspector()
	{
		MaxSdk.ShowMediationDebugger();
	}

	private void Awake()
	{
		instance = this;
		Popup[] array = UnityEngine.Object.FindObjectsByType<Popup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetupTweenCallback();
		}
		btDailyLogin.transform.localScale = Vector3.zero;
		btFreeGold.transform.localScale = Vector3.zero;
		btEvent.transform.localScale = Vector3.zero;
		btPiggyBank.transform.localScale = Vector3.zero;
		btBattlePass.transform.localScale = Vector3.zero;
		btPlay.transform.localScale = Vector3.zero;
		btChallengeLevel.transform.localScale = Vector3.zero;
		if (GameConfig.isGameLoaded)
		{
			HomeSetup();
		}
	}

	private void Start()
	{
		PiggySetup();
		if (QuestManager.instance.isInit)
		{
			QuestNotif();
		}
		if (GameConfig.isGameLoaded)
		{
			GameConfig.ShowCollapseBannerAd();
			HomeUI(firstLoad: false);
		}
	}

	public void HomeUI(bool firstLoad = true)
	{
		StartCoroutine(HomeAnim(firstLoad));
	}

	private void HomeSetup()
	{
		SetupTestControls();
		UpdateInfo();
		int currentLevel = GameConfig.GetCurrentLevel();
		txPlayButton.text = ((currentLevel > 1) ? ("level " + currentLevel) : "play!!!");
		btSettings.gameObject.SetActive(value: true);
		btMap.gameObject.SetActive(currentLevel >= 11);
		btQuest.gameObject.SetActive(currentLevel >= 11);
		btDailyLogin.gameObject.SetActive(currentLevel >= 5);
		btFreeGold.gameObject.SetActive(currentLevel >= 8);
		btPiggyBank.gameObject.SetActive(currentLevel >= Common.config_unlock_piggybank);
		btRank.gameObject.SetActive(currentLevel >= Common.config_unlock_rank && UnityAuthentication.IsLeaderboard());
		btEvent.gameObject.SetActive(currentLevel >= 15 && EventManager.instance.isEvent);
		btBattlePass.gameObject.SetActive(currentLevel >= Common.config_unlock_battlepass);
		btChallengeLevel.gameObject.SetActive(currentLevel >= Common.config_unlock_dailychallenge);
		ExpandableButtonMenu[] array = UnityEngine.Object.FindObjectsByType<ExpandableButtonMenu>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].RefreshChildButtonAvailability();
		}
		GameObject[] array2 = eventIcons;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].SetActive(value: false);
		}
		switch (EventManager.eventType)
		{
		case EventType.collect_event_1:
			txBtEvent.text = "collection";
			eventIcons[0].SetActive(value: true);
			break;
		case EventType.streak_event_1:
			txBtEvent.text = "streak";
			eventIcons[0].SetActive(value: true);
			break;
		case EventType.combo_event_1:
			txBtEvent.text = "combo";
			eventIcons[1].SetActive(value: true);
			break;
		default:
			txBtEvent.text = "";
			break;
		}
	}

	public IEnumerator HomeAnim(bool firstLoad)
	{
		if (firstLoad)
		{
			HomeSetup();
		}
		yield return null;
		AudioManager.instance.PlayBGMHome();
		if (firstLoad && btDailyLogin.gameObject.activeSelf && GameConfig.IsDailyLogin() && GameConfig.IsDailyLoginAvailable())
		{
			OnDailyLogin();
			yield return null;
			yield return new WaitUntil(() => !dailyLoginPopup.Showing);
		}
		if (GameConfig.IsDailyChallenge && UnityAuthentication.instance.IsPastLeaderboardPrizeClaim())
		{
			OnChallengeReward();
		}
		float delay0 = 0f;
		yield return null;
		if (btDailyLogin.gameObject.activeSelf && PlayerPrefs.GetInt("dailyloginfirst", 0) == 0)
		{
			Vector3 pos = btDailyLogin.transform.position;
			btDailyLogin.transform.position = Vector3.zero;
			PlayerPrefs.SetInt("dailyloginfirst", 1);
			yield return btDailyLogin.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack).OnComplete(delegate
			{
				btDailyLogin.transform.DOMove(pos, 0.5f).SetEase(Ease.OutBack);
				btDailyLogin.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
			})
				.WaitForCompletion();
		}
		else
		{
			float delay = delay0;
			delay0 += 0.1f;
			btDailyLogin.transform.DOScale(1f, 0.5f).SetDelay(delay).SetEase(Ease.OutBack);
		}
		if (btFreeGold.gameObject.activeSelf && PlayerPrefs.GetInt("freegoldfirst", 0) == 0)
		{
			Vector3 pos2 = btFreeGold.transform.position;
			btFreeGold.transform.position = Vector3.zero;
			PlayerPrefs.SetInt("freegoldfirst", 1);
			yield return btFreeGold.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack).OnComplete(delegate
			{
				btFreeGold.transform.DOMove(pos2, 0.5f).SetEase(Ease.OutBack);
				btFreeGold.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
			})
				.WaitForCompletion();
		}
		else
		{
			float delay2 = delay0;
			delay0 += 0.1f;
			btFreeGold.transform.DOScale(1f, 0.5f).SetDelay(delay2).SetEase(Ease.OutBack);
		}
		if (btEvent.gameObject.activeSelf && PlayerPrefs.GetInt("eventfirst", 0) == 0)
		{
			Vector3 pos3 = btEvent.transform.position;
			btEvent.transform.position = Vector3.zero;
			PlayerPrefs.SetInt("eventfirst", 1);
			yield return btEvent.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack).OnComplete(delegate
			{
				btEvent.transform.DOMove(pos3, 0.5f).SetEase(Ease.OutBack);
				btEvent.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
			})
				.WaitForCompletion();
		}
		else
		{
			float delay3 = delay0;
			delay0 += 0.1f;
			btEvent.transform.DOScale(1f, 0.5f).SetDelay(delay3).SetEase(Ease.OutBack);
		}
		if (btPiggyBank.gameObject.activeSelf && PlayerPrefs.GetInt("piggybankfirst", 0) == 0)
		{
			Vector3 pos4 = btPiggyBank.transform.position;
			btPiggyBank.transform.position = Vector3.zero;
			PlayerPrefs.SetInt("piggybankfirst", 1);
			yield return btPiggyBank.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack).OnComplete(delegate
			{
				btPiggyBank.transform.DOMove(pos4, 0.5f).SetEase(Ease.OutBack);
				btPiggyBank.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
			})
				.WaitForCompletion();
		}
		else
		{
			float delay4 = delay0;
			delay0 += 0.1f;
			btPiggyBank.transform.DOScale(1f, 0.5f).SetDelay(delay4).SetEase(Ease.OutBack);
		}
		if (btBattlePass.gameObject.activeSelf && PlayerPrefs.GetInt("battlepassfirst", 0) == 0)
		{
			Vector3 pos5 = btBattlePass.transform.position;
			btBattlePass.transform.position = Vector3.zero;
			PlayerPrefs.SetInt("battlepassfirst", 1);
			yield return btBattlePass.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack).OnComplete(delegate
			{
				btBattlePass.transform.DOMove(pos5, 0.5f).SetEase(Ease.OutBack);
				btBattlePass.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
			})
				.WaitForCompletion();
		}
		else
		{
			float delay5 = delay0;
			delay0 += 0.1f;
			btBattlePass.transform.DOScale(1f, 0.5f).SetDelay(delay5).SetEase(Ease.OutBack);
		}
		if (btChallengeLevel.gameObject.activeSelf)
		{
			float delay6 = delay0;
			delay0 += 0.1f;
			btChallengeLevel.transform.DOScale(1f, 0.5f).SetDelay(delay6).SetEase(Ease.OutBack);
		}
		btPlay.transform.DOScale(1f, 0.5f).SetDelay(delay0).SetEase(Ease.OutBack)
			.OnComplete(delegate
			{
				OnCompleteHomeUI();
			});
	}

	private void OnCompleteHomeUI()
	{
		btDailyLogin.onClick.AddListener(delegate
		{
			OnDailyLogin();
		});
		btSettings.onClick.AddListener(delegate
		{
			OnSettings();
		});
		btMap.onClick.AddListener(delegate
		{
			OnOpenMap();
		});
		btShop.onClick.AddListener(delegate
		{
			OnOpenShop();
		});
		btEvent.onClick.AddListener(delegate
		{
			OnOpenEvent();
		});
		btBattlePass.onClick.AddListener(delegate
		{
			OnOpenBattlePass();
		});
		btFreeGold.onClick.AddListener(delegate
		{
			OnOpenFreeGold();
		});
		btPiggyBank.onClick.AddListener(delegate
		{
			OnOpenPiggyBank();
		});
		btQuest.onClick.AddListener(delegate
		{
			OnOpenQuest();
		});
		btRank.onClick.AddListener(delegate
		{
			OnOpenRank();
		});
		btPlay.onClick.AddListener(delegate
		{
			Play();
		});
		btChallengeLevel.onClick.AddListener(delegate
		{
			PlayChallenge();
		});
		btEditPlayer.onClick.AddListener(delegate
		{
			OnOpenEditPlayer(delegate
			{
				UpdateInfo();
			});
		});
		canvasBlocker.SetActive(value: false);
	}

	private void OnValidate()
	{
		List<Popup> list = UnityEngine.Object.FindObjectsByType<Popup>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList();
		list.Sort((Popup a, Popup b) => GameConfig.CompareHierarchy(a.transform, b.transform));
		canvasBG.images = new List<Image> { canvasBG.bg };
		canvasBG.bgImages = new List<Image> { canvasBG.bg };
		foreach (Popup item in list)
		{
			if ((bool)item.bg)
			{
				canvasBG.images.Add(item.bg);
				if (item.isBGChange)
				{
					canvasBG.bgImages.Add(item.bg);
				}
			}
		}
	}

	public void TopBarUpdateGolds(Transform pos, int goldsGain, int mod = 1, Action callback = null)
	{
		StartCoroutine(topBarScript.UpdateGolds(pos, goldsGain, mod, callback));
	}

	public void Play()
	{
		GameConfig.reviveCount = 0;
		GameConfig.loseCount = 0;
		GameConfig.boosterCount = 0;
		GameConfig.isChallengeLevel = false;
		try
		{
			int num = int.Parse(levelInput.text);
			if (num <= 0)
			{
				return;
			}
			GameConfig.level = num;
			GameConfig.isTesting = true;
		}
		catch
		{
			if (GameplayProgressSave.TryLoad(out var data))
			{
				GameConfig.level = data.level;
			}
			else
			{
				GameConfig.level = GameConfig.GetCurrentLevel();
			}
			GameConfig.isTesting = false;
		}
		PoolManager.instance.ReleaseAllObjects();
		LoadingCamera.instance.StartLoad(GameConfig.gameScene);
	}

	public void PlayChallenge()
	{
		GameConfig.reviveCount = 0;
		GameConfig.loseCount = 0;
		GameConfig.boosterCount = 0;
		GameConfig.isChallengeLevel = true;
		try
		{
			UnityAuthentication.instance.dailyChallengeTestDayOfYear = int.Parse(levelInput.text);
			GameConfig.isTesting = true;
		}
		catch
		{
			GameConfig.isTesting = false;
		}
		PoolManager.instance.ReleaseAllObjects();
		LoadingCamera.instance.StartLoad(GameConfig.gameScene);
	}

	public void ChallengeLevelNotif()
	{
		if (btChallengeLevel.gameObject.activeSelf)
		{
			if (GameConfig.IsDailyChallenge)
			{
				int[] indexDate = GameConfig.GetIndexDate(GameConfig.GetCurrentDate());
				txChallengeLevel.text = $"{new DateTime(indexDate[0], indexDate[1], indexDate[2]):d}";
			}
			else
			{
				btChallengeLevel.gameObject.SetActive(value: false);
			}
		}
	}

	private void Update()
	{
		DailyLoginNotif();
		PiggyNotif();
		EventNotif();
		BattlePassNotif();
		ChallengeLevelNotif();
	}

	public void OnSettings()
	{
		settingsPopup.Open();
	}

	public void OpenRateUs()
	{
		rateUsPopup.Open();
	}

	public void OnOpenShop()
	{
		shopPopup.Open();
	}

	public void OnDailyLogin()
	{
		dailyLoginPopup.Open();
	}

	public void DailyLoginNotif()
	{
		if (GameConfig.IsDailyLoginAvailable())
		{
			dailyLoginNotif.SetActive(value: true);
			txDailyLoginTimer.gameObject.SetActive(value: false);
			return;
		}
		dailyLoginNotif.SetActive(value: false);
		TimeSpan timeSpan = TimeManager.instance.TimeToNextDay();
		txDailyLoginTimer.text = $"{Mathf.Floor((float)timeSpan.TotalHours):00}:{timeSpan:mm}:{timeSpan:ss}";
		txDailyLoginTimer.gameObject.SetActive(value: true);
	}

	public void OnOpenEvent()
	{
		switch (EventManager.eventType)
		{
		case EventType.collect_event_1:
			collectionEventPopup.Open();
			break;
		case EventType.streak_event_1:
			streakEventPopup.Open();
			break;
		case EventType.combo_event_1:
			comboEventPopup.Open();
			break;
		}
	}

	public void EventNotif()
	{
		if (!btEvent.gameObject.activeSelf)
		{
			return;
		}
		eventNotif.SetActive(EventManager.instance.IsPrizeClaimable());
		if (EventManager.instance.IsEventTimeLeft())
		{
			TimeSpan eventTimeLeft = EventManager.instance.GetEventTimeLeft();
			if (eventTimeLeft.TotalDays >= 1.0)
			{
				txEventTimer.text = $"{Mathf.Floor((float)eventTimeLeft.TotalDays)}d {eventTimeLeft:%h}h";
			}
			else
			{
				txEventTimer.text = $"{eventTimeLeft:%h}h {eventTimeLeft:%m}m";
			}
		}
		else
		{
			btEvent.gameObject.SetActive(value: false);
			EventManager.instance.isEvent = false;
		}
	}

	public void ResetEventLevel()
	{
		btEvent.gameObject.SetActive(value: false);
		EventManager.instance.isEvent = false;
		EventManager.instance.EndEvent();
	}

	public void OpenEventTutorial()
	{
		switch (EventManager.eventType)
		{
		case EventType.collect_event_1:
			collectionTutorialPopup.OpenTutorial();
			break;
		case EventType.combo_event_1:
			comboTutorialPopup.OpenTutorial();
			break;
		}
	}

	public void OnOpenBattlePass()
	{
		battlePassPopup.Open();
	}

	public void BattlePassNotif()
	{
		if (!btBattlePass.gameObject.activeSelf)
		{
			return;
		}
		if (BattlePassManager.instance.isBattlePass)
		{
			battlePassNotif.SetActive(BattlePassManager.instance.Claimable());
			TimeSpan timeSpan = BattlePassManager.instance.TimeLeftBattlePass();
			if (timeSpan.TotalDays >= 1.0)
			{
				txBattlePassTimer.text = $"{Mathf.Floor((float)timeSpan.TotalDays)}d {timeSpan:%h}h";
			}
			else
			{
				txBattlePassTimer.text = $"{timeSpan:%h}h {timeSpan:%m}m";
			}
		}
		else
		{
			btBattlePass.gameObject.SetActive(value: false);
		}
	}

	public void OpenBonus(bool daily)
	{
		bonusPopup.OpenBonus(daily);
	}

	public void OnOpenMap()
	{
		mapPopup.Open();
	}

	public void OnPreviewMap(int bgIndex, int bgType, bool unlock)
	{
		mapPreviewPopup.OpenMapPreview(bgIndex, bgType, unlock);
	}

	public void OnOpenFreeGold()
	{
		freeGoldPopup.Open();
	}

	public void PiggySetup()
	{
		txPiggyValue.text = $"{GameConfig.GetPiggyBank()}/{GameConfig.piggyMax1}";
	}

	public void OnOpenPiggyBank()
	{
		piggyBankPopup.Open();
	}

	public void PiggyNotif()
	{
		if (btPiggyBank.gameObject.activeSelf)
		{
			var (active, timeSpan) = TimeManager.instance.TimeEndPiggyBank();
			piggyNotif.SetActive(GameConfig.IsPiggySmash());
			btPiggyBank.gameObject.SetActive(active);
			txPiggyTimer.text = $"{Mathf.Floor((float)timeSpan.TotalHours):00}:{timeSpan:mm}:{timeSpan:ss}";
		}
	}

	public void BoughtPiggyBank()
	{
		btPiggyBank.gameObject.SetActive(value: false);
		piggyBankPopup.Close();
	}

	public void OnOpenQuest()
	{
		questPopup.Open();
	}

	public void QuestNotif()
	{
		questNotif.SetActive(QuestManager.GetDailyClaimAble() || QuestManager.GetAllTimeClaimAble());
	}

	public void OnOpenRank()
	{
		rankPopup.Open();
	}

	public void OnChallengeInfo()
	{
		challengeInfoPopup.Open();
	}

	public void OnChallengeReward()
	{
		int[] indexDate = GameConfig.GetIndexDate(UnityAuthentication.GetDailyChallengeLeaderboardDate());
		challengeRewardPopup.OpenChallengeReward(new DateTime(indexDate[0], indexDate[1], indexDate[2]), UnityAuthentication.instance.pastEntry.Rank);
	}

	public void TestChallengeReward()
	{
		if (UnityAuthentication.instance.dailyChallengeScore != null)
		{
			int[] indexDate = GameConfig.GetIndexDate(UnityAuthentication.GetDailyChallengeLeaderboardDate());
			challengeRewardPopup.OpenChallengeReward(new DateTime(indexDate[0], indexDate[1], indexDate[2]), UnityAuthentication.instance.dailyChallengeScore.Rank, test: true);
		}
	}

	public void OnOpenEditPlayer(Action callback)
	{
		editPlayerPopup.OpenWithCallback(callback);
	}

	public void UpdateInfo()
	{
		imgFrame.sprite = UnityAuthentication.instance.GetFrameSprite(GameConfig.GetPlayerFrame());
		imgAvatar.sprite = UnityAuthentication.instance.GetAvatarSprite(GameConfig.GetPlayerAvatar());
		imgFlag.sprite = UnityAuthentication.instance.GetFlagSprite(GameConfig.GetPlayerFlag());
		txDisplayName.text = GameConfig.GetDisplayName();
	}
}
