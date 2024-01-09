using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    Player player;
    public GameObject buttonPrefab;
    public bool menuOpen = false;
    void Start(){
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    public void CloseMenu(){
        menuOpen = false;
        foreach(Transform child in transform.GetComponentInChildren<Transform>()){
            Destroy(child.gameObject);
        }
    }
    public void OpenPersonalMenu(Vector2 screenPos){
        // create a circular menu
        transform.position = screenPos;
        int i = 0;
        float radius = 100;
        float angle;
        Vector3 pos;

        angle = i * Mathf.PI * 2 / (2);
        pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        GameObject newbutton = Instantiate(buttonPrefab, transform);
        newbutton.transform.localPosition = pos;
        Button but = newbutton.GetComponent<Button>();
        but.onClick.AddListener(() => player.CancelInteraction());
        but.GetComponentInChildren<TMP_Text>().text = "Cancel Action";

        i += 1;

        angle = i * Mathf.PI * 2 / (2);
        pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        newbutton = Instantiate(buttonPrefab, transform);
        newbutton.transform.localPosition = pos;
        but = newbutton.GetComponent<Button>();
        but.onClick.AddListener(() => CloseMenu());
        but.GetComponentInChildren<TMP_Text>().text = "Exit";
    }
    public void OpenMenu(Vector2 screenPos, Interaction[] interactions){
        transform.position = screenPos;
        int i = 0;
        float radius = 100;
        float angle;
        Vector3 pos;
        foreach(Interaction interaction in interactions){
            angle = i * Mathf.PI * 2 / (interactions.Length+1);
            pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            GameObject newbutton = Instantiate(buttonPrefab, transform);
            newbutton.transform.localPosition = pos;
            Button but = newbutton.GetComponent<Button>();
            but.onClick.AddListener(() => SelectInteraction(interaction));
            but.GetComponentInChildren<TMP_Text>().text = interaction.IntName;
            i += 1;
        }

        angle = i * Mathf.PI * 2 / (interactions.Length+1);
        pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        GameObject newbutton2 = Instantiate(buttonPrefab, transform);
        newbutton2.transform.localPosition = pos;
        Button but2 = newbutton2.GetComponent<Button>();
        but2.onClick.AddListener(() => CloseMenu());
        but2.GetComponentInChildren<TMP_Text>().text = "Exit";
        i += 1;
    }
    public void SelectInteraction(Interaction interaction){
        player.performAction(new Action{interaction=interaction,timeStarted=Time.time,priority=1,active=true});
        menuOpen = false;
        foreach(Transform child in transform.GetComponentInChildren<Transform>()){
            Destroy(child.gameObject);
        }
    }
}
