using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Thought : ScriptableObject
{
    // This can be attached to some trigger
    // examples of triggers would be: certain furniture on the same room
    // recent relation changes
    // etc
    public string ID = "";
    public string description = "";

    public float morale = 0;
}
