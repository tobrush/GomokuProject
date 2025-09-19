using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordListUI : MonoBehaviour
{
    [SerializeField] private RecordManager recordManager;
    [SerializeField] private BlockController blockController;

    public GameObject recordButtonPrefab; // 버튼 프리팹
    public Transform contentParent;       // ScrollView Content
    public ScrollRect scrollRect;         // ScrollView
    public Button leftButton;             // < 버튼
    public Button rightButton;            // > 버튼

    public Button firstButton;     // 처음으로
    public Button prevButton;      // 이전 수
    public Button nextButton;      // 다음 수
    public Button lastButton;      // 마지막 수
    public TMP_Text moveCounterText; // 중앙 텍스트 (예: 13/14)

    private GameRecord currentRecord;
    private int currentMoveIndex = -1; // -1이면 아무 돌도 없음

    private string selectedFileName = null;
    private GameObject selectedButtonObj = null;
    public Button deleteButton; // 화면 하단 중앙에 있는 삭제 버튼


    public float scrollStep = 0.2f;       // 버튼 클릭 시 이동 비율
    public float moveDuration = 0.3f;     // 이동 시간

    private List<string> fileNames;

    private bool isMoving = false;        // 중복 이동 방지

    void Start()
    {
      
        PopulateList();

        // 버튼 이벤트 등록
        leftButton.onClick.AddListener(ScrollLeft);
        rightButton.onClick.AddListener(ScrollRight);


        firstButton.onClick.AddListener(() => {
            currentMoveIndex = 0;
            ShowMovesUpTo(currentMoveIndex);
        });

        prevButton.onClick.AddListener(() => {
            if (currentMoveIndex > 0)
            {
                currentMoveIndex--;
                ShowMovesUpTo(currentMoveIndex);
            }
        });

        nextButton.onClick.AddListener(() => {
            if (currentMoveIndex < currentRecord.moves.Count)
            {
                currentMoveIndex++;
                ShowMovesUpTo(currentMoveIndex);
            }
        });

        lastButton.onClick.AddListener(() => {
            currentMoveIndex = currentRecord.moves.Count;
            ShowMovesUpTo(currentMoveIndex);
        });

        moveCounterText.text = "0/0"; // 초기 텍스트


        UpdateButtonVisibility();

        deleteButton.gameObject.SetActive(false); // 처음엔 숨김
        deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

    private void OnDeleteButtonClicked()
    {
        if (string.IsNullOrEmpty(selectedFileName)) return;

        bool success = recordManager.DeleteRecord(selectedFileName);
        if (success)
        {
            // 버튼 제거
            Destroy(selectedButtonObj);

            // 보드 초기화
            blockController.ClearBoard();

            // 선택 초기화
            selectedFileName = null;
            selectedButtonObj = null;

            // 삭제 버튼 숨기기
            deleteButton.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        UpdateButtonVisibility();
    }


    void PopulateList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        fileNames = recordManager.GetAllRecordFileNames();
        int index = 1;
        foreach (string fileName in fileNames)
        {
            GameObject btnObj = Instantiate(recordButtonPrefab, contentParent);
            btnObj.GetComponentInChildren<TMP_Text>().text = "<Size=50><Color=Yellow>" + index + "</Size></Color>\n" + FormatFileName(fileName);
            index++;
            string capturedName = fileName; // 클로저 방지
            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnRecordSelected(capturedName, btnObj);
            });
        }
        scrollRect.horizontalNormalizedPosition = 1f;
    }

    private string FormatFileName(string fileName)
    {
        // 파일명이 "yyyyMMdd_HHmmss" 형태일 경우
        if (DateTime.TryParseExact(
            fileName,
            "yyyyMMdd_HHmmss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime dateTime))
        {
            // 원하는 출력 형식 (예: 2025년 9월 18일 16시 55분)
            return dateTime.ToString("yyyy년\nM월 d일\nHH시 mm분");
        }

        // 혹시 파싱 실패하면 원래 문자열 리턴
        return fileName;
    }


    void OnRecordSelected(string fileName, GameObject buttonObj)
    {
        selectedFileName = fileName;
        selectedButtonObj = buttonObj;

        Debug.Log("기보 선택됨: " + fileName);

        // 실제 기보 불러오기
        currentRecord = recordManager.LoadRecord(fileName);


        if (currentRecord != null)
        {
            currentMoveIndex = currentRecord.moves.Count;
            ShowMovesUpTo(currentMoveIndex);
        }
        deleteButton.gameObject.SetActive(true);
    }

    void ShowMovesUpTo(int moveCount)
    {
        blockController.ClearBoard(); // 기존 돌 초기화

        for (int i = 0; i < moveCount && i < currentRecord.moves.Count; i++)
        {
            var move = currentRecord.moves[i];
            Block.MarkerType marker = move.player == 1
                ? Block.MarkerType.BlackStone
                : Block.MarkerType.WhiteStone;

            blockController.PlaceMaker(marker, move.y, move.x);

            int blockIndex = move.y * Constants.BlockColumnCount + move.x;
            Block block = blockController.GetBlock(blockIndex);
            if (block != null)
            {
                block.SetOrderNumber(move.order);
            }
        }

        moveCounterText.text = $"{moveCount}/{currentRecord.moves.Count}";
    }





    void ScrollLeft()
    {
        if (!isMoving)
            StartCoroutine(SmoothScroll(-scrollStep));
    }

    void ScrollRight()
    {
        if (!isMoving)
            StartCoroutine(SmoothScroll(scrollStep));
    }

    private IEnumerator SmoothScroll(float step)
    {
        isMoving = true;

        float start = scrollRect.horizontalNormalizedPosition;
        float target = Mathf.Clamp01(start + step);

        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float lerp = t / moveDuration;
            // 부드럽게 가속/감속
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, lerp));
            yield return null;
        }

        scrollRect.horizontalNormalizedPosition = target;
        isMoving = false;
    }


    void UpdateButtonVisibility()
    {
        float contentWidth = scrollRect.content.rect.width;
        float viewportWidth = scrollRect.viewport.rect.width;

        if (contentWidth <= viewportWidth)
        {
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
            return;
        }

        leftButton.gameObject.SetActive(scrollRect.horizontalNormalizedPosition > 0.01f);
        rightButton.gameObject.SetActive(scrollRect.horizontalNormalizedPosition < 0.99f);
    }

}
