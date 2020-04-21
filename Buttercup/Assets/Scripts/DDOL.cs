using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DDOL : MonoBehaviour
{
    // Start is called before the first frame update
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
