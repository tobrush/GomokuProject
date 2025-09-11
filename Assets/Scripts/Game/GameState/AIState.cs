using UnityEngine;
using static Constants;
using System.Collections.Generic;
using System.Collections;

public class AIState : BasePlayerState
{
   

    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {
        
        ProcessMove(gameLogic, Constants.PlayerType.PlayerB, row, col);
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        // 턴 표시
        GameManager.Instance.SetGameTurnPanel(GameUIController.GameTurnPanelType.BTurn);

        GameManager.Instance.StartCoroutine(AIMoveDelay(gameLogic));

    }

    public override void OnExit(GameLogic gameLogic)
    {
       
    }

    protected override void HandleNextTurn(GameLogic gameLogic)
    {
        gameLogic.SetState(gameLogic.firstPlayerState);
    }


    private IEnumerator AIMoveDelay(GameLogic gameLogic)
    {
        yield return null;
        GameManager.Instance.SetAILoading(true);

        float startTime = Time.realtimeSinceStartup;
        var board = gameLogic.GetBoard();
        var result = GameAI.GetBestMove(board);

        float elapsed = Time.realtimeSinceStartup - startTime;
        float minDelay = 1.0f; // 최소 보장 딜레이
        float remaining = Mathf.Max(0, minDelay - elapsed);

        // 남은 시간만큼만 기다림
        yield return new WaitForSeconds(remaining);

        GameManager.Instance.SetAILoading(false);

        if (result.HasValue)
        {
            HandleMove(gameLogic, result.Value.row, result.Value.col);
        }
        else
        {
            gameLogic.EndGame(GameLogic.GameResult.Draw);
        }
        
    }
}
