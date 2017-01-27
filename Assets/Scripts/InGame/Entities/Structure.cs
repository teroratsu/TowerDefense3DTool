using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StructData
{
    public int hpMax;           //Max amount of Health points
    public int upgradePrice;    //cost of the next upgrade
    public int downgradePrice;  //money gained when downgrading a structure
    public bool canUpgrade;     // UI stuff - lock state (for example during tutorials)
    public bool canDowngrade;   // UI stuff
}

public enum StructureType
{
    Turret,
    Shield,
    Spawner
}

public enum StructureState
{
    Ready,
    Moving,
    Rotating
}

public class Structure : MonoBehaviour {
    public StructureType type = StructureType.Turret;
    public string structName;

    public bool bought = false;

    public int level = 0; //lvl of the structure

    private EntityBehaviour behaviour;
    private GameControl controller;

    public int getHP()
    {
        return behaviour.getHP();
    }

    public int getMaxHP()
    {
        return behaviour.getMaxHP();
    }

    public void hit(float damage)
    {
        behaviour.hit(damage);
    }

    public void Update()
    {
        if (behaviour)
        {
            if(behaviour.enabled) behaviour.Execute();
        }
        else
            instantiateBehaviour();
    }

    public void FixedUpdate()
    {
        if (!behaviour.enabled && bought)
        {
            behaviour.enabled = (controller.isInGame()) ? true : false;
        }
    }

    public bool buy()
    {
        if (controller.buy(behaviour.buyPrice()))
            bought = true;
        return bought;
    }

    public void sell()
    {
        int amount = behaviour.sellPrice();
        controller.giveMoney(amount);
        bought = false;
        behaviour.die();
        StartCoroutine(controller.ShowMessage("You sold" + structName + " for " + amount, 2, Color.yellow));
    }

    public int getLevel()
    {
        return level;
    }

    public int sellPrice()
    {
        return behaviour.sellPrice();
    }

    public bool upgrade()
    {
        bool upgrade = (behaviour.canUpgrade() && controller.buy(behaviour.upgradePrice()));
        if (upgrade)
        {
            behaviour.LevelUp();
            ++level;
            StartCoroutine(controller.ShowMessage("successfully upgrade " + structName + " to level " + (level+1), 2, Color.yellow));
        }
        else
        {
            if(!behaviour.canUpgrade())
                StartCoroutine(controller.ShowMessage("You can't upgrade " + structName +" anymore", 2, Color.red));
            else
                StartCoroutine(controller.ShowMessage("You can't upgrade " + structName + " yet (not enough golds)", 2, Color.red));
        }
        return upgrade;
    }

    public void downgrade()
    {
        bool downgrade = (behaviour.canDowngrade());
        if (downgrade)
        {
            controller.giveMoney(behaviour.downgradePrice());
            behaviour.LevelDown();
            --level;
            StartCoroutine(controller.ShowMessage("successfully downgrade " + structName + " to level " + (level+1), 2, Color.yellow));
        }
        else
        {
            StartCoroutine(controller.ShowMessage("You can't downgrade " + structName + " anymore", 2, Color.red));
        }
        if (level < 0) DestroyImmediate(gameObject);
    }

    public int downgradePrice()
    {
        return behaviour.downgradePrice();
    }

    public int upgradePrice()
    {
        return behaviour.upgradePrice();
    }

    public bool canDowngrade()
    {
        return behaviour.canDowngrade();
    }

    public bool canUpgrade()
    {
        return behaviour.canUpgrade();
    }

    public void init(SpawnableStructure structure, Transform t)
    {
        controller = GameObject.Find("GameControl").GetComponent<GameControl>();

        type = structure.type;
        structName = structure.name;
        level = 0;

        instantiateBehaviour();
        behaviour.init(structure);
        behaviour.Reset(level);

        gameObject.layer = LayerMask.NameToLayer("StructureLayer");

        setTransform(t); 
    }

    public bool isStateEnabled()
    {
        if (!behaviour) return false;
        return behaviour.enabled;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }

    public void enableStructure()
    {
        if (!behaviour)
            instantiateBehaviour();

        behaviour.enabled = true;
    }

    public void disableStructure()
    {
        if (!behaviour)
            instantiateBehaviour();

        behaviour.enabled = false;
    }

    public void instantiateBehaviour()
    {
        switch (type)
        {
            case StructureType.Turret:
                {
                    behaviour = gameObject.AddComponent<TurretBehaviour>();
                }
                break;
            case StructureType.Shield:
                {
                    behaviour = gameObject.AddComponent<ShieldBehaviour>();
                }
                break;
            case StructureType.Spawner:
                {
                    behaviour = gameObject.AddComponent<FriendlySpawnerBehaviour>();
                }
                break;
        }
    }

    public void setTransform(Transform t)
    {
        gameObject.transform.position = t.position;
        gameObject.transform.rotation = t.rotation;
    }
}
