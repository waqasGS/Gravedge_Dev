using UnityEngine;
using UnityEngine.Events;

public class TrajectoryEvents : MonoBehaviour
{
    [System.Serializable]
    public class AimEvent : UnityEvent<Vector3, Vector3> { }
    [System.Serializable]
    public class AimUpdateEvent : UnityEvent<Vector3> { }
    [System.Serializable]
    public class AimEndEvent : UnityEvent { }

    public AimEvent onAimStart = new AimEvent();
    public AimUpdateEvent onAimUpdate = new AimUpdateEvent();
    public AimEndEvent onAimEnd = new AimEndEvent();

    private static TrajectoryEvents _instance;
    public static TrajectoryEvents Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("TrajectoryEvents");
                _instance = go.AddComponent<TrajectoryEvents>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
} 