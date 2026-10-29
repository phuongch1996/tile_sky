using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class DestroyedSelectionCleaner
{
    static DestroyedSelectionCleaner()
    {
        EditorApplication.hierarchyChanged += CleanSelection;
        EditorApplication.update += CleanSelection;
        EditorApplication.playModeStateChanged += _ => CleanSelection();
        Selection.selectionChanged += CleanSelection;
        EditorApplication.delayCall += CleanSelection;
    }

    private static void CleanSelection()
    {
        Object[] selectedObjects = Selection.objects;
        if (selectedObjects == null || selectedObjects.Length == 0)
            return;

        bool changed = false;
        List<Object> aliveObjects = new List<Object>(selectedObjects.Length);

        foreach (Object selectedObject in selectedObjects)
        {
            if (IsAlive(selectedObject))
            {
                aliveObjects.Add(selectedObject);
            }
            else
            {
                changed = true;
            }
        }

        if (changed)
        {
            Selection.objects = aliveObjects.ToArray();
            EditorApplication.delayCall += () =>
            {
                if (Selection.objects == null || Selection.objects.Length == 0)
                    Selection.activeObject = null;
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            };
        }
    }

    private static bool IsAlive(Object selectedObject)
    {
        if (selectedObject == null)
            return false;

        try
        {
            selectedObject.GetEntityId();
            _ = selectedObject.name;
            _ = selectedObject.hideFlags;

            if (selectedObject is Component component)
                return component.gameObject != null;

            if (selectedObject is GameObject gameObject)
                return gameObject.scene.IsValid() || !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(gameObject));

            return true;
        }
        catch (MissingReferenceException)
        {
            return false;
        }
        catch (MissingComponentException)
        {
            return false;
        }
    }
}
