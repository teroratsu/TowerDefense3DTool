using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructureDropdown : MonoBehaviour {

    public GameObject btnPrefab;

    public Transform turretPanel;
    private List<SpawnableStructure> turrets;
    public Transform shieldPanel;
    private List<SpawnableStructure> shields;
    public Transform spawnerPanel;
    private List<SpawnableStructure> spawners;

    private GameControl controller;
    private StructureSpawner structspawner;

	void Start () {
        controller = GameObject.Find("GameControl").GetComponent<GameControl>();
        structspawner = GameObject.Find("view").GetComponent<StructureSpawner>();

        turrets = new List<SpawnableStructure>();
        shields = new List<SpawnableStructure>();
        spawners = new List<SpawnableStructure>();

        int i_turret, i_shield, i_spawner;
        i_turret = i_shield = i_spawner = 0;

        foreach ( SpawnableStructure structure in controller.spawnableStructures)
        {
            int index = 0;
            StructureType type = structure.type;
            Transform parent = gameObject.transform;
            GameObject button = (GameObject)Instantiate(btnPrefab);
            button.GetComponentInChildren<Text>().text = structure.name + " (" + structure.basePrice + "g)";
            switch (structure.type)
            {
                case StructureType.Turret:
                    {
                        turrets.Add(structure);
                        index = i_turret++;
                        parent = turretPanel;
                    }; break;
                case StructureType.Shield:
                    {
                        shields.Add(structure);
                        index = i_shield++;
                        parent = shieldPanel;
                    }; break;
                case StructureType.Spawner:
                    {
                        spawners.Add(structure);
                        index = i_spawner++;
                        parent = spawnerPanel;
                    }; break;
            }
            button.GetComponent<Button>().onClick.AddListener(
                () => { setSelected(index, type); }
                );
            button.transform.SetParent(parent);
        }

        this.gameObject.SetActive(false);
    }
	
	void setSelected (int index, StructureType type) {
        bool canBuy = false;
        switch (type)
        {
            case StructureType.Turret:
                {
                    if(turrets[index].basePrice <= controller.golds)
                    {
                        canBuy = true;
                        structspawner.changeSelected(turrets[index]);
                    }
                }; break;
            case StructureType.Shield:
                {
                    if (shields[index].basePrice <= controller.golds)
                    {
                        canBuy = true;
                        structspawner.changeSelected(shields[index]);
                    }
                }; break;
            case StructureType.Spawner:
                {
                    if (spawners[index].basePrice <= controller.golds)
                    {
                        canBuy = true;
                        structspawner.changeSelected(spawners[index]);
                    }
                }; break;
        }
        if (canBuy)
            controller.switchBuildMode();
        else
            displayCantBuyMessage();
    }

    void displayCantBuyMessage()
    {
        StartCoroutine(controller.ShowMessage("Not enough money!", 2, Color.red));
    }
}
