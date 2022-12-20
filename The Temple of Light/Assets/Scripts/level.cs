using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class level : MonoBehaviour
{
    public Column end_col;
    public GameObject del_door;
    public GameObject exit;
    public GameObject player;
    public int[,] placeable;
    public int xdim;
    public int ydim;
    public int max_x;     // Level dimensions for placement shit
    public int max_z;
    public int min_x;
    public int min_z;
    public static bool globalPause; // stop creature movement but not anims


    internal List<LightBehavior> source_lights = new List<LightBehavior>();   // for refreshing on rotation
    internal List<Column> source_cols = new List<Column>();
    private bool has_won = false;

    // Start is called before the first frame update
    void Start()
    {
        if(!end_col)
          Debug.LogError("End Column not found.");
        if(!del_door)
          Debug.LogError("Doorway Block not found.");
        if(!player)
          Debug.LogError("Player object not found.");
    }

    // Update is called once per frame
    void Update()
    {
      if(end_col.is_lit) {
        del_door.GetComponent<MeshRenderer>().enabled = false;
      }
      if(exit.GetComponent<EndDoor>().player_collision) {
        has_won = true;
        Debug.Log("Player completed level");

        // TODO: other level changing stuff here
        exit.GetComponent<EndDoor>().player_collision = false;

      }
    }

    public static void setPause(bool pause)
    {
      if(pause) {
        Time.timeScale = 0;
      } else {
        Time.timeScale = 1;
      }
      return;
    }

    public static void softPause(bool pause)
    {
      if(pause) {
        globalPause = true;
      } else {
        globalPause = false;
      }
    }

    public void refreshLights(Column source) // Refreshing all the lights in the level
    {
      for(int i=0; i< source_lights.Count; i++) {
        Debug.Log(source_lights.Count);
        LightBehavior light_obj = source_lights[i];
        source_lights.Remove(light_obj);
        light_obj.Kill();
      }
      // foreach(Column source_col in source_cols) {
      //   LightBehavior newLight = source_col.CreateLight().GetComponent<LightBehavior>();
      //   source_lights.Add(newLight);
      // }
      if(source.parentLight)
        source.parentLight.ManualEnter(source.GetComponent<Collider>());
    }

}
