using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockZ : MonoBehaviour
{
    public float fixedZ;

    void LateUpdate()
    {
        var p = transform.position;
        p.z = fixedZ;
        transform.position = p;
    }
}
