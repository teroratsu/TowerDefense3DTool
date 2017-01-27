using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpawnerData : StructData
{
    public GameObject spawnedEntity;
    public Transform rallyPoint;
    public float range;
}

public class FriendlySpawnerBehaviour : EntityBehaviour
{
    public SpawnerData data;

    public override void hit(float damage)
    {
        if (currentHP > 0)
        {
            currentHP -= Mathf.CeilToInt(damage);
            if (currentHP <= 0)
            {
                this.enabled = false;
                die();
                
            }
        }
    }

    public void Start()
    {
        data = new SpawnerData();
        type = StructureType.Spawner;

        this.enabled = false;
    }

    public override void Execute()
    {

    }

    public override void LevelUp()
    {
        ++currentLevel;
        data = (SpawnerData)controller.getSpawnerData(currentLevel);
        currentHP = hpMax = data.hpMax;
    }
    public override void LevelDown()
    {
        --currentLevel;
        data = (SpawnerData)controller.getSpawnerData(currentLevel);
        currentHP = hpMax = data.hpMax;
    }

    public override void Reset(int level)
    {
        currentLevel = level;
        if (!controller)
            controller = GameObject.Find("GameControl").GetComponent<GameControl>();
        data = (SpawnerData)controller.getSpawnerData(currentLevel);
    }
    public override void init(SpawnableStructure structure) {
        basePrice = structure.basePrice;
        currentLevel = 0;
        if (data == null) Reset(currentLevel);
        currentHP = hpMax = data.hpMax;
    }

    override public bool canUpgrade()
    {
        return data.canUpgrade;
    }
    override public bool canDowngrade()
    {
        return data.canDowngrade;
    }
    public override int buyPrice()
    {
        return basePrice;
    }
    override public int sellPrice()
    {
        int price = 0;
        int level = currentLevel;
        while (level >= 0)
        {
            if (level > 0)
                price += controller.getSpawnerData(level).downgradePrice;
            else
                price += basePrice/2;
            --level;
        }
        return price;
      
    }
    override public int downgradePrice()
    {
        return data.downgradePrice;
    }
    override public int upgradePrice()
    {
        return data.upgradePrice;
    }
}
