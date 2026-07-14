using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingManager : MonoBehaviour
{
    [Header("Referensi Audio")]
    public Slider sliderSFX;
    public Slider sliderMusic;
    public TMP_Dropdown dropdownPlaylist;

    [Header("Referensi Gameplay & Grafis")]
    public Toggle toggleFPS;
    public Toggle toggleDriftAssist;
    public Toggle toggleShadow;

    [Header("Konfirmasi UI")]
    public GameObject panelKonfirmasi;

    private const string PREF_SFX = "Set_SFX";
    private const string PREF_MUSIC = "Set_Music";
    private const string PREF_PLAYLIST = "Set_Playlist";
    private const string PREF_FPS = "Set_FPS60";
    private const string PREF_DRIFT = "Set_DriftAssist";
    private const string PREF_SHADOW = "Set_ShadowDisable";

    private void Start()
    {
        MuatPengaturanTersimpan();
        TambahkanListenerKeUI();
        if (panelKonfirmasi != null) panelKonfirmasi.SetActive(false);
    }

    private void MuatPengaturanTersimpan()
    {
        float volSFX = PlayerPrefs.GetFloat(PREF_SFX, 1f);
        float volMusic = PlayerPrefs.GetFloat(PREF_MUSIC, 1f);

        if (sliderSFX != null) sliderSFX.value = volSFX;
        if (sliderMusic != null) sliderMusic.value = volMusic;
        if (dropdownPlaylist != null) dropdownPlaylist.value = PlayerPrefs.GetInt(PREF_PLAYLIST, 0);

        if (toggleFPS != null) toggleFPS.isOn = PlayerPrefs.GetInt(PREF_FPS, 1) == 1;
        if (toggleDriftAssist != null) toggleDriftAssist.isOn = PlayerPrefs.GetInt(PREF_DRIFT, 1) == 1;
        if (toggleShadow != null) toggleShadow.isOn = PlayerPrefs.GetInt(PREF_SHADOW, 0) == 1;

        CarControllerPro mobilAktif = FindAnyObjectByType<CarControllerPro>();
        if (mobilAktif != null) mobilAktif.SetVolumeSFX(volSFX);

        MainMenuManager menuManager = FindAnyObjectByType<MainMenuManager>();
        if (menuManager != null) menuManager.UpdateVolumeMusik(volMusic);

        UbahFPS(PlayerPrefs.GetInt(PREF_FPS, 1) == 1);
        UbahShadow(PlayerPrefs.GetInt(PREF_SHADOW, 0) == 1);
    }

    private void TambahkanListenerKeUI()
    {
        if (sliderSFX != null) sliderSFX.onValueChanged.AddListener(UbahVolumeSFX);
        if (sliderMusic != null) sliderMusic.onValueChanged.AddListener(UbahVolumeMusic);
        if (dropdownPlaylist != null) dropdownPlaylist.onValueChanged.AddListener(UbahPlaylist);
        
        if (toggleFPS != null) toggleFPS.onValueChanged.AddListener(UbahFPS);
        if (toggleDriftAssist != null) toggleDriftAssist.onValueChanged.AddListener(UbahDriftAssist);
        if (toggleShadow != null) toggleShadow.onValueChanged.AddListener(UbahShadow);
    }

    public void UbahVolumeSFX(float nilai)
    {
        PlayerPrefs.SetFloat(PREF_SFX, nilai);
        PlayerPrefs.Save();
        
        if (GameManager.Instance != null) GameManager.Instance.UpdateVolumeSFX(nilai);
        
        CarControllerPro mobilAktif = FindAnyObjectByType<CarControllerPro>();
        if (mobilAktif != null) mobilAktif.SetVolumeSFX(nilai);
    }

    public void UbahVolumeMusic(float nilai)
    {
        PlayerPrefs.SetFloat(PREF_MUSIC, nilai);
        PlayerPrefs.Save();
        if (GameManager.Instance != null) GameManager.Instance.UpdateVolumeMusik(nilai);
        
        MainMenuManager menuManager = FindAnyObjectByType<MainMenuManager>();
        if (menuManager != null) menuManager.UpdateVolumeMusik(nilai);
    }

    public void UbahPlaylist(int index)
    {
        PlayerPrefs.SetInt(PREF_PLAYLIST, index);
        PlayerPrefs.Save();
        if (GameManager.Instance != null) GameManager.Instance.GantiLagu(index);
    }

    public void UbahFPS(bool aktif60Fps)
    {
        PlayerPrefs.SetInt(PREF_FPS, aktif60Fps ? 1 : 0);
        PlayerPrefs.Save();
        Application.targetFrameRate = aktif60Fps ? 60 : 30;
    }

    public void UbahDriftAssist(bool aktif)
    {
        PlayerPrefs.SetInt(PREF_DRIFT, aktif ? 1 : 0);
        PlayerPrefs.Save();
        if (CarControllerPro.ActiveInstance != null) CarControllerPro.ActiveInstance.gunakanDriftAssist = aktif;
    }

    public void UbahShadow(bool nonaktifkan)
    {
        PlayerPrefs.SetInt(PREF_SHADOW, nonaktifkan ? 1 : 0);
        PlayerPrefs.Save();

        Light[] semuaLampu = FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (Light lampu in semuaLampu)
        {
            if (lampu.type == LightType.Directional)
            {
                lampu.shadows = nonaktifkan ? LightShadows.None : LightShadows.Soft;
            }
        }
    }

    public void HapusDataGame()
    {
        if (panelKonfirmasi != null) panelKonfirmasi.SetActive(true);
    }

    public void EksekusiHapusData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Time.timeScale = 1f; 
        SceneManager.LoadScene("Main Menu"); 
    }

    public void BatalHapusData()
    {
        if (panelKonfirmasi != null) panelKonfirmasi.SetActive(false);
    }
}