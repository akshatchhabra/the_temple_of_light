using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Animator animation_controller;
    private CharacterController character_controller;
    private float turn_smooth_velocity;
    private bool is_close_to_column;

    public Vector3 movement_direction;
    public float velocity;
    public float rotation_speed;
    public float turn_smooth_time;

    // Start is called before the first frame update
    void Start()
    {
        animation_controller = GetComponent<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        velocity = 3.0f;
        rotation_speed = 50.0f;
        turn_smooth_time = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        bool is_moving_column = Input.GetKey("e");
        animation_controller.SetBool("is_moving_column", is_moving_column);
        float horizontal_axis = Input.GetAxis("Horizontal");
        float vertical_axis = Input.GetAxis("Vertical");
        movement_direction = new Vector3(horizontal_axis, 0.0f, vertical_axis);
        movement_direction.Normalize();
        bool is_walking = Input.GetKey("w") || Input.GetKey("up")
                            || Input.GetKey("a") || Input.GetKey("left")
                            || Input.GetKey("s") || Input.GetKey("down") 
                            || Input.GetKey("d") || Input.GetKey("right") ;
        animation_controller.SetBool("is_walking", is_walking);
        float target_angle = Mathf.Atan2(movement_direction.x, movement_direction.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, target_angle, ref turn_smooth_velocity, turn_smooth_time);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
        if (movement_direction.magnitude >= 0.1f){
            character_controller.Move(movement_direction * velocity * Time.deltaTime);
        }
    }
}
