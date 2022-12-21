using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSwitcher : MonoBehaviour
{
    // Public fields
    public string current_level_id;
    public level current_level;
    public Camera main_camera;
    public Player player;
    public Text instructions_text;

    // Internal fields
    internal List<string> levelIDs;
    internal Dictionary<string,level> levels = new Dictionary<string,level>();
    internal Dictionary<string,string> instructions = new Dictionary<string, string>();
    // Private fields
    private Vector3 player_start_pos = new Vector3(-5f, 0f, -5f);
    private bool level_timeout = false;
    private string path = "save.txt";
    private GameObject pause_menu;
    private GameObject game_play_menu;
    private GameObject main_menu;
    private string state;

    // Start is called before the first frame update
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "level1") {
          levelIDs = new List<string>{"level1"};
          levels.Add("level1",GameObject.Find("level1").GetComponent<level>());
        }
        levelIDs = new List<string>{"mainmenu","tut01","tut02","tut03","tut04",
          "tut05","tut06","tut07","tut08","victory"}; //,"Level01","Level02","Level03"
        foreach(string name in levelIDs)
        {
          levels.Add(name, GameObject.Find(name).GetComponent<level>());
        }
        current_level_id = fileRead();
        if(current_level_id == null) {
          current_level_id = "tut07";
        }

// TODO info dictionary shit here

        game_play_menu = GameObject.Find("GameplayCanvas");
        pause_menu = GameObject.Find("PauseMenuCanvas");
        main_menu = GameObject.Find("MainMenuCanvas");
        game_play_menu.SetActive(false);
        pause_menu.SetActive(false);
        main_menu.SetActive(false);
        goToLevel(current_level_id);
        // state takes 3 values: pause, play, mainmenu
        // set this back to mainmenu
        state = "play";
    }

    // Update is called once per frame
    void Update()
    {
      if (state == "pause"){
        game_play_menu.SetActive(false);
        pause_menu.SetActive(true);
        main_menu.SetActive(false);
      }
      else if (state == "play"){
        game_play_menu.SetActive(true);
        pause_menu.SetActive(false);
        main_menu.SetActive(false);
      }
      else if (state == "mainmenu"){
        game_play_menu.SetActive(false);
        pause_menu.SetActive(false);
        main_menu.SetActive(true);
      }
      
    }

    public void moveToNextLevel()
    {
      goToLevel(current_level.next_level_id);

    }

    public void goToLevel(string levelID)
    {
      if(level_timeout) {
        return;
      }
      StartCoroutine(blockDoubleMove());
      level nextLevel = levels[levelID];
      Vector3 level_pos = nextLevel.transform.position;
      Vector3 cameraPos = new Vector3(0f, 15f, -8f);
      if(nextLevel.max_x == 24) { // 16x16 map, not 8x8
        cameraPos = new Vector3(8f, 25f, -8f);
      }
      Quaternion cam_angle = Quaternion.Euler(60f, 0f, 0f);
      if(nextLevel.name == "mainmenu" || nextLevel.name == "victory") {
        cameraPos = new Vector3(0f, 5f, -8f);
        cam_angle = Quaternion.Euler(15f, 0f, 0f);
      } else if(nextLevel.name == "victory") {
        //TODO victory angle
      }

      main_camera.transform.rotation = cam_angle;
      main_camera.transform.position = level_pos + cameraPos;
      player.GetComponent<CharacterController>().enabled = false;
      player.transform.position = level_pos + player_start_pos;
      player.GetComponent<CharacterController>().enabled = true;
      current_level = nextLevel;
      current_level_id = current_level.name;

    }

    public void resetToMainMenu()
    {
      fileWrite("mainmenu");
      Scene current_scene = SceneManager.GetActiveScene();
      SceneManager.LoadScene(current_scene.name);

    }

    public void resetToSameLevel()
    {
      fileWrite(current_level_id);
      Scene current_scene = SceneManager.GetActiveScene();
      SceneManager.LoadScene(current_scene.name);
    }

    IEnumerator blockDoubleMove()
    {
      level_timeout = true;
      yield return new WaitForSeconds(2);
      level_timeout = false;
    }

    private void fileWrite(string input) { // have I been using the same filewrite function
                                // since I wrote it for, like, assignment 3? yes.
        using(StreamWriter writer = File.CreateText(path)) {
          writer.WriteLine(input);
          writer.Close();
      }
    }

    private string fileRead()
    {
      string out_line = null;
      if(File.Exists(path)) {
        using(StreamReader reader = new StreamReader(path)) {
          out_line = reader.ReadLine();
          reader.Close();
        }
        File.Delete(path);
      }
      return out_line;
    }

    public void SetState(string curr_state){
      state = curr_state;
    }
}
