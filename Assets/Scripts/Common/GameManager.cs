using System.Collections;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Constants;

public class GameManager : Singleton<GameManager>
{
    public Constants.GameType _gameType;
    [SerializeField] public GameObject SignUpBtn;
    [SerializeField] public GameObject SignInBtn;
    [SerializeField] public GameObject SignOutBtn;
    [SerializeField] public GameObject multiPlayBtn;

    [SerializeField] private GameObject confrimPanel;
    [SerializeField] private GameObject signinPanel;
    [SerializeField] private GameObject signupPanel;

    private Canvas _canvas;

    private ConfirmPanelController _confirmPanelController;

    private GameLogic _gameLogic;

    private GameUIController _gameUIController;

    private void OnEnable()
    {
        // 로그인
        var sid = PlayerPrefs.GetString("sid");
        Debug.Log("SID: " + sid);

        if (string.IsNullOrEmpty(sid))
        {
           // multiPlayBtn.GetComponent<Button>().interactable = false;
        }
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

    public void ChangeToGameScene(Constants.GameType gameType)
    {
        _gameType = gameType;

        SceneManager.LoadScene("Game");

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

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
      _canvas = FindFirstObjectByType<Canvas>();

        if(scene.name == "Game")
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

}



