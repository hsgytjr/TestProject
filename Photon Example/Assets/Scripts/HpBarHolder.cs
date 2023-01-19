using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBarHolder : MonoBehaviour
{
    [SerializeField] private RectTransform frontRt = null;

    private RectTransform rt = null;
    private float oriSizeX = 0f; // 처음 사이즈

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        oriSizeX = frontRt.sizeDelta.x;
    }

    // 월드좌표를 스크린좌표로 변환을 해줘야한다.
    public void SetPosition(Vector3 _worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_worldPos);
        screenPos.y += 60f; // 타겟의 위치로 가긴 하는데 타겟 손잡이부분에 체력바가 박혀서 머리위로 올려줄거임.
        rt.position = screenPos;
    }

    public void UpdateHpBar(int _maxHp, int _curHp) // 최대체력과 현재체력력을 받음
    {
        // 0 ~ 1
        float percentage = (float)_curHp / _maxHp; // 둘다 정수니까 실수로 변형시켜줘야한다.

        // 현재체력을 전체체력으로 나눠주면 남은 체력의 퍼센테이지가 나오는데 그 퍼센테이지만큼 체력바를 깎아줘야한다.
        Vector2 newSize = frontRt.sizeDelta;
        newSize.x = oriSizeX * percentage;
        frontRt.sizeDelta = newSize;
    }
}
