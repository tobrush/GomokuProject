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

}
