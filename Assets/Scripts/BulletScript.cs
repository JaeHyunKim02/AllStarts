using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BulletScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    Vector2 dir;


	void Start()
	{
		Destroy(gameObject, 3.0f);
	}
	void Update() => transform.Translate(7 * Time.deltaTime* dir.normalized);


    void OnTriggerEnter2D(Collider2D col) // col을 RPC의 매개변수로 넘겨줄 수 없다
    {
        //if (col.tag == "Ground") PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        if (!PV.IsMine && col.tag == "Player" && col.GetComponent<PhotonView>().IsMine) // 느린쪽에 맞춰서 Hit판정
        {
            col.GetComponent<PlayerScript>().Hit();
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }


    [PunRPC]
    void DirRPC(Vector2 dir) => this.dir = dir;

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
