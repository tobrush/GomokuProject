using UnityEngine;

public class MultiPlayerState : BasePlayerState
{
    private Constants.PlayerType _playerType;
    private bool _isFirstPlayer;
    private MultiplayController _multiplayController;
    public bool IsFirstPlayer { get; private set; }
    public MultiPlayerState(bool isFirstPlayer, MultiplayController multiplayController)
    {
        _isFirstPlayer = isFirstPlayer;
        _multiplayController = multiplayController;
        _playerType = _isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        if (_playerType == Constants.PlayerType.PlayerA && GameAI.IsBanBlock(_playerType, row, col, gameLogic.GetBoard()))
        {
            Debug.LogWarning($"[MultiPlay] 금수 착수 시도 감지: ({row},{col})");
            return; // 무시
        }

        ProcessMove(gameLogic, _playerType, row, col);
    }

    public override void OnEnter(GameLogic gameLogic)
    {

        _multiplayController.onBlockDataChanged = blockIndex =>
        {
            var row = blockIndex / Constants.BlockColumnCount;
            var col = blockIndex % Constants.BlockColumnCount;
            UnityThread.executeInUpdate(() =>
            {
                HandleMove(gameLogic, row, col);
            });

        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        _multiplayController.onBlockDataChanged = null;
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
}
