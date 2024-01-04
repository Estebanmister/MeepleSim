using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Place : MonoBehaviour
{
    // Base class for a room
    public List<Meeple> meeples_on_site = new List<Meeple>();
    public List<Furniture> furnitures = new List<Furniture>();
    public Collider bounds;
    public bool observed = false;
    Outside outside;
    public MeshRenderer theCube;
    void Start(){
        GameObject.FindGameObjectWithTag("streets").GetComponent<Outside>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "meeple"){
            Meeple meeple = other.GetComponent<Meeple>();
            meeple.place = this;
            meeples_on_site.Add(meeple);
        }
        if(other.tag == "Player"){
            observed = true;
            Meeple meeple = other.GetComponent<Meeple>();
            meeple.place = this;
            meeples_on_site.Add(meeple);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "meeple"){
            Meeple meeple = other.GetComponent<Meeple>();
            meeple.place = outside;
            meeples_on_site.Remove(meeple);
        }
        if(other.tag == "Player"){
            observed = false;
            Meeple meeple = other.GetComponent<Meeple>();
            meeple.place = outside;
            meeples_on_site.Remove(meeple);
        }
    }
    void Update(){
        if(observed && theCube.enabled){
            theCube.enabled = false;
        }
        if(!observed && !theCube.enabled){
            theCube.enabled = true;
        }
    }

}
