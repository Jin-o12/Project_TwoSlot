using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlaneEnemyDmgTriiger : MonoBehaviour
{
public int hp = 0;
public int hpInit = 100;
public bool isDie = false;
private Animator animator;
public BoxCollider boxCol;
    void Awake()
    {
        boxCol = GameObject.Find("HitTrigger").GetComponent<BoxCollider>();
    }
    void Start()
    {
        hp =hpInit;
        animator = GetComponent<Animator>();
    }

    
    void Update()
    {
        
    }
}
