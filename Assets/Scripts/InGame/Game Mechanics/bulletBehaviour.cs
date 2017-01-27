using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Physical,
    Magical
}

[RequireComponent(typeof(Rigidbody))]
public class bulletBehaviour : MonoBehaviour {

    public float velocity = 100f;
    public float lifeSpan = 5f;

    private float damage;
    private DamageType damageType;

    private Rigidbody rb;
    private float elapsedTime;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
    }

    public void setProperties(DamageType type, float dmg)
    {
        if (!rb) rb = GetComponent<Rigidbody>();

        rb.velocity = Vector3.zero;
        
        damageType = type;
        damage = dmg;
        rb.velocity = transform.forward * velocity + transform.up;

        elapsedTime = 0;
    }
	
	// Update is called once per frame
	void Update () {
        elapsedTime += Time.deltaTime;
        if (elapsedTime - lifeSpan > 0) gameObject.SetActive(false);
	}

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("MobLayer"))
        {
            Mob mob = other.gameObject.GetComponent<Mob>();
            mob.hit(damageType, damage);
            gameObject.SetActive(false);
        }
    }
}
