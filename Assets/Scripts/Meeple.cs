using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Skill {
    public string id;
    public float level;
}
[Serializable]
public class Action {
    public Interaction interaction;
    public float timeStarted;
    public float timeSpentPerforming = 0;
    public float priority;
    public Vector3 position;
    public bool active = false;
}
public class Relation {
    public MeepleSim target;
    public float friendlyness = 50;
    public float hidden_attraction = 0;
    public float shown_attraction = 0;
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
    City city;

    // i know i said i wouldnt use these....... but dang its so useful
    // refactor this to array later in development
    List<Action> actionQueue = new List<Action>();
    public Action currentlyPerforming = null;

    List<Skill> skills = new List<Skill>();

    // Attached to a gameobject
    // if the meeple has been observed recently, an internal AI function will run every update, to determine what to do
    // otherwise, the City monobehaviour will take care of moving the meeple around

    // If the meeple enters a workplace that the player works in, this will count as being observed
    // If the meeple enters a household where the player built in, or is currently in, this will also count as being observed

    void Start(){
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        currentlyPerforming = null;
        // there is only one city game object
        city = GameObject.FindGameObjectWithTag("city").GetComponent<City>();
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
                    // begin skill check
                    if(test.skillCheck){
                        // index of what skill we are checking in the loop
                        int checkindex = 0;
                        bool passed = true;
                        foreach(string skill in test.skillsToCheck){
                            // find the skill in the meeple
                            int index = skills.FindIndex(sk => sk.id == skill);
                            if(index != -1){
                                // if found check the level
                                if(skills[index].level < test.requiredLevels[checkindex]){
                                    passed = false;
                                    // level check failed
                                    break;
                                }
                            } else {
                                // skill was never learnt
                                passed = false;
                                break;
                            }
                            checkindex += 1;
                        }
                        if(!passed){
                            // search for another interaction to perform
                            break;
                        }
                    }
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
                    if(actionQueue.FindIndex(action => action.interaction == interaction) == -1 && currentlyPerforming.interaction != interaction){
                        actionQueue.Add(new Action{interaction=interaction,timeStarted=city.globalTime,priority=needVals[needIndex],position=toInteract.interactionPoint.position});
                    }
                }
            }
        }
        
    }
    
    float temp_count = 0;
    
    // this counts the time to check whenver actions have to be deleted or not
    float erasure_counter = 0;

    // utility function
    // maps one value to another
    float map(float val, float oldmin, float oldmax, float newmin, float newmax){
        return (val - oldmin) * (newmax - newmin) / (oldmax - oldmin) + newmin;
    }
    bool PerformBasicInteraction(Action action){
        float time_elapsed = Time.deltaTime;
        action.timeSpentPerforming += time_elapsed;

        animator.Play(action.interaction.animationToPlay);

        meepleSim.hunger -= action.interaction.hunger * (action.interaction.interactionLength * time_elapsed);
        meepleSim.social -= action.interaction.social * (action.interaction.interactionLength * time_elapsed);
        meepleSim.sleep -= action.interaction.sleep * (action.interaction.interactionLength * time_elapsed);
        meepleSim.hygiene -= action.interaction.hygiene * (action.interaction.interactionLength * time_elapsed);
        meepleSim.bathroom -= action.interaction.bathroom * (action.interaction.interactionLength * time_elapsed);
        meepleSim.fun -= action.interaction.fun * (action.interaction.interactionLength * time_elapsed);
        
        meepleSim.fun = meepleSim.fun < 0 ? 0 : meepleSim.fun;
        meepleSim.bathroom = meepleSim.bathroom < 0 ? 0 : meepleSim.bathroom;
        meepleSim.hunger = meepleSim.hunger < 0 ? 0 : meepleSim.hunger;
        meepleSim.social = meepleSim.social < 0 ? 0 : meepleSim.social;
        meepleSim.sleep = meepleSim.sleep < 0 ? 0 : meepleSim.sleep;
        meepleSim.hygiene = meepleSim.hygiene < 0 ? 0 : meepleSim.hygiene;

        transform.LookAt(action.position);
        transform.rotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y,0);
        if(action.interaction.skillIncrease){
            int changeindex = 0;
            foreach(string skill in action.interaction.skillsToChange){
                int index = skills.FindIndex(sk => sk.id == skill);
                if(index != -1){
                    skills[index].level += action.interaction.skillLevelstoChange[changeindex] * (action.interaction.interactionLength * time_elapsed);
                } else {
                    skills.Add(new Skill{id = skill, level = 1});
                }
                changeindex += 1;
            }
        }
        if(action.timeSpentPerforming >= action.interaction.interactionLength){
            animator.Play("idle");
            return true;
        } else {
            return false;
        }
    }
    void Update(){
        if(observed){
            // we are being observed, run the ai
            meepleSim.hunger += (meepleSim.hunger<100) ? Time.deltaTime : 0;
            meepleSim.social += (meepleSim.social<100) ? Time.deltaTime : 0;
            meepleSim.sleep += (meepleSim.sleep<100) ? Time.deltaTime : 0;
            meepleSim.hygiene += (meepleSim.hygiene<100) ? Time.deltaTime : 0;
            meepleSim.bathroom += (meepleSim.bathroom<100) ? Time.deltaTime : 0;
            meepleSim.fun += (meepleSim.fun<100) ? Time.deltaTime : 0;

            actionQueue = actionQueue.OrderBy(x => x.priority).ToList();
            if(actionQueue.Count > 0 && !currentlyPerforming.active){
                currentlyPerforming = actionQueue[UnityEngine.Random.Range(0, actionQueue.Count > 3 ? 2 : actionQueue.Count)];
                actionQueue.Remove(currentlyPerforming);
                currentlyPerforming.active = true;
                agent.destination = currentlyPerforming.position;
            }
            // ladder of ifs...
            // to check if we reached out destination
            if(currentlyPerforming != null){
                if(currentlyPerforming.active){
                    if(!agent.pathPending){
                        if(agent.remainingDistance <= agent.stoppingDistance){
                            if(!agent.hasPath || agent.velocity.sqrMagnitude == 0f){
                                if(currentlyPerforming.interaction.ID == "basic"){
                                    bool complete = PerformBasicInteraction(currentlyPerforming);
                                    currentlyPerforming.active = !complete;
                                }
                            }
                        }
                    }
                }
            }
            //for debug only
            // the update rate for every meeple will be global on the final version probably
            temp_count += Time.deltaTime;
            if(temp_count > 2){
                temp_count = 0;
                run_AI();
            }

            erasure_counter += Time.deltaTime;
            if(erasure_counter > 4){
                // if any action is over an hour old
                // potential bug: this will reset all actions once the week ends
                // this shouldnt be a problem though
                int index = actionQueue.FindIndex(action => Mathf.Abs(city.globalTime - action.timeStarted) > 3600);
                if(index != -1){
                    // remove it
                    actionQueue.RemoveAt(index);
                }
                erasure_counter = 0;
            }
            meepleSim.position = transform.position;
        } else {
            // Let the city handle the simulation for this meepleSim object
            transform.position = meepleSim.position;
        }
    }
}
