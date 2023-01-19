using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.Demo.Cockpit;
using UnityEngine.SceneManagement;

public class PhotonLauncher : MonoBehaviourPunCallbacks // Pun에서 사용하고있는 Callback들이 모여있는 MonoBehaviour
{
    [SerializeField] private string gameVersion = "0.0.1";      // 포톤의 버전
    [SerializeField] private byte maxPlayerPerRoom = 4;         // 방당 최대 몇명까지 들어갈수있는지 정하기
    [SerializeField] private string nickName = string.Empty;    // 서버에 접속할때 사용할 닉네임
    [SerializeField] private Button connectButton = null;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // 포톤 네트워크에 자동으로 씬이랑 씽크를 맞추는것.
        // 방장과 다른사람들이 동시에 씬을 동기화시켜서 게임을 시작하기위해서 필요한 코드
    }

    private void Start()
    {
        connectButton.interactable = true; // 시작하는 버튼을 시작하면서 활성화시켜주는것. 별거없음
    }

    public void Connect() // 버튼과 연결되어있는 함수
    {
        if (string.IsNullOrEmpty(nickName)) // 우선 닉네임이 널이나 비어있거나 를 체크하는것
        {
            Debug.Log("NickName is Empty!"); // 원래는 이거 UI로 표시되게 해야함.
            return;
        }

        if (PhotonNetwork.IsConnected) // 내가 지금 서버에 들어와있는 상태인지 아닌지를 체크함.
        {
            PhotonNetwork.JoinRandomRoom(); // 서버에 들어와있으면 무작위 방을 찾아서 들어가라 라는것.
            // PhotonNetwork.room 까지 쳐보면 방에 대한 코드들이 많이 나온다.
            // PhotonNetwork.Lobby 로 로비관련된 내용도 많이 나온다.
        }
        else
        {
            Debug.LogFormat("Connect : {0}", gameVersion);

            PhotonNetwork.GameVersion = gameVersion;
            // 포톤클라우드에 접속을 시작하는 지점
            // 접속에 성공하면 OnConnectedToMaster 라는 함수가 자동으로 호출된다. 즉 콜백임.
            PhotonNetwork.ConnectUsingSettings(); 
        }
    }

    public void OnValueChangedNickName(string _nickName)
    {
        nickName = _nickName;
        PhotonNetwork.NickName = nickName;
    }

    public override void OnConnectedToMaster() // override = 부모가 가지고있는걸 상속받아서 재정의  
    {
        // 여기로 들어왔다는건 접속이 됐다는뜻.
        Debug.LogFormat("Connected to Master: {0}", nickName);
        connectButton.interactable = false; // 커넥트 버튼을 비활성화 시키고
        PhotonNetwork.JoinRandomRoom(); // 방을 찾는것.
        // 여기도 join room 이 있는 이유는 접속이 돼있으면 바로 방을찾고 접속이 돼있지 않으면 접속한 후 방을 찾기 위해서다.
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
        // 여기서 어느방에 들어갈지를 알아보는거고 실패하면 아래 OnJoinRandomFailed로 들어감.
        Debug.Log("Joined Room");

        // PhotonNetwork.LoadLevel("Room"); <-- 이걸 쓰면 모든사람이 동시에 씬이 바뀌는것이다.
        SceneManager.LoadScene("Room"); // LoadScene을 하려면 Build Settings에서 Add Open Scene에 불러오고싶은 Scene을 넣어줘야한다.
        // 그리고 SceneManager를 쓰고싶으면 using UnityEngine.SceneManagement가 있어야함.
        // 원래는 바로 씬을 전환시키면 안되고 레디상태로 만든 후에 방장이 시작버튼을 누르는 식으로 해줘야한다.
        // 단 배그같이 로비에 들어가서 놀고있을 수 있는 게임은 별개.
    }

    public override void OnJoinRandomFailed(short _returnCode, string _message)
    {
        // 방에 들어가는거 실패하면 여기로 들어옴.
        Debug.LogErrorFormat("JoinRandomRailed({0}): {1}", _returnCode, _message); // 접속실패했으니 에러 한번 띄우고

        connectButton.interactable = true; // 커넥트버튼 다시 활성화시키고

        Debug.Log("Create Room");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom }); // nnull 부분이 방제목임 저 방 제목을 가지고 Joinroom 하는것.
        // RoomOptions를 클릭하고 F12번을누르면 각종 내용들을 볼 수 있지만 당장은 방의 최대인원수만 정해줄것이다.
        // 방을 만들었으면 다시 OnJoinRoom을 실행한다.
    }
}
