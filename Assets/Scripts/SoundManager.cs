using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static bool isSoundEnabled = true;

    public Button soundToggleButton;
    public GameObject iconOn;  // 🔊 Image visible quand le son est activé
    public GameObject iconOff; // 🔇 Image visible quand le son est coupé

    void Start()
    {
        isSoundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        UpdateIcons();

        soundToggleButton.onClick.AddListener(ToggleSound);
    }

    void ToggleSound()
    {
        isSoundEnabled = !isSoundEnabled;
        PlayerPrefs.SetInt("SoundEnabled", isSoundEnabled ? 1 : 0);
        PlayerPrefs.Save();

        UpdateIcons();
    }

    void UpdateIcons()
    {
        if (iconOn != null) iconOn.SetActive(isSoundEnabled);
        if (iconOff != null) iconOff.SetActive(!isSoundEnabled);
    }
}
