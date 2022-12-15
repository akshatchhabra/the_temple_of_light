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
    public LightColor color;              // color of the filter or required color of the receiver
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
      facing_angle = new Angle(facing);
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

    /* Getters and Setters **/

    //  The internal fields should be readable from other scripts, no get needed.
    // However, none of the internal fields should be set manually.

    //  Use set_light(true, angle, color) to add a light beam to a column
    // and the remaining fields should autopopulate themselves.
    //  Likewise, use set_light(false) to remove light from a column.

    // NOTE: Albert, if you want to move some or all of this functionality
    //  over to your Light class, you're welcome to.
    //  I just figured that at least for mirrors and filters,
    //  it'd be easier for the column to just tell you where to go next
    //  and what color the beam should be.

    // Add light coming in. Only accepts one beam at the moment.
    // I'm assuming mirrors here are mirrored on both sides.
    // public void set_light(bool status, int angle = -1, LightColor in_color = LightColor.NONE)
    // {
    //   is_lit = status;
    //   lit_angle = angle;
    //   lit_color = in_color;
    //   // Calculate and update for mirrors
    //   if(is_lit && (type == ColType.MIRROR || type == ColType.ONEWAY))
    //   {
    //     //calculate out angle
    //     // TODO: Handling of semitransparent/oneway mirrors
    //     int in_relative = (lit_angle - facing_angle + 8) % 8;
    //     if(in_relative % 4 == 2) {
    //       // Hitting the side of the mirror: no reflection
    //       blocking = true;
    //       out_angle = -1;
    //       out_color = LightColor.NONE;
    //     } else if (in_relative == 0 || in_relative == 4)
    //     { // If it's reflecting right back down the line
    //       blocking = false;
    //       out_angle = lit_angle;
    //       out_color = lit_color;
    //     } else if(in_relative % 2 == 1) // If it's reflected at an angle
    //     {
    //       int rel_out = 8 - in_relative;  // calculate the reflected angle
    //       out_angle = (rel_out + facing_angle) % 8;
    //       out_color = lit_color;
    //     } else
    //     {
    //       Debug.LogError("Invalid Light Angle.");
    //     }
    //   }
    //   // Calculations for Color filter columns
    //   if(is_lit && type == ColType.COLOR) // Calculates the out color using the color enum defined above
    //   {
    //     out_angle = lit_angle; // part being reflected back
    //     out_color = (LightColor) Math.Max(((int)lit_color - (int)color), 0); // Reflected color (if we do that)
    //     through_angle = (lit_angle + 4)%8; // part going through
    //     through_color = (LightColor) Math.Max(((int)lit_color - (int)out_color), 0); // color that passes through (should be the column color or NONE)
    //   }
    //   // TODO: Handle the light creation column and light receiver column.
    //   //   Light creation may have to be mostly implemented in the Light class,
    //   //   as the current system doesn't handle vertical light angles.
    //   //   However, the receiver should be easily enough implemented in this script.
    //   //   I'll set up basic columns for both of them soon.
    // }

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



}
