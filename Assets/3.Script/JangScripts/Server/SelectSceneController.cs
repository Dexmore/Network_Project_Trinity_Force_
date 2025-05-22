using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SelectSceneController : MonoBehaviour
{
    public void SceneLoad(string name)
    {
        SceneManager.LoadScene(name);
    }
}
