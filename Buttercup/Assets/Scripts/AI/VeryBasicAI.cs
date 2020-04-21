using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VeryBasicAI : MonoBehaviour
{
    [SerializeField] GameManager gm;
    public Animator veryBasicAniCon;
    public Transform Spawnpoint;
    public int StateMachine;
    public bool isDead;

    //basic varibles that control speed and turn rate
    public float speed = 3.0f;
    public float obstacleRange = 5.0f;
    // Start is called before the first frame update
    void Start()
    {

        veryBasicAniCon = GetComponent<Animator>();  
    }

    // Update is called once per frame
    void Update()
    {
      
        veryBasicAniCon.SetInteger("AniState", StateMachine);
        veryBasicAniCon.SetBool("isDead", isDead);
        if (StateMachine == 1 && !isDead)
        {
            walk();
            wallCheck();
        }
        if(isDead)
        {
            StartCoroutine(killTimer());
        }
      
    }

    private IEnumerator killTimer()
    {
        yield return new WaitForSeconds(2f);
        isDead = false;
        this.gameObject.transform.position = Spawnpoint.transform.position;
        StateMachine = 0;
        
    }

        public void killAI()
    {
        isDead = true;
  
    }
    public void startAI()
    {
        StateMachine = 1;
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            gm.hp -= .1f;
            isDead = true;
        }
    }

    void walk()
    {
        transform.Translate(0, 0, speed * Time.deltaTime);
    }
    void wallCheck()
    {
        //Create the raycast to detect the player and walls
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.SphereCast(ray, 0.75f, out hit))
        {
            GameObject hitObject = hit.transform.gameObject;
            //if the raycast is closer to a wall then the obstacleRange it will change the enemys direction
            if (hit.distance < obstacleRange)
            {
                //angle is set to a random number between -110 and 110
                float angle = Random.Range(-110, 110);
                transform.Rotate(0, angle, 0);
            }
        }
    }
}
