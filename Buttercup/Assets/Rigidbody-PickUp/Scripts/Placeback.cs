using UnityEngine;
using System.Collections;

public class Placeback : MonoBehaviour {

    [HideInInspector]
    public Vector3 pos;
    [HideInInspector]
    public Quaternion rot;

	// Use this for initialization
	void Start () 
    {
        pos = transform.position;
        rot = transform.rotation;
        
	}
}
