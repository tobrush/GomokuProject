using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;



public class NetworkManager : Singleton<NetworkManager>
{

    [Serializable]
    public class MyData
    {
        public string nickname;    // 닉네임
        public int level;          // 레벨
        public int coin;           // 코인
        public int score;          // 점수
    }
    public MyData myData;
    public bool IsLoggedIn { get; private set; } = false;

    //자동 로그인
    public IEnumerator AutoSignin(Action success, Action<string> failure)
    {
        using (UnityWebRequest www = new UnityWebRequest(Constants.ServerURL + "/users/auto-signin", UnityWebRequest.kHttpVerbGET))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // 로그인할 때 저장한 sid 쿠키를 전송
            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                failure?.Invoke("서버 연결 오류");
            }
            else
            {
                string resultString = www.downloadHandler.text;
                //Debug.Log("AutoSignin Response: " + resultString);

                if (www.responseCode == 200)
                {
                    var result = JsonUtility.FromJson<SigninResult>(resultString);

                    if (result.result == 2) // SUCCESS
                    {
                        ApplyUserData(result);
                        IsLoggedIn = true;

                        success?.Invoke();
                    }
                    else
                    {
                        failure?.Invoke("세션이 만료되었거나 유효하지 않습니다");
                    }
                }
                else
                {
                    failure?.Invoke(resultString);
                }
            }
        }
    }
    private void ApplyUserData(SigninResult result)
    {
        GameManager.Instance.myData.nickname = result.nickname;
        GameManager.Instance.myData.level = result.level;
        GameManager.Instance.myData.score = result.score;
        GameManager.Instance.myData.coin = result.coin;

        GameManager.Instance.UpdateUserUI();
    }

    // 로그인
    public IEnumerator Signin(SigninData signinData, Action success, Action<int> failure)
    {
        string jsonString = JsonUtility.ToJson(signinData);
        byte[] byteRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www = new UnityWebRequest(Constants.ServerURL + "/users/signin",
                   UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(byteRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                // TODO: 서버 연결 오류에 대해 알림
            }
            else
            {
                var resultString = www.downloadHandler.text;
                var result = JsonUtility.FromJson<SigninResult>(resultString);

               // Debug.Log(result.result);
                if (result.result == 2)
                {
                    ApplyUserData(result);

                    //로그인 성공

                    var headers = www.GetResponseHeaders();
                    if (headers != null && headers.ContainsKey("set-cookie"))
                    {
                        string cookie = headers["set-cookie"];

                        if (!string.IsNullOrEmpty(cookie))
                        {
                            int lastIndex = cookie.LastIndexOf(';');
                            string sid = cookie.Substring(0, lastIndex);
                            PlayerPrefs.SetString("sid", sid);
                            PlayerPrefs.Save();
                            Debug.Log("SID 저장 완료: " + sid);
                        }
                    }
                    /*
                    var cookie = www.GetResponseHeader("set-cookie");
                    
                    Debug.Log("cookie : " + cookie);
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        int lastIndex = cookie.LastIndexOf(';');
                        string sid = cookie.Substring(0, lastIndex);

                        // 저장
                        PlayerPrefs.SetString("sid", sid);
                        PlayerPrefs.Save();
                        Debug.Log(sid);
                    }
                    */
                    success?.Invoke();
                }
                else
                {
                    // 로그인 실패
                    failure?.Invoke(result.result);
                }
            }
        };
    }
    public IEnumerator Signup(SignupData signupData, Action success, Action<string> failure)
    {
        string jsonString = JsonUtility.ToJson(signupData);
        byte[] byteRaw = System.Text.Encoding.UTF8.GetBytes(jsonString);

        using (UnityWebRequest www = new UnityWebRequest(Constants.ServerURL + "/users/signup",
                   UnityWebRequest.kHttpVerbPOST))
        {
            www.uploadHandler = new UploadHandlerRaw(byteRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                // TODO: 서버 연결 오류에 대해 알림
            }
            else
            {
                var resultString = www.downloadHandler.text;
                Debug.Log(resultString);

          
                if (resultString == "회원가입이 완료되었습니다")
                {
                    success?.Invoke();
                }
                else
                {
                    failure?.Invoke(resultString);
                }
                
            }
        };
    }

    public IEnumerator Signout(Action success, Action<string> failure)
    {
        using (UnityWebRequest www = new UnityWebRequest(Constants.ServerURL + "/users/signout",
                   UnityWebRequest.kHttpVerbPOST))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // 로그인할 때 저장한 sid 쿠키 꺼내서 전송
            string sid = PlayerPrefs.GetString("sid", "");
            if (!string.IsNullOrEmpty(sid))
            {
                www.SetRequestHeader("Cookie", sid);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                failure?.Invoke("서버 연결 오류");
            }
            else
            {
                string resultString = www.downloadHandler.text;
                Debug.Log("Signout Response: " + resultString);

                if (www.responseCode == 200)
                {
                    // 세션이 삭제됐으니 클라이언트에서도 sid 삭제
                    PlayerPrefs.DeleteKey("sid");
                    PlayerPrefs.Save(); // 확실히 지우려면 호출
                    success?.Invoke();
                }
                else
                {
                    failure?.Invoke(resultString);
                }
            }
        }
    }





    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode) { }
}
