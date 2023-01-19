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
            this.transform.Translate(direction * speed * Time.deltaTime); // �÷��̾ �Ѿ��� ����� �������� ���ư���.
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
        if (!photonView.IsMine) return; // ���� �� �̻����� �ƴ϶�� return����. �� Ŭ���̾�Ʈ���� ���� ��͵��� �����ϴ°��̴�. ���� �ٸ��� ������� ����.

        if (owner != _other.gameObject && _other.CompareTag("Player")) // �浹ü�� �ѽ�ְ� �ƴϰ�, �÷��̾� ��� �±׸� ���� ������Ʈ���
        {
            _other.GetComponent<PlayerCtrl>().OnDamage(1); // �������� 1 ���� �Ŀ�
            SelfDestroy(); // �����θ� �ı��Ѵ�.
        }
        // ���⼭ �´� _other�� �� Ŭ���̾�Ʈ �󿡼� ���� �÷��̾� ������Ʈ�� ���ϴ°��̴�.
        // �¶��� �󿡼� ���� ĳ���͸� ���ϴ°� �ƴϴ�.
        // ���� �� Ŭ���̾�Ʈ�� �ִ� ���� ĳ���Ͱ� �¾Ұ� �ٸ��ֵ����� �̸��� �˷����~ ��� ���غ��� ������ �̸��� ��� ���°��̴�.
        // ���⼭ ���ߴ� ������ �¶����� ���� �÷��̾�� �ϴϱ� RPC�� �����ؾ��Ѵ�.
        // �װ� ��������� �����غ�����.
    }
}
