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
    public int facing_angle;          // facing angle, 0-7 from 0=-x,0z to 7=-x,-z clockwise in 45 deg turns
    public int[] position;            // Can be set when placing down a column to calculate in world coords

    internal bool is_lit;             // whether or not the column is receiving a light beam
    internal int lit_angle;           // logical angle the light is coming from (0-7), -1 if not lit
    internal int out_angle;           // if mirrored, the angle incoming light gets reflected
    internal int through_angle;       // if clear & not lens, angle of nonreflected light
    internal bool blocking;           // Is it blocking the light? (wrong incoming angle, etc)


    // Start is called before the first frame update
    void Start()
    {
      is_lit = false;
      lit_angle = -1;
      out_angle = -1;
      blocking = false;
    }

    // Update is called once per frame
    void Update()
    {

    }


}
