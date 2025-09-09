using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using static Block;
using static ConfirmPanelController;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour
{
    [SerializeField] private Sprite oSprite;
    [SerializeField] private Sprite xSprite;
    [SerializeField] private SpriteRenderer makerSpriteRender; // maker SR


    public delegate void OnBlockClicked(int index);

    private OnBlockClicked _onBlockClicked;

    public enum MarkerType { None, O, X }

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
                break;
            case MarkerType.O:
                makerSpriteRender.sprite = oSprite;
                break;
            case MarkerType.X:
                makerSpriteRender.sprite = xSprite;
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
