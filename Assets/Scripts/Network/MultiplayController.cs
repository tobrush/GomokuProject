using Newtonsoft.Json;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UIElements;

// joinRoom/createRoom 이벤트 전달할 때 전달되는 정보의 타입
public class RoomData
{
    [JsonProperty("roomId")]
    public string roomId { get; set; }
}

// 상대방이 둔 마커 위치
public class BlockData
{
    [JsonProperty("blockIndex")]
    public int blockIndex { get; set; }

}


public class MultiplayController
{
    private SocketIOUnity _socket;

    private Action<Constants.MultiplayControllerState, string> _onMultiplayStateChanged;
    // 1. delegate 
    // 함수를 변수에 저장하고있다가 필요한시점에 실행
    // 함수의 타입 선언 > 해당 타입의 변수 선언 > 변수에 함수를 저장
    // 2. Action / Func 
    //
    // 3. Event / UnityEvent 

    public Action<int> onBlockDataChanged;

    public MultiplayController(Action<Constants.MultiplayControllerState, string> onMultiplayStateChanged)
    {
        //서버에서 이벤트가 발생하면 처리할 메서드를 _onMultiplayStateChanged에 등록
        _onMultiplayStateChanged = onMultiplayStateChanged;

        //socket.io 클라이언트 초기화
        var uri = new Uri(Constants.SocketServerURL);
        _socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        /*
        _socket.On("createRoom", CreateRoom);
        _socket.On("joinRoom", JoinRoom);
        _socket.On("startGame", StartGame);
        _socket.On("exitGame", ExitRoom);
        _socket.On("endGame", EndGame);
        _socket.On("doOpponent", DoOpponent);
        */
        _socket.OnUnityThread("createRoom", CreateRoom);
        _socket.OnUnityThread("joinRoom", JoinRoom);
        _socket.OnUnityThread("startGame", StartGame);
        _socket.OnUnityThread("exitGame", ExitRoom);
        _socket.OnUnityThread("endGame", EndGame);
        _socket.OnUnityThread("doOpponent", DoOpponent);


        _socket.Connect(); //서버접속
    }

    private void CreateRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.CreateRoom, data.roomId);
    }

    private void JoinRoom(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.JoinRoom, data.roomId);
    }

    private void StartGame(SocketIOResponse response)
    {
        var data = response.GetValue<RoomData>();
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.StartGame, data.roomId);
    }

    private void ExitRoom(SocketIOResponse response)
    {
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.ExitRoom, null);
    }

    private void EndGame(SocketIOResponse response)
    {
        _onMultiplayStateChanged?.Invoke(Constants.MultiplayControllerState.EndGame, null);
    }

    private void DoOpponent(SocketIOResponse response)
    {
        Debug.Log("DoOpponent");
        var data = response.GetValue<BlockData>();
        onBlockDataChanged?.Invoke(data.blockIndex);
    }


    #region Client => Server

    // 룸을 나올때 홏출하는 메서드 client -> server
    public void LeaveRoom(string roomId)
    {
        _socket.Emit("leaveRoom", roomId);
    }

    //플레이어가 marker를 두면 호출하는 메서드, clent -> server
    public void DoPlayer(string roomId, int position)
    {
        //_socket.Emit(eventName: "doPlayer", new { roomId, position });
        Debug.Log("DoPlayer");
        var payload = new { roomId = roomId, blockIndex = position };
        Debug.Log($"Emit doPlayer: {JsonUtility.ToJson(payload)}");
        _socket.Emit("doPlayer", payload);
    }



    #endregion


    public void Dispose()
    {
        if (_socket != null)
        {
            _socket.Disconnect();
            _socket.Dispose();
            _socket = null;
        }
    }
}
