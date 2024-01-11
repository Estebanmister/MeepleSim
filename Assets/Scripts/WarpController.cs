using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarpController : MonoBehaviour
{
    public Toggle x2;
    public Toggle x3;
    public Toggle x4;
    public Toggle x5;
    public Toggle xMax;
    public void Toggle2x(bool val){
        if(val){
            WorldProperties.timeWarp = 4;
        } else {
            x3.isOn = false;
            x4.isOn = false;
            x5.isOn = false;
            WorldProperties.timeWarp = 1;
        }
    }
    public void Toggle3x(bool val){
        if(val){
            x2.isOn = true;
            WorldProperties.timeWarp = 6;
        } else {
            x4.isOn = false;
            x5.isOn = false;
            WorldProperties.timeWarp = 4;
        }
    }
    public void Toggle4x(bool val){
        if(val){
            x2.isOn = true;
            x3.isOn = true;
            WorldProperties.timeWarp = 8;
        } else {
            x5.isOn = false;
            WorldProperties.timeWarp = 6;
        }
    }
    public void Toggle5x(bool val){
        if(val){
            x2.isOn = true;
            x3.isOn = true;
            x4.isOn = true;
            WorldProperties.timeWarp = 10;
        } else {
            xMax.isOn = false;
            WorldProperties.timeWarp = 8;
        }
    }
    public void ToggleMax(bool val){
        if(val){
            x2.isOn = true;
            x3.isOn = true;
            x4.isOn = true;
            x5.isOn = true;
            WorldProperties.timeWarp = 50;
        } else {
            WorldProperties.timeWarp = 10;
        }
    }
}
