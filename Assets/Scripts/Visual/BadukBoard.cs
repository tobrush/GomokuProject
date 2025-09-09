using System.Collections;
using UnityEngine;
using DG.Tweening;

public class BadukBoard : MonoBehaviour
{
    public int size = 19;              // 바둑판 격자 수 (19x19)
    public float spacing = 1f;         // 칸 간격
    public Material lineMaterial;      // Unlit/Color 같은 단색 머티리얼 권장
    public Material effectMaterial;      // Unlit/Color 같은 단색 머티리얼 권장
    public float pingPongDuration = 0.5f;

    private bool _effectLine;
    void Start()
    {
        DrawBoard();
    }

    void DrawBoard()
    {
        float half = (size - 1) * spacing * 0.5f; // 보드의 절반 길이 (offset)

        for (int i = 0; i < size; i++)
        {

            // 수평선: X축으로 뻗고 Y는 위아래로 배치
            Vector3 hCenter = new Vector3(0f, (i * spacing) - half, 0f);
            Vector3 hDir = new Vector3((size - 1) * spacing, 0f, 0f);
            CreateLine(hCenter, hDir, (size - 1) * spacing, 0.025f, lineMaterial, false);
            CreateLine(hCenter, hDir, (size - 1) * spacing, 0.05f, effectMaterial, true);

            // 수직선: Y축으로 뻗고 X는 좌우로 배치
            Vector3 vCenter = new Vector3((i * spacing) - half, 0f, 0f);
            Vector3 vDir = new Vector3(0f, (size - 1) * spacing, 0f);
            CreateLine(vCenter, vDir, (size - 1) * spacing, 0.025f, lineMaterial, false);
            CreateLine(vCenter, vDir, (size - 1) * spacing, 0.05f, effectMaterial, true);
        }
    }

    void CreateLine(Vector3 centerLocalPos, Vector3 dir, float length, float width, Material newMaterial, bool effectLine)
    {
        GameObject lineObj = new GameObject("Line");
        lineObj.transform.parent = transform;
        lineObj.transform.localPosition = centerLocalPos; // 부모(보드)의 중심 기준 위치
        if (effectLine)
        {
            lineObj.transform.localPosition += new Vector3(0, 0, -0.002f); // 카메라 기준 살짝 앞으로
        }
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.material = new Material(newMaterial); // 인스턴스 만들어서 각 라인 독립 색상 가능
        lr.startWidth = width;  // 굵기 절반
        lr.endWidth = width;
        lr.useWorldSpace = false; // 로컬 좌표 기준

        // 로컬에서 양쪽으로 절반씩 뻗게 설정
        Vector3 localDir = dir.normalized * (length * 0.5f);
        lr.SetPosition(0, -localDir);
        lr.SetPosition(1, localDir);

        lr.sortingLayerName = "Default"; // 또는 원하는 Sorting Layer
        lr.sortingOrder = effectLine ? 1 : 0;

        if (effectLine)
        {
            // DOTween으로 material.color PingPong
            lr.material.color = Color.black;
            DOTween.To(() => lr.material.color, x => lr.material.color = x, Color.white, pingPongDuration)
                   .SetLoops(-1, LoopType.Yoyo)
                   .SetEase(Ease.Linear);

            lr.material.EnableKeyword("_EMISSION"); // 에미션 사용 활성화
            Color startEmission = Color.black;
            Color endEmission = Color.white;

            lr.material.SetColor("_EmissionColor", startEmission);

            DOTween.To(
                () => lr.material.GetColor("_EmissionColor"),
                x => lr.material.SetColor("_EmissionColor", x),
                endEmission,
                pingPongDuration
            )
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
        }
        
    }
}
