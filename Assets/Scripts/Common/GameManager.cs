using System.Collections;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Constants;
using static NetworkManager;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public MyData myData;

    public Constants.GameType _gameType;

    [SerializeField] private GameObject confrimPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;

    private Canvas _canvas;

    private ConfirmPanelController _confirmPanelController;

    private GameLogic _gameLogic;

    private GameUIController _gameUIController;

    public bool IsPaused { get; private set; }
    public void PauseGame()
    {
        IsPaused = true;
    }

    public void ResumeGame()
    {
        IsPaused = false;
    }

    public void OpenSigninPanel()
    {
        if (_canvas != null)
        {
            var signinPanelObject = Instantiate(signinPanel, _canvas.transform);
            signinPanelObject.GetComponent<SigninPanelController>().Show();
        }
    }
    public void OpenSignupPanel()
    {
        if (_canvas != null)
        {
            var signupPanelObject = Instantiate(signupPanel, _canvas.transform);
            signupPanelObject.GetComponent<SignupPanelController>().Show();
        }
    }

    public void OpenSignOutPanel()
    {
        GameManager.Instance.OpenConfirmPanel(message: "로그아웃하시겠습니까?", onConfirmButtonClicked: () =>
        {
            StartCoroutine(NetworkManager.Instance.Signout(
                success: () =>
                {
                    Debug.Log("로그아웃 성공!");
                   // NetworkLoggingPanel.SetActive(true);
                   //  NetworkUserPanel.SetActive(false);
                   //  multiPlayBtn.interactable = false;
                   //  multiPlayBtn.GetComponent<Image>().sprite = multiPlayGray;
                },
                failure: (errorMsg) =>
                {
                   // Debug.LogError("로그아웃 실패: " + errorMsg);
                   
                }));
        });
    }

    public void ChangeToGameScene(Constants.GameType gameType)
    {
        _gameType = gameType;

        SceneManager.LoadScene("Game");

    }
    public void ChangeToRecordScene()
    {
        _gameType = Constants.GameType.Record;
        SceneManager.LoadScene("Record");

    }


    public void SetAILoading(bool isActive)
    {
        _gameUIController.SetAILoading(isActive);
    }


    public void ChangeToMainScene()
    {
        _gameLogic?.Dispose();
        _gameLogic = null;
        SceneManager.LoadScene("Main");
    }

    public void OpenConfirmPanel(string message, ConfirmPanelController.OnConfirmButtonClicked onConfirmButtonClicked)
    {
        if (_canvas == null) return;

        if (_confirmPanelController == null)
        {
            // 없으면 새로 생성
            var confirmPanelObject = Instantiate(confrimPanel, _canvas.transform);
            _confirmPanelController = confirmPanelObject.GetComponent<ConfirmPanelController>();
        }
        _confirmPanelController.Show(message, onConfirmButtonClicked);

    }


    public void UpdateUserUI()
    {
        Debug.Log(myData.nickname);
       //  NetworkMyID.text = $"ID : {myData.nickname}";
       // NetworkMyInfo.text = $"Lv: {myData.level} | Score: {myData.score} | Coin: {myData.coin}";
       // multiPlayBtn.interactable = true;
       // multiPlayBtn.GetComponent<Image>().sprite = multiPlayOrange;

      //  NetworkLoggingPanel.SetActive(false);
      //  NetworkUserPanel.SetActive(true);
    }

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
      _canvas = FindFirstObjectByType<Canvas>();

        if (scene.name == "Game")
        {
            //block 초기화
            var blockController = FindFirstObjectByType<BlockController>();

            if (blockController != null)
            {
                blockController.InitBlocks();
            }

            _gameUIController = FindFirstObjectByType<GameUIController>();

            if(blockController != null)
            {
                _gameUIController.SetGameTurnPanel(GameUIController.GameTurnPanelType.None);
            }
    

            //GameLogic 생성
            if (_gameLogic != null)
            {
                // TODO: 기존 게임 로직을 소멸
            }
            _gameLogic = new GameLogic(blockController, _gameType);
        }

        
    }

    public void SetGameTurnPanel(GameUIController.GameTurnPanelType gameTurnPanelType)
    {
        _gameUIController.SetGameTurnPanel(gameTurnPanelType);
    }

    public void SetGameTurnTime(float time)
    {
        _gameUIController?.SetTurnTimer(time);
    }

    public void InitTurnTimerUI(float maxTime)
    {
        _gameUIController?.InitTurnTimer(maxTime);
    }


    public void SaveRecord(GameRecord record, string fileName)
    {
        string json = JsonUtility.ToJson(record, true);
        string path = Application.dataPath + "/GameRecords/" + fileName + ".json";
        System.IO.File.WriteAllText(path, json);
        Debug.Log("Saved to " + path);
    }
}



