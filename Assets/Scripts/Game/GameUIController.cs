using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameObject PlayerATurnPanel;
    [SerializeField] private GameObject PlayerBTurnPanel;

    [SerializeField] private GameObject AILoadingPanel;
    
    [SerializeField] private TMP_Text TopText;

    public void Start()
    {
        if (TopText == null)
        {
            return;
        }

        switch(GameManager.Instance._gameType)
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
}
