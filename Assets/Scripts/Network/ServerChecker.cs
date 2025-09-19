using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ServerChecker : MonoBehaviour
{
    [Header("서버 URL")]
    public string serverUrl = "https://gomokuprojectserver.onrender.com/ping";
   
    [Header("UI")]
    public TMP_Text statusText; // 대기중/접속가능 표시
    public Image readyIcon;

    public Button multiPlayBtn;
    public Sprite orangeBtnSprite;

    [Header("설정")]
    public float retryInterval = 5f; // 슬립 깨우기 재시도 간격

    private bool isServerReady = false;

    void Start()
    {
        if (statusText != null)
        {
            statusText.text = "서버 접속 대기 중...";
            readyIcon.color = Color.yellow;
        }
            

        StartCoroutine(CheckServerStatus());
    }

    IEnumerator CheckServerStatus()
    {
        while (!isServerReady)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(serverUrl))
            {
                www.redirectLimit = 5; // 혹시 모를 리다이렉트 처리
                yield return www.SendWebRequest();

                // 성공 여부 확인
                bool success = (www.result == UnityWebRequest.Result.Success);

                if (success)
                {
                    //Debug.Log("서버 응답: " + www.downloadHandler.text);
                    isServerReady = true;

                    if (statusText != null)
                    {
                        statusText.text = "서버 준비 완료! 접속 가능";
                        readyIcon.color = Color.green;
                    }
                    EnableNetworkFeatures();
                }
                else
                {
                    //Debug.LogWarning($"서버 확인 실패: {www.responseCode}, {www.error}");
                    if (statusText != null)
                    {
                        statusText.text = $"서버 깨우는 중... 잠시만 기다려주세요. ({www.error})";
                        readyIcon.color = Color.red;
                    }

                    // 재시도 대기
                    yield return new WaitForSeconds(retryInterval);
                }
            }
        }
    }

    void EnableNetworkFeatures()
    {
        // Debug.Log("네트워크 기능 활성화됨.");
        //TODO 버튼 활성화 / 자동로그인

        multiPlayBtn.interactable = true;
        multiPlayBtn.GetComponent<Image>().sprite = orangeBtnSprite;
    }
}
