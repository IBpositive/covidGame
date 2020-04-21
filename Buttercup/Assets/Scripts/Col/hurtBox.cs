using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hurtBox : MonoBehaviour
{
    [SerializeField] public GameManager gm;
    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Player")
        {
            gm.hp -= .01f;
        }
    }
}
