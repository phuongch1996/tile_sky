using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Callbacks;

/// <summary>Keep the retained Firebase SDK disconnected from the previous game.</summary>
public static class DisableFirebaseAutoCollection
{
    [PostProcessBuild(int.MaxValue)]
    public static void OnPostprocessBuild(BuildTarget target, string outputPath)
    {
        if (target != BuildTarget.iOS && target != BuildTarget.tvOS)
            return;

        var path = Path.Combine(outputPath, "Info.plist");
        var plist = new XmlDocument { XmlResolver = null };
        plist.Load(path);
        var dictionary = plist.SelectSingleNode("/plist/dict");
        if (dictionary == null)
            throw new System.InvalidOperationException("Info.plist has no root dictionary.");

        SetBoolean(plist, dictionary, "FIREBASE_ANALYTICS_COLLECTION_DEACTIVATED", true);
        SetBoolean(plist, dictionary, "FIREBASE_ANALYTICS_COLLECTION_ENABLED", false);
        SetBoolean(plist, dictionary, "FirebaseMessagingAutoInitEnabled", false);
        SetBoolean(plist, dictionary, "FirebaseAppDelegateProxyEnabled", false);
        plist.Save(path);
    }

    private static void SetBoolean(XmlDocument plist, XmlNode dictionary, string key, bool value)
    {
        foreach (XmlNode node in dictionary.ChildNodes)
        {
            if (node.Name != "key" || node.InnerText != key)
                continue;

            var oldValue = node.NextSibling;
            while (oldValue != null && oldValue.NodeType != XmlNodeType.Element)
                oldValue = oldValue.NextSibling;
            if (oldValue == null)
                throw new System.InvalidOperationException("Missing Info.plist value for " + key);
            dictionary.ReplaceChild(plist.CreateElement(value ? "true" : "false"), oldValue);
            return;
        }

        var keyNode = plist.CreateElement("key");
        keyNode.InnerText = key;
        dictionary.AppendChild(keyNode);
        dictionary.AppendChild(plist.CreateElement(value ? "true" : "false"));
    }
}
