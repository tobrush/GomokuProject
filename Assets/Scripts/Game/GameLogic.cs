using System;
using System.Collections;
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

    private Coroutine _turnTimerCoroutine;
    private float _turnLimit = 30f; // 30초 제한

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

        // 기존 타이머 멈춤
        if (_turnTimerCoroutine != null)
        {
            BlockController.StopCoroutine(_turnTimerCoroutine);
            _turnTimerCoroutine = null;
        }
        // 새 턴이 플레이어 턴일 때만 타이머 시작
        if (_currentPlayerState != null && !(_currentPlayerState is AIState))
        {
            GameManager.Instance?.InitTurnTimerUI(_turnLimit);
            _turnTimerCoroutine = BlockController.StartCoroutine(TurnTimer());
        }
    }

    private IEnumerator TurnTimer()
    {
        float time = _turnLimit;

        while (time >= 0)
        {
            // 싱글/듀얼일 때만 정지 체크
            if (GameManager.Instance._gameType != Constants.GameType.MultiPlay)
            {
                // 팝업으로 일시정지 중이면 대기
                while (GameManager.Instance.IsPaused)
                {
                    yield return null; // 다음 프레임까지 대기
                }
            }
            time -= Time.deltaTime;

            GameManager.Instance?.SetGameTurnTime(time); // UI 갱신

            yield return null;
        }

        // 현재 턴인 플레이어 확인
        if (_currentPlayerState is PlayerState playerState)
        {
            if (playerState.IsFirstPlayer) // 흑돌 턴 초과
            {
                EndGame(GameResult.Lose);  // A(흑) 패배 B 승리
            }
            else // 백돌 턴 초과
            {
                EndGame(GameResult.Win);   // B(백) 패배 A 승리
            }
        }
    }

    public bool SetNewBoardVlaue(Constants.PlayerType playerType, int row, int col)
    {
        if (_board[row, col] != Constants.PlayerType.None) return false;

        if (playerType == Constants.PlayerType.PlayerA)
        {
            if (GameAI.IsBanBlock(playerType, row, col, _board))
            {
                // 금수라면 UI 표시 (X 마크) 및 착수 불가
                BlockController.PlaceMaker(Block.MarkerType.Ban, row, col);
                return false;
            }
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
    public Constants.PlayerType CheckWinner()
    {
        if (GameAI.CheckGameWin(Constants.PlayerType.PlayerA, _board))
            return Constants.PlayerType.PlayerA;
        if (GameAI.CheckGameWin(Constants.PlayerType.PlayerB, _board))
            return Constants.PlayerType.PlayerB;
        if (GameAI.CheckGameDraw(_board))
            return Constants.PlayerType.None; // 무승부
        return Constants.PlayerType.None;
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
        if (_turnTimerCoroutine != null)
        {
            BlockController.StopCoroutine(_turnTimerCoroutine);
            _turnTimerCoroutine = null;
        }

        
        var winner = CheckWinner();

        // 내가 흑인지 백인지 판정
        bool isFirstPlayer = false;
        if (_currentPlayerState is PlayerState ps)
            isFirstPlayer = ps.IsFirstPlayer;
        else if (_currentPlayerState is MultiPlayerState ms)
            isFirstPlayer = ms.IsFirstPlayer;

        //firstPlayerState = null;
        //secondPlayerState = null;

        SetState(null);


        string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        GameManager.Instance.SaveRecord(BlockController.gameRecord, fileName);
        Debug.Log("게임 기록 저장 완료: " + fileName);
    

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

                if (winner == Constants.PlayerType.None)
                {
                    resultMessage = "무승부";
                }
                else
                {
                    bool iWon = (winner == Constants.PlayerType.PlayerA && isFirstPlayer) ||
                                (winner == Constants.PlayerType.PlayerB && !isFirstPlayer);

                    resultMessage = iWon ? "승리!" : "패배...";
                }
                break;
        }
      
 
        if(GameManager.Instance._gameType != Constants.GameType.MultiPlay)
        {
            GameManager.Instance.OpenConfirmPanel(resultMessage, () =>
            {
                GameManager.Instance.ChangeToMainScene();
            });
        }
        else
        {
            GameManager.Instance.OpenConfirmPanel(resultMessage, () =>
            {
                GameManager.Instance.ChangeToMainScene();
                //TODO 서버 점수 추가 + 점수결과확인
            });
        }
       
        //Debug.Log("게임 오버");
    }

    public void Dispose()
    {
        if (_turnTimerCoroutine != null)
        {
            BlockController.StopCoroutine(_turnTimerCoroutine);
            _turnTimerCoroutine = null;
        }

        _multiplayController?.LeaveRoom(_roomId);
        _multiplayController?.Dispose();
    }

    public BasePlayerState GetCurrentState()
    {
        return _currentPlayerState;
    }

}

