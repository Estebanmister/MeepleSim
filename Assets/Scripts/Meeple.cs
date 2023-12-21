using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.AI;


public class Relation {
    public MeepleSim target;
    public float friendlyness = 50;
}

[Serializable]
public class MeepleSim {
    // All data a meeple has
    public string type = "worker";
    public float hunger,sleep,social,fun,hygiene,bathroom = 100.0f;
    public AnimationCurve[] needsCurves = new AnimationCurve[6];
    public float morale = 100.0f;
    public Vector3 position;
    public Household household;
    public Job job;
    public Meeple meepleGM;
    public Relation[] relations;
    public List<Thought> thoughts;
}
public class Meeple : MonoBehaviour
{
    // meepleSim is assigned by the city that has instantiated this game object
    public MeepleSim meepleSim;
    Animator animator;
    public bool observed = false;
    public Place place;
    NavMeshAgent agent;
    bool doingSomething;

    Furniture interactingWith;
    Interaction toDo;
    // Attached to a gameobject
    // if the meeple has been observed recently, an internal AI function will run every update, to determine what to do
    // otherwise, the City monobehaviour will take care of moving the meeple around

    // If the meeple enters a workplace that the player works in, this will count as being observed
    // If the meeple enters a household where the player built in, or is currently in, this will also count as being observed

    void Start(){
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }
    void run_AI(){
        float[] needs = {meepleSim.hunger,meepleSim.sleep,meepleSim.social,meepleSim.fun,meepleSim.hygiene,meepleSim.bathroom};
        string[] needNames = {"hunger", "sleep","social","fun","hygiene","bathroom"};
        int i = 0;
        float[] needVals = new float[6];
        foreach(float need in needs){
            needVals[i] = meepleSim.needsCurves[i].Evaluate(need/100);
            i += 1;
        }
        var result = Enumerable.Range(0, needVals.Length)
              .OrderByDescending(index => needVals[index])
              .ToList();

        foreach(int needIndex in result){
            if(needVals[needIndex] > 0.5f){
                float highest_f = 0;
                Furniture toInteract = null;
                Interaction interaction = null;
                foreach(Furniture furniture in place.furnitures){
                    Interaction test = furniture.GetInteraction(needNames[needIndex]);
                    // Check if we can pathfind to the interaction point of this furniture piece
                    // before doing anything else
                    NavMeshPath path = new NavMeshPath();
                    agent.CalculatePath(furniture.interactionPoint.position, path);
                    if (path.status != NavMeshPathStatus.PathComplete)
                    {
                        // furniture is not reachable by agent
                        // check other furniture in room
                        Debug.Log("UNREACHABLE " + furniture.name);
                        continue;
                    }
                    switch(needNames[needIndex]){
                        // this sucks ass
                        case "hunger":
                            if(test.hunger > highest_f){
                                highest_f = test.hunger;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "social":
                            if(test.social > highest_f){
                                highest_f = test.social;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "sleep":
                            if(test.sleep > highest_f){
                                highest_f = test.sleep;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "fun":
                            if(test.fun > highest_f){
                                highest_f = test.fun;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "bathroom":
                            if(test.bathroom > highest_f){
                                highest_f = test.bathroom;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "hygiene":
                            if(test.hygiene > highest_f){
                                highest_f = test.hygiene;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                    }
                }
                if(toInteract != null){
                    // move towards the furniture item
                    agent.destination = toInteract.interactionPoint.position;
                    toDo = interaction;
                    interactingWith = toInteract;
                    doingSomething = true;
                    break;
                }
            }
        }
        
    }
    float temp_count = 0;
    void Update(){
        if(observed){
            // we are being observed, run the ai
            meepleSim.hunger += (meepleSim.hunger<100) ? Time.deltaTime : 0;
            meepleSim.social += (meepleSim.social<100) ? Time.deltaTime : 0;
            meepleSim.sleep += (meepleSim.sleep<100) ? Time.deltaTime : 0;
            meepleSim.hygiene += (meepleSim.hygiene<100) ? Time.deltaTime : 0;
            meepleSim.bathroom += (meepleSim.bathroom<100) ? Time.deltaTime : 0;
            meepleSim.fun += (meepleSim.fun<100) ? Time.deltaTime : 0;
            if(doingSomething){
                if (!agent.pathPending)
                {
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                        {
                            animator.Play(toDo.animationToPlay);
                            meepleSim.hunger -= toDo.hunger;
                            meepleSim.hunger = meepleSim.hunger < 0 ? 0 : meepleSim.hunger;
                            meepleSim.social -= toDo.social;
                            meepleSim.social = meepleSim.social < 0 ? 0 : meepleSim.social;
                            meepleSim.sleep -= toDo.sleep;
                            meepleSim.sleep = meepleSim.sleep < 0 ? 0 : meepleSim.sleep;
                            meepleSim.hygiene -= toDo.hygiene;
                            meepleSim.hygiene = meepleSim.hygiene < 0 ? 0 : meepleSim.hygiene;
                            meepleSim.bathroom -= toDo.bathroom;
                            meepleSim.bathroom = meepleSim.bathroom < 0 ? 0 : meepleSim.bathroom;
                            meepleSim.fun -= toDo.fun;
                            meepleSim.fun = meepleSim.fun < 0 ? 0 : meepleSim.fun;
                            transform.LookAt(interactingWith.transform);
                            transform.rotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y,0);
                            doingSomething = false;
                        }
                    }
                }
            } else {
                //for debug only
                // the update rate for every meeple will be global on the final version probably
                temp_count += Time.deltaTime;
                if(temp_count > 2){
                    temp_count = 0;
                    run_AI();
                }
            }
            meepleSim.position = transform.position;
        } else {
            // Let the city handle the simulation for this meepleSim object
            transform.position = meepleSim.position;
        }
    }
}
