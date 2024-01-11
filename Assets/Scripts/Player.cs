using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
[Serializable]
public class Action {
    public Interaction interaction;
    public float timeStarted;
    public float timeSpentPerforming = 0;
    public float priority;
    public Transform point;
    public bool active = false;
}
[Serializable]
public class Skill {
    public string id;
    public float level;
}

[Serializable]
public class Relation {

}
public class Player : MonoBehaviour
{
    public float hunger,sleep,social,fun,hygiene,bathroom = 1.0f;
    public AnimationCurve[] needsCurves = new AnimationCurve[6];
    public float morale = 1.0f;
    public Relation[] relations;
    PlayerInput playerInput;
    List<Skill> skills = new List<Skill>();
    public List<ActiveThought> thoughts = new List<ActiveThought>();
    public Thought[] baseThoughtCollection = new Thought[6];
    public UIControl ui;
    CharacterController characterController;
    Animator animator;
    public Transform cameraPivot;
    float viewx, viewy = 0;
    public float sensitivity = 100;
    public float viewDistance = 10;
    public float maxZoom = 100;
    public float minZoom = 20;
    Action performing = null;
    Transform lastInteractionSpot;
    public void performAction(Interaction interaction){
        performing = new Action{interaction=interaction,timeStarted=Time.time,priority=1,active=true,point=lastInteractionSpot};
        performing.active = true;
    }

    public void CancelInteraction(){
        performing.active = false;
        animator.Play("idle");
        foreach(Thought thought in performing.interaction.induceThoughts){
            thoughts.Add(thought.activeThought.copy());
        }
    }

    public void click(InputAction.CallbackContext context){
        if(context.started && Application.isFocused){
            Ray ray = Camera.main.ScreenPointToRay(playerInput.actions["mousePos"].ReadValue<Vector2>());
            Debug.DrawLine(ray.origin,ray.direction*100, Color.magenta, 100);
            RaycastHit[] hits = Physics.RaycastAll(ray, 10);
            foreach(RaycastHit hit in hits){
                if(!ui.menuOpen){
                    if(performing != null){
                        if(performing.active){
                            ui.OpenPersonalMenu(playerInput.actions["mousePos"].ReadValue<Vector2>());
                            ui.menuOpen = true;
                            return;
                        }
                    }
                    if(Vector3.Distance(transform.position, hit.point) < 4){
                        if(hit.collider.transform.tag == "furniture"){
                            animator.transform.parent.LookAt(hit.point);
                            animator.transform.parent.rotation = Quaternion.Euler(0,animator.transform.parent.eulerAngles.y,0);
                            Furniture fun = hit.collider.GetComponent<Furniture>();
                            lastInteractionSpot = fun.interactionPoint;
                            ui.OpenMenu(playerInput.actions["mousePos"].ReadValue<Vector2>(), fun.interactions);
                            ui.menuOpen = true;
                            return;
                        }
                        if(hit.collider.transform.tag == "Player"){
                            ui.OpenPersonalMenu(playerInput.actions["mousePos"].ReadValue<Vector2>());
                            ui.menuOpen = true;
                            return;
                        }
                    }
                }
            }
        }
    }
    public void zoom(InputAction.CallbackContext context ){
        if(context.started && Application.isFocused){
            Transform cam = cameraPivot.GetChild(0);
            Vector3 tomove = -(cam.localPosition.normalized * (context.ReadValue<Vector2>().y/500));
            Debug.Log(cam.localPosition.magnitude);
            if(cam.localPosition.magnitude < 2){
                if(-context.ReadValue<Vector2>().y > 0){
                    cam.localPosition += tomove;
                }
            } else if (cam.localPosition.magnitude < 10){
                cam.localPosition += tomove;
            }else if(cam.localPosition.magnitude > 10){
                if(-context.ReadValue<Vector2>().y < 0){
                    cam.localPosition += tomove;
                }
            }
            
            /*
            Camera.main.fieldOfView -= context.ReadValue<Vector2>().y/50;
            
            if(Camera.main.fieldOfView > maxZoom){
                Camera.main.fieldOfView = maxZoom;
            }
            if(Camera.main.fieldOfView < minZoom){
                Camera.main.fieldOfView = minZoom;
            }
            sensitivity = map(Camera.main.fieldOfView, minZoom, maxZoom, 100, 250);
            */
        }
    }
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponentInChildren<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }
    bool DoInteraction(Action action){
        float time_between_frames = Time.deltaTime;
        action.timeSpentPerforming += time_between_frames;

        animator.Play(action.interaction.animationToPlay);

        hunger -= action.interaction.hunger * (time_between_frames/action.interaction.interactionLength);
        social -= action.interaction.social * (time_between_frames/action.interaction.interactionLength);
        sleep -= action.interaction.sleep * (time_between_frames/action.interaction.interactionLength);
        hygiene -= action.interaction.hygiene * (time_between_frames/action.interaction.interactionLength);
        bathroom -= action.interaction.bathroom * (time_between_frames/action.interaction.interactionLength);
        fun -= action.interaction.fun * (time_between_frames/action.interaction.interactionLength);
        
        fun = fun < 0 ? 0 : fun;
        bathroom = bathroom < 0 ? 0 : bathroom;
        hunger = hunger < 0 ? 0 : hunger;
        social = social < 0 ? 0 : social;
        sleep = sleep < 0 ? 0 : sleep;
        hygiene = hygiene < 0 ? 0 : hygiene;

        // sometimes some needs might increase while performing the action, keep them under 1
        fun = fun > 1 ? 1 : fun;
        bathroom = bathroom > 1 ? 1 : bathroom;
        hunger = hunger > 1 ? 1 : hunger;
        social = social > 1 ? 1 : social;
        sleep = sleep > 1 ? 1 : sleep;
        hygiene = hygiene > 1 ? 1 : hygiene;

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
            fun == 0 || hunger == 0 || social == 0 || sleep == 0 || hygiene == 0 || bathroom == 0){
            animator.Play("idle");
            foreach(Thought thought in action.interaction.induceThoughts){
                thoughts.Add(thought.activeThought.copy());
            }
            return true;
        } else {
            return false;
        }
    }
    void calculateMorale(){
        morale = 1.0f;
        List<ActiveThought> todelete = new List<ActiveThought>();
        foreach(ActiveThought thought in thoughts){
            morale += thought.morale;
            thought.length -= (Time.deltaTime );
            if(thought.length <= 0){
                // we dont delete them now because it will mess up the foreach loop
                // this is a quirk with how these foreach loop work, maybe using an index loop might be better
                todelete.Add(thought);
            }
        }
        foreach(ActiveThought thoughtToDelete in todelete){
            thoughts.Remove(thoughtToDelete);
        }
        if(morale > 1.0f){
            morale = 1.0f;
        }
        if(morale < 0){
            morale = 0;
        }
    }
    void Update()
    {
        // pan camera
        Vector2 mousepos = playerInput.actions["mouse"].ReadValue<Vector2>();
        if(playerInput.actions["middleClick"].IsPressed()){
            Vector2 view = Camera.main.ScreenToViewportPoint(playerInput.actions["mouse"].ReadValue<Vector2>())*viewDistance;
            cameraPivot.localPosition += Quaternion.Euler(0, viewx, 0) * new Vector3(view.x,0,view.y);
        } else {
            cameraPivot.localPosition = new Vector3(0,1.3f,0);
        }
        // rotate camera
        if(playerInput.actions["rightClick"].IsPressed()){
            Vector2 view = Camera.main.ScreenToViewportPoint(playerInput.actions["mouse"].ReadValue<Vector2>())*sensitivity;
            viewx += view.x;
            viewy += view.y;
            if(viewy >= 12){
                viewy = 12;
            }
            if(viewy <= -86){
                viewy = -86;
            }
            if(viewx >= 360){
                viewx -= 360;
            }
            if(viewx <= -360){
                viewx += 360;
            }
            cameraPivot.localRotation = Quaternion.Slerp(cameraPivot.localRotation, Quaternion.Euler(-viewy, viewx, 0), 0.8f);
        }
        // decay needs
        hunger += (hunger<1) ? (Time.deltaTime/150) : 0;
        social += (social<1) ? (Time.deltaTime/150) : 0;
        sleep += (sleep<1) ? (Time.deltaTime/150) : 0;
        hygiene += (hygiene<1) ? (Time.deltaTime/150) : 0;
        bathroom += (bathroom<1) ? (Time.deltaTime/150) : 0;
        fun += (fun<1) ? (Time.deltaTime/150) : 0;
        // todo speed changes with warp
        //agent.speed = base_speed ;
        //agent.acceleration = base_acceleration ;
        if(performing != null){
            if(performing.active){
                if(Vector3.Distance(performing.point.position, transform.position) > 0.2f){
                    transform.position = Vector3.Lerp(transform.position,performing.point.position, 0.1f);
                    animator.transform.parent.forward = performing.point.forward;
                } else {
                    performing.active = !DoInteraction(performing);
                }
                // cannot move while performing an action
                return;
            }
        }
        float[] needs = {hunger,sleep,social,fun,hygiene,bathroom};
        string[] needNames = {"hunger", "sleep","social","fun","hygiene","bathroom"};
        int i = 0;
        float[] needVals = new float[6];
        foreach(float need in needs){
            needVals[i] = needsCurves[i].Evaluate(need);
            if(needVals[i] > 0.5f){
                if(thoughts.FindIndex(tho => tho.ID == baseThoughtCollection[i].activeThought.ID) == -1){
                    thoughts.Add(baseThoughtCollection[i].activeThought.copy());
                }
            }
            i += 1;
        }
        calculateMorale();
        
        
    }
    void FixedUpdate(){
        if(performing != null){
            if(performing.active){
                return;
            }
        }
        Vector3 directionToMove = new Vector3(playerInput.actions["right"].ReadValue<float>(),0,playerInput.actions["forward"].ReadValue<float>());
        animator.SetBool("right", false);
        animator.SetBool("left", false);
        if(directionToMove.x > 0){
            animator.SetBool("right", true);
        }
        if(directionToMove.x < 0){
            animator.SetBool("left", true);
        }
        directionToMove = Quaternion.Euler(0, viewx, 0) * directionToMove;
        characterController.Move(directionToMove*Time.fixedDeltaTime);
        if(directionToMove.magnitude != 0){
            animator.transform.parent.forward = directionToMove;
            animator.SetBool("walking", true);
        } else {
            animator.SetBool("walking", false);
        }
    }
}
