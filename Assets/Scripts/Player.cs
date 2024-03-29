using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public float money;
    public float speed = 2;
    public float hunger,sleep,social,fun,hygiene,bathroom = 1.0f;
    public AnimationCurve[] needsCurves = new AnimationCurve[6];
    public float morale = 1.0f;
    public Relation[] relations;
    PlayerInput playerInput;
    Dictionary<string, float> skills = new Dictionary<string, float>();
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
    public List<Item> inventory = new List<Item>();
    public void performAction(Interaction interaction){
        if(interaction is StoreInteraction){
            StoreInteraction sh = (StoreInteraction)interaction;
            if(money > sh.price && sh.stock > 0){
                money -= sh.price;
                sh.stock -= 1;
                inventory.Add(sh.item);
            }
        } else {
            if(interaction.skillCheck){
                if(skills.All(i => interaction.skillsToCheck.Contains(i.Key))){
                    int i = 0;
                    foreach(string id in interaction.skillsToCheck){
                        if(skills[id] < interaction.requiredLevels[i]){
                            Debug.Log("Cannot be performed");
                            return;
                        }
                        i += 1;
                    }
                } else {
                    Debug.Log("Cannot be performed");
                    return;
                }
            }
            performing = new Action{interaction=interaction,timeStarted=Time.time,priority=1,active=true,point=lastInteractionSpot};
            performing.active = true;
        }
        
    }

    public void CancelInteraction(){
        performing.active = false;
        animator.Play("idle");
        foreach(Thought thought in performing.interaction.induceThoughts){
            thoughts.Add(thought.activeThought.copy());
        }
    }

                /* ----------------  Thanks to daveMennenoh on the Unity forums for this  ---------------- */
    int UILayer;
    //Returns 'true' if we touched or hovering on Unity UI element.
    public bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
 
 
    //Returns 'true' if we touched or hovering on Unity UI element.
    private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == UILayer)
                return true;
        }
        return false;
    }
 
    //Gets all event system raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

                /* ------------------------------------------------------------------------------------------ */

    public void click(InputAction.CallbackContext context){
        if(context.started && Application.isFocused){
            if(IsPointerOverUIElement()){
                return;
            }
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
                        if(hit.collider.tag == "item"){
                            ui.OpenItemMenu(playerInput.actions["mousePos"].ReadValue<Vector2>(), hit.collider.GetComponent<ItemHolder>().item);
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
        UILayer = LayerMask.NameToLayer("UI");
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponentInChildren<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }
    bool DoInteraction(Action action){
        float time_between_frames = (Time.deltaTime*WorldProperties.timeWarp);
        action.timeSpentPerforming += time_between_frames;
        if (action.interaction.needsDecay){
            DecayNeeds();
        }
        animator.Play(action.interaction.animationToPlay);
        float timeframe = action.interaction.interactionLength;
        if(timeframe == -1){
            // every need will increase per minute
            timeframe = 60;
        }
        hunger -= action.interaction.hunger * (time_between_frames/timeframe);
        sleep -= action.interaction.sleep * (time_between_frames/timeframe);
        hygiene -= action.interaction.hygiene * (time_between_frames/timeframe);
        bathroom -= action.interaction.bathroom * (time_between_frames/timeframe);
        
        bathroom = bathroom < 0 ? 0 : bathroom;
        hunger = hunger < 0 ? 0 : hunger;
        sleep = sleep < 0 ? 0 : sleep;
        hygiene = hygiene < 0 ? 0 : hygiene;

        // sometimes some needs might increase while performing the action, keep them under 1
        bathroom = bathroom > 1 ? 1 : bathroom;
        hunger = hunger > 1 ? 1 : hunger;
        sleep = sleep > 1 ? 1 : sleep;
        hygiene = hygiene > 1 ? 1 : hygiene;

        if(action.interaction.hunger <= 0 && hunger == 1){
            CancelInteraction();
            return true;
        }
        if(action.interaction.sleep <= 0 && sleep == 1){
            CancelInteraction();
            return true;
        }
        if(action.interaction.bathroom <= 0 && bathroom == 1){
            CancelInteraction();
            return true;
        }

        if(action.interaction.skillIncrease){
            int changeindex = 0;
            
            foreach(string skill in action.interaction.skillsToChange){
                if(!skills.ContainsKey(skill)){
                    skills.Add(skill, 0);
                }
                skills[skill] += action.interaction.skillLevelstoChange[changeindex] * Time.deltaTime * WorldProperties.timeWarp;
                changeindex += 1;
            }
            ui.ShowSkillBar(skills[action.interaction.skillsToChange[0]]);
        }
        if(action.interaction is WorkInteraction){
            money += (((WorkInteraction)action.interaction).dollarsPerMinute/60) * Time.deltaTime * WorldProperties.timeWarp;
        }
        if(hunger == 0 || sleep == 0 || hygiene == 0 || bathroom == 0){
            CancelInteraction();
            return true;
        }
        if(action.interaction.interactionLength == -1){
            // this interaction can last till needs are met
            return false;
         }else if (action.timeSpentPerforming >= action.interaction.interactionLength){
            CancelInteraction();
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
            thought.length -= ((Time.deltaTime*WorldProperties.timeWarp) );
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
    void DecayNeeds(){
        
        //social += (social<1) ? ((Time.deltaTime*WorldProperties.timeWarp)/150) : 0;
        
        if(playerInput.actions["sprint"].IsPressed()){
            hunger += (hunger<1) ? ((Time.deltaTime*WorldProperties.timeWarp)/200) : 0;
            sleep += (sleep<1) ? ((Time.deltaTime*WorldProperties.timeWarp)/400) : 0;
            hygiene += (hygiene<1) ? ((Time.deltaTime*WorldProperties.timeWarp)/200) : 0;
        } else {
            hunger += (hunger<1) ? ((Time.deltaTime*WorldProperties.timeWarp)/500) : 0;
            sleep += (sleep<1) ? ((Time.deltaTime*WorldProperties.timeWarp)/1000) : 0;
            hygiene += (hygiene<1) ? ((Time.deltaTime*WorldProperties.timeWarp)/500) : 0;
        }
        
        bathroom += (bathroom<1) ? ((Time.deltaTime*WorldProperties.timeWarp)/200) : 0;
        //fun += (fun<1) ? ((Time.deltaTime*WorldProperties.timeWarp)/150) : 0;
    }
    void Update()
    {
        animator.speed = WorldProperties.timeWarp;
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
       
        // todo speed changes with warp
        //agent.speed = base_speed ;
        //agent.acceleration = base_acceleration ;
        if(performing != null){
            if(performing.active){
                if(Vector3.Distance(performing.point.position, transform.position) > 0.05f && performing.interaction is not ItemInteraction){
                    transform.position = Vector3.Lerp(transform.position,performing.point.position, 0.1f);
                    animator.transform.parent.forward = performing.point.forward;
                } else {
                    DoInteraction(performing);
                }
                // cannot move while performing an action
                return;
            } else {
                DecayNeeds();
            }
        } else {
            DecayNeeds();
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
        float modifiedSpeed = speed;
        if(playerInput.actions["sprint"].IsPressed()){
            modifiedSpeed *= 2;
        }
        directionToMove = Quaternion.Euler(0, viewx, 0) * directionToMove;
        characterController.Move(directionToMove * modifiedSpeed *(Time.fixedDeltaTime*WorldProperties.timeWarp));
        if(directionToMove.magnitude != 0){
            animator.transform.parent.forward = directionToMove;
            if(playerInput.actions["sprint"].IsPressed()){
                animator.SetBool("running", true);
                animator.SetBool("walking", true);
            } else {
                animator.SetBool("walking", true);
                animator.SetBool("running", false);
            }
            
        } else {
            animator.SetBool("walking", false);
            animator.SetBool("running", false);
        }
    }
}
