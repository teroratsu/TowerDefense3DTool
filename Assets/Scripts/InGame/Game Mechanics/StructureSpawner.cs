using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpawnableStructure
{
    public StructureType type;
    public string name;
    public GameObject gameObject;

    public float damage;
    public DamageType damageType;

    public int basePrice;
}

public class StructureSpawner : MonoBehaviour {

    private GameControl controller;
    public SpawnableStructure selected;  //selected GameObject
     SpawnerState state;

    private SpawnerState placingState;
    private SpawnerState rotatingState;
    private SpawnerState readyState;

    public SpawnerState getRotatingState() { return rotatingState; }
    public SpawnerState getPlacingState() { return placingState; }
    public SpawnerState getReadyState() { return readyState; }

    public void changeSelected(SpawnableStructure newStruct)
    {
        selected = newStruct;
        if(state != readyState)
            state.destroyActiveObject();
        ChangeActiveState(placingState, null);
    }

    public void ChangeActiveState(SpawnerState other, GameObject go)
    {
        other.goCp = go;
        state = other;
        state.Enter();
    }

	// Use this for initialization
	void Start () {
        placingState = new PlacingState(this);
        rotatingState = new RotatingState(this);
        readyState = new ReadyState(this);
        state = placingState;
        controller = GameObject.Find("GameControl").GetComponent<GameControl>();
        this.enabled = false;
	}
    
	// Update is called once per frame
	void Update () {
        if (selected.gameObject)
            state.Handle();
    }

    public void showConfirmationMessage(bool bought)
    {
        String text = (bought) ? "Bought " + selected.name + " (-" + selected.basePrice + "g)" : "Not enough money to buy " + selected.name;
        Color color = (bought) ? Color.yellow : Color.white;
        StartCoroutine(controller.ShowMessage(text , 1.5f, color));
    }
    
}

public abstract class SpawnerState
{
    public StructureSpawner structureSpawner;
    public GameObject goCp;

    public int layerSeeThrough;

    public SpawnerState(StructureSpawner spawner)
    {
        goCp = null;
        structureSpawner = spawner;

        int layerId = LayerMask.NameToLayer("StructureLayer");
        layerSeeThrough = ~(1 << layerId);
    }

    public void destroyActiveObject()
    {
        GameObject.DestroyImmediate(goCp);
    }

    public abstract void Handle();
    public abstract void Leave();
    public abstract void Enter();
}

class PlacingState : SpawnerState
{
    private bool instanciated;

    private string spawningLayer;

    private Vector3 offset;
    private Ray ray;
    private RaycastHit hit;

    private Transform t_destination;
    private Structure s;

    public PlacingState(StructureSpawner spawner) : base(spawner)
    {
        spawningLayer = "SpawnableFloor";
    }

    public override void Handle()
    {
        if(!instanciated || !goCp) // instantiate go
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f, layerSeeThrough))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer(spawningLayer))
                {
                    goCp = GameObject.Instantiate(structureSpawner.selected.gameObject, hit.point, Quaternion.identity);
                    
                    s = goCp.AddComponent<Structure>();
                    Transform t = goCp.transform;
                    Bounds bounds = goCp.GetComponent<Renderer>().bounds;
                    float distFromGround = bounds.center.y - bounds.min.y;
                    offset = new Vector3(0, distFromGround, 0);
                    t.position += offset;
                    t_destination = goCp.transform;

                    s.init(structureSpawner.selected, t);
                    
                    instanciated = true;
                    goCp.GetComponent<Renderer>().material.color = Color.green;
                }
            }
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f, layerSeeThrough))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("SpawnableFloor"))
                {
                    t_destination.position = hit.point + offset;
                    s.setTransform(t_destination);
                }
            }
        }
        if (Input.GetButtonDown("Fire1"))
        {
            Leave();
            structureSpawner.ChangeActiveState(structureSpawner.getRotatingState(), goCp);
        }
        if (Input.GetButtonDown("Fire2"))
        {
            Leave();
            GameObject.Destroy(goCp.gameObject);
            structureSpawner.selected.gameObject = null;
        }
    }

    public override void Enter()
    {
        instanciated = true;
        if(goCp)
            goCp.GetComponent<Renderer>().material.color = Color.green;
    }

    public override void Leave()
    {
        instanciated = false;
    }
}

class RotatingState : SpawnerState
{
    private Ray ray;
    private RaycastHit hit;

    private Transform t_rotation;

    public RotatingState(StructureSpawner spawner) : base(spawner)
    {
    }

    public override void Handle()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f, layerSeeThrough))
        {
            if (!hit.collider.gameObject.Equals(goCp))
            {
                goCp.transform.LookAt(hit.point);
                goCp.transform.rotation = Quaternion.Euler(0, goCp.transform.rotation.eulerAngles.y, 0);
            }
        }
        if (Input.GetButtonDown("Fire1"))
        {
            Leave();
            structureSpawner.ChangeActiveState(structureSpawner.getReadyState(), goCp);
        }
        if (Input.GetButtonDown("Fire2"))
        {
            Leave();
            structureSpawner.ChangeActiveState(structureSpawner.getPlacingState(), goCp);
        }
    }

    public override void Enter()
    {
        t_rotation = goCp.transform;
        goCp.GetComponent<Renderer>().material.color = Color.blue;
    }

    public override void Leave() {}
}

class ReadyState : SpawnerState
{
    public ReadyState(StructureSpawner spawner) : base(spawner)
    {
    }

    public override void Handle(){ }

    public override void Enter()
    {
        bool bought = goCp.GetComponent<Structure>().buy();
        if (!bought)
            GameObject.Destroy(goCp);
        else
            goCp.GetComponent<Renderer>().material.color = Color.white;

        structureSpawner.showConfirmationMessage(bought);

        structureSpawner.selected.gameObject = null;
    }

    public override void Leave() { }
}
