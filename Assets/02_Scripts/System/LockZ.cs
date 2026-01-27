using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockZ : MonoBehaviour
{
    public float fixedZ;
    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void FixedUpdate()
    {
        if (!rb) return;

        var p = rb.position; p.z = fixedZ; rb.position = p;
        var v = rb.velocity; v.z = 0f; rb.velocity = v;
    }
}
