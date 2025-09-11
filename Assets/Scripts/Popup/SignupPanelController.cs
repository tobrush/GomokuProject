using TMPro;
using UnityEngine;

public struct SignupData
{
    public string username;
    public string password;
    public string nickname;
}

public struct SignupResult
{
    public string result;
}

public class SignupPanelController : PanelController
{
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField nicknameInputField;

    public void OnClickConfirmButton()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;
        string nickname = nicknameInputField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nickname))
        {
            Shake();
            return;
        }

        var signupData = new SignupData();
        signupData.username = username;
        signupData.password = password;
        signupData.nickname = nickname;

        StartCoroutine(NetworkManager.Instance.Signup(signupData,
            () =>
            {
                Hide();
                GameManager.Instance.OpenConfirmPanel("회원가입에 성공했습니다.",
                    () =>
                    {
                        Hide();
                    });

            },
            (resultString) =>
            {
                GameManager.Instance.OpenConfirmPanel(resultString,
                       () =>
                       {
                           Hide();
                       });
            }));
    }
}
