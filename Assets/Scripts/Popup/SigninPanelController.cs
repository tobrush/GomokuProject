using TMPro;
using UnityEngine;
using UnityEngine.UI;

public struct SigninData
{
    public string username;
    public string password;
}

public struct SigninResult
{
    public int result;
    public string nickname;    // 닉네임
    public int level;          // 레벨
    public int coin;           // 코인
    public int score;          // 점수
}

public class SigninPanelController : PanelController
{
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;

    public void OnClickConfirmButton()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Shake();
            return;
        }

        var signinData = new SigninData();
        signinData.username = username;
        signinData.password = password;

        StartCoroutine(NetworkManager.Instance.Signin(signinData,
            () =>
            {
                Hide();

               // GameManager.Instance.NetworkLoggingPanel.SetActive(false);

               // GameManager.Instance.NetworkMyID.text = "ID : " + "MyID"; //TODO

              //  GameManager.Instance.NetworkUserPanel.SetActive(true);

               // GameManager.Instance.multiPlayBtn.interactable = true;
              //  GameManager.Instance.multiPlayBtn.GetComponent<Image>().sprite = GameManager.Instance.multiPlayOrange;

            },
            (result) =>
            {
                if (result == 0)
                {
                    GameManager.Instance.OpenConfirmPanel("유저네임이 유효하지 않습니다.",
                        () =>
                        {
                            usernameInputField.text = "";
                            passwordInputField.text = "";
                        });
                }
                else if (result == 1)
                {
                    GameManager.Instance.OpenConfirmPanel("패스워드가 유효하지 않습니다.",
                        () =>
                        {
                            passwordInputField.text = "";
                        });
                }
            }));
    }
}
