using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class Player : MonoBehaviourPunCallbacks
{ 
	[SerializeField]
	Rigidbody2D rigidbody;
	[SerializeField]
	Animator animator;

	public PhotonView photonView;
	[SerializeField]
	GameObject Gun;

	Vector3 curPos;

	[SerializeField]
	float MoveSpeed;
	[SerializeField]
	float BulletDelay;

	[SerializeField]
	Text Name;

	[SerializeField]
	Camera camera;

	void Awake()
	{
		Name.text = photonView.IsMine ? PhotonNetwork.NickName : photonView.Owner.NickName;
		//Name.color = photonView.IsMine 

		if(photonView.IsMine)
		{
			camera.transform.position = transform.position;
		
		}
	}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
