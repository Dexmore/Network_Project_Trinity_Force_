using System.Collections;
using UnityEngine;

    // 알림창 Fade in Fade out
    // interactable : 버튼, 슬라이더 등 UI 요소 등이 눌리는가?
    // true - 작동 
    // 만약 alpha가 0이고, interactable이 true면 보이지 않아도 클릭이 됨
    // false - 회색처리, 단순히 활성화, 눌리지 않음 (예시: 버튼이 보여지지만 눌러도 반응 없음)

    // blocksRaycasts : 오브젝트가 마우스 클릭이 터치 이벤트를 막는가?
    // true - 마우스 클릭을 막음 -> 뒤쪽의 UI 클릭 못함
    // false - 클릭 이벤트가 투명하게 통과 -> 뒤쪽의 UI 클릭 가능
public class UIFade : MonoBehaviour
{
    //public IEnumerator FadeCanvas(CanvasGroup canvas, float duration)
    //{
    //    float t = 0f;
    //    while (t < 0.2f)
    //    {
    //        t += Time.deltaTime;
    //        canvas.alpha = Mathf.Lerp(0f, 1f, t / 0.2f);
    //        yield return null;
    //    }

    //    canvas.alpha = 1f;
    //    canvas.interactable = canvas.blocksRaycasts = true;

    //    yield return new WaitForSeconds(duration);

    //    t = 0f;
    //    while (t < 0.2f)
    //    {
    //        t += Time.deltaTime;
    //        canvas.alpha = Mathf.Lerp(1f, 0f, t / 0.2f);
    //        yield return null;
    //    }
    //    canvas.alpha = 0f;
    //    canvas.interactable = canvas.blocksRaycasts = false;
    //}

    public IEnumerator FadeCanvas(CanvasGroup canvas, float from, float to, float duration)
    {
        float t = 0f;
        canvas.alpha = from;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;

        // 점점 alpha 변화
        while (t < duration)
        {
            t += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;  // 한 프레임씩
        }
        canvas.alpha = to;

        // 완료 후
        if (to > 0.9f) // to가 1이면 알파가 1 => true값 반환
        {
            canvas.interactable = true; // 버튼이 작동
            canvas.blocksRaycasts = true;   // 뒤의 UI 클릭 불가능
        }

        else
        {
            canvas.interactable = false;    // 버튼 작동 안됨
            canvas.blocksRaycasts = false;  // 뒤의 UI 클릭 가능
            canvas.gameObject.SetActive(false); // 완전히 사라질 때 비활성화
        }

    }

    public void FadeIn(GameObject pannel)
    {
        CanvasGroup canvas = pannel.GetComponent<CanvasGroup>();
        pannel.SetActive(true);
        StartCoroutine(FadeCanvas(canvas, 0f, 1f, 0.3f));
    }

    public void Fadeout(GameObject pannel)
    {
        CanvasGroup canvas = pannel.GetComponent<CanvasGroup>();
        StartCoroutine(FadeCanvas(canvas, 1f, 0f, 0.3f));
        //pannel.SetActive(false);
    }

}
