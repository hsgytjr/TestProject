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

    // 네트워크에서 공유가 될거면 모든걸 프리팹으로 만들어줘야한다.
    // 네트워크에서 공유해줄 필요가 없는 배경같은건 프리팹으로 만들지 않아도 된다.
    //private HpBarHolder hpbarHolder = null;
    
    private List<GameObject> playerGoList = new List<GameObject>();

    private void Start()
    {
        if (playerPrefab != null)
        {
            GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3 // 프리팹을 던지는게 아니라 프리팹의 이름을 던져줘야함. 
                // playerPerfab.name 부분에 "P_Player" 이렇게 적어도 된다.
                (Random.Range(-10.0f, 10.0f), 0.0f, Random.Range(-10.0f, 10.0f)), Quaternion.identity, 0);
            // Instantiate를 유니티에 있는걸 쓰는게 아니라 PhotonNetwork에 있는 Instantiate를 사용해야한다.
            // Photon의 Instantiate를 사용하면 내가 따로 다른캐릭터들에 대한 정보를 가지고있지 않아도 서버에서 기억하고있는 정보를 토대로 다같이 만들어준다.
            // 이렇게하면 상대캐릭터든 내캐릭터든 같은 player 스크립트를 가지고있는 캐릭터기때문에 구분을 할 수 없게된다.
            // 따라서 내가 조종할 캐릭터는 이것입니다~ 하고 코드로 알려줘야한다.
            go.GetComponent<PlayerCtrl>().SetMaterial(PhotonNetwork.CurrentRoom.PlayerCount); // 플레이어의 색을 바꿔주는 코드.
            // 접속한 방에 접속한 숫자에 따라서 캐릭터에 색을 입혀주는 방식이다.
            // 문제는 내 색만 바꿔주고 있다는 사실이다. 이걸 어떻게 해결할지 생각해봐야함.
            GameObject hpGo = Instantiate(hpBarHolderPrefab, go.transform);
            hpGo.transform.position = go.transform.position;
        }
    }

    // PhotonNetwork.LeaveRoom 함수가 호출되면 OnLeftRoom함수 호출
    public override void OnLeftRoom()
    {
        Debug.Log("Left Room");
        SceneManager.LoadScene("Launcher"); // 다시 원래 씬으로 돌아가는것.
    }

    public override void OnPlayerEnteredRoom(Player _otherPlayer) // 어떤 플레이어가 방에 들어오면 호출된다.
    {
        // 모든 클라이언트가 호출받는 함수.
        Debug.LogFormat("Player Entered Room: {0}", _otherPlayer.NickName); // 들어온사람의 닉네임을 적어서 누가 들어왔는지를 알려줌.

        // photonView는 따로 변수를 지정하지 않아도 사용할 수 있다. transform같은애임.
        // photon의 상태를 지켜보고 통제하는애임.
        photonView.RPC("ApplyPlayerList", RpcTarget.All); // RPC(Remote Procedure Call) : 원격으로 다른 클라이언트의 함수를 내가 호출하는것.
        // 새로운 플레이어가 들어왔으니 플레이어목록을 다시 갱신하라는 함수를 호출한다. 누구한테? 모든 플레이어한테.
        // 여기서 All은 나또한 포함된다. 마스터만 호출하는 MasterClient, 마스터제외 나머지를 호출하는 Others 가 있다.
        // ㄴ 이거로 클라이언트방식 p2p방식 이 나눠지는것이다.
        // RPC를 사용하면 위에 플레이어별 색 바꾸는것도 적용시킬 수 있다.
    }

    [PunRPC] // RPC로 호출할 함수들은 앞에 [PunRPC]를 붙혀줘야한다.
    public void RPCApplyPlayerList()
    {
        int playerCnt = PhotonNetwork.CurrentRoom.PlayerCount;
        // 플레이어 리스트가 최신이라면 건너뜀
        if (playerCnt == playerGoList.Count) return;

        // 현재 방에 접속해 있는 플레이어의 수
        Debug.LogError("CurrentRoom PlayerCount : " + playerCnt);

        // 현재 생성되어 있는 모든 포톤뷰 가져오기
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>(); // PhotonView 라는 컴포넌트가 붙어있는 모든 오브젝트를 다가지고옴.
        // Find는 원래 쓰면 안좋은데 수업상 어쩔수없이 사용함.

        // 매번 재정렬을 하는게 좋으므로 플레이어 게임오브젝트 리스트를 초기화
        // System.Array.Clear(playerGoList, 0, playerGoList.Legnth); playerGoList가 배열이라면 이 방식으로 초기화.
        playerGoList.Clear(); /// playerGoList가 리스트니까 이 방식으로 초기화.
        // 현재 생성되어 있는 포톤뷰 전체와
        // 접속중인 플레이어들의 액터넘버를 비교해,
        // 플레이어 게임오브젝트 리스트에 추가
        for (int i = 0; i < playerCnt; ++i)
        {
            // 키는 0이 아닌 1부터 시작
            int key = i + 1; // dictionary에서 사용하던 키와 같은역할.
            for (int j = 0; j < photonViews.Length; ++j)
            {
                // 만약 PhotonNetwork.Instantiate를 통해서 생성된 포톤뷰가 아니라면 넘김
                if (photonViews[j].isRuntimeInstantiated == false) continue;
                // 만약 현재 키 값이 딕셔너리 내에 존재하지 않는다면 넘김
                if (PhotonNetwork.CurrentRoom.Players.ContainsKey(key) == false) continue;
                // ContainsKey = Dictionary처럼 해당 키를 가지고있느냐 를 찾아보는 함수.
                // 그럼 키값은 언제넣어줬느냐? Players 변수위에 마우스를 올려보면 Dictionary형식으로 되어있는걸 알 수 있다.
                // 자동으로 key값을 ++1씩 할당해주는것이다.

                // 포톤뷰의 액터넘버
                // 엑터넘버란? 해당 오브젝트에 번호를 붙혀주는것. Dictionary의 key값처럼 오브젝트마다 번호를 붙혀주고 해당 번호로 오브젝트를 관리한다.
                int viewNum = photonViews[j].Owner.ActorNumber;

                // 접속중인 플레이어의 액터넘버
                // 위 엑터넘버들 중 플레이어들의 엑터넘버만 따로 저장한다. 이걸 따로 저장해두면 플레이어가 몇명인지 누가 몇번인지를 알 수 있고 관리할 수 있다.
                // 클라이언트 내에선 오브젝트가 만들어지는 순서대로 오브젝트의 번호가 정해질텐데 위처럼 엑터넘버를 정해두고, 서버에서 위 엑터넘버대로 오브젝트를 정렬시키면
                // 모든 클라이언트에서 오브젝트들이 같은 순서대로 나열된다.
                int playerNum = PhotonNetwork.CurrentRoom.Players[key].ActorNumber;

                // 액터넘버가 같은 오브젝트가 있다면,
                if (viewNum == playerNum)
                {
                    // 게임오브젝트 이름도 알아보기 쉽게 변경
                    photonViews[j].gameObject.name = "Player_" + photonViews[j].Owner.NickName;
                    // 실제 게임오브젝트를 리스트에 추가
                    playerGoList.Add(photonViews[j].gameObject);
                }
            }
        }

        // 디버그용
        PrintPlayerList();
    }

    private void PrintPlayerList()
    {
        foreach (GameObject go in playerGoList)
        {
            if (go != null)
            {
                Debug.LogError(go.name); // 여기서 logError인 이유는 에러가 나서가 아니라 빌드한 이후 로그를 보려면 Error를 붙혀서 UI에 띄워줘야하기 때문이다.
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
        PhotonNetwork.LeaveRoom(); // 방 나갈땐 LeaveRoom을 호출한다는 것만 알면 된다.
    }
}
