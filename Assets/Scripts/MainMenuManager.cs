using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

[System.Serializable]
public class DataMobilGarasi
{
    public string namaMobil;
    public Sprite gambarMobil;
    public int levelMinimalBuka = 0;
}

public class MainMenuManager : MonoBehaviour
{
    [Header("Referensi Panel UI")]
    public GameObject panelMenuUtama;
    public GameObject panelPilihLevel;
    public GameObject panelLoading;
    public GameObject panelGarasi;
    public GameObject panelTutorialMenu; // Referensi untuk Panel Tutorial di Menu

    [Header("Referensi Slider & Teks Loading")]
    public Slider sliderLoading;
    public TextMeshProUGUI teksProgress;

    [Header("Tombol Level")]
    public Button[] tombolLevel;

    [Header("Referensi Audio Utama")]
    public AudioSource musicSource;

    [Header("Referensi UI Garasi")]
    public Image tampilanGambarMobil;
    public Button tombolPilihMobil;
    public TextMeshProUGUI teksPilihMobil;
    public GameObject gembokMobil;
    public TextMeshProUGUI teksNamaMobil;
    public DataMobilGarasi[] daftarMobilGarasi;

    private int indeksMobilDilihat = 0;

    private void Start()
    {
        Application.targetFrameRate = PlayerPrefs.GetInt("Set_FPS60", 1) == 1 ? 60 : 30;
        bool nonaktifkanBayangan = PlayerPrefs.GetInt("Set_ShadowDisable", 0) == 1;
        Light[] semuaLampu = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light lampu in semuaLampu)
        {
            if (lampu.type == LightType.Directional)
            {
                lampu.shadows = nonaktifkanBayangan ? LightShadows.None : LightShadows.Soft;
            }
        }

        if (panelMenuUtama != null) panelMenuUtama.SetActive(true);
        if (panelPilihLevel != null) panelPilihLevel.SetActive(false);
        if (panelLoading != null) panelLoading.SetActive(false);
        if (panelGarasi != null) panelGarasi.SetActive(false);
        if (panelTutorialMenu != null) panelTutorialMenu.SetActive(false); // Sembunyikan tutorial saat start

        if (musicSource != null)
        {
            musicSource.volume = PlayerPrefs.GetFloat("Set_Music", 1f);
            musicSource.loop = true;
            musicSource.Play();
        }

        PerbaruiKunciLevel();
    }

    public void BukaPanelLevel()
    {
        if (panelMenuUtama != null) panelMenuUtama.SetActive(false);
        if (panelPilihLevel != null) panelPilihLevel.SetActive(true);
        PerbaruiKunciLevel();
    }

    public void KembaliKeMenuUtama()
    {
        if (panelPilihLevel != null) panelPilihLevel.SetActive(false);
        if (panelGarasi != null) panelGarasi.SetActive(false);
        if (panelTutorialMenu != null) panelTutorialMenu.SetActive(false);
        if (panelMenuUtama != null) panelMenuUtama.SetActive(true);
    }

    // Fungsi Baru untuk membuka panel tutorial dari menu
    public void BukaPanelTutorial()
    {
        if (panelMenuUtama != null) panelMenuUtama.SetActive(false);
        if (panelTutorialMenu != null) panelTutorialMenu.SetActive(true);
    }

    // Fungsi Baru untuk menutup panel tutorial kembali ke menu utama
    public void TutupPanelTutorial()
    {
        if (panelTutorialMenu != null) panelTutorialMenu.SetActive(false);
        if (panelMenuUtama != null) panelMenuUtama.SetActive(true);
    }

    private void PerbaruiKunciLevel()
    {
        int levelTerbuka = PlayerPrefs.GetInt("LevelAktif", 0);
        for (int i = 0; i < tombolLevel.Length; i++)
        {
            if (tombolLevel[i] != null)
            {
                bool terbukan = (i <= levelTerbuka);
                tombolLevel[i].interactable = terbukan;
                
                Transform gembok = tombolLevel[i].transform.Find("Gembok");
                if (gembok != null)
                {
                    gembok.gameObject.SetActive(!terbukan);
                }
            }
        }

        if (levelTerbuka >= 4 && GameObject.Find("Tombol_JelajahBebas") == null)
        {
            BuatTombolJelajahProsedural();
        }
    }

    public void PilihLevel(int indeksMisi)
    {
        PlayerPrefs.SetInt("LevelAktif", indeksMisi);
        PlayerPrefs.Save();
        MulaiMuatSceneAsync("Car Game");
    }

    public void MulaiGameNormal()
    {
        int levelTerbuka = PlayerPrefs.GetInt("LevelAktif", 0);
        PlayerPrefs.SetInt("LevelAktif", levelTerbuka);
        PlayerPrefs.Save();
        MulaiMuatSceneAsync("Car Game");
    }

    public void KeluarGame()
    {
        Application.Quit();
    }

    private void MulaiMuatSceneAsync(string namaScene)
    {
        if (panelLoading != null)
        {
            if (panelMenuUtama != null) panelMenuUtama.SetActive(false);
            if (panelPilihLevel != null) panelPilihLevel.SetActive(false);
            if (panelGarasi != null) panelGarasi.SetActive(false);
            if (panelTutorialMenu != null) panelTutorialMenu.SetActive(false);
            
            panelLoading.SetActive(true);
            StartCoroutine(ProsesMuatScene(namaScene));
        }
        else
        {
            SceneManager.LoadScene(namaScene);
        }
    }

    private IEnumerator ProsesMuatScene(string namaScene)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(namaScene);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);
            if (sliderLoading != null) sliderLoading.value = progress;
            if (teksProgress != null) teksProgress.text = $"LOADING {Mathf.RoundToInt(progress * 100f)}%";

            if (op.progress >= 0.9f)
            {
                yield return new WaitForSecondsRealtime(0.8f);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    public void UpdateVolumeMusik(float volumeBaru)
    {
        if (musicSource != null) musicSource.volume = volumeBaru;
    }

    public void BukaPanelGarasi()
    {
        if (panelMenuUtama != null) panelMenuUtama.SetActive(false);
        if (panelGarasi != null) panelGarasi.SetActive(true);
        indeksMobilDilihat = PlayerPrefs.GetInt("MobilAktif", 0);
        PerbaruiTampilanGarasi();
    }

    public void MobilBerikutnya()
    {
        if (daftarMobilGarasi == null || daftarMobilGarasi.Length == 0) return;
        indeksMobilDilihat = (indeksMobilDilihat + 1) % daftarMobilGarasi.Length;
        PerbaruiTampilanGarasi();
    }

    public void MobilSebelumnya()
    {
        if (daftarMobilGarasi == null || daftarMobilGarasi.Length == 0) return;
        indeksMobilDilihat--;
        if (indeksMobilDilihat < 0) indeksMobilDilihat = daftarMobilGarasi.Length - 1;
        PerbaruiTampilanGarasi();
    }

    public void PilihMobil()
    {
        PlayerPrefs.SetInt("MobilAktif", indeksMobilDilihat);
        PlayerPrefs.Save();
        PerbaruiTampilanGarasi();
    }

    private void PerbaruiTampilanGarasi()
    {
        if (daftarMobilGarasi == null || daftarMobilGarasi.Length == 0) return;

        if (tampilanGambarMobil != null)
        {
            tampilanGambarMobil.sprite = daftarMobilGarasi[indeksMobilDilihat].gambarMobil;
        }

        if (teksNamaMobil != null)
        {
            teksNamaMobil.text = daftarMobilGarasi[indeksMobilDilihat].namaMobil;
        }

        int levelTerbuka = PlayerPrefs.GetInt("LevelAktif", 0);
        bool isTerbuka = (daftarMobilGarasi[indeksMobilDilihat].levelMinimalBuka <= levelTerbuka + 1);

        if (isTerbuka)
        {
            if (gembokMobil != null) gembokMobil.SetActive(false);

            int mobilTerpakai = PlayerPrefs.GetInt("MobilAktif", 0);
            if (indeksMobilDilihat == mobilTerpakai)
            {
                if (teksPilihMobil != null) teksPilihMobil.text = "TERPAKAI";
                if (tombolPilihMobil != null) tombolPilihMobil.interactable = false;
            }
            else
            {
                if (teksPilihMobil != null) teksPilihMobil.text = "PILIH";
                if (tombolPilihMobil != null) tombolPilihMobil.interactable = true;
            }
        }
        else
        {
            if (gembokMobil != null) gembokMobil.SetActive(true);
            if (teksPilihMobil != null) teksPilihMobil.text = "TERKUNCI";
            if (tombolPilihMobil != null) tombolPilihMobil.interactable = false;
        }
    }

    private void BuatTombolJelajahProsedural()
    {
        if (panelPilihLevel == null) return;

        GameObject btnObj = new GameObject("Tombol_JelajahBebas");
        btnObj.transform.SetParent(panelPilihLevel.transform, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(530, 60);
        rect.anchoredPosition = new Vector2(0, -180);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0f, 0.5f, 0f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(() => {
            PlayerPrefs.SetInt("LevelAktif", 99);
            PlayerPrefs.Save();
            MulaiMuatSceneAsync("Car Game");
        });

        GameObject txtObj = new GameObject("Teks");
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform rectTxt = txtObj.AddComponent<RectTransform>();
        rectTxt.anchorMin = Vector2.zero;
        rectTxt.anchorMax = Vector2.one;
        rectTxt.sizeDelta = Vector2.zero;

        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = "EKSPLORASI BEBAS";
        txt.fontSize = 20;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;
    }
}