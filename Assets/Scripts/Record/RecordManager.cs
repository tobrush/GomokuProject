using System;
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
    private string recordDirectory;
    private void Awake()
    {
        //json 파일 리스트 로드
        recordDirectory = Application.dataPath + "/GameRecords/";
        if (!System.IO.Directory.Exists(recordDirectory))
        {
            System.IO.Directory.CreateDirectory(recordDirectory);
        }
    }

    public List<string> GetAllRecordFileNames()
    {
        if (!System.IO.Directory.Exists(recordDirectory))
            return new List<string>();

        string[] files = System.IO.Directory.GetFiles(recordDirectory, "*.json");
        List<string> fileNames = new List<string>();
        foreach (string file in files)
        {
            fileNames.Add(System.IO.Path.GetFileNameWithoutExtension(file));
        }
        return fileNames;
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
