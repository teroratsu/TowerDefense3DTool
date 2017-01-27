using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolScript : MonoBehaviour {

    public GameObject pooledObject;
    public int pooledAmout = 10;
    public bool willGrow = true;

    List<GameObject> pooledObjects;

	// Use this for initialization
	void Start () {
        pooledObjects = new List<GameObject>();
        for(int i=0; i<pooledAmout; ++i)
        {
            GameObject obj = (GameObject)Instantiate(pooledObject);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
	}
	
	// Update is called once per frame
	public GameObject GetPooledGameObject () {
        if (pooledObjects == null) pooledObjects = new List<GameObject>();
        if (pooledObjects.Count > 0)
        {
            for (int i = 0; i < pooledObjects.Count; ++i)
            {
                if (!pooledObjects[i].activeInHierarchy) return pooledObjects[i];
            }

            if (willGrow)
            {
                GameObject obj = (GameObject)Instantiate(pooledObject);
                obj.SetActive(false);
                pooledObjects.Add(obj);
                return obj;
            }
        }

        return null;
	}
}
