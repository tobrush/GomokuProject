using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using static Block;
using static ConfirmPanelController;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    [SerializeField] private Sprite BlackStoneSprite;
    [SerializeField] private Sprite WhiteStoneSprite;
    [SerializeField] private Sprite ArrowSprite;
    [SerializeField] private SpriteRenderer makerSpriteRender; // maker SR
    public SpriteRenderer nowSpriteRender; // arrow SR


    public delegate void OnBlockClicked(int index);

    private OnBlockClicked _onBlockClicked;
    

    public enum MarkerType { None, BlackStone, WhiteStone }

    private int _blockIndex;

    //block SR
    public SpriteRenderer _spriteRenderer;
    public Color _defalutBlockColor;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defalutBlockColor = _spriteRenderer.color;
    }

    public void InitMaker(int blockIndex, OnBlockClicked onBlockClicked)
    {
        _blockIndex = blockIndex;

        SetMarker(MarkerType.None);


        _onBlockClicked = onBlockClicked; // 내부 필드 저장
        
        StartCoroutine(DelayInit());

 
    }
    private IEnumerator DelayInit()
    {
        yield return null; // 1프레임 대기
     
        SetBlockColor(_defalutBlockColor);
    }

    public void SetMarker(MarkerType markerType)
    {
        switch (markerType)
        {
            case MarkerType.None:
                makerSpriteRender.sprite = null;
                nowSpriteRender.sprite = null;
                break;
            case MarkerType.BlackStone:
                makerSpriteRender.sprite = BlackStoneSprite;
                nowSpriteRender.sprite = ArrowSprite;
                makerSpriteRender.size = new Vector2(0.3f, 0.3f);
                break;
            case MarkerType.WhiteStone:
                makerSpriteRender.sprite = WhiteStoneSprite;
                nowSpriteRender.sprite = ArrowSprite;
                makerSpriteRender.size = new Vector2(0.3f, 0.3f);
                break;

        }
    }

    public void SetBlockColor(Color color)
    {
        _spriteRenderer.color = color;
    }

    private void OnMouseUpAsButton()
    {
        if(EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        Debug.Log("selected Block : " + _blockIndex);

        _onBlockClicked?.Invoke(_blockIndex);
    }
}
