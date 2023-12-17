using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Furniture : MonoBehaviour
{
    // This is always attached to a prefab with configured interactions
    // ID is used as the display name
    public string ID = "none";

    // DO NOT change these directly, the scripts assigns them according to what interactions are available
    [HideInInspector]
    public float hunger,sleep,social,fun,hygiene,bathroom = 0;

    public Interaction[] interactions;

    void Start(){
        foreach(Interaction interaction in interactions){
            // find out which interaction has the highest need satisfaction for each need type
            if(hunger < interaction.hunger){
                hunger = interaction.hunger;
            }
            if(social < interaction.social){
                social = interaction.social;
            }
            if(sleep < interaction.sleep){
                sleep = interaction.sleep;
            }
            if(fun < interaction.fun){
                fun = interaction.fun;
            }
            if(bathroom < interaction.bathroom){
                bathroom = interaction.bathroom;
            }
            if(hygiene < interaction.hygiene){
                hygiene = interaction.hygiene;
            }
        }
    }
    public Interaction GetInteraction(string type = "hunger"){
        // Get the interaction that has the highest hunger, social, sleep, fun, bathroom or hygiene for this furniture
        type = type.ToLower();
        foreach(Interaction interaction in interactions){
            switch(type){
                case "hunger":
                    if(interaction.hunger == hunger){
                        return interaction;
                    }
                    break;
                case "social":
                    if(interaction.social == social){
                        return interaction;
                    }
                    break;
                case "sleep":
                    if(interaction.sleep == sleep){
                        return interaction;
                    }
                    break;
                case "fun":
                    if(interaction.fun == fun){
                        return interaction;
                    }
                    break;
                case "bathroom":
                    if(interaction.bathroom == bathroom){
                        return interaction;
                    }
                    break;
                case "hygiene":
                    if(interaction.hygiene == hygiene){
                        return interaction;
                    }
                    break;
            }
        }
        return null;
    }
}
