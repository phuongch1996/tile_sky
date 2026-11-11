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

	[SerializeField, Min(0.1f)] private float playExitDuration = 1f;
	[SerializeField] private RectTransform homeLogo;
	[SerializeField, Min(0.1f)] private float homeLogoRotateDuration = 5f;
	private const float HomeLogoSwayAngle = 3.5f;
	private const float HomeLogoPulseScale = 1.04f;
	private Tween homeLogoRotateTween;
	private Tween homeLogoPulseTween;
	private Vector3 homeLogoBaseScale;
	private enum PlayExitStyle { StaggeredEdges, Up, Down, Left, Right }
	private bool startingLevel;
	private Sequence playExitSequence;
	private Sequence homeEntranceSequence;
	private Coroutine homeAnimation;
	private readonly Dictionary<RectTransform, Vector3> homeIconPositions = new Dictionary<RectTransform, Vector3>();
	private RectTransform homeFeatureRow;

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
		ResetPlayTransition();
		SetupRewardButtonImages();
		SetupTestControls();
	}

	private void SetupRewardButtonImages()
	{
		// The old animated icons used disabled Images on the button roots.
		// Enable the replacement sprites even in an already-loaded Home scene.
		foreach (var button in new[] { btDailyLogin, btBattlePass })
		{
			if (button == null || button.image == null)
				continue;

			button.image.enabled = true;
			button.image.overrideSprite = null;
			button.image.color = Color.white;
			button.image.preserveAspect = true;
			button.image.canvasRenderer.SetAlpha(1f);
		}
	}

	private void OnDisable()
	{
		ResetPlayTransition();
		StopHomeLogoIdleAnimation();
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
		AdmobInspector();
	}

	private void Awake()
	{
		instance = this;
		QuestNotif();
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
		if (GameConfig.isGameLoaded)
		{
			GameConfig.ShowCollapseBannerAd();
			if (LoadingCamera.instance != null && LoadingCamera.instance.IsHomeReturnTransition)
				AnimateReturnHomeEntrance();
			else
				HomeUI(firstLoad: false);
		}
	}

	private void AnimateReturnHomeEntrance()
	{
		AudioManager.instance.PlayBGMHome();
		var icons = GetHomeExitIcons();
		if (canvasBlocker != null)
		{
			canvasBlocker.SetActive(true);
			canvasBlocker.transform.SetAsLastSibling();
		}
		homeEntranceSequence = DOTween.Sequence().SetUpdate(true);
		float stagger = icons.Count > 1 ? 0.3f / (icons.Count - 1) : 0f;
		for (int i = 0; i < icons.Count; i++)
		{
			var icon = icons[i];
			icon.DOKill();
			var targetScale = icon.localScale == Vector3.zero ? Vector3.one : icon.localScale;
			icon.localScale = Vector3.zero;
			homeEntranceSequence.Insert(i * stagger,
				icon.DOScale(targetScale, 0.45f).SetEase(Ease.OutBack));
		}
		homeEntranceSequence.OnComplete(() =>
		{
			homeEntranceSequence = null;
			StartHomeLogoIdleAnimation();
			OnCompleteHomeUI();
		});
	}

	private RectTransform ResolveHomeLogo()
	{
		if (homeLogo == null && btPlay != null && btPlay.transform.parent != null)
			homeLogo = btPlay.transform.parent.Cast<Transform>().OfType<RectTransform>()
				.FirstOrDefault(rect => rect.name.StartsWith("logo", StringComparison.OrdinalIgnoreCase));
		return homeLogo;
	}

	private void StartHomeLogoIdleAnimation()
	{
		var logo = ResolveHomeLogo();
		if (logo == null)
			return;

		StopHomeLogoIdleAnimation();
		logo.localRotation = Quaternion.identity;
		homeLogoBaseScale = logo.localScale == Vector3.zero ? Vector3.one : logo.localScale;

		// A slow sway plus a soft breathing pulse so the already-looping
		// Spine animation feels alive instead of sitting perfectly still.
		float half = Mathf.Max(0.1f, homeLogoRotateDuration) * 0.5f;
		homeLogoRotateTween = logo.DORotate(new Vector3(0f, 0f, HomeLogoSwayAngle), half)
			.SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
		homeLogoPulseTween = logo.DOScale(homeLogoBaseScale * HomeLogoPulseScale, half)
			.SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
	}

	private void StopHomeLogoIdleAnimation()
	{
		// Only restore the transform when an idle animation was actually running,
		// so scene-load calls (OnEnable) never overwrite the designed scale.
		bool wasAnimating = homeLogoRotateTween != null || homeLogoPulseTween != null;
		homeLogoRotateTween?.Kill(false);
		homeLogoRotateTween = null;
		homeLogoPulseTween?.Kill(false);
		homeLogoPulseTween = null;
		if (wasAnimating && homeLogo != null)
		{
			homeLogo.localRotation = Quaternion.identity;
			homeLogo.localScale = homeLogoBaseScale == Vector3.zero ? Vector3.one : homeLogoBaseScale;
		}
	}

	public void HomeUI(bool firstLoad = true)
	{
		ResetPlayTransition();
		if (homeAnimation != null)
			StopCoroutine(homeAnimation);
		homeAnimation = StartCoroutine(HomeAnim(firstLoad));
	}

	private void HomeSetup()
	{
		SetupRewardButtonImages();
		SetupTestControls();
		UpdateInfo();
		int currentLevel = GameConfig.GetCurrentLevel();
		txPlayButton.text = ((currentLevel > 1) ? ("level " + currentLevel) : "play!!!");
		btSettings.gameObject.SetActive(value: true);
		btMap.gameObject.SetActive(currentLevel >= 11);
		QuestNotif();
		btDailyLogin.gameObject.SetActive(currentLevel >= 5);
		btFreeGold.gameObject.SetActive(currentLevel >= 8);
		btPiggyBank.gameObject.SetActive(currentLevel >= Common.config_unlock_piggybank);
		btRank.gameObject.SetActive(currentLevel >= Common.config_unlock_rank);
		btEvent.gameObject.SetActive(currentLevel >= 15 && EventManager.instance.isEvent);
		btBattlePass.gameObject.SetActive(currentLevel >= Common.config_unlock_battlepass);
		btChallengeLevel.gameObject.SetActive(currentLevel >= Common.config_unlock_dailychallenge);
		SetupStandaloneHomeFeatures();
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

	private void SetupStandaloneHomeFeatures()
	{
		if (homeFeatureRow == null)
		{
			var avatar = (RectTransform)btEditPlayer.transform;
			var menu = btSettings.GetComponentInParent<ExpandableButtonMenu>(true);
			if (menu == null) return;
			var menuRect = (RectTransform)menu.transform;
			homeFeatureRow = new GameObject("Home feature row", typeof(RectTransform)).GetComponent<RectTransform>();
			homeFeatureRow.gameObject.layer = avatar.gameObject.layer;
			homeFeatureRow.SetParent(avatar.parent, false);
			homeFeatureRow.anchorMin = avatar.anchorMin;
			homeFeatureRow.anchorMax = avatar.anchorMax;
			homeFeatureRow.anchoredPosition = avatar.anchoredPosition;
			homeFeatureRow.sizeDelta = Vector2.zero;

			// Move the whole profile, including its nameplate, to the old menu slot.
			avatar.SetParent(menuRect.parent, false);
			avatar.anchorMin = menuRect.anchorMin;
			avatar.anchorMax = menuRect.anchorMax;
			avatar.anchoredPosition = menuRect.anchoredPosition;
			menu.ReleaseButtonsForStandaloneLayout();
			foreach (var button in new[] { btSettings, btQuest, btMap, btRank })
			{
				if (button != null) button.transform.SetParent(homeFeatureRow, false);
			}
			menu.gameObject.SetActive(false);
		}

		btSettings.gameObject.SetActive(true);
		var backgroundIconSize = ((RectTransform)btMap.transform).sizeDelta;
		foreach (var button in new[] { btSettings, btRank })
		{
			var rect = (RectTransform)button.transform;
			// These 256x234 sprites have different transparent margins. Match the
			// visible artwork height to Background's 208px, rather than the canvas.
			float artworkScale = button == btSettings ? 208f / 183f : 208f / 197f;
			rect.sizeDelta = backgroundIconSize * artworkScale;
			if (button.image != null) button.image.preserveAspect = true;
		}
		var features = new[]
		{
			new { Button = btSettings, Level = int.MinValue },
			new { Button = btQuest, Level = 0 },
			new { Button = btMap, Level = 11 },
			new { Button = btRank, Level = Common.config_unlock_rank }
		};
		int index = 0;
		foreach (var feature in features.OrderBy(feature => feature.Level))
		{
			if (feature.Button == null || !feature.Button.gameObject.activeSelf) continue;
			var rect = (RectTransform)feature.Button.transform;
			rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
			float spacing = Mathf.Max(150f, backgroundIconSize.x * (208f / 183f) + 12f);
			rect.anchoredPosition = new Vector2(index++ * spacing, 0);
			rect.SetAsLastSibling();
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
				StartHomeLogoIdleAnimation();
				OnCompleteHomeUI();
			});
	}

	private void OnCompleteHomeUI()
	{
		if (startingLevel)
			return;
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
		if (startingLevel)
			return;
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
		AnimateHomeExit();
	}

	public void PlayChallenge()
	{
		if (startingLevel)
			return;
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
		AnimateHomeExit();
	}

	private void AnimateHomeExit()
	{
		startingLevel = true;
		StopHomeLogoIdleAnimation();
		if (homeAnimation != null)
		{
			StopCoroutine(homeAnimation);
			homeAnimation = null;
		}
		if (levelInput != null)
			levelInput.DeactivateInputField();
		if (canvasBlocker != null)
		{
			canvasBlocker.SetActive(true);
			canvasBlocker.transform.SetAsLastSibling();
		}

		Canvas.ForceUpdateCanvases();
		var icons = GetHomeExitIcons();
		var style = (PlayExitStyle)UnityEngine.Random.Range(0, 5);
		bool staggered = style == PlayExitStyle.StaggeredEdges;
		float duration = Mathf.Max(0.1f, playExitDuration);
		float slideDuration = icons.Count > 1 ? duration * 0.4f : duration;
		float stagger = icons.Count > 1 ? (duration - slideDuration) / (icons.Count - 1) : 0f;
		var groupOffset = staggered ? Vector3.zero : GetHomeGroupExitOffset(icons, style);
		playExitSequence = DOTween.Sequence().SetUpdate(true);
		playExitSequence.AppendInterval(duration);
		for (int i = 0; i < icons.Count; i++)
		{
			var icon = icons[i];
			icon.DOKill();
			homeIconPositions[icon] = icon.localPosition;
			var destination = staggered ? GetUIExitPosition(icon) : icon.position + groupOffset;
			playExitSequence.Insert(i * stagger,
				icon.DOMove(destination, slideDuration).SetEase(Ease.InBack));
		}
		playExitSequence.OnComplete(() =>
		{
			playExitSequence = null;
			PoolManager.instance.ReleaseAllObjects();
			LoadingCamera.instance.StartLoad(GameConfig.gameScene);
		});
	}

	private List<RectTransform> GetHomeExitIcons()
	{
		var candidates = new List<Transform>();
		// Also resolve the logo in scenes that were already open before this field was added.
		if (homeLogo == null)
			ResolveHomeLogo();
		if (homeLogo != null)
			candidates.Add(homeLogo);
		foreach (var button in new[] { btSettings, btShop, btDailyLogin, btEvent, btBattlePass,
			btMap, btQuest, btFreeGold, btPiggyBank, btRank, btEditPlayer, btChallengeLevel, btPlay })
		{
			if (button != null)
				candidates.Add(button.transform);
		}
		if (topBarScript != null && topBarScript.goldsCounter != null)
			candidates.Add(topBarScript.goldsCounter.transform);
		if (levelInput != null)
			candidates.Add(levelInput.transform);
		foreach (var graphic in new Graphic[] { imgFrame, imgAvatar, imgFlag, txDisplayName })
		{
			if (graphic != null)
				candidates.Add(graphic.transform);
		}
		foreach (var menu in UnityEngine.Object.FindObjectsByType<ExpandableButtonMenu>(FindObjectsSortMode.None))
		{
			if (menu.gameObject.scene == gameObject.scene)
				candidates.Add(menu.transform);
		}

		// Animate each visible group once so child icons do not move twice with their parent.
		var icons = candidates.OfType<RectTransform>().Where(icon => icon.gameObject.activeInHierarchy)
			.Distinct().ToList();
		return icons.Where(icon => !icons.Any(parent => parent != icon && icon.IsChildOf(parent)))
			.OrderByDescending(icon => icon.position.y).ThenBy(icon => icon.position.x).ToList();
	}

	private static Vector3 GetHomeGroupExitOffset(List<RectTransform> icons, PlayExitStyle style)
	{
		if (icons.Count == 0)
			return Vector3.zero;
		var canvasRect = (RectTransform)icons[0].GetComponentInParent<Canvas>().rootCanvas.transform;
		var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, icons[0]);
		for (int i = 1; i < icons.Count; i++)
			bounds.Encapsulate(RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, icons[i]));
		var screen = canvasRect.rect;
		Vector3 direction;
		float distance;
		switch (style)
		{
			case PlayExitStyle.Up: direction = Vector3.up; distance = screen.yMax - bounds.min.y; break;
			case PlayExitStyle.Down: direction = Vector3.down; distance = bounds.max.y - screen.yMin; break;
			case PlayExitStyle.Left: direction = Vector3.left; distance = bounds.max.x - screen.xMin; break;
			case PlayExitStyle.Right: direction = Vector3.right; distance = screen.xMax - bounds.min.x; break;
			default: return Vector3.zero;
		}
		// A shared displacement preserves the layout and takes even the last icon fully offscreen.
		return canvasRect.TransformVector(direction * (Mathf.Max(0f, distance) + 80f));
	}

	// Edge: -1 = nearest, 0 = left, 1 = right, 2 = up, 3 = down.
	internal static Vector3 GetUIExitPosition(RectTransform icon, int exitEdge = -1)
	{
		var canvas = icon.GetComponentInParent<Canvas>().rootCanvas;
		var canvasRect = (RectTransform)canvas.transform;
		var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(canvasRect, icon);
		var screen = canvasRect.rect;
		const float margin = 80f;
		var distances = new[] { bounds.max.x - screen.xMin, screen.xMax - bounds.min.x,
			screen.yMax - bounds.min.y, bounds.max.y - screen.yMin };
		var directions = new[] { Vector3.left, Vector3.right, Vector3.up, Vector3.down };
		int edge = exitEdge;
		if (edge < 0)
		{
			edge = 0;
			for (int i = 1; i < distances.Length; i++)
			{
				if (distances[i] < distances[edge])
					edge = i;
			}
		}
		return icon.position + canvasRect.TransformVector(directions[edge] * (Mathf.Max(0f, distances[edge]) + margin));
	}

	private void ResetPlayTransition()
	{
		StopHomeLogoIdleAnimation();
		homeEntranceSequence?.Kill(false);
		homeEntranceSequence = null;
		playExitSequence?.Kill();
		playExitSequence = null;
		foreach (var item in homeIconPositions)
		{
			if (item.Key != null)
				item.Key.localPosition = item.Value;
		}
		homeIconPositions.Clear();
		startingLevel = false;
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
		// Retained for serialized UnityEvents from older scenes; Quest has been removed.
		QuestNotif();
	}

	public void QuestNotif()
	{
		if (btQuest != null)
		{
			btQuest.interactable = false;
			btQuest.gameObject.SetActive(false);
		}
		if (questNotif != null)
			questNotif.SetActive(false);
		if (questPopup != null && questPopup.panel != null)
			questPopup.panel.SetActive(false);
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
