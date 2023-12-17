using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Relation {
    public MeepleSim target;
    public float friendlyness = 50;
}
public class MeepleSim {
    // All data a meeple has
    public string type = "worker";
    public float hunger,sleep,social,fun,hygiene,bathroom = 100.0f;
    public float morale = 100.0f;
    public Vector3 position;
    public Household household;
    public Job job;
    public Meeple meepleGM;
    public Relation[] relations;
    public List<Thought> thoughts;
}
public class Meeple : MonoBehaviour
{
    public MeepleSim meepleSim;
    public bool observed = false;
    // Attached to a gameobject
    // if the meeple has been observed recently, an internal AI function will run every update, to determine what to do
    // otherwise, the City monobehaviour will take care of moving the meeple around

    // If the meeple enters a workplace that the player works in, this will count as being observed
    // If the meeple enters a household where the player built in, or is currently in, this will also count as being observed
}
