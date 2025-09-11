using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
   
            SceneManager.sceneLoaded += OnSceneLoad;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    protected virtual void OnDestroy()
    {
        // 인스턴스가 자기 자신일 때만 정리
        if (_instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            _instance = null;
        }
    }
    protected virtual void OnApplicationQuit()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
        _instance = null;
    }
    protected abstract void OnSceneLoad(Scene scene, LoadSceneMode mode);
}