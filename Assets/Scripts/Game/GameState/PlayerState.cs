using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerState : BasePlayerState
{
    private bool _isFirstPlayer;
    public bool IsFirstPlayer => _isFirstPlayer;

    private Constants.PlayerType _playerType;

    private MultiplayController _multiplayController;
    private string _roomId;
    private bool _isMultiplay;

    // 착수 대기 좌표
    private int? pendingRow = null;
    private int? pendingCol = null;

    // 마지막 임시 표시된 블록
    private Block lastPreviewBlock = null;


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
        if (_isFirstPlayer)
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.ATurn);
        else
            GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn);

        // 공통: 블록 클릭 → 임시 착수 위치 저장
        gameLogic.BlockController.OnBlockClickedDelegate = (row, col) =>
        {
            pendingRow = row;
            pendingCol = col;

            // 기존 임시 돌 지우기
            if (lastPreviewBlock != null)
                lastPreviewBlock.SetMarker(Block.MarkerType.None);

            // 새 위치에 임시 돌 표시
            gameLogic.BlockController.PlaceMaker( Block.MarkerType.Aim ,row, col);

            // 현재 표시된 블록 저장
            int blockIndex = row * Constants.BlockColumnCount + col;
            lastPreviewBlock = gameLogic.BlockController.GetBlock(blockIndex);
        };
    }

    public override void OnExit(GameLogic gameLogic)
    {
        gameLogic.BlockController.OnBlockClickedDelegate = null;
    }

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        // 버튼 방식 → 여기서는 사용하지 않음
    }

    // 착수 버튼 눌렀을 때 최종 확정
    public void ConfirmMove(GameLogic gameLogic)
    {
        if (pendingRow.HasValue && pendingCol.HasValue)
        {
            ProcessMove(gameLogic, _playerType, pendingRow.Value, pendingCol.Value);

            if (_isMultiplay)
            {
                _multiplayController.DoPlayer(
                    _roomId, pendingRow.Value * Constants.BlockColumnCount + pendingCol.Value
                );
            }

            pendingRow = null;
            pendingCol = null;
            lastPreviewBlock = null; // 임시 돌 초기화
        }
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        if (_isFirstPlayer)
            gameLogic.SetState(gameLogic.secondPlayerState);
        else
            gameLogic.SetState(gameLogic.firstPlayerState);
    }

    #endregion
}
