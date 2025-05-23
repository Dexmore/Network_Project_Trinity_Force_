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
        // ����� ���� �ҷ�����
        float savedVolume = PlayerPrefs.GetFloat("BGM", 1.0f);
        SetVolume(savedVolume);

        // �����̴� ���� �� �̺�Ʈ ����
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // BGM ��� ����
        if (!bgmSource.isPlaying)
            bgmSource.Play();
    }

    public void SetVolume(float value)
    {
        float adjusted = Mathf.Pow(value, 0.5f); // �ε巴�� ����
        bgmSource.volume = adjusted;

        PlayerPrefs.SetFloat("BGM", value);
        PlayerPrefs.Save();
    }
}
