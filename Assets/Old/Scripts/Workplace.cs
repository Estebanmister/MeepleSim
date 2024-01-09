using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workplace : Place
{
    // If this reaches 0, no meeple can assign themselves the Job type associated with this workplace
    public int jobs_available = 0;
}
