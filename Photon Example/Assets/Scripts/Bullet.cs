using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Unity.VisualScripting;

public class Bullet : MonoBehaviourPun
{
    private bool isShoot = false;
    private Vector3 direction = Vector3.zero;
    private float speed = 10.0f;
    private float duration = 5.0f;
    private GameObject owner = null;

    private void Update()
    {
        if (isShoot)
        {
            this.transform.Translate(direction * speed * Time.deltaTime); // 플레이어가 총알을 만들면 방향으로 날아가기.
        }
    }

    public void Shoot(GameObject _owner, Vector3 _dir)
    {
        owner = _owner;
        direction = _dir;
        isShoot = true;

        if (photonView.IsMine) Invoke("SelfDestroy", duration);
    }

    private void SelfDestroy()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (!photonView.IsMine) return; // 내가 쏜 미사일이 아니라면 return해줌. 각 클라이언트에서 각자 쏜것들을 관리하는것이다. 물론 다르게 만들수도 있음.

        if (owner != _other.gameObject && _other.CompareTag("Player")) // 충돌체가 총쏜애가 아니고, 플레이어 라는 태그를 가진 오브젝트라면
        {
            _other.GetComponent<PlayerCtrl>().OnDamage(1); // 데미지를 1 깎은 후에
            SelfDestroy(); // 스스로를 파괴한다.
        }
        // 여기서 맞는 _other는 내 클라이언트 상에서 상대방 플레이어 오브젝트를 말하는것이다.
        // 온라인 상에서 상대방 캐릭터를 말하는게 아니다.
        // 따라서 내 클라이언트에 있는 상대방 캐릭터가 맞았고 다른애들한테 이름을 알려줘라~ 라고 말해봐야 본인의 이름만 계속 띄우는것이다.
        // 여기서 맞추는 상대방이 온라인의 상대방 플레이어여야 하니까 RPC로 관리해야한다.
        // 그걸 어떻게할지를 생각해봐야함.
    }
}
