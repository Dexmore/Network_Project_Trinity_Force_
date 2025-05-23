using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public static VolumeController Instance;

    [Header("BGM")]
    public AudioSource bgmSource;
    public Slider volumeSlider;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 저장된 볼륨 불러오기
        float savedVolume = PlayerPrefs.GetFloat("BGM", 1.0f);
        SetVolume(savedVolume);

        // 슬라이더 설정 및 이벤트 연결
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // BGM 재생 시작
        if (!bgmSource.isPlaying)
            bgmSource.Play();
    }

    public void SetVolume(float value)
    {
        float adjusted = Mathf.Pow(value, 0.5f); // 부드럽게 감소
        bgmSource.volume = adjusted;

        PlayerPrefs.SetFloat("BGM", value);
        PlayerPrefs.Save();
    }
}
