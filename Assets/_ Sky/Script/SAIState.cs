using UnityEngine;
using static Constants;
using System.Collections.Generic;
using System.Collections;

public class SAIState : BasePlayerState
{


    public override void HandleMove(GameLogic gameLogic, int row, int col)
    {

        ProcessMove(gameLogic, Constants.PlayerType.PlayerB, row, col);
    }

    public override void OnEnter(GameLogic gameLogic)
    {
        // 추가 : 급수 정보 로드
        SGameAI.LoadRankInfo();

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

        // 추가 : 현재 플레이어 급수 ( 없어도 무관 )
        DisplayCurrentRank();


        float startTime = Time.realtimeSinceStartup;
        var board = gameLogic.GetBoard();

        // 변경 : 매개변수에 플레이어 급수 추가
        var result = SGameAI.GetBestMove(board, SGameAI.playerRankInfo.currentRank);

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

    // 추가 : 현재 급수 정보 표시
    private void DisplayCurrentRank()
    {
        string rankInfo = SGameAI.playerRankInfo.GetRankString();
        Debug.Log($"현재 급수 : {rankInfo}");
    }
}
