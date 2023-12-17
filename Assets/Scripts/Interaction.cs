using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Interaction : ScriptableObject
{
    public string ID = "nothing";
    public AnimationClip animationToPlay;
    public float hunger,sleep,social,fun,hygiene,bathroom = 0;
}
