using System;
using System.Collections.Generic;
using UnityEngine;

public class DestructiblePropRegistry : MonoBehaviour
{
    public static DestructiblePropRegistry Instance;

    private readonly HashSet<SmashableProp> registeredProps = new HashSet<SmashableProp>();

    [SerializeField] private PropFragmentPool fragmentPool;

    public event Action<SmashableProp, DestructibleHitInfo> PropDestroyed;

    public PropFragmentPool FragmentPool => fragmentPool;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fragmentPool == null)
        {
            fragmentPool = GetComponent<PropFragmentPool>();
        }

        if (fragmentPool == null)
        {
            fragmentPool = gameObject.AddComponent<PropFragmentPool>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RegisterProp(SmashableProp prop)
    {
        if (prop == null)
        {
            return;
        }

        registeredProps.Add(prop);
    }

    public void UnregisterProp(SmashableProp prop)
    {
        if (prop == null)
        {
            return;
        }

        registeredProps.Remove(prop);
    }

    public bool IsRegistered(SmashableProp prop)
    {
        return prop != null && registeredProps.Contains(prop);
    }

    public IReadOnlyCollection<SmashableProp> GetAllProps()
    {
        return registeredProps;
    }

    public void NotifyDestroyed(SmashableProp prop, DestructibleHitInfo hitInfo)
    {
        if (prop == null)
        {
            return;
        }

        PropDestroyed?.Invoke(prop, hitInfo);
    }
}
