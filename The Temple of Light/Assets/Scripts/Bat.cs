using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bat : MonoBehaviour
{

    private Animator animation_controller;
    // private CharacterController character_controller;
    private NavMeshAgent bat;
    private bool is_taking_damage;
    private float prev_hit_time;

    public Vector3 bat_original_position;
    public Transform player_position;
    public float detection_radius;
    public float reset_radius;
    public bool is_dead;
    public float time_of_death;
    public Player player_dummy;

    // Start is called before the first frame update
    void Start()
    {
        animation_controller = GetComponent<Animator>();
        bat = GetComponent<NavMeshAgent>();
        bat_original_position = transform.position;
        detection_radius = 10.0f;
        reset_radius = 0.5f;
        is_dead = false;
        time_of_death = Time.time;
        is_taking_damage = false;
        prev_hit_time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (!level.globalPause){
            if (Vector3.Distance(player_position.position, transform.position) < detection_radius){
                bat.SetDestination(player_position.position);
            }
            else{
                bat.SetDestination(bat_original_position);
            }
            if (is_dead && Time.time - time_of_death > 3.0f){
                Destroy(gameObject);
            }
            if (is_taking_damage && Time.time - prev_hit_time > 2.0f){
                player_dummy.TakeDamage(1);
                prev_hit_time = Time.time;
            }
            if(is_dead){
                player_dummy.is_hit = false;
            }
        }
    }

    // Bat only activates if player enters square/rectangular area that's near the bat
    // if player exits that area, bat goes back to its original position.
    void OnTriggerEnter(Collider collider){
        if (collider.name == "Player"){
            if (!level.globalPause){
                animation_controller.SetBool("is_attacking", true);
                Player player = collider.GetComponent<Player>();
                player.is_hit = true;
                is_taking_damage = true;
            }
        }
    }
    void OnTriggerExit(Collider collider){
        if (collider.name == "Player"){
            animation_controller.SetBool("is_attacking", false);
            Player player = collider.GetComponent<Player>();
            player.is_hit = false;
            is_taking_damage = false;
        }
    }
}
