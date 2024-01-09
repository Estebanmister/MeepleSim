using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class Job
{
    // This class is created by the City, depending on the amount and size of Workplace rooms
    // Only one of these is made by job type
    // multiple meeples can refer to the same job type
    // in order to know if this job type is full, refer to the workplace object
    public Workplace workplace;
    public Furniture[] associated_furniture;
    public float starts_at = 9.00f;
    public float ends_at = 16.00f;

    public float required_morale = 20.0f;
}
