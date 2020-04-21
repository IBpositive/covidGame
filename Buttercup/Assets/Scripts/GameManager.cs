using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private GameObject MannPrefab;
    private GameObject Mann;
    public bool isDead;
    public bool mann0Dead;
    public playerHud hud;
    public float maxHP = 1.0f;
    public float hp;
    public float maxStamina = 1.0f;
    public float stamina;
    public int mann0Limit = 1;
    public int currentMann0 = 0;
    public int mann0State;
    public GameObject player;
    public GameObject respawnPoint;
    public Transform spawn0;
    // Start is called before the first frame update


    void MakeSingleton()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        } else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Awake()
    {
        MakeSingleton();
        spawnMann0();

    }
    void Start()
    {
        hp = maxHP;
        hud.SetMaxHealth(maxHP);
        stamina = maxStamina;
        hud.SetMaxStamina(maxStamina);
    }

    // Update is called once per frame
    void Update()
    {
        hud.SetHealth(hp);
        hud.SetMaxStamina(stamina);
        if (hp >= maxHP)
        {
            hp = maxHP;
        }
        if (hp <= 0)
        {
            isDead = true;
        }
        if (stamina <= maxStamina)
        {
            stamina += .005f;
        }
        if (stamina >= maxStamina)
        {
            stamina = maxStamina;
        }
        if (isDead)
        {

            player.transform.position = respawnPoint.transform.position;
            player.transform.rotation = respawnPoint.transform.rotation;
            isDead = false;
            hp = maxHP;

        }
        if (mann0Limit >= currentMann0)
        {
            spawnMann0();
        }
        if(mann0Dead)
        {
            StartCoroutine(ReleaseCoroutine());
        }
    }
    private IEnumerator ReleaseCoroutine()
    {
        yield return new WaitForSeconds(3f);
        mann0State = 0;
      //  mann0Dead = false;
    }
    public void spawnMann0()
    {
            Mann = Instantiate(MannPrefab) as GameObject;
            Mann.transform.position = spawn0.position;
            currentMann0 += 1;
    }
    public void mann0_0() {
        mann0State = 0;
        }
    public void mann0_1()
    {
        mann0State = 1;
    }
    public void mann0_2()
    {
        mann0State = 2;
    }
    public void mann0_3()
    {
        mann0State = 3;
    }
    public void mann0_spawner()
    {
        mann0Limit += 1;
    }
}
