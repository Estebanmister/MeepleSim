using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class Item : ScriptableObject
{
    public GameObject placeAblePrefab;
    public ItemInteraction[] interactions;
    public bool isViewable = false;
    public string viewAbleText = "";
}

public class ItemHolder : MonoBehaviour
{
    public Item item;
}