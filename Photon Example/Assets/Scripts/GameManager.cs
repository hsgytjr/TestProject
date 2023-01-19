using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using System.Linq;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private GameObject hpBarHolderPrefab = null;

    // ��Ʈ��ũ���� ������ �ɰŸ� ���� ���������� ���������Ѵ�.
    // ��Ʈ��ũ���� �������� �ʿ䰡 ���� ��氰���� ���������� ������ �ʾƵ� �ȴ�.
    //private HpBarHolder hpbarHolder = null;
    
    private List<GameObject> playerGoList = new List<GameObject>();

    private void Start()
    {
        if (playerPrefab != null)
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3 // �������� �����°� �ƴ϶� �������� �̸��� ���������. 
                // playerPerfab.name �κп� "P_Player" �̷��� ��� �ȴ�.
                (Random.Range(-10.0f, 10.0f), 0.0f, Random.Range(-10.0f, 10.0f)), Quaternion.identity, 0);
            // Instantiate�� ����Ƽ�� �ִ°� ���°� �ƴ϶� PhotonNetwork�� �ִ� Instantiate�� ����ؾ��Ѵ�.
            // Photon�� Instantiate�� ����ϸ� ���� ���� �ٸ�ĳ���͵鿡 ���� ������ ���������� �ʾƵ� �������� ����ϰ��ִ� ������ ���� �ٰ��� ������ش�.
            // �̷����ϸ� ���ĳ���͵� ��ĳ���͵� ���� player ��ũ��Ʈ�� �������ִ� ĳ���ͱ⶧���� ������ �� �� ���Եȴ�.
            // ���� ���� ������ ĳ���ʹ� �̰��Դϴ�~ �ϰ� �ڵ�� �˷�����Ѵ�.
            go.GetComponent<PlayerCtrl>().SetMaterial(PhotonNetwork.CurrentRoom.PlayerCount); // �÷��̾��� ���� �ٲ��ִ� �ڵ�.
            // ������ �濡 ������ ���ڿ� ���� ĳ���Ϳ� ���� �����ִ� ����̴�.
            // ������ �� ���� �ٲ��ְ� �ִٴ� ����̴�. �̰� ��� �ذ����� �����غ�����.
            GameObject hpGo = Instantiate(hpBarHolderPrefab, go.transform);
            hpGo.transform.position = go.transform.position;
        }
    }

    // PhotonNetwork.LeaveRoom �Լ��� ȣ��Ǹ� OnLeftRoom�Լ� ȣ��
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
        SceneManager.LoadScene("Launcher"); // �ٽ� ���� ������ ���ư��°�.
    }

    public override void OnPlayerEnteredRoom(Player _otherPlayer) // � �÷��̾ �濡 ������ ȣ��ȴ�.
    {
        // ��� Ŭ���̾�Ʈ�� ȣ��޴� �Լ�.
        Debug.LogFormat("Player Entered Room: {0}", _otherPlayer.NickName); // ���»���� �г����� ��� ���� ���Դ����� �˷���.

        // photonView�� ���� ������ �������� �ʾƵ� ����� �� �ִ�. transform��������.
        // photon�� ���¸� ���Ѻ��� �����ϴ¾���.
        photonView.RPC("ApplyPlayerList", RpcTarget.All); // RPC(Remote Procedure Call) : �������� �ٸ� Ŭ���̾�Ʈ�� �Լ��� ���� ȣ���ϴ°�.
        // ���ο� �÷��̾ �������� �÷��̾����� �ٽ� �����϶�� �Լ��� ȣ���Ѵ�. ��������? ��� �÷��̾�����.
        // ���⼭ All�� ������ ���Եȴ�. �����͸� ȣ���ϴ� MasterClient, ���������� �������� ȣ���ϴ� Others �� �ִ�.
        // �� �̰ŷ� Ŭ���̾�Ʈ��� p2p��� �� �������°��̴�.
        // RPC�� ����ϸ� ���� �÷��̾ �� �ٲٴ°͵� �����ų �� �ִ�.
    }

    [PunRPC] // RPC�� ȣ���� �Լ����� �տ� [PunRPC]�� ��������Ѵ�.
    public void RPCApplyPlayerList()
    {
        int playerCnt = PhotonNetwork.CurrentRoom.PlayerCount;
        // �÷��̾� ����Ʈ�� �ֽ��̶�� �ǳʶ�
        if (playerCnt == playerGoList.Count) return;

        // ���� �濡 ������ �ִ� �÷��̾��� ��
        Debug.LogError("CurrentRoom PlayerCount : " + playerCnt);

        // ���� �����Ǿ� �ִ� ��� ����� ��������
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>(); // PhotonView ��� ������Ʈ�� �پ��ִ� ��� ������Ʈ�� �ٰ������.
        // Find�� ���� ���� �������� ������ ��¿������ �����.

        // �Ź� �������� �ϴ°� �����Ƿ� �÷��̾� ���ӿ�����Ʈ ����Ʈ�� �ʱ�ȭ
        // System.Array.Clear(playerGoList, 0, playerGoList.Legnth); playerGoList�� �迭�̶�� �� ������� �ʱ�ȭ.
        playerGoList.Clear(); /// playerGoList�� ����Ʈ�ϱ� �� ������� �ʱ�ȭ.
        // ���� �����Ǿ� �ִ� ����� ��ü��
        // �������� �÷��̾���� ���ͳѹ��� ����,
        // �÷��̾� ���ӿ�����Ʈ ����Ʈ�� �߰�
        for (int i = 0; i < playerCnt; ++i)
        {
            // Ű�� 0�� �ƴ� 1���� ����
            int key = i + 1; // dictionary���� ����ϴ� Ű�� ��������.
            for (int j = 0; j < photonViews.Length; ++j)
            {
                // ���� PhotonNetwork.Instantiate�� ���ؼ� ������ ����䰡 �ƴ϶�� �ѱ�
                if (photonViews[j].isRuntimeInstantiated == false) continue;
                // ���� ���� Ű ���� ��ųʸ� ���� �������� �ʴ´ٸ� �ѱ�
                if (PhotonNetwork.CurrentRoom.Players.ContainsKey(key) == false) continue;
                // ContainsKey = Dictionaryó�� �ش� Ű�� �������ִ��� �� ã�ƺ��� �Լ�.
                // �׷� Ű���� �����־������? Players �������� ���콺�� �÷����� Dictionary�������� �Ǿ��ִ°� �� �� �ִ�.
                // �ڵ����� key���� ++1�� �Ҵ����ִ°��̴�.

                // ������� ���ͳѹ�
                // ���ͳѹ���? �ش� ������Ʈ�� ��ȣ�� �����ִ°�. Dictionary�� key��ó�� ������Ʈ���� ��ȣ�� �����ְ� �ش� ��ȣ�� ������Ʈ�� �����Ѵ�.
                int viewNum = photonViews[j].Owner.ActorNumber;

                // �������� �÷��̾��� ���ͳѹ�
                // �� ���ͳѹ��� �� �÷��̾���� ���ͳѹ��� ���� �����Ѵ�. �̰� ���� �����صθ� �÷��̾ ������� ���� ��������� �� �� �ְ� ������ �� �ִ�.
                // Ŭ���̾�Ʈ ������ ������Ʈ�� ��������� ������� ������Ʈ�� ��ȣ�� �������ٵ� ��ó�� ���ͳѹ��� ���صΰ�, �������� �� ���ͳѹ���� ������Ʈ�� ���Ľ�Ű��
                // ��� Ŭ���̾�Ʈ���� ������Ʈ���� ���� ������� �����ȴ�.
                int playerNum = PhotonNetwork.CurrentRoom.Players[key].ActorNumber;

                // ���ͳѹ��� ���� ������Ʈ�� �ִٸ�,
                if (viewNum == playerNum)
                {
                    // ���ӿ�����Ʈ �̸��� �˾ƺ��� ���� ����
                    photonViews[j].gameObject.name = "Player_" + photonViews[j].Owner.NickName;
                    // ���� ���ӿ�����Ʈ�� ����Ʈ�� �߰�
                    playerGoList.Add(photonViews[j].gameObject);
                }
            }
        }

        // ����׿�
        PrintPlayerList();
    }

    private void PrintPlayerList()
    {
        foreach (GameObject go in playerGoList)
        {
            if (go != null)
            {
                Debug.LogError(go.name); // ���⼭ logError�� ������ ������ ������ �ƴ϶� ������ ���� �α׸� ������ Error�� ������ UI�� �������ϱ� �����̴�.
            }
        }
    }

    public override void OnPlayerLeftRoom(Player _otherPlayer)
    {
        Debug.LogFormat("Player Left Room: {0}", _otherPlayer.NickName);       
    }

    public void LeaveRoom() 
    {
        Debug.Log("Leave Room");
        PhotonNetwork.LeaveRoom(); // �� ������ LeaveRoom�� ȣ���Ѵٴ� �͸� �˸� �ȴ�.
    }
}
