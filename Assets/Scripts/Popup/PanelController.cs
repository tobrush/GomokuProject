using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
  
    [SerializeField] private RectTransform panelRectTransform;

    CanvasGroup _backCanvasGroup;

    Image _backImage;

    //패널이 Hide 될때 해야할 동작
    public delegate void PanelControllerHideDelegate();

    private void Awake()
    {
        _backCanvasGroup = GetComponent<CanvasGroup>();
        _backImage = GetComponent<Image>();
    }


    public void Show()
    {
        if (GameManager.Instance._gameType != Constants.GameType.MultiPlay)
        {
            GameManager.Instance.PauseGame();
        }

        _backCanvasGroup.alpha = 0;
        panelRectTransform.localScale = Vector3.zero;
        _backImage.raycastTarget = true;
        _backCanvasGroup.DOFade(1, 0.3f).SetEase(Ease.Linear);
        panelRectTransform.DOScale(1, 0.3f).SetEase(Ease.OutBack);

    }

    public void XtoHide()
    {
        Hide();
    }


    public void Hide(PanelControllerHideDelegate hideDelegate = null)
    {
        if (GameManager.Instance._gameType != Constants.GameType.MultiPlay)
        {
            GameManager.Instance.ResumeGame();
        }
           
        _backCanvasGroup.alpha = 1;
        panelRectTransform.localScale = Vector3.one;
        _backImage.raycastTarget = false;
        _backCanvasGroup.DOFade(0, 0.3f).SetEase(Ease.Linear);
        panelRectTransform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() => {

            hideDelegate?.Invoke();
            Destroy(gameObject);
        });


    }

    protected void Shake()
    {
        panelRectTransform.DOShakeAnchorPos(0.3f);
    }
}
