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
        if (!photonView.IsMine) return; // ����ȭ�� �÷��̾� 1 2 3 4 �� �� �÷��̾� ������Ʈ�� �ƴ϶�� return ���ش�.
        // ��� �÷��̾�� ���� �÷��̾� ��ũ��Ʈ�� ����ϱ⶧���� �ڵ�ȿ� �ϳ��ϳ��� ����ó���� ������Ѵ�.
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

        LookAtMouseCursor(); // ���콺Ŀ�� ������ ���� ������.
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
            // �Ѿ˵� ��� ��ǻ�Ϳ��� �Ȱ��� �������ϴϱ� PhotonNetwork.Instantiate�� ������Ѵ�.
            // �̶� �����ϴ� �������� ������ ��ü�� �ƴ϶� �������� �̸��� ã�Ƽ� �������� ������ Ŭ���̾�Ʈ���� �������� �����ص� �޸��� ��ġ�� �ٸ� �� �ֱ� �����̴�.
            go.GetComponent<Bullet>().Shoot(this.gameObject, this.transform.forward);
        }
    }

    public void LookAtMouseCursor()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerPos = Camera.main.WorldToScreenPoint(this.transform.position); // ��ũ���� �ִ°� ����� ���� �� ��ġ�� ����ϴ°ͺ���
        // ���忡 �ִ°� ��ũ������ �����ͼ� ��ġ�� ����ϴ°� ���ϱ⿡ �÷��̾��� ��ġ�� ��ũ������ �����´�.
        Vector3 dir = mousePos - playerPos; // ���콺 - �÷��̾� = �÷��̾ ���콺�� �ٶ󺸴� ���⺤�Ͱ� ����.
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; // �� ������ ������ ��ũź��Ʈ�� ������ ������ ���� �� ����. �� x�� y�� �ݴ�� �־������.
        // �ﰢ�Լ��� ���Ȱ��̴ϱ� ��׸��� �ٲ��ִ� �۾��� ���ָ� ������ ���´�.
        this.transform.rotation = Quaternion.AngleAxis(-angle + 90.0f, Vector3.up); // Vector3.up�� ������ǥ�� �������� ���ʹ��� �� y���� �������� ȸ���϶�� ���̴�.
        // +90�� ���ִ� ������ ����Ƽ�� y�� 0���� ������ ���������� ���ִ°� �ƴ϶� �������� ���ֱ� ������ 90���� ������� ����Ƽ�������� ������ �� �ֱ� �����̴�.
        // -angle �� ���� ������ ����Ƽ�������� ȸ���Ҷ� -�� �������������� �ݴ�� ���ư��� �����̴�.
    }

    [PunRPC] // HP�� ��� Ŭ���̾�Ʈ���� �����ؾ��ϴϱ� RPC�� ����������ϰ� RPC�� ����ȭ��������ϴ°� PunRPC�� ��������Ѵ�.
    public void ApplyHP(int _hp)
    {
        hp = _hp; // �� HP���� ���°� �ƴ϶� ���ο� hp�� ���Ž����ִ� ����̴�.
        Debug.LogErrorFormat("{0} hp : {1}", PhotonNetwork.NickName, hp); // ������ ü���� @�̴� ��� �˷��ִ°�.

        //hpbarHolder.UpdateHpBar(3, _hp);
        //hpText.text = hp.ToString();
        
        if (hp <= 0) // �׾�����
        {
            Debug.LogErrorFormat("Destroy: {0}", PhotonNetwork.NickName);
            isDead = true; // �׾��ٰ� ǥ��
            PhotonNetwork.Destroy(this.gameObject); // �ı��Ҷ��� PhotonNetwork.Destroy ����� ��� Ŭ���̾�Ʈ���� �ı��ȴ�.
        }
    }

    [PunRPC]
    public void OnDamage(int _dmg)
    {
        hp -= _dmg; // �� ü���� ��������ŭ �� �Ŀ� �� ü���� �ٸ�Ŭ���̾�Ʈ���� ����ȭ�����~ ��� �ϴ°��̴�.
        photonView.RPC("ApplyHP", RpcTarget.Others, hp);
    }
}

