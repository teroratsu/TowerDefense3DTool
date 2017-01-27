using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MobTemplate
{
    public GameObject mesh;
    public MobParams mobParameters;
    public int count;
    public float spawnRate;
}

public class MobToSpawn{
    public MobToSpawn(MobPool pool_,int count_, float spawnRate_)
    {
        pool = pool_;
        count = count_;
        spawnRate = spawnRate_;
        timeElapsed = 0;
    }
    public int count;
    public MobPool pool;
    public float timeElapsed;
    public float spawnRate;
}

public class MobSpawner : MonoBehaviour {

    public List<MobTemplate> mobs = new List<MobTemplate>();

    List<MobToSpawn> pools;
    MobToSpawn mobBread;

    private GameControl controller;

    public float timePending = 0;
    private float elapsedTime;
    private bool pending;

    private int mobCount;

    // Use this for initialization
    void Start () {
        elapsedTime = 0;
        pending = true;
        MobParams parameters;
        Mob mob;
        pools = new List<MobToSpawn>();
        controller = GameObject.Find("GameControl").GetComponent<GameControl>();
        foreach (MobTemplate bread in mobs)
        {
            mob = bread.mesh.GetComponent<Mob>();
            if (!mob)
                mob = bread.mesh.AddComponent<Mob>();

            parameters = bread.mobParameters;
            mob.setParameters(parameters);

            controller.mobCount += bread.count;

            pools.Add(new MobToSpawn(new MobPool(bread.mesh, 1), bread.count, bread.spawnRate));
        }
	}

    void FixedUpdate()
    {
        if(controller.isInGame())
        {
            if (pending)
            {
                elapsedTime += Time.fixedDeltaTime;
                if(elapsedTime - timePending > 0)
                {
                    pending = false;
                }
            }
            else
            {
                for (int i = 0; i < pools.Count; ++i)
                {
                    mobBread = (MobToSpawn)pools[i];
                    if (mobBread.count > 0)
                    {
                        mobBread.timeElapsed += Time.fixedDeltaTime;
                        if (mobBread.timeElapsed - mobBread.spawnRate > 0)
                        {
                            mobBread.timeElapsed = 0;
                            GameObject go = pools[i].pool.GetPooledGameObject();
                            go.transform.position = transform.position;
                            go.transform.rotation = transform.rotation;
                            go.GetComponent<Mob>().resetBehaviour();
                            go.SetActive(true);
                            mobBread.count -= 1;
                        }
                    }
                    else
                    {
                        mobBread.pool.clear();
                    }
                }
            }
        }
    }
}

public class MobPool
{
    public GameObject pooledObject;
    public int pooledAmout;
    public bool willGrow = true;

    List<GameObject> pooledObjects;

    // Use this for initialization
    public MobPool(GameObject pooledObject_, int amount = 10)
    {
        pooledObject = pooledObject_;
        pooledAmout = amount;
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < amount; ++i)
        {
            GameObject obj = (GameObject)GameObject.Instantiate(pooledObject);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    public void clear()
    {
        foreach(GameObject go in pooledObjects)
        {
            if(!go.activeInHierarchy)
            {
                pooledObjects.Remove(go);
            }
        }
    }

    // Update is called once per frame
    public GameObject GetPooledGameObject()
    {
        if (pooledObjects == null) pooledObjects = new List<GameObject>();
        if (pooledObjects.Count > 0)
        {
            for (int i = 0; i < pooledObjects.Count; ++i)
            {
                if (!pooledObjects[i].activeInHierarchy) return pooledObjects[i];
            }

            if (willGrow)
            {
                GameObject obj = (GameObject)GameObject.Instantiate(pooledObject);
                obj.SetActive(false);
                pooledObjects.Add(obj);
                return obj;
            }
        }

        return null;
    }
}



