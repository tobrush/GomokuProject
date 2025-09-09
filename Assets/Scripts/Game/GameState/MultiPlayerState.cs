using UnityEngine;

public class MultiPlayerState : BasePlayerState
{
    private Constants.PlayerType _playerType;
    private bool _isFirstPlayer;
    private MultiplayController _multiplayController;

    public MultiPlayerState(bool isFirstPlayer, MultiplayController multiplayController)
    {
        _isFirstPlayer = isFirstPlayer;
        _multiplayController = multiplayController;
        _playerType = _isFirstPlayer ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        ProcessMove(gameLogic, _playerType, row, col);
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        Debug.Log($"[MultiPlayerState] OnEnter called, isFirst:{_isFirstPlayer}");

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
