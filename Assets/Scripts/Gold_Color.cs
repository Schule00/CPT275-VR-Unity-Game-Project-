using UnityEngine;

public class ColorGold : MonoBehaviour
{
    void Start()
    {
        // Gets the renderer component and changes the material's color to Red
        GetComponent<Renderer>().material.color = Color.gold;
    }
}
