using System.Collections;
using Unity.Advertisement.IosSupport.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Advertisement.IosSupport.Samples
{
    /// <summary>
    /// This component will trigger the context screen to appear when the scene starts,
    /// if the user hasn't already responded to the iOS tracking dialog.
    /// </summary>
    public class ContextScreenManager : MonoBehaviour
    {
        /// <summary>
        /// The prefab that will be instantiated by this component.
        /// The prefab has to have an ContextScreenView component on its root GameObject.
        /// </summary>
        public ContextScreenView contextScreenPrefab;

        void Start()
        {
            StartCoroutine(GetATT());
        }
        private IEnumerator GetATT()
        {
            yield return null;
            StartCoroutine(LoadNextScence());
#if UNITY_IOS
            yield return new WaitForSeconds(1);

            // check with iOS to see if the user has accepted or declined tracking
            if (GameConfig.IsiOSVersionAtLeast14())
            {
                var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                {
                    var contextScreen = Instantiate(contextScreenPrefab).GetComponent<ContextScreenView>();

                    // after the Continue button is pressed, and the tracking request
                    // has been sent, automatically destroy the popup to conserve memory
                    contextScreen.sentTrackingAuthorizationRequest += () => Destroy(contextScreen.gameObject);
                }
            }
#else
            Debug.Log("Unity iOS Support: App Tracking Transparency status not checked, because the platform is not iOS.");
#endif
        }
        private IEnumerator LoadNextScence()
        {
#if UNITY_IOS && !UNITY_EDITOR

            yield return new WaitForEndOfFrame();
            if (Config.IsiOSVersionAtLeast14())
            {
                var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                while (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
                {
                    status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                    yield return null;
                }
            }
#endif
            if (!GameConfig.CHEAT_ENABLED)
            {
                UMPManager.Instance.InitUMP(() =>
                {
                    Debug.Log("InitUMP");
                    AdmobAd.instance.InitializeGoogleMobileAds();
                    MaxAd.instance.Initialize();
                    GameConfig.SetActionTime("ads_inter");
                    GameConfig.SetActionTime("ads_collaps");
                    GameConfig.isUMP = true;
                }, () =>
                {
                    Debug.Log("InitUMPPause");
                    Time.timeScale = 0;
                }, () =>
                {
                    Debug.Log("InitUMPResume");
                    Time.timeScale = 1;
                });
            }
            else
            {
                GameConfig.isUMP = true;
            }
            yield return new WaitForEndOfFrame();
            var async = SceneManager.LoadSceneAsync(GameConfig.gameScene, LoadSceneMode.Additive);
            while (!(async.isDone && GameConfig.isUMP))
                yield return null;
            SceneManager.LoadSceneAsync(GameConfig.homeScene);
            yield return null;
        }
    }
}
