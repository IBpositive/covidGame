using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicPatrolAI : MonoBehaviour
{
    [SerializeField] GameManager gm;
    public GameObject AI;
    public NavMeshAgent navMeshAgent;
    public Transform[] waypoints;
    public int StateMachine;
    public Rigidbody rb;
    // public Transform Player;
    public Transform Spawnpoint;
    int m_CurrentWaypointIndex;
    public Animator veryBasicAniCon;
    public bool isDead;
    float lockPos = 0;
    // Start is called before the first frame update
    void Start()
    { 

        veryBasicAniCon = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isDead)
        {       
            StartCoroutine(killTimer());          
        }
        veryBasicAniCon.SetInteger("AniState", StateMachine);
        veryBasicAniCon.SetBool("isDead", isDead);
        if (StateMachine == 0 && !isDead)
        {
            transform.rotation = Quaternion.Euler(lockPos, lockPos, lockPos);

        }
        if (StateMachine == 1 && !isDead)
        {
            rb.isKinematic = false;
            navMeshAgent.isStopped = false;
            navMeshAgent.acceleration = 6;
            navMeshAgent.speed = 5;
            WaypointPatrol();
        }
    }
    private IEnumerator killTimer()
    {
      
        yield return new WaitForSeconds(2f);
      
        StartCoroutine(RespawnTimer());

    }
    private IEnumerator RespawnTimer()
    {
        this.gameObject.transform.position = Spawnpoint.transform.position;
        yield return new WaitForSeconds(2f);
        isDead = false;
        this.gameObject.transform.position = Spawnpoint.transform.position;
        StateMachine = 0;

    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            rb.isKinematic = true;
            navMeshAgent.isStopped = true;
            gm.hp -= .1f;
            isDead = true;
        }
    }

    public void startPatrol()
    {
        StateMachine = 1;
        print(StateMachine);
        navMeshAgent.SetDestination(waypoints[0].position);
    }
    private void WaypointPatrol()
    {
        if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
        {
            navMeshAgent.ResetPath();
            m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
            navMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].position);
        }
    }
}
