using UnityEngine;

public class MainPanelController : MonoBehaviour
{
    public void OnClickSinglePlayBtn()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.SinglePlay);
    }
    public void OnClickDualPlayBtn()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.DualPlay);
    }
    public void OnClickMultiPlayBtn()
    {
        GameManager.Instance.ChangeToGameScene(Constants.GameType.MultiPlay);
    }
    public void OnClickRecordBtn()
    {
        GameManager.Instance.ChangeToRecordScene();
    }
    public void OnClickSettingBtn()
    {

    }
    public void OnClickSigninBtn()
    {
        GameManager.Instance.OpenSigninPanel();
    }
    public void OnClickSignupBtn()
    {
        GameManager.Instance.OpenSignupPanel();
    }
}
