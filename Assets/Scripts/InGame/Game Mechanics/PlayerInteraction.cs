using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour {
    Ray ray;
    RaycastHit hit;
    private string structureLayer, enemyLayer;

    public GameObject persistentUI;
    private GameObject structureInterface;
    private GameObject mobInterface;

    private Text mobHealth;
    private Text mobName;
    private Slider mobHealthSlider;

    private Text structureHealth;
    private Text structureName;
    private Slider structureHealthSlider;
    private Text upgradeText;
    private Text downgradeText;
    private Text sellText;

    private bool display;
    private float timeElapsed;
    public float refreshRate = .2f;

    public GameObject target;
    public GameObject selectedTarget;

    Mob t_mob;
    Structure t_structure;

    // Use this for initialization
    void Start()
    {
        structureLayer = "StructureLayer";
        enemyLayer = "MobLayer";

        structureInterface = persistentUI.transform.FindChild("structureInterface").gameObject;
        mobInterface = persistentUI.transform.FindChild("mobInterface").gameObject;

        mobHealth =                 mobInterface.transform.Find("health").gameObject.GetComponent<Text>();
        mobName =                   mobInterface.transform.Find("name").gameObject.GetComponent<Text>();
        mobHealthSlider =           mobInterface.transform.Find("HPSlider").gameObject.GetComponent<Slider>();

        structureHealth =           structureInterface.transform.Find("health").gameObject.GetComponent<Text>();
        structureHealthSlider =     structureInterface.transform.Find("HPSlider").gameObject.GetComponent<Slider>();
        structureName =             structureInterface.transform.Find("name").gameObject.GetComponent<Text>();
        upgradeText =               structureInterface.transform.Find("upgrade").gameObject.GetComponent<Text>();
        downgradeText =             structureInterface.transform.Find("downgrade").gameObject.GetComponent<Text>();
        sellText =                  structureInterface.transform.Find("sell").gameObject.GetComponent<Text>();

        structureInterface.SetActive(false);
        mobInterface.SetActive(false);

        target = null;
        selectedTarget = null;
        display = false;
    }

    public Structure getSelectedStructure()
    {
        return t_structure;
    }

    void Update()
    {
        display = (GameObject.Equals(target, selectedTarget)) ? true : false;
        timeElapsed += Time.deltaTime;
        if(timeElapsed - refreshRate > 0)
        {
            updateInterface();
            timeElapsed = 0;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            if(!GameObject.Equals(target, hit.collider.gameObject))
            {
                target = hit.collider.gameObject;
                if (LayerMask.LayerToName(target.layer) == structureLayer)
                {
                    if (target.CompareTag("Structure"))
                    {
                        t_structure = target.GetComponent<Structure>();
                        t_mob = null;
                        selectedTarget = target;
                    }
                    else
                    {
                        t_structure = null;
                        t_mob = null;
                    }
                }
                else
                {
                    if (target.layer == LayerMask.NameToLayer(enemyLayer))
                    {
                        t_mob = target.GetComponent<Mob>();
                        t_structure = null;
                        selectedTarget = target;
                    }
                    else
                    {
                        t_structure = null;
                        t_mob = null;
                    }
                }
            }
        }
    }

    void updateInterface()
    {
        if(display)
        {
            if(t_structure)
            {
                int currentHP, maxHP;
                currentHP = t_structure.getHP();
                maxHP = t_structure.getMaxHP();
                structureHealth.text = currentHP.ToString() + "/" + maxHP.ToString();
                structureName.text = t_structure.structName + " (lvl " + (t_structure.getLevel()+1) + ")";
                structureHealthSlider.value = (float)currentHP / (float)maxHP;
                if(t_structure.isStateEnabled())
                {
                    if (t_structure.canDowngrade())
                    {
                        downgradeText.text = "press " + PlayerPrefs.GetString("Downgrade") + " to downgrade (+" + t_structure.downgradePrice() + "g)";
                    }
                    else downgradeText.text = "";
                    if (t_structure.canUpgrade())
                    {
                        upgradeText.text = "press " + PlayerPrefs.GetString("Upgrade") + " to upgrade (-" + t_structure.upgradePrice() + "g)";
                    }
                    else upgradeText.text = "";
                    sellText.text = "press " + PlayerPrefs.GetString("Sell") + " to sell (-" + t_structure.sellPrice() + "g)";
                }
                else
                {
                    downgradeText.text = "";
                    upgradeText.text = "";
                    sellText.text = "";
                }
                if (!structureInterface.activeInHierarchy) structureInterface.SetActive(true);
            }
            else
            {
                if (structureInterface.activeInHierarchy) structureInterface.SetActive(false);
            }
            if(t_mob)
            {
                int currentHP, maxHP;
                currentHP = t_mob.life;
                maxHP = t_mob.maxLife;
                mobHealth.text = currentHP.ToString() + "/" + maxHP.ToString();
                mobName.text = t_mob.mobName;
                mobHealthSlider.value = (float)currentHP / (float)maxHP;
                if (!mobInterface.activeInHierarchy) mobInterface.SetActive(true);
            }
            else
            {
                if (mobInterface.activeInHierarchy) mobInterface.SetActive(false);
            }
        }
        else
        {
            if (structureInterface.activeInHierarchy) structureInterface.SetActive(false);
            if (mobInterface.activeInHierarchy) mobInterface.SetActive(false);
        }
    }
}
