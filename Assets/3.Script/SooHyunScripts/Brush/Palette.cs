using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Palette : MonoBehaviour, IPointerDownHandler
{
    [Header("UI 오브젝트")]
    public Button PaletteButton;       // 팔레트 열기 버튼
    public Image RGB;                  // 팔레트 이미지 (Sprite가 들어간 Image)
    public RawImage colorPreview;      // 선택된 색상 미리보기 (선택사항)
    public TexturePainter painter;
    private void Start()
    {
        RGB.gameObject.SetActive(false);
        colorPreview.gameObject.SetActive(false);

        // 버튼 클릭 시 팔레트 활성화
        PaletteButton.onClick.AddListener(() =>
        {
            RGB.gameObject.SetActive(true);
            colorPreview.gameObject.SetActive(true);
        });
    }

    private void Update()
    {
        // 팔레트 켜져 있고 마우스 클릭했을 때
        if (RGB.gameObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            // UI 바깥 클릭 시 비활성화
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                RGB.gameObject.SetActive(false);
                colorPreview.gameObject.SetActive(false);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!RGB.gameObject.activeSelf) return;

        // RectTransform 기준 좌표 구하기
        RectTransform rectTransform = RGB.GetComponent<RectTransform>();
        Vector2 localCursor;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        Rect rect = rectTransform.rect;

        // 정규화된 좌표 (0 ~ 1)
        float normX = (localCursor.x - rect.x) / rect.width;
        float normY = (localCursor.y - rect.y) / rect.height;

        // 범위 제한
        normX = Mathf.Clamp01(normX);
        normY = Mathf.Clamp01(normY);

        // Sprite의 텍스처 가져오기
        Texture2D texture = RGB.sprite.texture;

        // 원래 Sprite가 사용하는 UV 영역 보정 (Sprite가 텍스처의 일부만 쓸 경우)
        Rect spriteRect = RGB.sprite.textureRect;
        float u = spriteRect.x / texture.width + normX * spriteRect.width / texture.width;
        float v = spriteRect.y / texture.height + normY * spriteRect.height / texture.height;

        // 색상 추출
        Color selectedColor = texture.GetPixelBilinear(u, v);
        Debug.Log("선택된 색상: " + selectedColor);

        // 색상 미리보기 (선택 사항)
        if (colorPreview != null)
        {
            colorPreview.color = selectedColor;
        }

        // 선택한 색상 브러시에 적용하려면 여기에 추가
        painter.SetBrushColor(selectedColor);
    }
}
