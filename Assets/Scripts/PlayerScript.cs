using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D RB;
    public Animator AN;
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NickNameText;
    public Image HealthImage;

	[SerializeField]
	private GameObject Gun;
    bool isGround;
    Vector3 curPos;

	Vector3 m_Pos;
	Vector3 m_GunDir;

	float MyDelay;
	[SerializeField]
	float DelayTime = 0.092f;


	[SerializeField]
	float MoveSpeed = 10.0f;
	Vector3 MoveCon;
    void Awake()
    {
        // 닉네임
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

        if (PV.IsMine)
        {
            // 2D 카메라
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;
        }
		MyDelay = 0;
	}


	void Update()
    { 
        if (PV.IsMine)
		{
			/* #region  // ← → 이동
			float axis = Input.GetAxisRaw("Horizontal");
			float axis2 = Input.GetAxisRaw("Vertical");

			m_Pos = gameObject.transform.position;

			m_Pos.x += 0.05f * axis;
			m_Pos.y += 0.05f * axis2;

			gameObject.transform.position = m_Pos;

			RB.velocity = new Vector2(4 * axis, RB.velocity.y);
			#endregion
			*/

			MoveCon.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
			MoveCon = MoveCon.normalized * MoveSpeed * Time.deltaTime;
			RB.MovePosition(transform.position + MoveCon);

			//if (Input.GetAxisRaw("Horizontal"))
			//{
			//	AN.SetBool("walk", true);
			//	PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axis); // 재접속시 filpX를 동기화해주기 위해서 AllBuffered
			//}
			//else AN.SetBool("walk", false);

			Vector2 len = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
			float z = Mathf.Atan2(len.y, len.x) * Mathf.Rad2Deg;
			Gun.transform.rotation = Quaternion.Euler(0, 0, z-90);
			//m_GunDir = new Vector3(Gun.transform.rotation.x, Gun.transform.rotation.y);

			MyDelay += Time.deltaTime;
			if (Input.GetMouseButton(0) && MyDelay > DelayTime)
			{
				Vector2 dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;

				PhotonNetwork.Instantiate("Bullet", transform.position + new Vector3(0, 0, 0), Quaternion.identity)
					.GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, dir);
				MyDelay = 0.0f;
				AN.SetTrigger("shot");
			}
			PV.RPC("GunDir", RpcTarget.AllBuffered, Gun.transform.rotation);

		}
		// IsMine이 아닌 것들은 부드럽게 위치 동기화
		else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }


    [PunRPC]
    void FlipXRPC(float axis) => SR.flipX = axis == -1;

    [PunRPC]
    void JumpRPC()
    {
        RB.velocity = Vector2.zero;
        RB.AddForce(Vector2.up * 700);
    }

    public void Hit()
    {
        HealthImage.fillAmount -= 0.1f;
        if (HealthImage.fillAmount <= 0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // AllBuffered로 해야 제대로 사라져 복제버그가 안 생긴다
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

	[PunRPC]
	void GunDir(Vector3 gunDir) => this.m_GunDir = gunDir;


	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(HealthImage.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
