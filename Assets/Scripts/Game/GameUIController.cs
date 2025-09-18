using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameObject PlayerATurnPanel;
    [SerializeField] private GameObject PlayerBTurnPanel;

    [SerializeField] private GameObject AILoadingPanel;
    
    [SerializeField] private TMP_Text TopText;

    [SerializeField] private TMP_Text turnTimerText;
    [SerializeField] private Slider turnTimerSlider;
    private float maxTurnTime;

    public void Start()
    {
        if (TopText == null)
        {
            return;
        }

        switch (GameManager.Instance._gameType)
        {
            case Constants.GameType.SinglePlay:
                TopText.text = "싱글 플레이";
                break;
            case Constants.GameType.DualPlay:
                TopText.text = "듀얼 플레이";
                break;
            case Constants.GameType.MultiPlay:
                TopText.text = "멀티 플레이";
                break;
            case Constants.GameType.Record:
                TopText.text = "기보";
                break;

        }

        //TopText.text = GameManager.Instance._gameType.ToString();
    }


    public void SetAILoading(bool isActive)
    {
        AILoadingPanel.SetActive(isActive);
    }
    
    public enum GameTurnPanelType { None, ATurn, BTurn }

    public void OnClickBackBtn()
    {
        GameManager.Instance.OpenConfirmPanel(message: "게임을 종료하시겠습니까?", onConfirmButtonClicked: () =>
        {
            GameManager.Instance.ChangeToMainScene();
        });

    }
    public void OnClickBackNotGameBtn()
    {
        GameManager.Instance.OpenConfirmPanel(message: "기보 보기를 종료하시겠습니까?", onConfirmButtonClicked: () =>
        {
            GameManager.Instance.ChangeToMainScene();
        });

    }

    public void SetGameTurnPanel(GameTurnPanelType gameTurnPanelType)
    {
        switch (gameTurnPanelType)
        {
            case GameTurnPanelType.None:
                PlayerATurnPanel.SetActive(false);
                PlayerBTurnPanel.SetActive(false);
                break;
            case GameTurnPanelType.ATurn:
                PlayerATurnPanel.SetActive(true);
                PlayerBTurnPanel.SetActive(false);
                break;
            case GameTurnPanelType.BTurn:
                PlayerATurnPanel.SetActive(false);
                PlayerBTurnPanel.SetActive(true);
                break;
        }
    }

    public void OnClickConfirmMove()
    {
        if (GameManager.Instance != null)
        {
            var gameLogic = typeof(GameManager)
                .GetField("_gameLogic", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(GameManager.Instance) as GameLogic;

            if (gameLogic != null)
            {
                if (gameLogic.GetCurrentState() is PlayerState playerState)
                {
                    playerState.ConfirmMove(gameLogic);
                }
            }
        }
    }

    public void SetTurnTimer(float time)
    {
        if (turnTimerText != null)
        {
            turnTimerText.text = $"{Mathf.CeilToInt(time)}s";
        }

        if (turnTimerSlider != null)
        {
            turnTimerSlider.value = time; // 슬라이더 값 반영
        }
    }

    // 제한시간 초기화 시 호출
    public void InitTurnTimer(float maxTime)
    {
        maxTurnTime = maxTime;
        if (turnTimerSlider != null)
        {
            turnTimerSlider.maxValue = maxTime;
            turnTimerSlider.value = maxTime;
        }
    }
}
