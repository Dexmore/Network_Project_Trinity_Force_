using System.Collections;
using UnityEngine;

    // �˸�â Fade in Fade out
    // interactable : ��ư, �����̴� �� UI ��� ���� �����°�?
    // true - �۵� 
    // ���� alpha�� 0�̰�, interactable�� true�� ������ �ʾƵ� Ŭ���� ��
    // false - ȸ��ó��, �ܼ��� Ȱ��ȭ, ������ ���� (����: ��ư�� ���������� ������ ���� ����)

    // blocksRaycasts : ������Ʈ�� ���콺 Ŭ���� ��ġ �̺�Ʈ�� ���°�?
    // true - ���콺 Ŭ���� ���� -> ������ UI Ŭ�� ����
    // false - Ŭ�� �̺�Ʈ�� �����ϰ� ��� -> ������ UI Ŭ�� ����
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

        // ���� alpha ��ȭ
        while (t < duration)
        {
            t += Time.deltaTime;
            canvas.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;  // �� �����Ӿ�
        }
        canvas.alpha = to;

        // �Ϸ� ��
        if (to > 0.9f) // to�� 1�̸� ���İ� 1 => true�� ��ȯ
        {
            canvas.interactable = true; // ��ư�� �۵�
            canvas.blocksRaycasts = true;   // ���� UI Ŭ�� �Ұ���
        }

        else
        {
            canvas.interactable = false;    // ��ư �۵� �ȵ�
            canvas.blocksRaycasts = false;  // ���� UI Ŭ�� ����
            canvas.gameObject.SetActive(false); // ������ ����� �� ��Ȱ��ȭ
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
