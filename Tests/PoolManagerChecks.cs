#if STANDALONE_POOL_CHECKS
// Runs the production PoolManager/PoolObject sources against a small Unity-object model.
// This exercises pool bookkeeping and Unity-style destroyed references, not native Play Mode.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityEngine
{
    public class Object
    {
        internal bool destroyed;
        public static bool operator ==(Object a, Object b)
        {
            bool emptyA = ReferenceEquals(a, null) || a.destroyed;
            bool emptyB = ReferenceEquals(b, null) || b.destroyed;
            return emptyA || emptyB ? emptyA == emptyB : ReferenceEquals(a, b);
        }
        public static bool operator !=(Object a, Object b) => !(a == b);
        public override bool Equals(object other) => ReferenceEquals(this, other);
        public override int GetHashCode() => base.GetHashCode();
        public static void Destroy(Object target)
        {
            target.destroyed = true;
            if (target is GameObject go)
                foreach (var component in go.components) component.destroyed = true;
        }
        public static void DontDestroyOnLoad(Object target) { }
        public static T Instantiate<T>(T source, Transform parent, bool worldPositionStays) where T : Component, new()
        {
            var clone = new GameObject("clone").AddComponent<T>();
            clone.transform.SetParent(parent);
            return clone;
        }
    }
    public class GameObject : Object
    {
        internal readonly List<Component> components = new List<Component>();
        public readonly Transform transform;
        public bool activeSelf = true;
        public GameObject(string name = "")
        {
            transform = new Transform { gameObject = this };
            components.Add(transform);
        }
        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T { gameObject = this };
            components.Add(component);
            return component;
        }
        public void SetActive(bool active) { activeSelf = active; }
    }
    public class Component : Object
    {
        public GameObject gameObject;
        public Transform transform => destroyed ? throw new Exception("Destroyed object accessed") : gameObject.transform;
        public T GetComponent<T>() where T : Component => gameObject.components.OfType<T>().FirstOrDefault();
    }
    public class MonoBehaviour : Component
    {
        public bool pendingInvoke;
        public void Invoke(string method, float seconds) { pendingInvoke = true; }
        public void CancelInvoke() { pendingInvoke = false; }
    }
    public class Transform : Component
    {
        readonly List<Transform> children = new List<Transform>();
        public Transform parent;
        public Vector3 localPosition;
        public int childCount => children.Count;
        public Transform GetChild(int index) => children[index];
        public void SetParent(Transform value)
        {
            parent?.children.Remove(this);
            parent = value;
            parent?.children.Add(this);
        }
    }
    public struct Vector3 { public static Vector3 zero => default; }
    public sealed class HideInInspector : Attribute { }
}

internal static class PoolManagerChecks
{
    static int assertions;
    static void Check(bool valid, string message)
    {
        assertions++;
        if (!valid) throw new Exception(message);
    }
    static int Main()
    {
        var manager = new UnityEngine.GameObject("manager").AddComponent<PoolManager>();
        PoolManager.instance = manager;
        var root = new UnityEngine.GameObject("pool root").transform;
        root.SetParent(manager.transform);
        var prefab = new UnityEngine.GameObject("template").AddComponent<PoolObject>();
        var parameters = (List<PoolParam>)typeof(PoolManager).GetField("poolParams",
            BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager);
        parameters.Add(new PoolParam(prefab, root));
        var registry = new List<PoolObject>();
        manager.poolObjectsDict = new Dictionary<UnityEngine.Transform, List<PoolObject>> { { root, registry } };

        var first = manager.GetObject<PoolObject>(prefab, null, 1);
        var second = manager.GetObject<PoolObject>(prefab, null);
        Check(first != second, "A parentless request reused an object that was still active");
        manager.ReleaseObject(first);
        Check(!first.pendingInvoke, "Delayed release survived returning an object to the pool");
        Check(first.PoolerStatus == ObjPoolerStatus.Inactive && first.transform.parent == root,
            "Release did not return the object to its pool");
        Check(manager.GetObject<PoolObject>(prefab, null) == first, "Inactive object was not reused");

        UnityEngine.Object.Destroy(first.gameObject);
        manager.ReleaseObject(first); // Must not access the destroyed object's transform.
        registry.Add(null);
        manager.ReleaseAllObjects();
        Check(registry.Count == 1 && registry[0] == second, "Destroyed/null registry entries were retained");
        Check(second.PoolerStatus == ObjPoolerStatus.Inactive, "Live objects were not released after a stale entry");

        var sceneParent = new UnityEngine.GameObject("scene parent").transform;
        second.transform.SetParent(sceneParent); // Even inactive objects must leave a scene before unload.
        manager.ReleaseAllObjects();
        Check(second.transform.parent == root, "Inactive object was left under a disposable scene parent");
        manager.ReleaseObject(null);
        var third = manager.GetObject<PoolObject>(prefab, null);
        Check(third == second, "Pool was unusable after cleanup");
        UnityEngine.Object.Destroy(third.gameObject);
        var replacement = manager.GetObject<PoolObject>(prefab, null);
        Check(replacement != null && registry.Count == 1, "GetObject did not replace a destroyed entry");
        Console.WriteLine("PASS: " + assertions + " production pool checks using a Unity-object model (not Play Mode).");
        return 0;
    }
}
#endif
