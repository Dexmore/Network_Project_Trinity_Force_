using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class RegisterUI : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public InputField phoneNumInput;
    public Button registerButton;
    public Button backSceneButton;
    public Text feedbackText;

    private void Start()
    {
        registerButton.onClick.AddListener(OnRegisterButtonClick);
        backSceneButton.onClick.AddListener(BackScene);
    }

    private void OnRegisterButtonClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        string phoneNum = phoneNumInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(phoneNum))
        {
            feedbackText.text = "모든 필드를 입력해주세요.";
            return;
        }

        bool isRegistered = SQLManager1.instance.Register(username, password, phoneNum);
        if (isRegistered)
        {
            feedbackText.text = "회원가입 성공!";
            SceneManager.LoadScene("SelectScene");
        }
        else
        {
            feedbackText.text = "회원가입 실패. 다시 시도해주세요.";
        }
    }

    private void BackScene()
    {
        SceneManager.LoadScene("SelectScene");
    }
}
