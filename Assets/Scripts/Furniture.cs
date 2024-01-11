using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    public string Fname = "";
    // Where the player/meeple will stand while performing the interaction animation
    public Transform interactionPoint;
    // for meeples
    public string animationIDToPlay = "interact";
    public float timeRequired;

    //multiple meeples can use
    public int seatsAvailable=1;
    public int seatsTaken=0;
    //but if multiple meeples can use then they will all go to the same spot

    //for npc meeples
    public float health = 0;
    public float morale = 0;
    // for the player
    public Interaction[] interactions;
}
