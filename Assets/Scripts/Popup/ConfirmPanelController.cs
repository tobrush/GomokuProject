using TMPro;
using UnityEngine;

public class ConfirmPanelController : PanelController
{
    [SerializeField] private TMP_Text messageText;

    public delegate void OnConfirmButtonClicked();

    private OnConfirmButtonClicked _onConfirmButtonClicked;
    
    public void Show(string message, OnConfirmButtonClicked onConfirmButtonClicked)
    {
        _onConfirmButtonClicked = onConfirmButtonClicked;
        messageText.text = message;
        base.Show();
    }
    public void OnClickConfirmBtn()
    {
        Hide(() =>
        {
            _onConfirmButtonClicked?.Invoke();
        });
    }

    public void OnClickCloseBtn()
    {
        Hide();
    }

}
