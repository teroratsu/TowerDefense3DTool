using UnityEngine;
using System.Collections;
using UnityEditor;

public class SpawnEntity : ScriptableWizard
{
    [Range (0, 1000)]public int count;
    public MobSpeed speed;
    public MobArmour armour;

    [MenuItem ("My Tools/Create Entity")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<SpawnEntity>("Create Entity", "Create new", "Update selected");
    }

    void OnWizardCreate()
    {
        GameObject entity = new GameObject();
        Mob e = entity.AddComponent<Mob>();
        e.speed = speed;
        e.armour = armour;
    }

    void OnWizardOtherButton()
    {
        if(Selection.activeTransform != null)
        {
            Mob e = Selection.activeGameObject.GetComponent<Mob>();
            e.speed = speed;
            e.armour = armour;
        }
    }

}
