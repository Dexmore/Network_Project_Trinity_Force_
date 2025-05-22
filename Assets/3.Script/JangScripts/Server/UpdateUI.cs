using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UpdateUI : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public InputField phoneNumInput;
    public Button registerButton;
    public Button backSceneButton;
    public Text feedbackText;
    

    private void Start()
    {
        registerButton.onClick.AddListener(OnUpdateButtonClick);
        backSceneButton.onClick.AddListener(BackScene);
    }

    private void OnUpdateButtonClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;
        string phoneNum = phoneNumInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(phoneNum))
        {
            feedbackText.text = "��� �ʵ带 �Է����ּ���.";
            return;
        }

        bool isUpdate = SQLManager1.instance.UpdateUser(username, password, phoneNum);
        if (isUpdate)
        {
            feedbackText.text = "���� ����!";
            SceneManager.LoadScene("SelectScene");
        }
        else
        {
            feedbackText.text = "�������� ����.";
        }
    }

    private void BackScene()
    {
        SceneManager.LoadScene("SelectScene");
    }
}
