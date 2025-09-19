using System;
using System.Drawing;
using UnityEngine;
using static Block;

public class BlockController : MonoBehaviour
{
    [SerializeField] private Block[] blocks;

    public Block LastBlock;

    public GameObject blockPrefab;

    public BadukBoard board;

    public GameRecord gameRecord = new GameRecord();
    private int moveOrder = 1; // 착수 순서 추적용



    public delegate void OnBlockkClicked(int row, int col);
    public OnBlockkClicked OnBlockClickedDelegate;

    public void Awake()
    {
        // 배열 갯수 초기화
        blocks = new Block[board.size * board.size];

        int index = 0;

        float half = (board.size - 1) * board.spacing * 0.5f;

        for (int h = 0; h < board.size; h++)
        {
            for (int v = 0; v < board.size; v++)
            {
                Vector3 pos = new Vector3((v * board.spacing) - half, (h * board.spacing) - half, -0.01f);
                GameObject _block = Instantiate(blockPrefab, transform);
                _block.transform.localPosition = pos;

                _block.name = $"Block_{h}_{v}";

                Block block = _block.GetComponent<Block>();
                if (block != null)
                {
                    blocks[index] = block;
                }
                else
                {
                    Debug.LogWarning($"Block 컴포넌트가 {_block.name}에 없음!");
                }
                index++;
            }
        }

    }

    public void InitBlocks()
    {

        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].InitMaker(i, blockIndex =>
            {
                var row = blockIndex / Constants.BlockColumnCount;
                var col = blockIndex % Constants.BlockColumnCount;

                OnBlockClickedDelegate?.Invoke(row, col);
            });
        }
    }
    public void PlaceMaker(Block.MarkerType markerType, int row, int col)
    {
        if (LastBlock != null)
        {
            if (LastBlock.nowSpriteRender.sprite != null)
            {
                LastBlock.nowSpriteRender.sprite = null;
            }
        }

        // row, col >> index 변환
        var blockIndex = row * Constants.BlockColumnCount + col;
        blocks[blockIndex].SetMarker(markerType);
        LastBlock = blocks[blockIndex];

        if (markerType == Block.MarkerType.BlackStone || markerType == Block.MarkerType.WhiteStone)
        {
            // 마커 타입에 따라 플레이어 결정 (예: 흑=1, 백=2)
            int player = markerType == Block.MarkerType.BlackStone ? 1 : 2;
            // Move 객체 생성 및 기록
            Move move = new Move
            {
                x = col,
                y = row,
                player = player,
                order = moveOrder++
            };
            gameRecord.moves.Add(move);
        }
    }

    public void SetBlockColor()
    {
        //TODO : 게임로직이 완성되면 구현
    }

    public Block GetBlock(int index)
    {
        if (index >= 0 && index < blocks.Length)
            return blocks[index];
        return null;
    }

    public void ClearBoard()
    {
        foreach (var block in blocks)
        {
            block.SetMarker(Block.MarkerType.None);
            block.SetOrderNumber(0);
        }
        LastBlock = null;
        gameRecord = new GameRecord();
        moveOrder = 1;
    }

}
