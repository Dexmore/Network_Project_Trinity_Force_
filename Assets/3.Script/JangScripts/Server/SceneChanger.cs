using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    public Button changeRegisterSceneButton;
    public Button changeUpdateSceneButton;
    public Button changeDeleteSceneButton;
    public string sceneName = "Register";
    public string sceneName1 = "Update";
    public string sceneName2 = "Delete";

    private void Start()
    {
        changeRegisterSceneButton.onClick.AddListener(ChangeRegisterScene);
        changeUpdateSceneButton.onClick.AddListener(ChangeUpdateScene);
        changeDeleteSceneButton.onClick.AddListener(ChangeDeleteScene);
    }

    private void ChangeRegisterScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    private void ChangeUpdateScene()
    {
        SceneManager.LoadScene(sceneName1);
    }

    private void ChangeDeleteScene()
    {
        SceneManager.LoadScene(sceneName2);
    }
}
