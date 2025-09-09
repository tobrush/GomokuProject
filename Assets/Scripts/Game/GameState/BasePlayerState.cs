using UnityEngine;

public abstract class BasePlayerState 
{
    public abstract void OnEnter(GameLogic gameLogic);
    public abstract void OnExit(GameLogic gameLogic);
    public abstract void HandleMove(GameLogic gameLogic, int row, int col); // 마커 표시

    protected abstract void HandleNextTurn(GameLogic gameLogic); // 턴 전환


    protected void ProcessMove(GameLogic gameLogic, Constants.PlayerType playerType, int row, int col)
    {
        if (gameLogic.SetNewBoardVlaue(playerType, row, col))
        {
            var gameResult = gameLogic.CheckGameResult();

            if (gameResult == GameLogic.GameResult.None)
            {
                HandleNextTurn(gameLogic);
            }
            else
            {

                gameLogic.EndGame(gameResult);
            }
        }
    }
}

