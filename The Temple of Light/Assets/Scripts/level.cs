using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class level : MonoBehaviour
{
    public Column end_col;
    public Column second_end_col; // Yes I'm aware there are better ways.
    public Column third_end_col;  // But it's 4:30 am.
    public int num_ends;
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
    public string next_level_id;

    internal List<LightBehavior> source_lights = new List<LightBehavior>();   // for refreshing on rotation
    internal List<Column> source_cols = new List<Column>();
    internal Vector3 start_pos;
    internal LevelSwitcher ls;

    // Start is called before the first frame update
    void Start()
    {
        if(!end_col)
          Debug.LogError("End Column not found.");
        if(!end_col.end_door)
          Debug.LogError("Doorway Block not found.");
        if(!player)
          Debug.LogError("Player object not found.");
        ls = GameObject.Find("LevelSwitcher").GetComponent<LevelSwitcher>();
    }

    // Update is called once per frame
    void Update()
    {
      checkCompletion();
      if(exit.GetComponent<EndDoor>().player_collision) {
        exit.GetComponent<EndDoor>().player_collision = false;
        ls.moveToNextLevel();
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

    private bool checkCompletion() {
      if(num_ends == 1) {
        if(end_col.is_lit && end_col.checkColor()) {
          end_col.end_door.GetComponent<MeshRenderer>().enabled = false;
          end_col.end_door.SetActive(false);
          return true;
        }
      } else if(num_ends == 2) {
        if(end_col.is_lit && end_col.checkColor() &&
        second_end_col.is_lit && second_end_col.checkColor()) {
          end_col.end_door.GetComponent<MeshRenderer>().enabled = false;
          end_col.end_door.SetActive(false);
          return true;
        }
      } else if(num_ends == 3) {
        if(end_col.is_lit && end_col.checkColor() &&
        second_end_col.is_lit && second_end_col.checkColor() &&
        third_end_col.is_lit && third_end_col.checkColor()) {
          end_col.end_door.GetComponent<MeshRenderer>().enabled = false;
          end_col.end_door.SetActive(false);
          return true;
        }
      } else {
        Debug.LogError("No end columns!");
      }
      return false;
    }

}
