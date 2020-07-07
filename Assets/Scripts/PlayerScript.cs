using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.EventSystems;
enum Kinds
{
	DEVIL,
	TOBI,

}
public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable //, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public Rigidbody2D rigidbody;
    public Animator animator;
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NickNameText;
    public Image HealthImage;

	[SerializeField]
	GameObject HpBar2;

	[SerializeField]
	private GameObject Gun;
    bool isGround;
    Vector3 curPos;

	Vector3 m_Pos;
	Vector3 m_GunDir;
	Kinds Kind;
	float MyDelay;
	[SerializeField]
	float DelayTime = 0.092f;


	[SerializeField]
	float MoveSpeed = 10.0f;
	Vector3 MoveCon;

	[SerializeField]
	private float MoveRad;

	[SerializeField]
	private float AtkRad;

	bool IsMove = false;
	bool IsAtk = false;
	public VariableJoystick variableJoystick;
	public VariableJoystick AtkJoyStick;

	private GameObject HPBAR;

	[SerializeField]
	float SkillCoolTime1 = 0;
		
	[SerializeField]
	float SkillCoolTime2 = 0;


	GameObject SkillIcon1;
	GameObject SkillIcon2;


	int BulletCount;
	float ReroadTime;
	Text BulletText;
	void Awake()
    {
		if(PV.IsMine)
		{

		}
		if(PV.IsMine)
		{
			BulletCount = 30;
			ReroadTime = 0;
			BulletText = GameObject.FindWithTag("BulletText").GetComponent<Text>();
			SkillIcon1 = GameObject.Find("Skill1");
			SkillIcon2 = GameObject.Find("Skill2");
			variableJoystick = GameObject.FindWithTag("GameController").GetComponent< VariableJoystick>();
			AtkJoyStick = GameObject.FindWithTag("ShootController").GetComponent<VariableJoystick>();
			HPBAR = GameObject.Find("PlayerHpBar");
			HPBAR.GetComponent<Image>().fillAmount = 1;
		}

		//HpBar2 = GameObject.Find("PlayerHpBar");
		//HpBar2.GetComponent<Image>().fillAmount = 1;
		// 닉네임
		NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;

		animator.SetBool("Idle", true);
		animator.SetBool("Run", false);	
		animator.SetBool("Die", false);

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
			BulletText.text = "총알:"+BulletCount;
			Vector2 MoveTemp =new Vector2 (variableJoystick.Horizontal, variableJoystick.Vertical);

			MoveCon.Set(MoveTemp.x, MoveTemp.y, 0);
			MoveCon = MoveCon.normalized * MoveSpeed * Time.deltaTime;
			rigidbody.MovePosition(transform.position + MoveCon);

			if (variableJoystick.Horizontal != 0)
			{
				animator.SetBool("Run", true);
				animator.SetBool("Idle", false);

				if (variableJoystick.Horizontal > 0 || AtkJoyStick.Horizontal > 0)
				{
					Gun.GetComponent<SpriteRenderer>().flipX = true;
					PV.RPC("FlipXRPC", RpcTarget.AllBuffered, true);
				}
				else if (variableJoystick.Horizontal < 0 )
				{
					Gun.GetComponent<SpriteRenderer>().flipX = false;
					PV.RPC("FlipXRPC", RpcTarget.AllBuffered, false);
				}
			}
			else
			{
				animator.SetBool("Run", false);
				animator.SetBool("Idle", true);
			}

			if(BulletCount<= 0)
			{
				ReroadTime += Time.deltaTime;
				if(ReroadTime >=3f )
				{
					BulletCount = 30;  
				}
			}
			MyDelay += Time.deltaTime;
			if ((AtkJoyStick.Horizontal != 0 || AtkJoyStick.Vertical !=0 )&& MyDelay > DelayTime && BulletCount>0)
			{
				if (AtkJoyStick.Horizontal > 0)
				{
					Gun.GetComponent<SpriteRenderer>().flipX = true;
					PV.RPC("FlipXRPC", RpcTarget.AllBuffered, true);
				}
				else if (AtkJoyStick.Vertical < 0)
				{
					Gun.GetComponent<SpriteRenderer>().flipX = false;
					PV.RPC("FlipXRPC", RpcTarget.AllBuffered, false);
				}
				float x = AtkJoyStick.Horizontal;
				float y = AtkJoyStick.Vertical;
				Vector2 dir = new Vector2(x, y);

				dir.Normalize();
				PhotonNetwork.Instantiate("DevilBullet", transform.position + new Vector3(0, 0, 0), Quaternion.identity)
				.GetComponent<PhotonView>().RPC("DirRPC", RpcTarget.All, dir);
				MyDelay = 0.0f;
				BulletCount -= 1;

				animator.SetTrigger("shot");
			}
		}
		// IsMine이 아닌 것들은 부드럽게 위치 동기화
		else if ((transform.position - curPos).sqrMagnitude >= 100) transform.position = curPos;
        else transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10);
    }


	[PunRPC]
	void FlipXRPC(bool axis) => SR.flipX = axis;// < -1;

    [PunRPC]
    void JumpRPC()
    {
		rigidbody.velocity = Vector2.zero;
		rigidbody.AddForce(Vector2.up * 700);
    }

    public void Hit()
    {
		//HealthImage.GetComponent<Image>().fillAmount -= 0.1f;
		HPBAR.GetComponent<Image>().fillAmount -= 0.1f;
		if (HPBAR.GetComponent<Image>().fillAmount <= 0 )
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            PV.RPC("DestroyRPC", RpcTarget.AllBuffered); // AllBuffered로 해야 제대로 사라져 복제버그가 안 생긴다
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

	//[PunRPC]
	//void GunDir(Vector3 gunDir) => this.m_GunDir = gunDir;


	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
           // stream.SendNext(HealthImage.fillAmount);
        }
        else
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }
}
