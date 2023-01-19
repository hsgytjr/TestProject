using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBarHolder : MonoBehaviour
{
    [SerializeField] private RectTransform frontRt = null;

    private RectTransform rt = null;
    private float oriSizeX = 0f; // ó�� ������

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        oriSizeX = frontRt.sizeDelta.x;
    }

    // ������ǥ�� ��ũ����ǥ�� ��ȯ�� ������Ѵ�.
    public void SetPosition(Vector3 _worldPos)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_worldPos);
        screenPos.y += 60f; // Ÿ���� ��ġ�� ���� �ϴµ� Ÿ�� �����̺κп� ü�¹ٰ� ������ �Ӹ����� �÷��ٰ���.
        rt.position = screenPos;
    }

    public void UpdateHpBar(int _maxHp, int _curHp) // �ִ�ü�°� ����ü�·��� ����
    {
        // 0 ~ 1
        float percentage = (float)_curHp / _maxHp; // �Ѵ� �����ϱ� �Ǽ��� ������������Ѵ�.

        // ����ü���� ��üü������ �����ָ� ���� ü���� �ۼ��������� �����µ� �� �ۼ���������ŭ ü�¹ٸ� �������Ѵ�.
        Vector2 newSize = frontRt.sizeDelta;
        newSize.x = oriSizeX * percentage;
        frontRt.sizeDelta = newSize;
    }
}
