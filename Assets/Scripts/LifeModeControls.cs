using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LifeModeControls : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;
    public Transform cameraPivot;
    float viewx, viewy = 0;
    public float sensitivity = 100;
    public float viewDistance = 10;
    public float maxZoom = 100;
    public float minZoom = 20;
    float map(float val, float oldmin, float oldmax, float newmin, float newmax){
        return (val - oldmin) * (newmax - newmin) / (oldmax - oldmin) + newmin;
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
    }
    void Update()
    {
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
