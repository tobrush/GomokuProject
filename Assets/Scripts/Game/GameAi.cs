using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameAI
{
    /*
    // 현재 상태를 전달하면 다음 최적의 수를 반환하는 메서드
    public static (int row, int col)? GetBestMove(Constants.PlayerType[,] board)
    {
        float bestScore = -1000;
        (int row, int col) movePosition = (-1, -1);

        for (var row = 0; row < board.GetLength(0); row++)
        {
            for (var col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == Constants.PlayerType.None)
                {
                    board[row, col] = Constants.PlayerType.PlayerB;
                    var score = GameAI.DoMiniMax(board, 0, false);
                    board[row, col] = Constants.PlayerType.None;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        movePosition = (row, col);
                    }
                }
            }
        }

        if (movePosition != (-1, -1))
        {
            return (movePosition.row, movePosition.col);
        }
        return null;
    }

    private static float DoMiniMax(Constants.PlayerType[,] board, int depth, bool isMaximizing)
    {
        // 게임 종료 상태 체크
        if (CheckGameWin(Constants.PlayerType.PlayerA, board))
            return -10 + depth;
        if (CheckGameWin(Constants.PlayerType.PlayerB, board))
            return 10 - depth;
        if (CheckGameDraw(board))
            return 0;

        if (isMaximizing)
        {
            var bestScore = float.MinValue;
            for (var row = 0; row < board.GetLength(0); row++)
            {
                for (var col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == Constants.PlayerType.None)
                    {
                        board[row, col] = Constants.PlayerType.PlayerB;
                        var score = DoMiniMax(board, depth + 1, false);
                        board[row, col] = Constants.PlayerType.None;
                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
        else
        {
            var bestScore = float.MaxValue;
            for (var row = 0; row < board.GetLength(0); row++)
            {
                for (var col = 0; col < board.GetLength(1); col++)
                {
                    if (board[row, col] == Constants.PlayerType.None)
                    {
                        board[row, col] = Constants.PlayerType.PlayerA;
                        var score = DoMiniMax(board, depth + 1, true);
                        board[row, col] = Constants.PlayerType.None;
                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
    }

    */



    public static (int row, int col)? GetBestMove(Constants.PlayerType[,] board, int maxDepth = 2)
    {
        float bestScore = float.MinValue;
        (int row, int col) movePosition = (-1, -1);

        var candidates = GetCandidateMoves(board);

        foreach (var (row, col) in candidates)
        {
            board[row, col] = Constants.PlayerType.PlayerB;
            var score = DoMiniMax(board, 0, false, maxDepth);
            board[row, col] = Constants.PlayerType.None;

            if (score > bestScore)
            {
                bestScore = score;
                movePosition = (row, col);
            }
        }

        if (movePosition != (-1, -1))
            return movePosition;

        return null;
    }

    private static List<(int row, int col)> GetCandidateMoves(Constants.PlayerType[,] board, int radius = 1)
    {
        var candidates = new List<(int row, int col)>();
        int size = board.GetLength(0);

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (board[r, c] != Constants.PlayerType.None) continue;

                // 주변 radius 안에 돌이 있으면 후보로 등록
                bool nearStone = false;
                for (int dr = -radius; dr <= radius && !nearStone; dr++)
                {
                    for (int dc = -radius; dc <= radius && !nearStone; dc++)
                    {
                        int nr = r + dr;
                        int nc = c + dc;
                        if (nr < 0 || nc < 0 || nr >= size || nc >= size) continue;
                        if (board[nr, nc] != Constants.PlayerType.None)
                            nearStone = true;
                    }
                }
                if (nearStone)
                    candidates.Add((r, c));
            }
        }
        return candidates;
    }

    private static float DoMiniMax(Constants.PlayerType[,] board, int depth, bool isMaximizing, int maxDepth, float alpha = float.MinValue, float beta = float.MaxValue)
    {
        if (CheckGameWin(Constants.PlayerType.PlayerA, board))
            return -10000 + depth;
        if (CheckGameWin(Constants.PlayerType.PlayerB, board))
            return 10000 - depth;
        if (CheckGameDraw(board))
            return 0;
        if (depth >= maxDepth)
            return EvaluateBoard(board);

        if (isMaximizing)
        {
            float bestScore = float.MinValue;
            foreach (var (row, col) in GetCandidateMoves(board))
            {
                board[row, col] = Constants.PlayerType.PlayerB;
                float score = DoMiniMax(board, depth + 1, false, maxDepth, alpha, beta);
                board[row, col] = Constants.PlayerType.None;
                bestScore = Mathf.Max(score, bestScore);
                alpha = Mathf.Max(alpha, score);
                if (beta <= alpha) break; // 가지치기
            }
            return bestScore;
        }
        else
        {
            float bestScore = float.MaxValue;
            foreach (var (row, col) in GetCandidateMoves(board))
            {
                board[row, col] = Constants.PlayerType.PlayerA;
                float score = DoMiniMax(board, depth + 1, true, maxDepth, alpha, beta);
                board[row, col] = Constants.PlayerType.None;
                bestScore = Mathf.Min(score, bestScore);
                beta = Mathf.Min(beta, score);
                if (beta <= alpha) break; // 가지치기
            }
            return bestScore;
        }
    }

    private static int EvaluateBoard(Constants.PlayerType[,] board)
    {
        int size = board.GetLength(0);
        int score = 0;

        // 네 방향 검사: 가로, 세로, 대각선 ↘, 대각선 ↙
        int[][] directions = new int[][]
        {
        new[] {1, 0},   // 가로
        new[] {0, 1},   // 세로
        new[] {1, 1},   // ↘ 대각
        new[] {1, -1}   // ↙ 대각
        };

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                if (board[r, c] == Constants.PlayerType.None) continue;

                foreach (var dir in directions)
                {
                    int count = 1; // 연속된 돌 개수
                    int blocked = 0; // 양쪽이 막혔는지 여부

                    int pr = r - dir[0];
                    int pc = c - dir[1];
                    if (pr >= 0 && pr < size && pc >= 0 && pc < size)
                    {
                        if (board[pr, pc] != Constants.PlayerType.None && board[pr, pc] != board[r, c])
                            blocked++;
                    }

                    int nr = r + dir[0];
                    int nc = c + dir[1];
                    while (nr >= 0 && nr < size && nc >= 0 && nc < size && board[nr, nc] == board[r, c])
                    {
                        count++;
                        nr += dir[0];
                        nc += dir[1];
                    }

                    if (nr < 0 || nr >= size || nc < 0 || nc >= size ||
                        (board[nr, nc] != Constants.PlayerType.None && board[nr, nc] != board[r, c]))
                    {
                        blocked++;
                    }

                    int patternScore = GetPatternScore(count, blocked, board[r, c]);
                    score += patternScore;
                }
            }
        }

        return score;
    }

    private static int GetPatternScore(int count, int blocked, Constants.PlayerType player)
    {
        // 점수 테이블
        int[,] scoreTable =
        {
        { 0, 0, 0 },        // dummy (count=0)
        { 10, 1, 0 },       // 1목: 열린10, 막힌1
        { 100, 10, 0 },     // 2목
        { 1000, 100, 0 },   // 3목
        { 10000, 1000, 0 }, // 4목
        { 100000, 100000, 100000 } // 5목: 무조건 승리
    };

        int idx = Math.Min(count, 5);
        int baseScore = 0;

        if (blocked == 0) baseScore = scoreTable[idx, 0]; // 열린
        else if (blocked == 1) baseScore = scoreTable[idx, 1]; // 막힌 한쪽
        else baseScore = scoreTable[idx, 2]; // 양쪽 다 막힘 → 0점

        if (player == Constants.PlayerType.PlayerA)
            return -baseScore; // 상대 점수는 음수
        else
            return baseScore;  // AI 점수는 양수
    }


    // 비겼는지 확인
    public static bool CheckGameDraw(Constants.PlayerType[,] board)
    {
        for (var row = 0; row < board.GetLength(0); row++)
        {
            for (var col = 0; col < board.GetLength(1); col++)
            {
                if (board[row, col] == Constants.PlayerType.None) return false;
            }
        }
        return true;
    }

    // 게임 승리 확인 <- 3x3 틱택토용이라서 (0,0),(0,1),(0,2) 이런 식으로 고정 인덱스만 체크
    /*
    public static bool CheckGameWin(Constants.PlayerType playerType, Constants.PlayerType[,] board)
    {
        // Col 체크 후 일자면 True
        for (var row = 0; row < board.GetLength(0); row++)
        {
            if (board[row, 0] == playerType &&
                board[row, 1] == playerType &&
                board[row, 2] == playerType)
            {
                return true;
            }
        }
        // Row 체크 후 일자면 True
        for (var col = 0; col < board.GetLength(1); col++)
        {
            if (board[0, col] == playerType &&
                board[1, col] == playerType &&
                board[2, col] == playerType)
            {
                return true;
            }
        }

        // 대각선 일자면 True
        if (board[0, 0] == playerType &&
            board[1, 1] == playerType &&
            board[2, 2] == playerType)
        {
            return true;
        }
        if (board[0, 2] == playerType &&
            board[1, 1] == playerType &&
            board[2, 0] == playerType)
        {
            return true;
        }
        return false;
    }
    */
    public static bool CheckGameWin(Constants.PlayerType playerType, Constants.PlayerType[,] board)
    {
        int size = board.GetLength(0);
        int winLength = 5;

        // 1) 가로 체크
        for (int r = 0; r < size; r++)
        {
            int count = 0;
            for (int c = 0; c < size; c++)
            {
                if (board[r, c] == playerType)
                {
                    count++;
                    if (count >= winLength) return true;
                }
                else
                {
                    count = 0;
                }
            }
        }

        // 2) 세로 체크
        for (int c = 0; c < size; c++)
        {
            int count = 0;
            for (int r = 0; r < size; r++)
            {
                if (board[r, c] == playerType)
                {
                    count++;
                    if (count >= winLength) return true;
                }
                else
                {
                    count = 0;
                }
            }
        }

        // 3) ↘ 대각선 체크
        for (int r = 0; r <= size - winLength; r++)
        {
            for (int c = 0; c <= size - winLength; c++)
            {
                int count = 0;
                for (int i = 0; i < winLength && r + i < size && c + i < size; i++)
                {
                    if (board[r + i, c + i] == playerType)
                        count++;
                    else
                        break;
                }
                if (count >= winLength) return true;
            }
        }

        // 4) ↙ 대각선 체크
        for (int r = 0; r <= size - winLength; r++)
        {
            for (int c = winLength - 1; c < size; c++)
            {
                int count = 0;
                for (int i = 0; i < winLength && r + i < size && c - i >= 0; i++)
                {
                    if (board[r + i, c - i] == playerType)
                        count++;
                    else
                        break;
                }
                if (count >= winLength) return true;
            }
        }

        return false;
    }



    public static void Test()
    {
        int i = 30;

        Test();

        i = 20;
    }
}
