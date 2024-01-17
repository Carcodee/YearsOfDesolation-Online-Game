using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Crosshairs", order = 1)]
public class CrosshairScriptableObj : ScriptableObject
{

    public Color color;
    public float width;
    public float gap;
    public float length;
    public bool isStatic;

}
