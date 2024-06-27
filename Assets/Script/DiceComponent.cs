using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceComponent : MonoBehaviour
{
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public bool IsItPickeable()
    {
        return true;
    }

    public bool IsItGridPlacable()
    {
        return false;
    }
}
