using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    void Start()
    {
        GetComponent<Renderer>().material.color = Color.red;
    }
}

