using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class roomScript : MonoBehaviour
{
    public bool isWorkPlace=false;
    public GameObject roomOwner;
    public GameObject[] allFurniture; //for stat interactions 
    public GameObject[] allMeeples; //for social interactions 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //keeps track of all the objects within the room (meeples and furniture)
    }
}
