using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
public class Scene : MonoBehaviour
{
	public AudioSource SoundPlayer;
	public AudioClip BGM;
	private void Start()
	{
		SoundPlayer.clip = BGM;
		SoundPlayer.Play();
	}
	// Update is called once per frame
	void Update()
    {
		if(Input.GetMouseButtonDown(0))
		{
			SceneManager.LoadScene("test");
			SoundPlayer.Stop();
		}
    }
}
