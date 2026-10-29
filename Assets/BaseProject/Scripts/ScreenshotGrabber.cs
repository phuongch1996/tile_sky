#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public class ScreenshotGrabber
{
    [MenuItem("Screenshot/Grab")]
    public static void Grab()
    {
        //"Screenshots/" + (TimeManager.Instance.currentTime - DateTime.MinValue).TotalSeconds + ".png"
        //ScreenCapture.CaptureScreenshot("Assets/Screenshot.png", 1);
        ScreenCapture.CaptureScreenshot("Screenshots/" + (DateTime.Now - DateTime.MinValue).TotalSeconds + ".png", 1);
    }
}
#endif