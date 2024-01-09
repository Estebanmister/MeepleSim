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
    public float hunger,sleep,social,fun,hygiene,bathroom = 1.0f;
    public AnimationCurve[] needsCurves = new AnimationCurve[6];
    public float morale = 1.0f;
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

    // thoughts for needs
    // basically, thoughts that are made when the meeple is hungry, sleepy, etc
    public Thought[] baseThoughtCollection = new Thought[6];

    // thoughts felt by the meeple
    // todo: if slow, change to array
    public List<ActiveThought> thoughts = new List<ActiveThought>();

    NavMeshAgent agent;
    City city;

    // i know i said i wouldnt use these....... but dang its so useful
    // refactor this to array later in development
    List<Action> actionQueue = new List<Action>();
    public Action currentlyPerforming = null;

    List<Skill> skills = new List<Skill>();

    float base_speed = 3.5f;
    float base_acceleration = 8;
    float update_rate = 2;

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
        base_speed = agent.speed;
        base_acceleration = agent.acceleration;
        meepleSim.fun = UnityEngine.Random.Range(0f,0.5f);
        meepleSim.bathroom = UnityEngine.Random.Range(0f,0.5f);
        meepleSim.hunger = UnityEngine.Random.Range(0f,0.5f);
        meepleSim.social = UnityEngine.Random.Range(0f,0.5f);
        meepleSim.sleep = UnityEngine.Random.Range(0f,0.5f);
        meepleSim.hygiene = UnityEngine.Random.Range(0f,0.5f);
    }

    void calculateMorale(){
        meepleSim.morale = 1.0f;
        List<ActiveThought> todelete = new List<ActiveThought>();
        foreach(ActiveThought thought in thoughts){
            meepleSim.morale += thought.morale;
            thought.length -= (Time.deltaTime * city.timeWarp)+update_rate;
            if(thought.length <= 0){
                // we dont delete them now because it will mess up the foreach loop
                // this is a quirk with how these foreach loop work, maybe using an index loop might be better
                todelete.Add(thought);
            }
        }
        foreach(ActiveThought thoughtToDelete in todelete){
            thoughts.Remove(thoughtToDelete);
        }
        if(meepleSim.morale > 1.0f){
            meepleSim.morale = 1.0f;
        }
        if(meepleSim.morale < 0){
            meepleSim.morale = 0;
        }
    }

    void run_AI(){
        float[] needs = {meepleSim.hunger,meepleSim.sleep,meepleSim.social,meepleSim.fun,meepleSim.hygiene,meepleSim.bathroom};
        string[] needNames = {"hunger", "sleep","social","fun","hygiene","bathroom"};
        int i = 0;
        float[] needVals = new float[6];
        foreach(float need in needs){
            needVals[i] = meepleSim.needsCurves[i].Evaluate(need);
            if(needVals[i] > 0.5f){
                if(thoughts.FindIndex(tho => tho.ID == baseThoughtCollection[i].activeThought.ID) == -1){
                    thoughts.Add(baseThoughtCollection[i].activeThought.copy());
                }
            }
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
                    if(test == null){
                        continue;
                    }
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
                    float v = 0;
                    switch(needNames[needIndex]){
                        // this sucks ass
                        case "hunger":
                            v = ((test.hunger*100)-test.interactionLength)+(Mathf.Pow(needVals[needIndex]*100f,3)*(1f/10000f));
                            if(v > highest_f){
                                highest_f = v;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "social":
                            v = ((test.social*100)-test.interactionLength)+(Mathf.Pow(needVals[needIndex]*100f,3)*(1f/10000f));
                            if(v > highest_f){
                                highest_f = v;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "sleep":
                            v = ((test.sleep*100)-test.interactionLength)+(Mathf.Pow(needVals[needIndex]*100f,3)*(1f/10000f));
                            if(v > highest_f){
                                highest_f =v;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "fun":
                            v = ((test.fun*100)-test.interactionLength)+(Mathf.Pow(needVals[needIndex]*100f,3)*(1f/10000f));
                            if(v > highest_f){
                                highest_f = v;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "bathroom":
                            v = ((test.bathroom*100)-test.interactionLength)+(Mathf.Pow(needVals[needIndex]*100f,3)*(1f/10000f));
                            if(v > highest_f){
                                highest_f = v;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                        case "hygiene":
                            v = ((test.hygiene*100)-test.interactionLength)+(Mathf.Pow(needVals[needIndex]*100f,3)*(1f/100f));
                            if(v > highest_f){
                                highest_f = v;
                                interaction = test;
                                toInteract = furniture;
                            }
                            break;
                    }
                }
                if(toInteract != null){
                    if(actionQueue.FindIndex(action => action.interaction == interaction) == -1){
                        if(currentlyPerforming != null){
                            if(currentlyPerforming.interaction != interaction){
                                actionQueue.Add(new Action{interaction=interaction,timeStarted=city.globalTime,priority=needVals[needIndex],position=toInteract.interactionPoint.position});
                            }
                        } else {
                            actionQueue.Add(new Action{interaction=interaction,timeStarted=city.globalTime,priority=needVals[needIndex],position=toInteract.interactionPoint.position});
                        }
                        
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
        float time_between_frames = Time.deltaTime*city.timeWarp;
        action.timeSpentPerforming += time_between_frames;

        animator.Play(action.interaction.animationToPlay);

        meepleSim.hunger -= action.interaction.hunger * (time_between_frames/action.interaction.interactionLength);
        meepleSim.social -= action.interaction.social * (time_between_frames/action.interaction.interactionLength);
        meepleSim.sleep -= action.interaction.sleep * (time_between_frames/action.interaction.interactionLength);
        meepleSim.hygiene -= action.interaction.hygiene * (time_between_frames/action.interaction.interactionLength);
        meepleSim.bathroom -= action.interaction.bathroom * (time_between_frames/action.interaction.interactionLength);
        meepleSim.fun -= action.interaction.fun * (time_between_frames/action.interaction.interactionLength);
        
        meepleSim.fun = meepleSim.fun < 0 ? 0 : meepleSim.fun;
        meepleSim.bathroom = meepleSim.bathroom < 0 ? 0 : meepleSim.bathroom;
        meepleSim.hunger = meepleSim.hunger < 0 ? 0 : meepleSim.hunger;
        meepleSim.social = meepleSim.social < 0 ? 0 : meepleSim.social;
        meepleSim.sleep = meepleSim.sleep < 0 ? 0 : meepleSim.sleep;
        meepleSim.hygiene = meepleSim.hygiene < 0 ? 0 : meepleSim.hygiene;

        // sometimes some needs might increase while performing the action, keep them under 1
        meepleSim.fun = meepleSim.fun > 1 ? 1 : meepleSim.fun;
        meepleSim.bathroom = meepleSim.bathroom > 1 ? 1 : meepleSim.bathroom;
        meepleSim.hunger = meepleSim.hunger > 1 ? 1 : meepleSim.hunger;
        meepleSim.social = meepleSim.social > 1 ? 1 : meepleSim.social;
        meepleSim.sleep = meepleSim.sleep > 1 ? 1 : meepleSim.sleep;
        meepleSim.hygiene = meepleSim.hygiene > 1 ? 1 : meepleSim.hygiene;

        transform.LookAt(action.position);
        transform.rotation = Quaternion.Euler(0,transform.rotation.eulerAngles.y,0);
        if(action.interaction.skillIncrease){
            int changeindex = 0;
            foreach(string skill in action.interaction.skillsToChange){
                int index = skills.FindIndex(sk => sk.id == skill);
                if(index != -1){
                    skills[index].level += action.interaction.skillLevelstoChange[changeindex] * (time_between_frames/action.interaction.interactionLength);
                } else {
                    skills.Add(new Skill{id = skill, level = 1});
                }
                changeindex += 1;
            }
        }
        if(action.timeSpentPerforming >= action.interaction.interactionLength || 
            meepleSim.fun == 0 || meepleSim.hunger == 0 || meepleSim.social == 0 || meepleSim.sleep == 0 || meepleSim.hygiene == 0 || meepleSim.bathroom == 0){
            animator.Play("idle");
            foreach(Thought thought in action.interaction.induceThoughts){
                thoughts.Add(thought.activeThought.copy());
            }
            return true;
        } else {
            return false;
        }
    }
    void Update(){
        if(observed){
            // needs increase over time
            meepleSim.hunger += (meepleSim.hunger<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
            meepleSim.social += (meepleSim.social<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
            meepleSim.sleep += (meepleSim.sleep<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
            meepleSim.hygiene += (meepleSim.hygiene<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
            meepleSim.bathroom += (meepleSim.bathroom<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
            meepleSim.fun += (meepleSim.fun<1) ? (Time.deltaTime/150)*city.timeWarp : 0;

            agent.speed = base_speed * city.timeWarp;
            agent.acceleration = base_acceleration * city.timeWarp;

            actionQueue = actionQueue.OrderBy(x => x.priority).ToList();
            if(actionQueue.Count > 0){
                if(currentlyPerforming != null){
                    if(!currentlyPerforming.active){
                        currentlyPerforming = actionQueue[UnityEngine.Random.Range(0, actionQueue.Count > 3 ? 2 : actionQueue.Count)];
                        actionQueue.Remove(currentlyPerforming);
                        currentlyPerforming.active = true;
                        agent.destination = currentlyPerforming.position;
                    }
                } else {
                    currentlyPerforming = actionQueue[UnityEngine.Random.Range(0, actionQueue.Count > 3 ? 2 : actionQueue.Count)];
                    actionQueue.Remove(currentlyPerforming);
                    currentlyPerforming.active = true;
                    agent.destination = currentlyPerforming.position;
                }
                
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

            temp_count += Time.deltaTime * city.timeWarp;
            if(temp_count > update_rate){
                temp_count = 0;
                run_AI();
                calculateMorale();
            }

            erasure_counter += Time.deltaTime * city.timeWarp;
            if(erasure_counter > update_rate*2){
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
