using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Animator animation_controller;
    private CharacterController character_controller;

    public Vector3 movement_direction;
    public float walking_velocity;
    public float velocity;
    public Vector3 lrot;
    public Vector3 rrot;
    public float rotationSpeed;
    
    // Start is called before the first frame update
    void Start()
    {
        animation_controller = GetComponent<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        walking_velocity = 1.0f;
        velocity = 1.0f;
        lrot = new Vector3(0.0f, -1.0f, 0.0f);
        rrot = new Vector3(0.0f, 1.0f, 0.0f);
        rotationSpeed = 50.0f;
        
    }

    // Update is called once per frame
    void Update()
    {
        bool is_walking_forward = Input.GetKey("w") || Input.GetKey("up");
        animation_controller.SetBool("is_walking_forward", is_walking_forward);
        bool is_walking_back = Input.GetKey("s") || Input.GetKey("down");
        animation_controller.SetBool("is_walking_back", is_walking_back);
        movement_direction = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

        if (is_walking_forward){
            character_controller.Move(movement_direction * velocity * Time.deltaTime);
        }
        if (is_walking_back){
            character_controller.Move(-1 * movement_direction * velocity * Time.deltaTime);
        }
        if (Input.GetKey("left") || Input.GetKey("a"))
        {
            transform.Rotate(lrot * rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey("right") || Input.GetKey("d"))
        {
            transform.Rotate(rrot * rotationSpeed * Time.deltaTime);
        }
    }
}
