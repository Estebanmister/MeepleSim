using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    LifeModeControls player;
    City city;
    public GameObject buttonPrefab;
    void Start(){
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<LifeModeControls>();
        city = GameObject.FindGameObjectWithTag("city").GetComponent<City>();
    }
    public void CloseMenu(){
        foreach(Transform child in transform.GetComponentInChildren<Transform>()){
            Destroy(child.gameObject);
        }
    }
    public void OpenMenu(Vector2 screenPos, Interaction[] interactions){
        transform.position = screenPos;
        int i = 0;
        float radius = 2;
        foreach(Interaction interaction in interactions){
            var angle = i * Mathf.PI * 2 / interactions.Length;
            var pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            GameObject newbutton = Instantiate(buttonPrefab, transform);
            newbutton.transform.localPosition = pos;
            Button but = newbutton.GetComponent<Button>();
            but.onClick.AddListener(() => SelectInteraction(interaction));
            but.GetComponentInChildren<TMP_Text>().text = interaction.description;
            i += 1;
        }
    }
    public void SelectInteraction(Interaction interaction){
        player.performAction(new Action{interaction=interaction,timeStarted=city.globalTime,priority=1});
        player.menuOpen = false;
        foreach(Transform child in transform.GetComponentInChildren<Transform>()){
            Destroy(child.gameObject);
        }
    }
}
