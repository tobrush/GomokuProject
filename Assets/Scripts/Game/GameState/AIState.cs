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
        GameManager.Instance.SetAILoading(true);
        yield return new WaitForSeconds(1.0f); // 1.0초 기다림
        GameManager.Instance.SetAILoading(false);
        var board = gameLogic.GetBoard();
        var result = GameAI.GetBestMove(board);
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
