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

public enum LightColor { // where any light color is the sum of its parts
  NONE = 0,
  RED = 1,        // basic colors are powers of 2, others are sums of them
  GREEN = 2,      // and white is the sum of all three
  YELLOW = 3,     // This can be moved to whichever classes handle light if it's easier
  BLUE = 4,
  MAGENTA = 5,
  CYAN = 6,
  WHITE = 7,
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

    internal bool is_lit;             // whether or not the column is receiving a light beam
    internal int lit_angle;           // logical angle the light is coming from (0-7), -1 if not lit
    internal int out_angle;           // if mirrored, the angle incoming light gets reflected
    internal int through_angle;       // if clear & not lens, angle of nonreflected light
    internal bool blocking;           // Is it blocking the light? (wrong incoming angle, etc)
    internal LightColor lit_color;         // color of incoming light. "NONE" if not lit.
    internal LightColor out_color;         // color of outgoing reflected light, "NONE" if not lit.
    internal LightColor through_color;     // color of light passing through, "NONE" if not lit or wrong color in
        // Remaining light handling can be done in the actual light class (or all of it, if you want)

    private bool being_carried;
    private LightBehavior childLight;

    // Start is called before the first frame update
    void Start()
    {
      is_lit = false;
      lit_angle = -1;
      out_angle = -1;
      blocking = false;
      lit_color = LightColor.NONE;
      out_color = LightColor.NONE;
      if((int)type < 7) {
        movable = true;
      }
      being_carried = false;
      facing = (int)((transform.eulerAngles.y - 90) / 45f);
      if(type == ColType.LIGHT_EMIT)
        facing = (int)((transform.eulerAngles.y + 90) / 45f) % 8;
      if(type == ColType.LIGHT_RECV)
        facing = (int)((transform.eulerAngles.y + 180) / 45f) % 8;
      facing_angle = new Angle(facing);
      if (type == ColType.LIGHT_EMIT)
      {
        childLight = CreateLight().GetComponent<LightBehavior>();
      }

    }

    // Update is called once per frame
    void Update()
    {
      if(being_carried) {
        transform.position = player_reference.transform.position + new Vector3(0f,2f,0f);
      }

    }

    private GameObject CreateLight()
    {
        GameObject childLight = Instantiate(lightObject);
        LightBehavior childBehavior = childLight.GetComponent<LightBehavior>();
        childBehavior.direction = facing_angle;
        childBehavior.source = gameObject;
        childLight.SetActive(true);
        return childLight;
    }


    // I'll be impressed if this works first try but hey, I can't test it
    public bool pickUp() {
      if(!movable)
        return false;

      transform.localScale = new Vector3(.125f, .125f, .125f);
      // Values subject to change, working blind here
      transform.position = player_reference.transform.position + new Vector3(0f,2f,0f);
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
      return true;
    }


}
