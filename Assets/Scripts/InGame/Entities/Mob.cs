using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MobSpeed // speed Base speed multiplier (%)
{
    VeryFast = 150,
    Fast = 125,
    Normal = 100,
    Slow = 75,
    VerySlow = 50,
    ExtremelySlow = 25
}

public enum MobArmour // resistance to Physical damage (%)
{
    None = 0,
    VeryWeak = 25,
    Weak = 40,
    Normal = 50,
    Strong = 60,
    VeryStrong = 75,
    ExtremelyStrong = 90
}

public enum MobResistance // resistance to Magical damage (%)
{
    None = 0,
    VeryWeak = 5,
    Weak = 10,
    Normal = 50,
    Strong = 30,
    VeryStrong = 35,
    ExtremelyStrong = 90
}

[System.Serializable]
public struct MobParams
{
    public MobSpeed speed;
    public MobArmour armour;
    public MobResistance resistance;

    public int life;
    public int maxLife;
    public float damage;
    public float attackRate; // number of attack per second

    public string mobName;
    public int reward;
}

[System.Serializable]
[RequireComponent(typeof(NavMeshAgent))]
public class Mob : MonoBehaviour {
    public MobSpeed speed;
    public MobArmour armour;
    public MobResistance resistance;

    public int life = 100;
    public int maxLife = 100;
    public float damage;
    public float attackRate; // number of attack per second

    public string mobName = "Monster";
    public int reward = 0;

    private int PhysicalArmourReduction = 0;
    private int MagicalArmourReduction = 0;

    private GameControl controller;

    public Transform goal;
    private NavMeshAgent agent;

    private bool attacking;
    private GameObject target;

    private string targetMask;
    private float elaspedTime;
    private float detectionUpdateElapsedTime;

    private Collider[] structuresWithinRange;
    private float detectRange = 5f;

    // Use this for initialization
    void Start() {
        targetMask = "StructureLayer";

        agent = GetComponent<NavMeshAgent>();
        if(goal)
        {
            agent.SetDestination(goal.position);
            target = goal.gameObject;
        }
        agent.speed = 3.5f;
        agent.speed = agent.speed * ((float)speed / 100f);
        agent.stoppingDistance = 2f;
        elaspedTime = 0;
        detectionUpdateElapsedTime = 0;
        life = maxLife;

        controller = GameObject.Find("GameControl").GetComponent<GameControl>();
	}

    public void setParameters(MobParams parameters)
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 3.5f;
        agent.speed = agent.speed * ((float)parameters.speed / 100f);
        agent.stoppingDistance = 2f;

        reward = parameters.reward;
        armour = parameters.armour;
        resistance = parameters.resistance;
        attackRate = parameters.attackRate;
        damage = parameters.damage;
        life = parameters.life;
        maxLife = parameters.maxLife;
        mobName = parameters.mobName;
    }

    // Update is called once per frame
    void Update()
    {
        if (!target && attacking) attacking = false;
        else
        {
            if(attacking)
            {
                elaspedTime += Time.deltaTime;
                if (elaspedTime - attackRate > 0)
                {
                    switch (target.tag)
                    {
                        case "Structure":
                            {
                                target.GetComponent<Structure>().hit(damage);
                            }
                            break;
                        case "Objective":
                            {
                                target.GetComponent<Objective>().hit(damage);
                            }
                            break;
                    }
                    elaspedTime = 0;
                }
            }
            else
            {
                checkForCloseEnnemies();
            }
        }

        if (!target && !attacking) checkForCloseEnnemies();
    }

    void checkForCloseEnnemies()
    {
        int layerId = LayerMask.NameToLayer(targetMask);
        int layerMask = 1 << layerId;
        structuresWithinRange = Physics.OverlapSphere(transform.position, detectRange, layerMask);
        if (structuresWithinRange.Length > 0)
        {
            target = structuresWithinRange[0].gameObject;
            float dist = Vector3.Distance(transform.position, target.transform.position);
            foreach (Collider c in structuresWithinRange)
            {
                if (c.GetComponent<Structure>().isStateEnabled())
                {
                    float tempDist = Vector3.Distance(transform.position, c.transform.position);
                    if (tempDist < dist) target = c.gameObject;
                }
            }
            if (target.GetComponent<Structure>().isStateEnabled())
                agent.SetDestination(target.transform.position);
            else
                target = null;
        }

        if (!target)
        {
            target = GameObject.Find("Objective");
            goal = (target) ? target.transform : transform;
            agent.SetDestination(goal.position);
        }
    }

    void FixedUpdate()
    {
        detectionUpdateElapsedTime += Time.fixedDeltaTime;
        if(detectionUpdateElapsedTime - .5f > 0)
        {
            checkForCloseEnnemies();
        }
        // Check if we've reached the destination
        if (!agent.pathPending && !attacking)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    if(!GameObject.Equals(gameObject, goal))
                    {
                        if(goal)
                        {
                            if(goal.gameObject.layer == LayerMask.NameToLayer(targetMask))
                            {
                                attacking = true;
                            }
                        }
                       
                    }
                    else
                    {
                        attacking = false;
                        target = null;
                    }
                }
            }
        }
    }

    void die()
    {
        controller.giveMoney(reward);
        controller.mobCount -= 1;
        gameObject.SetActive(false);
    }

    public void hit(DamageType type, float damage)
    {
        float damageF = (type == DamageType.Physical) ? ((100f-((float)armour-PhysicalArmourReduction))/100f)*damage : ((100f - (float)resistance-MagicalArmourReduction) / 100f)*damage;
        life = life - Mathf.CeilToInt(damageF);
        if (life <= 0) die();
    }

    public void resetBehaviour()
    {
        attacking = false;
        target = null;
        life = maxLife;
    }
}
