using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    public Player player;
    public RectTransform hunger_back;
    public RectTransform hunger_fore;

    public RectTransform sleep_back;
    public RectTransform sleep_fore;
    
    public RectTransform hygiene_back;
    public RectTransform hygiene_fore;
    
    public RectTransform toilet_back;
    public RectTransform toilet_fore;
    bool active = false;
    public void toggle(){
        active = !active;
        gameObject.SetActive(active);
    }

    float map(float val, float oldmin, float oldmax, float newmin, float newmax){
        return (val - oldmin) * (newmax - newmin) / (oldmax - oldmin) + newmin;
    }
    
    void Update()
    {
        hunger_fore.offsetMax = new Vector3(hunger_fore.offsetMax.x, map(player.hunger, 1,0,hunger_back.offsetMin.y,hunger_back.offsetMax.y));
        sleep_fore.offsetMax = new Vector3(sleep_fore.offsetMax.x, map(player.sleep, 1,0,sleep_back.offsetMin.y,sleep_back.offsetMax.y));
        hygiene_fore.offsetMax = new Vector3(hygiene_fore.offsetMax.x, map(player.hygiene, 1,0,hygiene_back.offsetMin.y,hygiene_back.offsetMax.y));
        toilet_fore.offsetMax = new Vector3(toilet_fore.offsetMax.x, map(player.bathroom, 1,0,toilet_back.offsetMin.y,toilet_back.offsetMax.y));
    }
}
