using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CeillngEnemy : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePos;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            Instantiate(bulletPrefab, firePos.position, Quaternion.identity);
        }
    }


}
