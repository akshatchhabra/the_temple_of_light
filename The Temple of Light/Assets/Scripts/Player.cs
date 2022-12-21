using System.Collections;
using System.Collections.Generic;
using System;
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

    private bool in_placing_mode;         // Toggling the movement lock
    private int[] player_pos;
    private bool carrying;
    private Column carried_column;
    private bool just_entered;            // Stops placement from autoexiting
    // Defs for the placement mode
    private GameObject sel_indicator;
    private int[] sel_pos;


    // Start is called before the first frame update
    void Start()
    {
        animation_controller = GetComponent<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        velocity = 5.0f;
        rotation_speed = 50.0f;
        turn_smooth_time = 0.1f;
        in_placing_mode = false;
        player_pos = new int[2]{0,0};
    }

    // Update is called once per frame
    void Update()
    {
        // Only allow movement if not placing an object
        if(!in_placing_mode) {
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

        // Item placement mode toggle on
        if(Input.GetKeyDown("e") && !in_placing_mode)
        {
          in_placing_mode = true;
          Debug.Log("Entering Placing Mode");
          just_entered = true;
          sel_indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
          level.setPause(true);
          // Get the closest center square of the player
          player_pos[0] = roundToOdd(transform.position.x);
          player_pos[1] = roundToOdd(transform.position.z);
          sel_pos = new int[] {0,1}; //relative selection position
        }

        if(in_placing_mode)
        {
          bool placeable;
          // Put a cube down at the base of the selected area
          sel_indicator.transform.localScale = new Vector3(2f,0.25f, 2f);
          // Move the selection, keeping it adjacent to the player's position
          if(Input.GetKeyDown("w") || Input.GetKeyDown(KeyCode.UpArrow)) {
            sel_pos[1] = Math.Min(sel_pos[1] + 1, 1);
          }
          if(Input.GetKeyDown("a") || Input.GetKeyDown(KeyCode.LeftArrow)) {
            sel_pos[0] = Math.Max(sel_pos[0] - 1, -1);
          }
          if(Input.GetKeyDown("s") || Input.GetKeyDown(KeyCode.DownArrow)) {
            sel_pos[1] = Math.Max(sel_pos[1] - 1, -1);
          }
          if(Input.GetKeyDown("d") || Input.GetKeyDown(KeyCode.RightArrow)) {
            sel_pos[0] = Math.Min(sel_pos[0] + 1, 1);
          }
          // Check if the spot is available, get the column if not
          placeable = isEmpty((float)(player_pos[0] + 2*sel_pos[0]), (float)(player_pos[1] + 2*sel_pos[1]));
          Column col = getColumn(player_pos[0] + 2*sel_pos[0], player_pos[1] + 2*sel_pos[1]);

          if(placeable) {
            sel_indicator.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
          }
          if(carrying && !placeable) {
            sel_indicator.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
          }
          if(!carrying && col) {
            if(col.movable) {
              sel_indicator.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            } else {
              sel_indicator.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            }
          }
          if(sel_pos[0] == 0 && sel_pos[1] == 0) {
            sel_indicator.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
          }
          // Moving the cube
          sel_indicator.transform.position = new Vector3(player_pos[0] + 2*sel_pos[0], 0.125f, player_pos[1] + 2*sel_pos[1]);
          // Placing/picking up a column
          if(Input.GetKeyDown("e") && !just_entered) {
            if(sel_pos[0] == 0 && sel_pos[1] == 0) { // can't place it on yourself
              Destroy(sel_indicator);
              in_placing_mode = false;
              level.setPause(false);
              return;
            }
            if(placeable && carrying) { // put down carried column
              carried_column.putDown(player_pos[0] + 2*sel_pos[0], player_pos[1] + 2*sel_pos[1]);
              carrying = false;
              carried_column = null;
              Destroy(sel_indicator);
              in_placing_mode = false;
              level.setPause(false);
            }
            if(!carrying && col) { // pick up column
              if(col.pickUp()){ // returns false if column is immovable
                carried_column = col;
                carrying = true;
                Destroy(sel_indicator);
                in_placing_mode = false;
                level.setPause(false);
              }
            }
            if(carrying && col) { // Columns in both places - no
              Destroy(sel_indicator);
              in_placing_mode = false;
              level.setPause(false);
            }
            if(!carrying && placeable) { // empty and empty - exit
              Destroy(sel_indicator);
              in_placing_mode = false;
              level.setPause(false);
            }
          }
          if(Input.GetKeyDown("r") && col) {
            col.RotateCol();
          }
          just_entered = false;

          if(Input.GetKeyDown(KeyCode.Escape)) { // leave placing mode
            Destroy(sel_indicator);
            in_placing_mode = false;
            level.setPause(false);
          }
        }

    }

    // Quick helper function to get the closest odd (center of square) integer
    int roundToOdd(float x)
    {
      int out_x;
      if(x >= 0){
        out_x = 2*((int)x /2) + 1;
      } else {
        out_x = 2*((int)x/2) - 1;
      }
      return out_x;
    }

    bool isEmpty(float x, float z)
    {
      Collider[] colliders = Physics.OverlapSphere(new Vector3 (x, 2f, z), 0.9f);
      if(colliders.Length >= 1) {
        Debug.Log("Collisions: " + colliders.Length);
        foreach(Collider collider in colliders) {
          GameObject c_object = collider.gameObject;
          // Only return false for things that block placement - cols and walls
          if(c_object.tag != "Wall" && c_object.tag != "Column") {
            Debug.Log(c_object.name);
            continue;
          } else {
            return false;
          }
        }  // If there's nothing blocking it
        return true;
      } else {
        return true;
      }
    }

    Column getColumn(float x, float z) {
      Collider[] colliders = Physics.OverlapSphere(new Vector3 (x, 2f, z), 0.9f);
      if(colliders.Length > 1) {
        foreach(Collider collider in colliders) {
          GameObject c_object = collider.gameObject;
          if(c_object.tag == "Column") {
            return c_object.GetComponent<Column>();
          }
        }  // If there's nothing blocking it
        return null;
      } else {
        return null;
      }
    }

}
