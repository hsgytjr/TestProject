using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.Cockpit;
using UnityEngine.SceneManagement;

public class PhotonLauncher : MonoBehaviourPunCallbacks // Pun���� ����ϰ��ִ� Callback���� ���ִ� MonoBehaviour
{
    [SerializeField] private string gameVersion = "0.0.1";      // ������ ����
    [SerializeField] private byte maxPlayerPerRoom = 4;         // ��� �ִ� ������ �����ִ��� ���ϱ�
    [SerializeField] private string nickName = string.Empty;    // ������ �����Ҷ� ����� �г���
    [SerializeField] private Button connectButton = null;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // ���� ��Ʈ��ũ�� �ڵ����� ���̶� ��ũ�� ���ߴ°�.
        // ����� �ٸ�������� ���ÿ� ���� ����ȭ���Ѽ� ������ �����ϱ����ؼ� �ʿ��� �ڵ�
    }

    private void Start()
    {
        connectButton.interactable = true; // �����ϴ� ��ư�� �����ϸ鼭 Ȱ��ȭ�����ִ°�. ���ž���
    }

    public void Connect() // ��ư�� ����Ǿ��ִ� �Լ�
    {
        if (string.IsNullOrEmpty(nickName)) // �켱 �г����� ���̳� ����ְų� �� üũ�ϴ°�
        {
            Debug.Log("NickName is Empty!"); // ������ �̰� UI�� ǥ�õǰ� �ؾ���.
            return;
        }

        if (PhotonNetwork.IsConnected) // ���� ���� ������ �����ִ� �������� �ƴ����� üũ��.
        {
            PhotonNetwork.JoinRandomRoom(); // ������ ���������� ������ ���� ã�Ƽ� ���� ��°�.
            // PhotonNetwork.room ���� �ĺ��� �濡 ���� �ڵ���� ���� ���´�.
            // PhotonNetwork.Lobby �� �κ���õ� ���뵵 ���� ���´�.
        }
        else
        {
            Debug.LogFormat("Connect : {0}", gameVersion);

            PhotonNetwork.GameVersion = gameVersion;
            // ����Ŭ���忡 ������ �����ϴ� ����
            // ���ӿ� �����ϸ� OnConnectedToMaster ��� �Լ��� �ڵ����� ȣ��ȴ�. �� �ݹ���.
            PhotonNetwork.ConnectUsingSettings(); 
        }
    }

    public void OnValueChangedNickName(string _nickName)
    {
        nickName = _nickName;
        PhotonNetwork.NickName = nickName;
    }

    public override void OnConnectedToMaster() // override = �θ� �������ִ°� ��ӹ޾Ƽ� ������  
    {
        // ����� ���Դٴ°� ������ �ƴٴ¶�.
        Debug.LogFormat("Connected to Master: {0}", nickName);
        connectButton.interactable = false; // Ŀ��Ʈ ��ư�� ��Ȱ��ȭ ��Ű��
        PhotonNetwork.JoinRandomRoom(); // ���� ã�°�.
        // ���⵵ join room �� �ִ� ������ ������ �������� �ٷ� ����ã�� ������ ������ ������ ������ �� ���� ã�� ���ؼ���.
    }

    public override void OnDisconnected(DisconnectCause _cause)
    {
        Debug.LogWarningFormat("Disconnected: {0}", _cause);
        connectButton.interactable = true;

        Debug.Log("Create Room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom });
    }

    public override void OnJoinedRoom()
    {
        // ���⼭ ����濡 ������ �˾ƺ��°Ű� �����ϸ� �Ʒ� OnJoinRandomFailed�� ��.
        Debug.Log("Joined Room");

        // PhotonNetwork.LoadLevel("Room"); <-- �̰� ���� ������� ���ÿ� ���� �ٲ�°��̴�.
        SceneManager.LoadScene("Room"); // LoadScene�� �Ϸ��� Build Settings���� Add Open Scene�� �ҷ�������� Scene�� �־�����Ѵ�.
        // �׸��� SceneManager�� ��������� using UnityEngine.SceneManagement�� �־����.
        // ������ �ٷ� ���� ��ȯ��Ű�� �ȵǰ� ������·� ���� �Ŀ� ������ ���۹�ư�� ������ ������ ������Ѵ�.
        // �� ��װ��� �κ� ���� ������� �� �ִ� ������ ����.
    }

    public override void OnJoinRandomFailed(short _returnCode, string _message)
    {
        // �濡 ���°� �����ϸ� ����� ����.
        Debug.LogErrorFormat("JoinRandomRailed({0}): {1}", _returnCode, _message); // ���ӽ��������� ���� �ѹ� ����

        connectButton.interactable = true; // Ŀ��Ʈ��ư �ٽ� Ȱ��ȭ��Ű��

        Debug.Log("Create Room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom }); // nnull �κ��� �������� �� �� ������ ������ Joinroom �ϴ°�.
        // RoomOptions�� Ŭ���ϰ� F12���������� ���� ������� �� �� ������ ������ ���� �ִ��ο����� �����ٰ��̴�.
        // ���� ��������� �ٽ� OnJoinRoom�� �����Ѵ�.
    }
}
