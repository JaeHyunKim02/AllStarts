using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

using Photon.Realtime;
public class Fild : MonoBehaviourPunCallbacks
{
	[SerializeField]
	private float Time;

	private void Awake()
	{
		   Time = 0;
	}
	
}
