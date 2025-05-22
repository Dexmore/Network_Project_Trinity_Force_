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
            feedbackText.text = "모든 필드를 입력해주세요.";
            return;
        }

        bool isDelete = SQLManager1.instance.DeleteUser(username);
        if (isDelete)
        {
            feedbackText.text = "회원탈퇴";
            SceneManager.LoadScene("SelectScene");
        }
        else
        {
            feedbackText.text = "존재하지 않는 아이디입니다";
        }
    }

    private void BackScene()
    {
        SceneManager.LoadScene("SelectScene");
    }
}
