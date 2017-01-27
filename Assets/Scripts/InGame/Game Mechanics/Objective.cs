using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Objective : MonoBehaviour
{

    public int health = 100;
    public int maxHealth = 100;

    public Slider healthBar;
    public Text healthInfo;

    public GameObject mesh; //mesh to destroy

    // Use this for initialization
    void Start()
    {
        maxHealth = health;

        mesh.AddComponent<TriangleExplosion>();
    }

    void FixedUpdate()
    {
        healthBar.value = (float)health/(float)maxHealth;
        healthInfo.text = health.ToString() + "/" + maxHealth.ToString();
    }

    public void hit(float damage)
    {
        if (health > 0)
        {
            health -= Mathf.CeilToInt(damage);
            if (health <= 0) explode();
        }
    }

    void explode()
    {
        if(mesh.GetComponent<MeshFilter>().sharedMesh.isReadable)
            StartCoroutine(mesh.GetComponent<TriangleExplosion>().SplitMesh(false));
        health = 0;
        healthBar.value = (float)health / (float)maxHealth;
        healthInfo.text = health.ToString() + "/" + maxHealth.ToString();
        gameObject.SetActive(false);
    }

    public bool isAlive()
    {
        return health > 0;
    }
}
