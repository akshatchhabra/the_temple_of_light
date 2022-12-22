using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Camera main_camera;
    public AudioClip move_sound;
    public AudioClip hurt_sound;
    public AudioClip aggro_sound;
    public AudioClip door_sound;
    public AudioClip bat_death_sound;
    public AudioClip music;

    public AudioSource music_player;
    public AudioSource sound_player;
    // Start is called before the first frame update
    void Start()
    {
        music_player = main_camera.gameObject.AddComponent<AudioSource>();
        sound_player = main_camera.gameObject.AddComponent<AudioSource>();
        music_player.clip  = music;
        music_player.Play();
    }

    // Update is called once per frame
    void Update()
    {
      if(Bat.just_activated) {
        Bat.just_activated = false;
        sound_player.clip = aggro_sound;
        sound_player.Play();
      }
      if(Bat.just_died) {
        Bat.just_died = false;
        sound_player.clip = bat_death_sound;
        sound_player.Play();

      }
      if(Player.just_hit) {
        Player.just_hit = false;
        sound_player.clip = hurt_sound;
        sound_player.Play();

      }
      if(Column.moved_col) {
        Column.moved_col = false;
        sound_player.clip = move_sound;
        sound_player.Play();
      }
      if(level.door_opened) {
        level.door_opened = false;
        sound_player.clip = door_sound;
        sound_player.Play();
      }

    }
}
