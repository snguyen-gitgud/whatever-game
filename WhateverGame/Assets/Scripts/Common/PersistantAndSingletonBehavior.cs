using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistantAndSingletonBehavior<T> : MonoBehaviour where T : PersistantAndSingletonBehavior<T>
{
    private static T _instance;

    public virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    public static T GetInstance()
    {
        return _instance;
    }
}

public class SingletonBehavior<T> : MonoBehaviour where T : SingletonBehavior<T>
{
    private static T _instance;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = (T)this;
        }
        else
        {
            Destroy(this);
        }
    }

    public static T GetInstance()
    {
        return _instance;
    }
}
