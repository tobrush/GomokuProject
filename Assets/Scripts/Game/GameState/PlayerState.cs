using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerState : BasePlayerState
{
    private bool _isFirstPlayer;
    private Constants.PlayerType _playerType;

    private MultiplayController _multiplayController;
    private string _roomId;
    private bool _isMultiplay;

    public PlayerState(bool isFirstPlayer)
    {
        _isFirstPlayer = isFirstPlayer;
        _playerType = _isFirstPlayer ?
            Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
        _isMultiplay = false;
    }
    public PlayerState(bool isFirstPlayer, MultiplayController multiplayController, string roomId)
      : this(isFirstPlayer)
    {
        _multiplayController = multiplayController;
        _roomId = roomId;
        _isMultiplay = true;
    }

    #region 필수메서드


    public override void OnEnter(GameLogic gameLogic)
    {
       // Debug.Log($"[PlayerState] OnEnter called, isFirst:{_isFirstPlayer}, isMultiplay:{_isMultiplay}");
     

        if (_isFirstPlayer)
        {
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn);
        }
        else
        {
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn);
        }

        gameLogic.BlockController.OnBlockClickedDelegate = (row, col) =>
        {
            HandleMove(gameLogic, row, col);
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.BlockController.OnBlockClickedDelegate = null;

    }
    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, _playerType, row, col);
        //Debug.Log($"[PlayerState] HandleMove called, row:{row}, col:{col}, isMultiplay:{_isMultiplay}, roomId:{_roomId}");
        if (_isMultiplay)   // 서버에 Marker 정보 전달
        {
            Debug.Log("[PlayerState] Sending DoPlayer to server");
            _multiplayController.DoPlayer(_roomId, row * Constants.BlockColumnCount + col);
        }
    }


    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
        {
            gameLogic.SetState(gameLogic.secondPlayerState);
        }
        else
        {
            gameLogic.SetState(gameLogic.firstPlayerState);
        }
    }

    #endregion
}
