using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meepleScript : MonoBehaviour
{
    public string full_name = "";

    public GameObject current_room;

    private int health; // 0 to 100
    private int morale; // 0 to 100

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //basics: checking needs and forfilling them 

        //checks needs every x number of seconds 

        //finds all avaible furniture within a room 
        //sort the avaible options 

        //walks towards the furniture interaction location (navmesh)
        //interact with it (play animation, take time) 
        //need is forfilled 

        //complex: all the other actions + routine

    }
}
