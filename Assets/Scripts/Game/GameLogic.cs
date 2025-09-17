using UnityEngine;
using static Constants;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameLogic
{
    public BlockController BlockController;

    private Constants.PlayerType[,] _board;

    public BasePlayerState firstPlayerState; //A
    public BasePlayerState secondPlayerState; //B
    public enum GameResult { None, Win, Lose, Draw }

    private BasePlayerState _currentPlayerState; //현재 턴 플레이어


    private MultiplayController _multiplayController;
    private string _roomId;


    public Constants.PlayerType[,] GetBoard()
    {
        return _board;
    }


    public GameLogic(BlockController blockController, Constants.GameType gameType)
    {
        BlockController = blockController;

        _board = new Constants.PlayerType[Constants.BlockColumnCount, Constants.BlockColumnCount];

        // Game Type 초기화
        switch (gameType)
        {
            case Constants.GameType.SinglePlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new AIState();

                SetState(firstPlayerState);
                break;
            case Constants.GameType.DualPlay:
                firstPlayerState = new PlayerState(true);
                secondPlayerState = new PlayerState(false);
                // 게임 시작
                SetState(firstPlayerState);
                break;
            case Constants.GameType.MultiPlay:
                _multiplayController = new MultiplayController((state, roomId) =>
                {
                    _roomId = roomId;
                    Debug.Log(roomId + "/" + state);
                    switch (state)
                    {
                        case Constants.MultiplayControllerState.CreateRoom:
                            Debug.Log("## Create Room ##");
                            // TODO: 대기 화면 UI 표시
                            break;
                        case Constants.MultiplayControllerState.JoinRoom:
                            Debug.Log("## Join Room ##");
                            firstPlayerState = new MultiPlayerState(true, _multiplayController);
                            secondPlayerState = new PlayerState(false, _multiplayController, _roomId);
                            SetState(firstPlayerState);
                            break;
                        case Constants.MultiplayControllerState.StartGame:
                            Debug.Log("## Start Game ##");
                            firstPlayerState = new PlayerState(true, _multiplayController, _roomId);
                            secondPlayerState = new MultiPlayerState(false, _multiplayController);
                            SetState(firstPlayerState);
                            break;
                        case Constants.MultiplayControllerState.ExitRoom:
                            Debug.Log("## Exit Room ##");
                            // TODO: 팝업 띄우고 메인화면으로 이동
                            break;
                        case Constants.MultiplayControllerState.EndGame:
                            Debug.Log("## End Game ##");
                            // TODO: 팝업 띄우고 메인화면으로 이동
                            break;
                    }
                });
                break;
        }
    }


    public void SetState(BasePlayerState state)
    {
        _currentPlayerState?.OnExit(this);
        _currentPlayerState = state;
        _currentPlayerState?.OnEnter(this);
    }

    public bool SetNewBoardVlaue(Constants.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != Constants.PlayerType.None) return false;

        if (playerType == Constants.PlayerType.PlayerA)
        {
            _board[row, col] = playerType;
            BlockController.PlaceMaker(Block.MarkerType.BlackStone, row, col);
       
            return true;
        }
        else if (playerType == Constants.PlayerType.PlayerB)
        {
            _board[row, col] = playerType;
            BlockController.PlaceMaker(Block.MarkerType.WhiteStone, row, col);
           
            return true;
        }
        return false;
    }

    public GameResult CheckGameResult()
    {
        if (GameAI.CheckGameWin(Constants.PlayerType.PlayerA, _board)) { return GameResult.Win; }
        if (GameAI.CheckGameWin(Constants.PlayerType.PlayerB, _board)) { return GameResult.Lose; }
        if (GameAI.CheckGameDraw(_board)) { return GameResult.Draw; }
        return GameResult.None;
    }
   
    public void EndGame(GameResult gameResult)
    {
        SetState(null);
        firstPlayerState = null;
        secondPlayerState = null;


        string resultMessage = "게임오버";

        switch (GameManager.Instance._gameType)
        {
            case Constants.GameType.SinglePlay:

                switch (gameResult)
                {
                    case GameResult.None:
                        resultMessage = "오류";
                        break;
                    case GameResult.Win:
                        resultMessage = "승리";
                        break;
                    case GameResult.Lose:
                        resultMessage = "패배";
                        break;
                    case GameResult.Draw:
                        resultMessage = "비김";
                        break;
                }
                break;
            case Constants.GameType.DualPlay:
                switch (gameResult)
                {
                    case GameResult.None:
                        resultMessage = "오류";
                        break;
                    case GameResult.Win:
                        resultMessage = "A player Win";
                        break;
                    case GameResult.Lose:
                        resultMessage = "B player Win";
                        break;
                    case GameResult.Draw:
                        resultMessage = "Draw";
                        break;
                }
                break;
            case Constants.GameType.MultiPlay:
                /*
                switch (gameResult)
                {
                    case GameResult.None:
                        resultMessage = "오류";
                        break;
                    case GameResult.Win:
                        resultMessage = "승리";
                        break;
                    case GameResult.Lose:
                        resultMessage = "패배";
                        break;
                    case GameResult.Draw:
                        resultMessage = "비김";
                        break;
                }*/
                break;
        }
      
 
        if(GameManager.Instance._gameType != Constants.GameType.MultiPlay)
        {
            GameManager.Instance.OpenConfirmPanel(resultMessage, () =>
            {
                GameManager.Instance.ChangeToMainScene();
            });
        }
       
        //Debug.Log("게임 오버");
    }

    public void Dispose()
    {
        _multiplayController?.LeaveRoom(_roomId);
        _multiplayController?.Dispose();
    }

    public BasePlayerState GetCurrentState()
    {
        return _currentPlayerState;
    }

}

