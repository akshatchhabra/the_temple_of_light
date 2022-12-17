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


}
