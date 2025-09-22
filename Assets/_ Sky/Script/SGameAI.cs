using System;
using System.Collections.Generic;
using UnityEngine;

public static class SGameAI
{
    // 급수 정보를 저장할 클래스
    [System.Serializable]
    public class RankInfo
    {
        public int currentRank = 18;  // 현재 급수 (18급부터 시작)
        public int promotionPoints = 0;  // 현재 승급 포인트

        // 급수별 필요한 승급 포인트
        public int GetRequiredPromotionPoints()
        {
            if (currentRank >= 10) return 3;      // 10급~18급: 3점
            else if (currentRank >= 5) return 5;  // 5급~9급: 5점
            else return 10;                       // 1급~4급: 10점
        }

        // 승리 시 처리
        public bool OnWin()
        {
            promotionPoints++;
            if (promotionPoints >= GetRequiredPromotionPoints() && currentRank > 1)
            {
                currentRank--; // 급수 상승 (숫자가 작아짐)
                promotionPoints = 0;
                return true; // 승급 발생
            }
            return false; // 승급 없음
        }

        // 패배 시 처리
        public bool OnLose()
        {
            promotionPoints--;
            if (promotionPoints <= -3 && currentRank < 18)
            {
                currentRank++; // 급수 하락 (숫자가 커짐)
                promotionPoints = 0;
                return true; // 강등 발생
            }
            return false; // 강등 없음
        }

        // 현재 상태를 문자열로 반환
        public string GetRankString()
        {
            if (currentRank == 1) return "1급 (최고급)";
            return $"{currentRank}급 (승급포인트: {promotionPoints}/{GetRequiredPromotionPoints()})";
        }
    }

    // 급수 정보 (게임에서 관리)
    public static RankInfo playerRankInfo = new RankInfo();

    // 급수에 따른 AI 설정
    public static (int row, int col)? GetBestMove(Constants.PlayerType[,] board, int playerRank = 18)
    {
        
        // 급수에 따른 AI 강도 조절
        var aiSettings = GetAISettings(playerRank);

        var candidates = GetCandidateMoves(board, aiSettings.searchRadius);

        // 급수가 높을수록 더 정교한 이동 정렬
        if (playerRank <= 10)
        {
            candidates = SortMovesByPriority(board, candidates);
        }

        // 실수에 따른 랜덤 선택 로직
        if (aiSettings.errorRate > 0)
        {
            float randomFactor = UnityEngine.Random.Range(0f, 1f);

            if(randomFactor < aiSettings.errorRate)
            {
                Debug.Log("AI 실수 발생");
                int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
                var randomMove = candidates[randomIndex];

                return randomMove;
            }
        }

        // 실수가 발생하지 않은 경우
        Debug.Log(" AI 정상 동작 ");
        float bestScore = float.MinValue;
        (int row, int col) movePosition = (-1, -1);

        foreach (var (row, col) in candidates)
        {
            board[row, col] = Constants.PlayerType.PlayerB;
            var score = DoMiniMax(board, 0, false, aiSettings.maxDepth, aiSettings.useAlphaBeta);
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

    // 급수별 AI 설정을 반환하는 구조체
    public struct AISettings
    {
        public int maxDepth;
        public float errorRate;
        public int searchRadius;
        public bool useAlphaBeta;

        public AISettings(int depth, float error, int radius, bool alphaBeta)
        {
            maxDepth = depth;
            errorRate = error;
            searchRadius = radius;
            useAlphaBeta = alphaBeta;
        }
    }

    // 급수에 따른 AI 설정 반환
    private static AISettings GetAISettings(int playerRank)
    {
        Debug.Log($"GetAISettings 호출: 플레이어 급수 = {playerRank}급");

        AISettings settings;

        if (playerRank >= 16) // 16급~18급: 매우 쉬움
        {
            settings = new AISettings(1, 0.5f, 1, false); // depth=1, 실수율 50%
            Debug.Log("AI 난이도: 매우 쉬움 (16-18급)");
        }
        else if (playerRank >= 13) // 13급~15급: 쉬움
        {
            settings = new AISettings(1, 0.4f, 1, false); // depth=1, 실수율 40%
            Debug.Log("AI 난이도: 쉬움 (13-15급)");
        }
        else if (playerRank >= 10) // 10급~12급: 초급
        {
            settings = new AISettings(2, 0.3f, 1, true); // depth=2, 실수율 30%
            Debug.Log("AI 난이도: 초급 (10-12급)");
        }
        else if (playerRank >= 7) // 7급~9급: 중급
        {
            settings = new AISettings(2, 0.2f, 2, true); // depth=2, 실수율 20%
            Debug.Log("AI 난이도: 중급 (7-9급)");
        }
        else if (playerRank >= 4) // 4급~6급: 고급
        {
            settings = new AISettings(3, 0.1f, 2, true); // depth=3, 실수율 10%
            Debug.Log("AI 난이도: 고급 (4-6급)");
        }
        else // 1급~3급: 최고급
        {
            settings = new AISettings(4, 0.05f, 2, true); // depth=4, 실수율 5%
            Debug.Log("AI 난이도: 최고급 (1-3급)");
        }

        Debug.Log($"적용된 설정: depth={settings.maxDepth}, error={settings.errorRate}, radius={settings.searchRadius}, alphaBeta={settings.useAlphaBeta}");
        return settings;
    }

    private static List<(int row, int col)> SortMovesByPriority(Constants.PlayerType[,] board, List<(int row, int col)> candidates)
    {
        var scoredMoves = new List<((int row, int col) move, int priority)>();

        foreach (var move in candidates)
        {
            int priority = GetMovePriority(board, move.row, move.col);
            scoredMoves.Add((move, priority));
        }

        scoredMoves.Sort((a, b) => b.priority.CompareTo(a.priority));

        var result = new List<(int row, int col)>();
        foreach (var item in scoredMoves)
        {
            result.Add(item.move);
        }
        return result;
    }

    private static int GetMovePriority(Constants.PlayerType[,] board, int row, int col)
    {
        int priority = 0;

        // 즉시 승리 수 확인
        board[row, col] = Constants.PlayerType.PlayerB;
        if (CheckGameWin(Constants.PlayerType.PlayerB, board))
        {
            board[row, col] = Constants.PlayerType.None;
            return 10000; // 승리 수
        }
        board[row, col] = Constants.PlayerType.None;

        // 상대방 승리 막기
        board[row, col] = Constants.PlayerType.PlayerA;
        if (CheckGameWin(Constants.PlayerType.PlayerA, board))
        {
            board[row, col] = Constants.PlayerType.None;
            return 9000; // 방어 수
        }
        board[row, col] = Constants.PlayerType.None;

        // 패턴 평가
        priority += EvaluatePositionValue(board, row, col);

        return priority;
    }

    private static int EvaluatePositionValue(Constants.PlayerType[,] board, int row, int col)
    {
        int value = 0;
        int size = board.GetLength(0);

        int[][] directions = new int[][]
        {
            new[] {1, 0}, new[] {0, 1}, new[] {1, 1}, new[] {1, -1}
        };

        foreach (var dir in directions)
        {
            // AI 돌 놓았을 때의 가치 평가
            board[row, col] = Constants.PlayerType.PlayerB;
            int aiValue = EvaluateDirection(board, row, col, dir[0], dir[1], Constants.PlayerType.PlayerB);
            board[row, col] = Constants.PlayerType.None;

            // 플레이어 돌 놓았을 때의 가치 평가 (방어적 측면)
            board[row, col] = Constants.PlayerType.PlayerA;
            int playerValue = EvaluateDirection(board, row, col, dir[0], dir[1], Constants.PlayerType.PlayerA);
            board[row, col] = Constants.PlayerType.None;

            value += aiValue + (playerValue / 2); // 공격보다 방어를 약간 덜 중요하게
        }

        return value;
    }

    private static int EvaluateDirection(Constants.PlayerType[,] board, int row, int col, int dr, int dc, Constants.PlayerType player)
    {
        int count = 1;
        int openEnds = 0;
        int size = board.GetLength(0);

        // 양방향으로 같은 돌 개수 세기
        for (int direction = -1; direction <= 1; direction += 2)
        {
            for (int i = 1; i < 5; i++)
            {
                int nr = row + dr * i * direction;
                int nc = col + dc * i * direction;

                if (nr < 0 || nr >= size || nc < 0 || nc >= size) break;
                if (board[nr, nc] != player)
                {
                    if (board[nr, nc] == Constants.PlayerType.None) openEnds++;
                    break;
                }
                count++;
            }
        }

        // 패턴별 점수
        switch (count)
        {
            case 5: return 100000; // 5목
            case 4: return openEnds > 0 ? 10000 : 1000; // 4목
            case 3: return openEnds > 1 ? 1000 : (openEnds > 0 ? 100 : 10); // 3목
            case 2: return openEnds > 1 ? 100 : (openEnds > 0 ? 10 : 1); // 2목
            default: return openEnds > 0 ? 1 : 0;
        }
    }

    private static List<(int row, int col)> GetCandidateMoves(Constants.PlayerType[,] board, int radius = 1)
    {
        var candidates = new List<(int row, int col)>();
        int size = board.GetLength(0);

        // 보드가 비어있으면 중앙에서 시작
        bool isEmpty = true;
        for (int r = 0; r < size && isEmpty; r++)
        {
            for (int c = 0; c < size && isEmpty; c++)
            {
                if (board[r, c] != Constants.PlayerType.None)
                    isEmpty = false;
            }
        }

        if (isEmpty)
        {
            candidates.Add((size / 2, size / 2));
            return candidates;
        }

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

    private static float DoMiniMax(Constants.PlayerType[,] board, int depth, bool isMaximizing, int maxDepth, bool useAlphaBeta = true, float alpha = float.MinValue, float beta = float.MaxValue)
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
                float score = DoMiniMax(board, depth + 1, false, maxDepth, useAlphaBeta, alpha, beta);
                board[row, col] = Constants.PlayerType.None;
                bestScore = Mathf.Max(score, bestScore);

                if (useAlphaBeta)
                {
                    alpha = Mathf.Max(alpha, score);
                    if (beta <= alpha) break; // 가지치기
                }
            }
            return bestScore;
        }
        else
        {
            float bestScore = float.MaxValue;
            foreach (var (row, col) in GetCandidateMoves(board))
            {
                board[row, col] = Constants.PlayerType.PlayerA;
                float score = DoMiniMax(board, depth + 1, true, maxDepth, useAlphaBeta, alpha, beta);
                board[row, col] = Constants.PlayerType.None;
                bestScore = Mathf.Min(score, bestScore);

                if (useAlphaBeta)
                {
                    beta = Mathf.Min(beta, score);
                    if (beta <= alpha) break; // 가지치기
                }
            }
            return bestScore;
        }
    }

    private static int EvaluateBoard(Constants.PlayerType[,] board)
    {
        int size = board.GetLength(0);
        int score = 0;

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
                    int count = 1;
                    int blocked = 0;

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
        int[,] scoreTable =
        {
            { 0, 0, 0 },        // dummy (count=0)
            { 10, 1, 0 },       // 1목
            { 100, 10, 0 },     // 2목
            { 1000, 100, 0 },   // 3목
            { 10000, 1000, 0 }, // 4목
            { 100000, 100000, 100000 } // 5목
        };

        int idx = Math.Min(count, 5);
        int baseScore = 0;

        if (blocked == 0) baseScore = scoreTable[idx, 0];
        else if (blocked == 1) baseScore = scoreTable[idx, 1];
        else baseScore = scoreTable[idx, 2];

        if (player == Constants.PlayerType.PlayerA)
            return -baseScore;
        else
            return baseScore;
    }

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

    public static bool CheckGameWin(Constants.PlayerType playerType, Constants.PlayerType[,] board)
    {
        int size = board.GetLength(0);
        int winLength = 5;

        // 가로 체크
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
                else count = 0;
            }
        }

        // 세로 체크
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
                else count = 0;
            }
        }

        // ↘ 대각선 체크
        for (int r = 0; r <= size - winLength; r++)
        {
            for (int c = 0; c <= size - winLength; c++)
            {
                int count = 0;
                for (int i = 0; i < winLength && r + i < size && c + i < size; i++)
                {
                    if (board[r + i, c + i] == playerType)
                        count++;
                    else break;
                }
                if (count >= winLength) return true;
            }
        }

        // ↙ 대각선 체크
        for (int r = 0; r <= size - winLength; r++)
        {
            for (int c = winLength - 1; c < size; c++)
            {
                int count = 0;
                for (int i = 0; i < winLength && r + i < size && c - i >= 0; i++)
                {
                    if (board[r + i, c - i] == playerType)
                        count++;
                    else break;
                }
                if (count >= winLength) return true;
            }
        }

        return false;
    }

    // 게임 결과 처리 (승급/강등)
    public static void ProcessGameResult(bool playerWon)
    {
        if (playerWon)
        {
            bool promoted = playerRankInfo.OnWin();
            if (promoted)
            {
                Debug.Log($"축하합니다! {playerRankInfo.currentRank}급으로 승급했습니다!");
            }
            else
            {
                Debug.Log($"승리! {playerRankInfo.GetRankString()}");
            }
        }
        else
        {
            bool demoted = playerRankInfo.OnLose();
            if (demoted)
            {
                Debug.Log($"{playerRankInfo.currentRank}급으로 강등되었습니다.");
            }
            else
            {
                Debug.Log($"패배... {playerRankInfo.GetRankString()}");
            }
        }

        // 급수 정보를 PlayerPrefs에 저장 (영구 저장)
        SaveRankInfo();
    }

    // 급수 정보 저장
    public static void SaveRankInfo()
    {
        PlayerPrefs.SetInt("PlayerRank", playerRankInfo.currentRank);
        PlayerPrefs.SetInt("PromotionPoints", playerRankInfo.promotionPoints);
        PlayerPrefs.Save();
    }

    // 급수 정보 로드
    public static void LoadRankInfo()
    {
        playerRankInfo.currentRank = PlayerPrefs.GetInt("PlayerRank", 18);
        playerRankInfo.promotionPoints = PlayerPrefs.GetInt("PromotionPoints", 0);
    }

    // 급수 정보 초기화
    public static void ResetRankInfo()
    {
        playerRankInfo.currentRank = 18;
        playerRankInfo.promotionPoints = 0;
        SaveRankInfo();
    }
}