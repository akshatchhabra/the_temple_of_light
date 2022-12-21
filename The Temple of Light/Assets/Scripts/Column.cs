using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public string levelID;
    public GameObject lit_light;
    public GameObject end_door;         // only if an end column
    public Material tint_mat;

    public bool is_lit;             // whether or not the column is receiving a light beam
    internal Color lit_color;         // color of incoming light. "NONE" if not lit.
    internal LightBehavior childLight;
    internal LightBehavior parentLight;

    private level Level;
    private float carry_height;
    private bool being_carried;
    private Vector3 offset;


    // Start is called before the first frame update
    void Start()
    {
      Level = GameObject.Find(levelID).GetComponent<level>();
      if(!Level)
        Debug.LogError("Level not found.");
      player_reference = GameObject.Find("Player");
      if(!player_reference) {
        Debug.LogError("Player not found");
      }

      is_lit = false;
      lit_color = Color.black;

      if(color == null || color == Color.black || color == new Color(0f,0f,0f,0f)) {
        color = Color.white;
      }
      being_carried = false;
      facing = (((int)transform.eulerAngles.y + 1) / 45) + 2 % 8;
      if(type == ColType.ONEWAY)
        facing = facing -2;
      if(type == ColType.LIGHT_EMIT)
        facing = (((int)transform.eulerAngles.y + 91) / -45) % 8;
      if(type == ColType.LIGHT_RECV)
        facing = (((int)transform.eulerAngles.y + 181) / -45) % 8;
      facing_angle = new Angle(facing);
      if (type == ColType.LIGHT_EMIT)
      {
        childLight = CreateColorLight(color).GetComponent<LightBehavior>();
        Level.source_cols.Add(this);
        Level.source_lights.Add(childLight);
        is_lit = true;
      }
      carry_height = 5;
      offset = new Vector3(0f,carry_height,0f);

      // Visibly show locked columns
      if(type != ColType.LIGHT_EMIT && type != ColType.LIGHT_RECV && !movable) {
        GameObject lock_indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lock_indicator.transform.localScale = new Vector3(2.2f,0.6f, 2.2f);
        lock_indicator.transform.position = this.transform.position;
        lock_indicator.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
      }

      // Use the same to tint colored emitters/receivers
      // if((type == ColType.LIGHT_EMIT || type == ColType.LIGHT_RECV) && (color != Color.white && color!= Color.black)) {
      //   GameObject tint_indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
      //   tint_indicator.transform.localScale = new Vector3(2.2f, 1f, 2.2f);
      //   tint_indicator.transform.position = this.transform.position;
      //   tint_indicator.GetComponent<Renderer>().material = tint_mat;
      //   tint_indicator.GetComponent<Renderer>().material.SetColor("_Color", color);
      // }

    }


    // Update is called once per frame
    void Update()
    {
      if(being_carried) {
        transform.position = player_reference.transform.position + offset;
      }

      if(!parentLight && type != ColType.LIGHT_EMIT) {
        is_lit = false;
        lit_color = new Color(0f,0f,0f,0f);
      }

      if(is_lit) {
        lit_light.SetActive(true);
      } else {
        lit_light.SetActive(false);
      }

      if (type == ColType.LIGHT_EMIT && !childLight)
      {
        childLight = CreateColorLight(color).GetComponent<LightBehavior>();
        Level.source_cols.Add(this);
        Level.source_lights.Add(childLight);
      }



    }

    internal GameObject CreateLight(Color color)
    {
        GameObject childLight = Instantiate(lightObject);
        LightBehavior childBehavior = childLight.GetComponent<LightBehavior>();
        childBehavior.direction = facing_angle;
        childBehavior.source = gameObject;
        childLight.SetActive(true);
        return childLight;
    }

    internal GameObject CreateColorLight(Color color)
    {

        GameObject childLight = Instantiate(lightObject);
        Material childMaterial = childLight.GetComponent<Renderer>().material;
        Color colorA = new Color(color.r, color.g, color.b, 0.5f);
        childMaterial.SetColor("_Color", colorA);
        childMaterial.SetColor("_EmissionColor", colorA);
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

    private Color mergeColors(Color color1, Color color2) {
      float r = Math.Max(color1.r, color2.r);
      float g = Math.Max(color1.g, color2.g);
      float b = Math.Max(color1.b, color2.b);
      float a = Math.Max(color1.a, color2.a);
      return new Color(r,g,b,a);
    }

    internal void addColor(Color new_color) {
      if(lit_color == null) {
        lit_color = new_color;
      } else {
        lit_color = mergeColors(lit_color, new_color);
      }
    }

    internal bool checkColor()
    {
        return lit_color.r >= color.r && lit_color.g >= color.g && lit_color.b >= color.b;
    }

}
