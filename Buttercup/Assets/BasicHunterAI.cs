using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicHunterAI : MonoBehaviour
{
    private GameObject spawnPoint;
    private GameObject player;
    private Rigidbody rb;
    private void Awake()
    {
        spawnPoint = GameObject.Find("spawnpoint0");
        player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
