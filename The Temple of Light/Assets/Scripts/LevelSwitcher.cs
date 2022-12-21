using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSwitcher : MonoBehaviour
{
    // Public fields
    public string current_level_id;
    public level current_level;
    public Camera main_camera;
    public Player player;

    // Internal fields
    internal List<string> levelIDs;
    internal Dictionary<string,level> levels = new Dictionary<string,level>();

    // Private fields
    private Vector3 player_start_pos = new Vector3(-5f, 0f, -5f);

    // Start is called before the first frame update
    void Start()
    {

        //levelIDs = new List<string>{"tut01","tut02","tut03","tut04",
        //  "tut05","tut06","tut07","tut08","Level01","Level02","Level03","victory"};
        levelIDs = new List<string>{"tut01","mainmenu"};
        foreach(string name in levelIDs)
        {
          levels.Add(name, GameObject.Find(name).GetComponent<level>());
        }

        current_level_id = "mainmenu";
        goToLevel("tut01");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void moveToNextLevel()
    {
      goToLevel(current_level.next_level_id);

    }

    public void goToLevel(string levelID)
    {
      level nextLevel = levels[levelID];
      Vector3 level_pos = nextLevel.transform.position;
      Vector3 cameraPos = new Vector3(0f, 15f, -8f);
      if(nextLevel.max_x == 24) { // 16x16 map, not 8x8
        cameraPos = new Vector3(8f, 25f, -8f);
      }
      Quaternion cam_angle = Quaternion.Euler(60f, 0f, 0f);
      if(nextLevel.name == "mainmenu") {
        cameraPos = new Vector3(0f, 5f, -8f);
        cam_angle = Quaternion.Euler(15f, 0f, 0f);
      } else if(nextLevel.name == "victory") {
        //TODO victory angle
      }

      main_camera.transform.rotation = cam_angle;
      main_camera.transform.position = level_pos + cameraPos;
      player.transform.position = level_pos + player_start_pos;
      current_level = nextLevel;
      current_level_id = current_level.name;
    }
}
