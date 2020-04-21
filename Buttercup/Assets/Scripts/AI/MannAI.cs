using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MannAI : MonoBehaviour
{
    private GameObject spawnPoint;
    private GameObject player;
    public bool isDead = false;
    private int StateMachine;
    private Rigidbody rb;
    private Animator mannAniCon;
    private NavMeshAgent mannMeshAgent;
    public GameObject[] waypoints;
    int m_CurrentWaypointIndex;
    private const float rotSpeed = 20f;
    // Start is called before the first frame update
    private void Awake()
    {
        spawnPoint = GameObject.Find("spawnpoint0");
        player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody>();
        mannAniCon = GetComponent<Animator>();
        mannMeshAgent = GetComponent<NavMeshAgent>();
        waypoints = new GameObject[4];
        waypoints[0] = GameObject.Find("0waypointA");
        waypoints[1] = GameObject.Find("0waypointB");
        waypoints[2] = GameObject.Find("0waypointC");
        waypoints[3] = GameObject.Find("0waypointD");
        
    }
    void Start()
    {

    }
  
    // Update is called once per frame
    void Update()
    {
        StateMachine = GameManager.instance.mann0State;
        mannAniCon.SetInteger("AniState", StateMachine);
        if (StateMachine == 0)
        {
            mannMeshAgent.isStopped = true;
        }
        if (StateMachine == 1)
        {
            mannMeshAgent.speed = 2;
            mannMeshAgent.isStopped = false;
            WaypointPatrol();
        }
        if(StateMachine == 2)
        {
            mannMeshAgent.speed = 8;
            mannMeshAgent.isStopped = false;
            WaypointPatrol();
        }
        if (StateMachine == 3)
        {
            mannMeshAgent.isStopped = true;
            StartCoroutine(ReleaseCoroutine());
        }
        if (GameManager.instance.mann0Dead)
        {
            print("j");
            GameManager.instance.currentMann0 -= 1;
            GameManager.instance.mann0Dead = false;
            Destroy(this.gameObject);
        }
        InstantlyTurn(mannMeshAgent.destination);
    }
  
    private void InstantlyTurn(Vector3 destination)
{
    //When on target -> dont rotate!
    if ((destination - transform.position).magnitude < 0.1f) return;

    Vector3 direction = (destination - transform.position).normalized;
    Quaternion qDir = Quaternion.LookRotation(direction);
    transform.rotation = Quaternion.Slerp(transform.rotation, qDir, Time.deltaTime * rotSpeed);
}

    private IEnumerator ReleaseCoroutine()
    {
        yield return new WaitForSeconds(3f);
        GameManager.instance.mann0Dead = true;
    }

    public void startPatrol()
    {
            StateMachine = 1;
            print(StateMachine);
            mannMeshAgent.SetDestination(waypoints[0].transform.position);
    }
    private void WaypointPatrol()
    {

        if (mannMeshAgent.remainingDistance < mannMeshAgent.stoppingDistance)
        {
            mannMeshAgent.ResetPath();
            m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Length;
            mannMeshAgent.SetDestination(waypoints[m_CurrentWaypointIndex].transform.position);
        }
    }

   
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            GameManager.instance.mann0State = 3;
            GameManager.instance.mann0Dead = true;
        }
    }
}

