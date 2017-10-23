﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiteNetLibAssets : MonoBehaviour
{
    public LiteNetLibIdentity[] registeringPrefabs;
    protected readonly Dictionary<string, LiteNetLibIdentity> guidToPrefabs = new Dictionary<string, LiteNetLibIdentity>();
    protected readonly Dictionary<long, LiteNetLibIdentity> spawnedObjects = new Dictionary<long, LiteNetLibIdentity>();
    protected long objectIdCounter = 0;

    private LiteNetLibManager manager;
    public LiteNetLibManager Manager
    {
        get
        {
            if (manager == null)
                manager = GetComponent<LiteNetLibManager>();
            return manager;
        }
    }

    public void ClearRegisterPrefabs()
    {
        guidToPrefabs.Clear();
    }

    public void RegisterPrefabs()
    {
        foreach (var registeringPrefab in registeringPrefabs)
        {
            RegisterPrefab(registeringPrefab);
        }
    }

    public void RegisterPrefab(LiteNetLibIdentity prefab)
    {
        guidToPrefabs.Add(prefab.assetId, prefab);
    }

    public bool UnregisterPrefab(LiteNetLibIdentity prefab)
    {
        return guidToPrefabs.Remove(prefab.assetId);
    }

    public void ClearSpawnedObjects()
    {
        foreach (var objectId in spawnedObjects.Keys)
        {
            NetworkDestroy(objectId);
        }
    }

    public LiteNetLibIdentity NetworkSpawn(GameObject gameObject)
    {
        if (gameObject == null)
        {
            if (Manager.LogWarn) Debug.LogWarning("[" + name + "] LiteNetLibAssets::NetworkSpawn - GameObject is null.");
            return null;
        }
        var obj = gameObject.GetComponent<LiteNetLibIdentity>();
        return NetworkSpawn(obj);
    }

    public LiteNetLibIdentity NetworkSpawn(LiteNetLibIdentity obj)
    {
        if (obj == null)
        {
            if (Manager.LogWarn) Debug.LogWarning("[" + name + "] LiteNetLibAssets::NetworkSpawn - LiteNetLibIdentity is null.");
            return null;
        }
        return NetworkSpawn(obj.assetId);
    }

    public LiteNetLibIdentity NetworkSpawn(string assetId)
    {
        LiteNetLibIdentity spawningObject = null;
        if (guidToPrefabs.TryGetValue(assetId, out spawningObject))
            spawnedObjects.Add(++objectIdCounter, spawningObject);
        else if (Manager.LogWarn)
            Debug.LogWarning("[" + name + "] LiteNetLibAssets::NetworkSpawn - Asset Id: " + assetId + " is not registered.");
        return spawningObject;
    }

    public bool NetworkDestroy(GameObject gameObject)
    {
        if (gameObject == null)
        {
            if (Manager.LogWarn) Debug.LogWarning("[" + name + "] LiteNetLibAssets::NetworkDestroy - GameObject is null.");
            return false;
        }
        var obj = gameObject.GetComponent<LiteNetLibIdentity>();
        return NetworkDestroy(obj);
    }

    public bool NetworkDestroy(LiteNetLibIdentity obj)
    {
        if (obj == null)
        {
            if (Manager.LogWarn) Debug.LogWarning("[" + name + "] LiteNetLibAssets::NetworkDestroy - LiteNetLibIdentity is null.");
            return false;
        }
        return NetworkDestroy(obj.objectId);
    }

    public bool NetworkDestroy(long objectId)
    {
        LiteNetLibIdentity spawnedObject;
        if (spawnedObjects.TryGetValue(objectId, out spawnedObject) && spawnedObjects.Remove(objectId))
        {
            Destroy(spawnedObject.gameObject);
            return true;
        }
        else if (Manager.LogWarn)
            Debug.LogWarning("[" + name + "] LiteNetLibAssets::NetworkDestroy - Object Id: " + objectId + " is not spawned.");
        return false;
    }
}
