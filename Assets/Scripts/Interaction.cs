using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu()]
public class Interaction : ScriptableObject
{
    public string IntName = "";
    public string animationToPlay = "interact";
    public float hunger,sleep,social,fun,hygiene,bathroom = 0;
    public float interactionLength = 1;
    public bool skillCheck = false;
    public string[] skillsToCheck;
    public int[] requiredLevels;
    public bool skillIncrease = false;
    public string[] skillsToChange;
    public float[] skillLevelstoChange;
    public Thought[] induceThoughts;
    // Do player's needs decay while perforing the action?
    public bool needsDecay = true;
}


