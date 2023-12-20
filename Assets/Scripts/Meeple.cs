using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;


public class Relation {
    public MeepleSim target;
    public float friendlyness = 50;
}
public class MeepleSim {
    // All data a meeple has
    public string type = "worker";
    public float hunger,sleep,social,fun,hygiene,bathroom = 100.0f;
    public AnimationCurve[] needsCurves = new AnimationCurve[6];
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
    Place place;
    // Attached to a gameobject
    // if the meeple has been observed recently, an internal AI function will run every update, to determine what to do
    // otherwise, the City monobehaviour will take care of moving the meeple around

    // If the meeple enters a workplace that the player works in, this will count as being observed
    // If the meeple enters a household where the player built in, or is currently in, this will also count as being observed

    void run_AI(){
        float[] needs = {meepleSim.hunger,meepleSim.sleep,meepleSim.social,meepleSim.fun,meepleSim.hygiene,meepleSim.bathroom};
        string[] needNames = {"hunger", "sleep","social","fun","hygiene","bathroom"};
        int i = 0;
        float highest = 0;
        string type = "";
        foreach(float need in needs){
            if(meepleSim.needsCurves[i].Evaluate(need) > highest){
                highest = meepleSim.needsCurves[i].Evaluate(need);
                type = needNames[i];
            }
            i += 1;
        }
    }
    void Update(){
        
    }
}
