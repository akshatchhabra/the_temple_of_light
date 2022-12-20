using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum ColType {
  MIRROR = 1,
  CONCAVE = 2,
  CONVEX = 3,
  ONEWAY = 4,
  COLOR = 5,
  PRISM = 6,
  LIGHT_EMIT = 7,
  LIGHT_RECV = 8

}


public class Column : MonoBehaviour
{
    // Public variables: Determine column type, angle, color
    public ColType type;              // column type, an enum from 1-6 for player cols, 7-8 for game design cols
    public int facing;
    public Angle facing_angle;          // facing angle, 0-7 from 0=-x,0z to 7=-x,-z clockwise in 45 deg turns
    public int[] position;            // Can be set when placing down a column to calculate in world coords
    public bool movable;
    public Color color;              // color of the filter or required color of the receiver
                                        // "white" if the column is not colored
    public GameObject player_reference;
    public GameObject lightObject;
    public level Level;

    public bool is_lit;             // whether or not the column is receiving a light beam
    internal Color lit_color;         // color of incoming light. "NONE" if not lit.
    internal LightBehavior childLight;
    internal LightBehavior parentLight;

    private float carry_height;
    private bool being_carried;
    private Vector3 offset;


    // Start is called before the first frame update
    void Start()
    {
      Level = GameObject.Find("Level").GetComponent<level>();
      if(!Level)
        Debug.LogError("Level not found.");
      is_lit = false;
      lit_color = Color.black;
      if((int)type < 7) {
        movable = true;
      }
      being_carried = false;
      facing = (((int)transform.eulerAngles.y + 1) / 45);
      if(type == ColType.LIGHT_EMIT)
        facing = (((int)transform.eulerAngles.y + 91) / -45) % 8;
      if(type == ColType.LIGHT_RECV)
        facing = (((int)transform.eulerAngles.y + 181) / -45) % 8;
      facing_angle = new Angle(facing);
      if (type == ColType.LIGHT_EMIT)
      {
        childLight = CreateLight().GetComponent<LightBehavior>();
        Level.source_cols.Add(this);
        Level.source_lights.Add(childLight);
      }
      player_reference = GameObject.Find("Player");
      if(!player_reference) {
        Debug.LogError("Player not found");
      }
      carry_height = 4;
      offset = new Vector3(0f,carry_height,0f);
    }


    // Update is called once per frame
    void Update()
    {
      if(being_carried) {
        transform.position = player_reference.transform.position + offset;
      }
      // //DEBUG
      // if(Input.GetKeyDown("r")) {
      //   RotateCol();
      //}
      if(!parentLight) {
        is_lit = false;
      }

      if (type == ColType.LIGHT_EMIT && !childLight)
      {
        childLight = CreateLight().GetComponent<LightBehavior>();
        Level.source_cols.Add(this);
        Level.source_lights.Add(childLight);
      }



    }

    internal GameObject CreateLight()
    {
        GameObject childLight = Instantiate(lightObject);
        LightBehavior childBehavior = childLight.GetComponent<LightBehavior>();
        childBehavior.direction = facing_angle;
        childBehavior.source = gameObject;
        childLight.SetActive(true);
        return childLight;
    }


    public bool pickUp() {
      if(!movable)
        return false;

      transform.localScale = new Vector3(.125f, .125f, .125f);
      // Values subject to change, working blind here
      transform.position = player_reference.transform.position + offset;
      being_carried = true;
      return true;
    }

    public bool putDown(int xcoord, int ycoord) {
      if(!movable) {
        Debug.LogError("Immovable columns should not be able to be put down.");
        return false;
      }
      transform.localScale = new Vector3(.25f, .25f, .25f);
      transform.position = new Vector3(xcoord, 0, ycoord);
      being_carried = false;
      return true;
    }

    public bool RotateCol(int dir = 1) {
      if(!movable) {
        return false;
      }
      facing = (facing + dir) %8;
      facing_angle = new Angle(facing);
      transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y + (45*dir), 0f);
      Level.refreshLights(this);

      return true;
    }


}
