using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActiveThought
{
    // This gets copied into a meeple's thoughts
    // so that it can be modified
    public string ID = "";
    public string description = "";
    public float length = 1;

    public float morale = 0;
    public ActiveThought copy(){
        return new ActiveThought{ID=ID,description=description,length=length,morale=morale};
    }
    
}

[CreateAssetMenu()]
public class Thought : ScriptableObject
{
    // This can be attached to some trigger
    // examples of triggers would be: certain furniture on the same room
    // recent relation changes
    // etc
    public ActiveThought activeThought;
}
