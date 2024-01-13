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
    public TMP_Text textBox;
    public RectTransform skillBar;
    public RectTransform skillBarBackground;
    public TMP_Text skillReadout;

    public void ShowSkillBar(float value){
        skillBar.transform.parent.gameObject.SetActive(true);
        skillReadout.text = value.ToString();
        skillBar.offsetMax = new Vector3(skillBar.offsetMax.x, MathUtil.Map(value%1, 0,1,skillBarBackground.offsetMin.y,skillBarBackground.offsetMax.y));
    }

    void Start(){
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }
    float count = 0;
    void Update(){
        if(skillBar.transform.parent.gameObject.activeSelf){
            count += Time.deltaTime;
            if(count > 2){
                skillBar.transform.parent.gameObject.SetActive(false);
                count = 0;
            }
        }
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
        Image image = newbutton.GetComponent<Image>();
        image.color = Color.green;
        but.onClick.AddListener(() => player.CancelInteraction());
        but.onClick.AddListener(() => CloseMenu());
        but.GetComponentInChildren<TMP_Text>().text = "Cancel Action";

        i += 1;

        angle = i * Mathf.PI * 2 / (2);
        pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        newbutton = Instantiate(buttonPrefab, transform);
        newbutton.transform.localPosition = pos;
        but = newbutton.GetComponent<Button>();
        image = newbutton.GetComponent<Image>();
        image.color = Color.red;
        but.onClick.AddListener(() => CloseMenu());
        but.GetComponentInChildren<TMP_Text>().text = "Close Menu";
    }
    public void ShowDocument(Item item){
        CloseMenu();
        textBox.text = item.viewAbleText;
        textBox.transform.parent.gameObject.SetActive(true);
        menuOpen = true;
    }
    public void OpenItemMenu(Vector2 screenPos, Item item){
        transform.position = screenPos;
        int i = 0;
        float radius = 100;
        float angle;
        Interaction[] interactions = item.interactions;
        Vector3 pos;
        float length = (interactions.Length+1);
        if(item.isViewable){
            length += 1;
            angle = i * Mathf.PI * 2 / length;
            pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            GameObject newbutton = Instantiate(buttonPrefab, transform);
            newbutton.transform.localPosition = pos;
            Button but = newbutton.GetComponent<Button>();

            Image image = newbutton.GetComponent<Image>();
            image.color = Color.blue;

            but.onClick.AddListener(() => ShowDocument(item));
            but.GetComponentInChildren<TMP_Text>().text = "Look at";
            i += 1;
        }
        foreach(Interaction interaction in interactions){
            angle = i * Mathf.PI * 2 / length;
            pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            GameObject newbutton = Instantiate(buttonPrefab, transform);
            newbutton.transform.localPosition = pos;
            Button but = newbutton.GetComponent<Button>();
            Image image = newbutton.GetComponent<Image>();
            if(interaction is StoreInteraction){
                image.color = Color.yellow;
            } else {
                image.color = Color.green;
            }
            
            but.onClick.AddListener(() => SelectInteraction(interaction));
            but.GetComponentInChildren<TMP_Text>().text = interaction.IntName;
            i += 1;
        }
        angle = i * Mathf.PI * 2 / length;
        pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        GameObject newbutton2 = Instantiate(buttonPrefab, transform);
        newbutton2.transform.localPosition = pos;
        Button but2 = newbutton2.GetComponent<Button>();
        Image image2 = newbutton2.GetComponent<Image>();
        image2.color = Color.red;
        but2.onClick.AddListener(() => CloseMenu());
        but2.GetComponentInChildren<TMP_Text>().text = "Close Menu";
        i += 1;
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
            Image image = newbutton.GetComponent<Image>();
            if(interaction is StoreInteraction){
                image.color = Color.yellow;
            } else {
                image.color = Color.green;
            }
            but.onClick.AddListener(() => SelectInteraction(interaction));
            but.GetComponentInChildren<TMP_Text>().text = interaction.IntName;
            i += 1;
        }

        angle = i * Mathf.PI * 2 / (interactions.Length+1);
        pos = new Vector3 (Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
        GameObject newbutton2 = Instantiate(buttonPrefab, transform);
        newbutton2.transform.localPosition = pos;
        Button but2 = newbutton2.GetComponent<Button>();
        Image image2 = newbutton2.GetComponent<Image>();
        image2.color = Color.red;
        but2.onClick.AddListener(() => CloseMenu());
        but2.GetComponentInChildren<TMP_Text>().text = "Close Menu";
        i += 1;
    }
    public void SelectInteraction(Interaction interaction){
        player.performAction(interaction);
        menuOpen = false;
        foreach(Transform child in transform.GetComponentInChildren<Transform>()){
            Destroy(child.gameObject);
        }
    }
}
