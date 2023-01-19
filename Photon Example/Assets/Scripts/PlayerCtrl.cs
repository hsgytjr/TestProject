using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using JetBrains.Annotations;
using TMPro;

public class PlayerCtrl : MonoBehaviourPun
{
    private Rigidbody rb = null;
    [SerializeField] private GameObject bulletPrefab = null;
    [SerializeField] private Color[] colors = null;
    [SerializeField] private float speed = 3.0f;
    //[SerializeField] private TextMeshProUGUI hpText = null;

    private int hp = 3;
    private bool isDead = false;
    private HpBarHolder hpbarHolder = null;


    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        isDead = false;
    }

    private void Update()
    {
        if (!photonView.IsMine) return; // 동기화된 플레이어 1 2 3 4 중 내 플레이어 오브젝트가 아니라면 return 해준다.
        // 모든 플레이어는 같은 플레이어 스크립트를 사용하기때문에 코드안에 하나하나의 예외처리를 해줘야한다.
        if (isDead) return;

        if (Input.GetKey(KeyCode.W))
            rb.AddForce(Vector3.forward * speed);
        if (Input.GetKey(KeyCode.S))
            rb.AddForce(Vector3.back * speed);
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(Vector3.left * speed);
        if (Input.GetKey(KeyCode.D))
            rb.AddForce(Vector3.right * speed);

        if (Input.GetMouseButtonDown(0)) ShootBullet();

        LookAtMouseCursor(); // 마우스커서 방향을 보게 만들어둠.
    }

    public void SetMaterial(int _playerNum)
    {
        Debug.LogError(_playerNum + " :  " + colors.Length);
        if (_playerNum > colors.Length) return;

        this.GetComponent<MeshRenderer>().material.color = colors[_playerNum - 1];
    }

    private void ShootBullet()
    {
        if (bulletPrefab)
        {
            GameObject go = PhotonNetwork.Instantiate(bulletPrefab.name, this.transform.position, Quaternion.identity);
            // 총알도 모든 컴퓨터에서 똑같이 보여야하니까 PhotonNetwork.Instantiate를 해줘야한다.
            // 이때 생성하는 프리팹을 프리팹 자체가 아니라 프리팹의 이름을 찾아서 가져오는 이유는 클라이언트마다 프리팹을 저장해둔 메모리의 위치가 다를 수 있기 때문이다.
            go.GetComponent<Bullet>().Shoot(this.gameObject, this.transform.forward);
        }
    }

    public void LookAtMouseCursor()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerPos = Camera.main.WorldToScreenPoint(this.transform.position); // 스크린에 있는걸 월드로 보낸 후 위치를 계산하는것보단
        // 월드에 있는걸 스크린으로 가져와서 위치를 계산하는게 편하기에 플레이어의 위치를 스크린으로 가져온다.
        Vector3 dir = mousePos - playerPos; // 마우스 - 플레이어 = 플레이어가 마우스를 바라보는 방향벡터가 나옴.
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // 위 방향을 가지고 아크탄젠트를 돌리면 각도를 구할 수 있음. 단 x와 y를 반대로 넣어줘야함.
        // 삼각함수는 라디안값이니까 디그리로 바꿔주는 작업을 해주면 각도가 나온다.
        this.transform.rotation = Quaternion.AngleAxis(-angle + 90.0f, Vector3.up); // Vector3.up은 월드좌표를 기준으로 윗쪽방향 즉 y축을 기준으로 회전하라는 뜻이다.
        // +90을 해주는 이유는 유니티는 y축 0도의 시작이 오른쪽으로 돼있는게 아니라 위쪽으로 돼있기 때문에 90도를 더해줘야 유니티방향으로 맞춰줄 수 있기 때문이다.
        // -angle 을 붙인 이유도 유니티기준으로 회전할때 -를 붙혀주지않으면 반대로 돌아가기 때문이다.
    }

    [PunRPC] // HP도 모든 클라이언트에서 공유해야하니까 RPC로 관리해줘야하고 RPC로 동기화시켜줘야하는건 PunRPC를 붙혀줘야한다.
    public void ApplyHP(int _hp)
    {
        hp = _hp; // 내 HP에서 빼는게 아니라 새로운 hp로 갱신시켜주는 방식이다.
        Debug.LogErrorFormat("{0} hp : {1}", PhotonNetwork.NickName, hp); // 누구의 체력이 @이다 라고 알려주는것.

        //hpbarHolder.UpdateHpBar(3, _hp);
        //hpText.text = hp.ToString();
        
        if (hp <= 0) // 죽었으면
        {
            Debug.LogErrorFormat("Destroy: {0}", PhotonNetwork.NickName);
            isDead = true; // 죽었다고 표시
            PhotonNetwork.Destroy(this.gameObject); // 파괴할때도 PhotonNetwork.Destroy 해줘야 모든 클라이언트에서 파괴된다.
        }
    }

    [PunRPC]
    public void OnDamage(int _dmg)
    {
        hp -= _dmg; // 내 체력을 데미지만큼 뺀 후에 그 체력을 다른클라이언트에도 동기화해줘라~ 라고 하는것이다.
        photonView.RPC("ApplyHP", RpcTarget.Others, hp);
    }
}

