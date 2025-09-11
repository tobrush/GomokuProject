using UnityEngine;

public class Constants : MonoBehaviour
{
    public const string ServerURL = "https://gomokuprojectserver.onrender.com";
    public const string SocketServerURL = "wss://gomokuprojectserver.onrender.com";

    public enum MultiplayControllerState
    {
        CreateRoom,     // 방 생성
        JoinRoom,       // 생성된 방에 참여
        StartGame,      // 생성한 방에 다른 유저가 참여해서 게임을 시작
        ExitRoom,       // 클라이언트가 방을 빠져 나왔을 때
        EndGame         // 상대방이 접속을 끊거나 방을 나갔을 때
    }

    public enum GameType { SinglePlay, DualPlay, MultiPlay }
    public enum PlayerType { None, PlayerA, PlayerB }

    public const int BlockColumnCount = 13;

}
