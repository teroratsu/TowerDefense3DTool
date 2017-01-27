using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TurretData : StructData
{
    public float range;        //portée
    public float fireRate;     //vitesse de tir en 
    public float attackMult;   //multiplicateur de dégats
}

public class TurretBehaviour : EntityBehaviour
{
    private Collider[] entitiesWithinRange;
    private Collider closestEnemy;
    private TurretData data;
    private int layerId;
    private int layerMask;
    
    private float elapsedTime;

    private Transform endPoint;
    private Transform canon;

    private GameObject bullet_tampon;

    private DamageType damageType;
    private float baseDamage;

    public override void hit(float damage)
    {
        if(currentHP > 0)
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

        data = new TurretData();
        Reset(0);
        type = StructureType.Turret;
        

        layerId = LayerMask.NameToLayer("MobLayer");
        layerMask = 1 << layerId;
        
        elapsedTime = 0;

        this.enabled = false;

        currentHP = hpMax = data.hpMax;
    }

    public override void init(SpawnableStructure structure)
    {
        basePrice = structure.basePrice;
        damageType = structure.damageType;
        baseDamage = structure.damage;
        currentLevel = 0;
        foreach(Transform child in gameObject.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.name == "endpoint") endPoint = child;
            if (child.gameObject.name == "canon") canon = child;
        }
        if (data == null) Reset(currentLevel);
        currentHP = hpMax = data.hpMax;
    }

	public override void Execute()
    {
        elapsedTime += Time.deltaTime;
        if (data == null) Reset(currentLevel);
        entitiesWithinRange = Physics.OverlapSphere(transform.position, data.range, layerMask);
        if(entitiesWithinRange.Length > 0)
        {
            closestEnemy = entitiesWithinRange[0];
            float dist = Vector3.Distance(transform.position, closestEnemy.transform.position);
            foreach (Collider c in entitiesWithinRange)
            {
                float tempDist = Vector3.Distance(transform.position, c.transform.position);
                if (tempDist < dist) closestEnemy = c;
            }
            canon.transform.LookAt(closestEnemy.transform);
            if (elapsedTime - (1/data.fireRate) > 0)
            {
                bullet_tampon = controller.bulletPool.GetPooledGameObject();
                
                bullet_tampon.transform.rotation = canon.transform.rotation;
                bullet_tampon.transform.position = endPoint.transform.position;
                bullet_tampon.GetComponent<bulletBehaviour>().setProperties(damageType, data.attackMult*baseDamage);

                bullet_tampon.SetActive(true);
                
                elapsedTime = 0;
            }
        }
    }

    public override void LevelUp()
    {
        ++currentLevel;
        data = (TurretData) controller.getTurretData(currentLevel);
        currentHP = hpMax = data.hpMax;
    }
    public override void LevelDown()
    {
        --currentLevel;
        data = (TurretData)controller.getTurretData(currentLevel);
        currentHP = hpMax = data.hpMax;
    }

    public override void Reset(int level)
    {
        currentLevel = level;
        if (!controller)
            controller = GameObject.Find("GameControl").GetComponent<GameControl>();
        data = (TurretData) controller.getTurretData(currentLevel);
        currentHP = data.hpMax;
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
                price += controller.getTurretData(level).downgradePrice;
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
