using Squax.Patterns;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameObjectPoolManager : UnitySingleton<GameObjectPoolManager>
{
    public struct SpawnedObjectTimer
    {
        public string id;
        public float time;
        public float completeTime;
        public GameObject instance;
    }

    private Dictionary<string, List<GameObject>> instances = new Dictionary<string, List<GameObject>>();

    private List<GameObject> activeInstances = new List<GameObject>();

    private List<SpawnedObjectTimer> trackedSpawnedObjects = new List<SpawnedObjectTimer>();

    void Awake()
    {
        OnAwake();
    }

    /// <summary>
    /// Register and generate pool.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="prefab"></param>
    /// <param name="parent"></param>
    /// <param name="poolSize"></param>
    public void Register(string id, GameObject prefab, Transform parent, int poolSize)
    {
        if (instances.ContainsKey(id) == false)
        {
            instances.Add(id, new List<GameObject>());
        }

        var list = instances[id];

        for (int i = 0; i < poolSize; ++i)
        {
            var instance = GameObject.Instantiate(prefab, parent);

            instance.SetActive(false);

            list.Add(instance);
        }
    }

    /// <summary>
    /// Spawn an object from pool.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="returnAfterTime"></param>
    /// <returns></returns>
    public GameObject Spawn(string id, Vector3 position, Quaternion rotation, bool activate = true, float returnAfterTime = 0f)
    {
        if (instances.ContainsKey(id) == false)
        {
            return null;
        }

        var list = instances[id];

        if(list.Count == 0)
        {
            // Pool empty.
            return null;
        }

        var instance = list[list.Count - 1];
        list.RemoveAt(list.Count - 1);

        // Create tracked object.
        if (returnAfterTime > 0)
        {
            var trackedInstance = new SpawnedObjectTimer()
            {
                id = id,
                time = 0f,
                completeTime = returnAfterTime,
                instance = instance
            };

            trackedSpawnedObjects.Add(trackedInstance);

            activeInstances.Add(instance);
        }

        instance.transform.position = position;
        instance.transform.rotation = rotation;

        instance.SetActive(activate);

        return instance;
    }

    /// <summary>
    /// Return to pool.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="instance"></param>
    public void Return(string id, GameObject instance)
    {
        if (instances.ContainsKey(id) == false)
        {
            return;
        }

        if(instance.activeSelf == false)
        {
            return;
        }

        for (int i = trackedSpawnedObjects.Count - 1; i >= 0; --i)
        {
            if (trackedSpawnedObjects[i].instance == instance)
            {
                trackedSpawnedObjects.RemoveAt(i);

                break;
            }
        }

        instance.SetActive(false);

        var list = instances[id];

        list.Add(instance);

        activeInstances.Remove(instance);
    }

    private void Update()
    {
        for(int i = trackedSpawnedObjects.Count-1; i >= 0; --i)
        {
            var instance = trackedSpawnedObjects[i];
            instance.time += Time.deltaTime;

            if(instance.time >= instance.completeTime)
            {
                if (activeInstances.Contains(instance.instance) == true)
                {
                    // Return to pool.
                    Return(instance.id, instance.instance);
                }
            }
            else
            {
                trackedSpawnedObjects[i] = instance;
            }
        }
    }

    public void ClearAll()
    {
        trackedSpawnedObjects.Clear();
        activeInstances.Clear();
        instances.Clear();
    }
}
