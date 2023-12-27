using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class City : MonoBehaviour
{
    // never resets;
    public int weeks;
    // resets after 7 days
    public float globalTime;
    // resets after 24 hours
    public float clockTime;
    public Meeple[] meeples;
    public GameObject meeplePrefab;

    Job[] jobs;
    Workplace[] workplaces;
    Household[] households;
    void Start()
    {
    }

    void Update(){
        globalTime += Time.deltaTime;
        // after 7 days
        if(globalTime > 604800){
            globalTime = 0;
            weeks += 1;
        }
    }

    // Here will be the simulator logic for all unobserved meeple
    // although needs are overall position is simplified, relations still affect morale and thoughts
    // relations update exactly the same way as if the meeple were observed
    
}
