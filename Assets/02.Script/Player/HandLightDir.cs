using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandLightDir : MonoBehaviour
{
    public Transform playerTf;
    

    void Start()
    {
        playerTf = GetComponentInParent<Transform>();
    }

    void Update()
    {
        
    }
}
