using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelCollision : MonoBehaviour
{

    WheelCollider wheel;

    // Start is called before the first frame update
    void Start()
    {
        wheel = GetComponent<WheelCollider>();
        Debug.Log("Hello");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionExit(Collision other) 
    {
        Debug.Log($"Wheel {wheel.name} has left {other.collider.tag}");
    }

    private void OnCollisionEnter(Collision other) 
    {
        Debug.Log($"Wheel {wheel.name} has entered {other.collider.tag}");
    }
}
