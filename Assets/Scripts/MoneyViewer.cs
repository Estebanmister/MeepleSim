using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyViewer : MonoBehaviour
{
    Player player;
    TMP_Text text;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        text = GetComponent<TMP_Text>();
    }

    void Update(){
        text.text = player.money.ToString("C");
    }
}
