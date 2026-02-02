using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeillingEnemyBullet : MonoBehaviour
{
    void Update()
    {
        gameObject.transform.Translate(Vector3.down * 50.0f * Time.deltaTime, Space.World);
        Destroy(this, 50.0f);
    }
}
