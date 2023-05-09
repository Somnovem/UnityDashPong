using System.Net;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class AuthorizeFromServer : MonoBehaviour
{
    #region UI
    public TMPro.TMP_InputField serverIP;
    public TMPro.TMP_InputField username;
    public TMPro.TMP_InputField password;
    public TMPro.TextMeshProUGUI statusText;

    public Button btnLogin;
    public Button btnSignup;
    public Button btnExit;
    #endregion
    private AuthorizationClient authorizator;
    private bool isLoginning;

    void Start()
    {
        authorizator = new AuthorizationClient();
        authorizator.ConnectionResult += ConnectionResultHandler;
    }

    public void ConnectionResultHandler(AuthorizationResult result)
    {
        switch(result)
        {
            case AuthorizationResult.Success:
                    Dispatcher.RunOnMainThread(()=>
                    {
                        SceneManager.LoadScene("LobbyScene");
                    });
                    Dispatcher.user = username.text;
                    break;
            case AuthorizationResult.InvalidCredentials:
                    string msg = isLoginning ? "Invalid login or password" : "Such username already exists...";
                    Dispatcher.RunOnMainThread(()=>
                    {
                        statusText.text = msg;
                        SetButtonsEnabled(true);
                    });
                    break;
            case AuthorizationResult.Banned:
                    Dispatcher.RunOnMainThread(()=>
                    {
                        statusText.text = "This user was banned permanently!";
                        SetButtonsEnabled(true);
                    });
                    break;
            case AuthorizationResult.AlreadyOnline:
                    Dispatcher.RunOnMainThread(()=>
                    {
                        statusText.text = "This account is already being used!";
                        SetButtonsEnabled(true);
                    });
                    break;
            case AuthorizationResult.ServerClosed:
                    Dispatcher.RunOnMainThread(()=>
                    {
                        statusText.text = "Server is closed.Try again later!";
                        SetButtonsEnabled(true);
                    });
                    break;
            default:
                    break;
        }
    }

    public void SignIn()
    {
        isLoginning = true;
        SetButtonsEnabled(false);
        if(CheckCredentialsFormat())authorizator.Login(username.text,password.text);
        else SetButtonsEnabled(true);
    }
    public void SignUp()
    {
        isLoginning = false;
        SetButtonsEnabled(false);
        if(CheckCredentialsFormat())authorizator.Signup(username.text,password.text);
        else SetButtonsEnabled(true);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public bool CheckCredentialsFormat()
    {
        IPAddress test;

        if(!IPAddress.TryParse(serverIP.text,out test))
        {
            statusText.text = "Invalid IP format";
            return false;
        }

        if(serverIP.text.StartsWith("127"))
        {
            statusText.text = "Can't use loopback IP";
            return false;
        }

        AuthorizationClient.serverIP = serverIP.text;
        if(username.text.Length < 8 || username.text.Length > 16)
        {
            statusText.text = "Invalid Login length";
            return false;
        }
        if(password.text.Length < 8 || password.text.Length > 20)
        {
            statusText.text = "Invalid Password length";
            return false;
        }
        string pattern = "^[A-Za-z0-9]+$";
        if(!Regex.IsMatch(username.text,pattern))
        {
            statusText.text = "Invalid Login format";
            return false;
        }
        if(!Regex.IsMatch(password.text,pattern))
        {
            statusText.text = "Invalid Password format";
            return false;
        }
        return true;
    }

    private void SetButtonsEnabled(bool isEnabled)
    {
        btnLogin.interactable = isEnabled;
        btnSignup.interactable = isEnabled;
        username.interactable = isEnabled;
        password.interactable = isEnabled;
        btnExit.interactable = isEnabled;
    }
}
