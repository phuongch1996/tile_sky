using UnityEngine;

public class UGSManager : MonoBehaviour
{
    public static UGSManager instance;
    private void Awake()
    {
        if (instance != null && instance.gameObject != gameObject)
            Destroy(gameObject);
        else
            instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Compatibility component for existing scenes. A new Tile Space backend
    // must be configured before Unity Gaming Services can be initialized again.
}
