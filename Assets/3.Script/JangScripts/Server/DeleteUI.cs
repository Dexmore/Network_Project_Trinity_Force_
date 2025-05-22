using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class DeleteUI : MonoBehaviour
{
    public InputField usernameInput;
    public Button registerButton;
    public Button backSceneButton;
    public Text feedbackText;
    
    private void Start()
    {
        registerButton.onClick.AddListener(OnDeleteButtonClick);
        backSceneButton.onClick.AddListener(BackScene);
    }

    private void OnDeleteButtonClick()
    {
        string username = usernameInput.text;

        if (string.IsNullOrEmpty(username))
        {
            feedbackText.text = "��� �ʵ带 �Է����ּ���.";
            return;
        }

        bool isDelete = SQLManager1.instance.DeleteUser(username);
        if (isDelete)
        {
            feedbackText.text = "ȸ��Ż��";
            SceneManager.LoadScene("SelectScene");
        }
        else
        {
            feedbackText.text = "�������� �ʴ� ���̵��Դϴ�";
        }
    }

    private void BackScene()
    {
        SceneManager.LoadScene("SelectScene");
    }
}
