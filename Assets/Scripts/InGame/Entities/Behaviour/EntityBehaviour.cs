using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class EntityBehaviour : MonoBehaviour {
    public StructureType type;
    public int currentLevel = 0;
    public bool maxLevel;
    public int currentHP;
    public int hpMax;
    public int basePrice;

    public GameControl controller;

    public void Awake()
    {
        controller = GameObject.Find("GameControl").GetComponent<GameControl>();
        maxLevel = false;
    }
    abstract public void Execute();
    abstract public void Reset(int level);
    abstract public void LevelUp();
    abstract public void LevelDown();

    public int getHP()
    {
        return currentHP;
    }

    public int getMaxHP()
    {
        return hpMax;
    }

    abstract public void init(SpawnableStructure structure);
    abstract public void hit(float damage);
    public void die()
    {
        this.enabled = false;
        foreach (Transform t in transform)
        {
            if (t.gameObject.GetComponent<MeshRenderer>())
            {
                if(t.gameObject.GetComponent<MeshFilter>().sharedMesh.isReadable)
                {
                    t.gameObject.AddComponent<TriangleExplosion>();
                    StartCoroutine(t.GetComponent<TriangleExplosion>().SplitMesh(true));
                }
            }
        }
        Destroy(gameObject, .5f);
    }

    abstract public bool canUpgrade();
    abstract public bool canDowngrade();
    abstract public int buyPrice();
    abstract public int sellPrice();
    abstract public int downgradePrice();
    abstract public int upgradePrice();
}
