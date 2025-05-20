using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviour
{
    [Header("UI")]
    public Image Clock_Filled;

    [Header("타이머 설정")]
    public float time_limit;
    private float time_start = 0f;
    private bool isClick = false;

    private void Update()
    {
        if (Clock_Filled == null) return;

        time_start += Time.deltaTime;
        float t = Mathf.Clamp01(time_start / time_limit);
        Clock_Filled.fillAmount = t;

        if (t == 1f || isClick)
        {
            isClick = false;
            time_start = 0f;

            GoToNextScene();
        }
    }

    private void GoToNextScene()
    {
        if (SceneManager.GetActiveScene().name == "TextInputScene")
        {
            SceneManager.LoadScene("DrawScene");
        }
        else
        {
            SceneManager.LoadScene("TextInputScene");
        }
    }
}
