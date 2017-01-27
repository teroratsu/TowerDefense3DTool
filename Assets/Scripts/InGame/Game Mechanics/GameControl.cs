using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class GameControl : MonoBehaviour {

    public static GameControl control;

    private ExtendedFlycam flycam;
    public GameObject flycamHolder;
    public GameObject buildUI;
    public GameObject battleGUI;
    public Objective objective;

    public GameObject messageGUI;
    private Text textHandler;
    GUIStyle style ;

    public int mobCount = 0;

    private StructureSpawner structureSpawner;
    private PlayerInteraction playerInteract;

    public bool onConstructMode;
    private bool inGame;


    [Header("Structures Settings")]
    public List<TurretData> turretData = new List<TurretData>();
    public List<ShieldData> shieldData = new List<ShieldData>();
    public List<SpawnerData> spawnerData = new List<SpawnerData>();

    public SpawnableStructure[] spawnableStructures;       // List of all spawnabke gameObjects

    public GameObject bulletPrefab;
    public ObjectPoolScript bulletPool;    // container for all bullets 

    [Header("Misc Settings")]
    public int golds; // amount of money

	void Awake () {
        if (control == null)
        {
            DontDestroyOnLoad(gameObject);
            control = this;
        }
        else if(control != this)
        {
            Destroy(gameObject);
        }
        onConstructMode = false;
        flycam = flycamHolder.GetComponent<ExtendedFlycam>();
        structureSpawner = GameObject.Find("view").GetComponent<StructureSpawner>();
        playerInteract = GameObject.Find("view").GetComponent<PlayerInteraction>();
        objective = GameObject.Find("Objective").GetComponent<Objective>();
        bulletPool = gameObject.AddComponent<ObjectPoolScript>();
        bulletPool.pooledObject = bulletPrefab;
        style = new GUIStyle();
        style.fontSize = 20;
        if (messageGUI)
            textHandler = messageGUI.GetComponent<Text>();
    }

    public bool isInGame()
    {
        return inGame;
    }

    void Update()
    {
        if (Input.GetButtonDown("ToggleBuildMode"))
        {
            switchBuildMode();
        }
        if(Input.GetButtonDown("StartWave"))
        {
            inGame = true;
        }
        if(Input.GetButtonDown("Upgrade"))
        {
            Structure s = playerInteract.getSelectedStructure();
            if (s) s.upgrade();
        }
        if (Input.GetButtonDown("Downgrade"))
        {
            Structure s = playerInteract.getSelectedStructure();
            if (s) s.downgrade();
        }
        if (Input.GetButtonDown("Sell"))
        {
            Structure s = playerInteract.getSelectedStructure();
            if (s) s.sell();
        }

        if(inGame && !objective.isAlive())
        {
            StartCoroutine(ShowMessage("YOU FAILED!", 5f, Color.red));
        }
        else
        {
            if(inGame && objective.isAlive() && mobCount <= 0)
            {
                StartCoroutine(ShowMessage("YOU WIN!", 5f, Color.green));
            }
        }
    }

    public bool buy(int amount)
    {
        bool canUpgrade = (golds - amount) > 0;
        if (canUpgrade)
            golds -= amount;
        return canUpgrade;
    }

    public void giveMoney(int amount)
    {
        golds += amount;
    }

    public void switchBuildMode()
    {
        onConstructMode = !onConstructMode;
        flycam.enabled = !onConstructMode;
        buildUI.SetActive(onConstructMode);
        battleGUI.SetActive(!onConstructMode);
        Cursor.lockState = (Cursor.lockState == CursorLockMode.None) ? CursorLockMode.Locked : CursorLockMode.None;
        structureSpawner.enabled = !onConstructMode;
    }

	void OnGUI () {
        GUI.Label(new Rect(50, Screen.height - 50, 200, 50), "Golds left: " + golds, style);
        if(!inGame)
        {
            GUI.Label(new Rect(Screen.width - 300, Screen.height - 50, 300, 50), "Press 'G' to start the wave", style);
        }
        else
        {
            GUI.Label(new Rect(Screen.width - 300, Screen.height - 50, 300, 50), "Mob left : " + mobCount, style);
        }
    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/playerInfo.dat");

        PlayerData data = new PlayerData();
        data.golds = golds;

        bf.Serialize(file, data);
        file.Close();
    }

    public void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            golds = data.golds;
        }
    }

    public IEnumerator ShowMessage(string message, float delay, Color color)
    {
        textHandler.color = color;
        textHandler.text = message;
        textHandler.enabled = true;
        yield return new WaitForSeconds(delay);
        textHandler.enabled = false;
    }

    public TurretData getTurretData(int level) 
    {
        return (TurretData) turretData[level];
    }
    public ShieldData getShieldData(int level)
    {
        return (ShieldData)shieldData[level];
    }
    public SpawnerData getSpawnerData(int level)
    {
        return (SpawnerData)spawnerData[level];
    }
}

[Serializable]
class PlayerData
{
    public int golds;
}
