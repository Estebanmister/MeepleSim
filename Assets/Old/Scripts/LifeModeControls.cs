using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.InputSystem;

public class LifeModeControls : MonoBehaviour
{
    PlayerInput playerInput;
    public MeepleSim meepleSim;
    List<Skill> skills = new List<Skill>();
    public List<ActiveThought> thoughts = new List<ActiveThought>();
    public Thought[] baseThoughtCollection = new Thought[6];
    public UIControl ui;
    CharacterController characterController;
    Animator animator;
    City city;
    public Transform cameraPivot;
    float viewx, viewy = 0;
    public float sensitivity = 100;
    public float viewDistance = 10;
    public float maxZoom = 100;
    public float minZoom = 20;
    Action performing = null;
        [SerializeField] private LayerMask _targetLayerMask;

    float map(float val, float oldmin, float oldmax, float newmin, float newmax){
        return (val - oldmin) * (newmax - newmin) / (oldmax - oldmin) + newmin;
    }
    public void performAction(Action action){
        performing = action;
        performing.active = true;
    }
    public bool menuOpen = false;
    public void click(InputAction.CallbackContext context){
        if(context.started){
            Ray ray = Camera.main.ScreenPointToRay(playerInput.actions["mousePos"].ReadValue<Vector2>());
            RaycastHit hit;
            Debug.DrawLine(ray.origin,ray.direction*100, Color.magenta, 100);
            if(Physics.Raycast(ray, out hit, _targetLayerMask)){
                if(LayerMask.LayerToName(hit.collider.gameObject.layer) == "UI"){
                    return;
                }
                if(!menuOpen){
                    if(Vector3.Distance(transform.position, hit.point) < 4){
                        if(hit.collider.transform.tag == "furniture"){
                            ui.OpenMenu(playerInput.actions["mousePos"].ReadValue<Vector2>(), hit.collider.GetComponent<Furniture>().interactions);
                            menuOpen = true;
                            return;
                        }
                }}
            }
            if(menuOpen){
                    ui.CloseMenu();
                    menuOpen = false;
                    return;
                }
            
        }
    }
    public void zoom(InputAction.CallbackContext context){
        if(context.started){
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
        city = GameObject.FindGameObjectWithTag("city").GetComponent<City>();
    }
    bool DoInteraction(Action action){
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
    void calculateMorale(){
        meepleSim.morale = 1.0f;
        List<ActiveThought> todelete = new List<ActiveThought>();
        foreach(ActiveThought thought in thoughts){
            meepleSim.morale += thought.morale;
            thought.length -= (Time.deltaTime * city.timeWarp);
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
    void Update()
    {
        meepleSim.hunger += (meepleSim.hunger<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
        meepleSim.social += (meepleSim.social<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
        meepleSim.sleep += (meepleSim.sleep<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
        meepleSim.hygiene += (meepleSim.hygiene<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
        meepleSim.bathroom += (meepleSim.bathroom<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
        meepleSim.fun += (meepleSim.fun<1) ? (Time.deltaTime/150)*city.timeWarp : 0;
        // todo speed changes with warp
        //agent.speed = base_speed * city.timeWarp;
        //agent.acceleration = base_acceleration * city.timeWarp;
        if(performing != null){
            if(performing.active){
                performing.active = !DoInteraction(performing);
                // cannot move while performing an action
                return;
            }
        }
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
        calculateMorale();
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
        characterController.Move(directionToMove*Time.deltaTime);
        if(directionToMove.magnitude != 0){
            characterController.transform.forward = directionToMove;
            animator.SetBool("walking", true);
        } else {
            animator.SetBool("walking", false);
        }
        transform.position = characterController.transform.position;
        Vector2 mousepos = playerInput.actions["mouse"].ReadValue<Vector2>();
        if(playerInput.actions["middleClick"].IsPressed()){
            Vector2 view = Camera.main.ScreenToViewportPoint(playerInput.actions["mouse"].ReadValue<Vector2>())*viewDistance;
            cameraPivot.localPosition += Quaternion.Euler(0, viewx, 0) * new Vector3(view.x,0,view.y);
        } else {
            cameraPivot.localPosition = new Vector3(0,1.3f,0);
        }
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
    }
}