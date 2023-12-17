using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class City : MonoBehaviour
{
    public Meeple[] meeples;
    public GameObject meeplePrefab;

    Job[] jobs;
    Workplace[] workplaces;
    Household[] households;
    void Start()
    {
        meeples = new Meeple[2];
        for(int i = 0; i < 2; i += 1){
            GameObject newmeeple = Instantiate(meeplePrefab);
            meeples[i] = newmeeple.GetComponent<Meeple>();
        }
    }

    // Here will be the simulator logic for all unobserved meeple
    // although needs are overall position is simplified, relations still affect morale and thoughts
    // relations update exactly the same way as if the meeple were observed
    
}
