using UnityEngine;
using static Block;

public class BlockController : MonoBehaviour
{
    [SerializeField] private Block[] blocks;

    public delegate void OnBlockkClicked(int row, int col);
    public OnBlockkClicked OnBlockClickedDelegate;

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
        // row, col >> index 변환
        var blockIndex = row * Constants.BlockColumnCount + col;
        blocks[blockIndex].SetMarker(markerType);
    }

    public void SetBlockColor()
    {
        //TODO : 게임로직이 완성되면 구현
    }
}
