using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class StoreInteraction : Interaction
{
    public float price = 0;
    public Item item;
    public int stock = 1;
}