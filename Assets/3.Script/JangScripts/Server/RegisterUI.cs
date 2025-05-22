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
            feedbackText.text = "��� �ʵ带 �Է����ּ���.";
            return;
        }

        bool isRegistered = SQLManager1.instance.Register(username, password, phoneNum);
        if (isRegistered)
        {
            feedbackText.text = "ȸ������ ����!";
            SceneManager.LoadScene("SelectScene");
        }
        else
        {
            feedbackText.text = "ȸ������ ����. �ٽ� �õ����ּ���.";
        }
    }

    private void BackScene()
    {
        SceneManager.LoadScene("SelectScene");
    }
}
