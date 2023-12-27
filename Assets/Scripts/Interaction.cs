using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu()]
public class Interaction : ScriptableObject
{
    public string ID = "basic";
    public string description = "";
    public string animationToPlay = "interact";
    public float hunger,sleep,social,fun,hygiene,bathroom = 0;
    public float interactionLength = 1;
    public bool skillCheck = false;
    public string[] skillsToCheck;
    public int[] requiredLevels;
    public bool skillIncrease = false;
    public string[] skillsToChange;
    public int[] skillLevelstoChange;
}

public class SocialInteraction : Interaction
{
    public SocialInteraction(){
        ID = "social";
    }
    public Meeple otherMeeple;
}

public class TransportInteraction: Interaction
{
    public TransportInteraction(){
        ID = "transport";
    }
    public Vector3 position;
}