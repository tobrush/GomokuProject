using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Move
{
    public int x;
    public int y;
    public int player; // 1=흑, 2=백
    public int order;  // 착수 순서
}

[System.Serializable]
public class GameRecord
{
    public List<Move> moves = new List<Move>();
}

public class RecordManager : MonoBehaviour
{
    public void SaveRecord(GameRecord record, string fileName)
    {
        string json = JsonUtility.ToJson(record, true);
        string path = Application.dataPath + "/GameRecords/" + fileName + ".json";
        System.IO.File.WriteAllText(path, json);
        Debug.Log("Saved to " + path);
    }

    public GameRecord LoadRecord(string fileName)
    {
        string path = Application.dataPath + "/GameRecords/" + fileName + ".json";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return null;
        }
        string json = System.IO.File.ReadAllText(path);
        return JsonUtility.FromJson<GameRecord>(json);
    }
}

