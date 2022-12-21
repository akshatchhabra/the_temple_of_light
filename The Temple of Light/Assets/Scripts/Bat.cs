using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bat : MonoBehaviour
{

    private Animator animation_controller;
    // private CharacterController character_controller;
    private NavMeshAgent bat;

    public Vector3 bat_original_position;
    public Transform player_position;
    public float detection_radius;
    public float reset_radius;

    // Start is called before the first frame update
    void Start()
    {
        animation_controller = GetComponent<Animator>();
        // character_controller = GetComponent<CharacterController>();
        bat = GetComponent<NavMeshAgent>();
        bat_original_position = transform.position;
        detection_radius = 10.0f;
        reset_radius = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(player_position.position, transform.position) < detection_radius){
            bat.SetDestination(player_position.position);
        }
        else{
            bat.SetDestination(bat_original_position);
            //reset rotation?
        }
    }

    // Bat only activates if player enters square/rectangular area that's near the bat
    // if player exits that area, bat goes back to its original position.
    void OnTriggerEnter(Collider collider){
        if (collider.name == "Player"){
            animation_controller.SetBool("is_attacking", true);
        }
        // if (collider.name == "light(Clone)"){
        //     Debug.Log("Here");
        //     animation_controller.SetBool("is_dead", true);
        //     // yield return new WaitForSeconds(GetComponent<Animation>()["bat_death"].length);
        //     Destroy(gameObject);
        //     //find time since light hit to determine how long 
        // }
    }
    void OnTriggerExit(Collider collider){
        if (collider.name == "Player"){
            animation_controller.SetBool("is_attacking", false);
        }
    }
}
