using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public class DataMisi
{
    public string namaMisi;
    public GameObject folderMisi; 
    public Transform titikStartMisi; 
    public Transform targetPanahAwal; 
    public bool gunakanWaktu;
    public float waktuBatasDetik;
    public int targetJumlahBarang;
    public bool selesaiInstanSaatTargetTercapai;
    public string namaSatuanBarang;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public DataMisi[] daftarMisi;
    public int misiAktifSaatIni = 0;

    public GameObject[] daftarMobil;
    private int indeksMobilAktif = 0;
    public Transform objekFollowKamera;

    [Header("Syarat Level Buka Mobil")]
    public int[] levelMinimalBukaMobil;

    [Header("Pengaturan Audio")]
    public AudioClip[] daftarLaguTheme;
    public AudioSource musicSource; 
    public AudioSource sfxSource;
    public AudioClip sfxKoin;
    public AudioClip sfxKlikUI;

    public TextMeshProUGUI teksMulai;        
    public TextMeshProUGUI teksSelesai;   
    public TextMeshProUGUI teksKecepatan;    
    public TextMeshProUGUI teksWaktu;   
    public TextMeshProUGUI teksBarang;  
    
    public Transform kameraMinimap; 
    public GameObject layarMinimap;
    public float tinggiMinimap = 150f; 

    public GameObject tombolUtilitasUI;
    public GameObject panelKontrolMobile;

    public bool gameSudahMulai = false;
    public bool gameSelesai = false;

    private float sisaWaktu = 0f;
    private int barangTerkumpul = 0;
    private TextMeshProUGUI labelTombolUtilitas;

    private Vector3 posisiAwalGarisStart;
    private Quaternion rotasiAwalGarisStart;
    private bool awalGarisStartTercatat = false;

    private AudioSource musikLatar;
    private int sisaWaktuDetikSebelumnya = -1;
    private int kecepatanSebelumnya = -1;

    private GameObject panelTamatProsedural;

    private int indeksLevelEksplorasiBebas = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        Application.targetFrameRate = 60;
        Time.timeScale = 1f;
    }

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

        musikLatar = GetComponent<AudioSource>();
        if (musikLatar != null)
        {
            musikLatar.volume = PlayerPrefs.GetFloat("Set_Music", 1f);
            int indeksLagu = PlayerPrefs.GetInt("Set_Playlist", 0);
            GantiLagu(indeksLagu);
        }

        if (sfxSource != null)
        {
            sfxSource.volume = PlayerPrefs.GetFloat("Set_SFX", 1f);
        }

        if (daftarMobil == null || daftarMobil.Length == 0) return;

        if (daftarMobil[0] != null)
        {
            posisiAwalGarisStart = daftarMobil[0].transform.position;
            rotasiAwalGarisStart = daftarMobil[0].transform.rotation;
            awalGarisStartTercatat = true;
        }

        misiAktifSaatIni = PlayerPrefs.GetInt("LevelAktif", 0);

        if (misiAktifSaatIni == 99)
        {
            if (daftarMisi != null && daftarMisi.Length > 0)
            {
                indeksLevelEksplorasiBebas = Random.Range(0, daftarMisi.Length);

                for (int i = 0; i < daftarMisi.Length; i++)
                {
                    if (daftarMisi[i].folderMisi != null)
                    {
                        daftarMisi[i].folderMisi.SetActive(i == indeksLevelEksplorasiBebas);
                    }
                }

                if (daftarMisi[indeksLevelEksplorasiBebas].folderMisi != null)
                {
                    BarangMisi[] semuaBarang = daftarMisi[indeksLevelEksplorasiBebas].folderMisi.GetComponentsInChildren<BarangMisi>(true);
                    foreach (BarangMisi barang in semuaBarang)
                    {
                        barang.gameObject.SetActive(false);
                    }

                    GarisFinishMisi[] semuaFinis = daftarMisi[indeksLevelEksplorasiBebas].folderMisi.GetComponentsInChildren<GarisFinishMisi>(true);
                    foreach (GarisFinishMisi finis in semuaFinis)
                    {
                        finis.gameObject.SetActive(false);
                    }
                }
            }
            
            InisialisasiPosPosition();

            for (int i = 0; i < daftarMobil.Length; i++)
            {
                if (daftarMobil[i] != null) daftarMobil[i].SetActive(i == indeksMobilAktif);
            }
            
            PasangKameraKeMobilAktif();

            UpdatePanahNavigasi(null);

            if (tombolUtilitasUI != null)
            {
                labelTombolUtilitas = tombolUtilitasUI.GetComponentInChildren<TextMeshProUGUI>();
                if (labelTombolUtilitas != null) labelTombolUtilitas.text = "Ganti Mobil";
            }

            if (teksMulai != null)
            {
                teksMulai.text = "<b><color=#FFD700>MODE JELAJAH BEBAS</color></b>\n<size=70%>Nikmati mengemudi santai tanpa batas!</size>\n<size=50%>Tap untuk mulai</size>";
                teksMulai.gameObject.SetActive(true);
            }
            if (teksSelesai != null) teksSelesai.gameObject.SetActive(false);
            if (teksWaktu != null) teksWaktu.gameObject.SetActive(false);
            if (teksBarang != null) teksBarang.gameObject.SetActive(false);

            if (tombolUtilitasUI != null) tombolUtilitasUI.SetActive(true);
            if (panelKontrolMobile != null) panelKontrolMobile.SetActive(false);
            
            return;
        }

        if (misiAktifSaatIni >= daftarMisi.Length && daftarMisi.Length > 0)
        {
            misiAktifSaatIni = daftarMisi.Length - 1; 
            PlayerPrefs.SetInt("LevelAktif", misiAktifSaatIni);
        }

        InisialisasiPosPosition();
        if (daftarMisi.Length > 0) MuatMisi(misiAktifSaatIni);

        for (int i = 0; i < daftarMobil.Length; i++)
        {
            if (daftarMobil[i] != null) daftarMobil[i].SetActive(i == indeksMobilAktif);
        }
        
        PasangKameraKeMobilAktif();

        if (tombolUtilitasUI != null)
        {
            labelTombolUtilitas = tombolUtilitasUI.GetComponentInChildren<TextMeshProUGUI>();
            if (labelTombolUtilitas != null) labelTombolUtilitas.text = "Ganti Mobil";
        }

        if (teksMulai != null) teksMulai.gameObject.SetActive(true);
        if (teksSelesai != null) teksSelesai.gameObject.SetActive(false);
        if (teksWaktu != null) teksWaktu.gameObject.SetActive(false);
        if (teksBarang != null) teksBarang.gameObject.SetActive(false);

        if (tombolUtilitasUI != null) tombolUtilitasUI.SetActive(true);
        if (panelKontrolMobile != null) panelKontrolMobile.SetActive(false);
    }

    private void Update()
    {
        if (gameSelesai) return;

        if (!gameSudahMulai)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.cKey.wasPressedThisFrame) GantiMobil();
                if (Keyboard.current.enterKey.wasPressedThisFrame) MulaiTancapGas();
            }

            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            {
                if (!IsPointerOverUI()) MulaiTancapGas();
            }
        }
        else
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetMobilAktif();
            }

            if (misiAktifSaatIni != 99 && daftarMisi.Length > 0 && daftarMisi[misiAktifSaatIni].gunakanWaktu)
            {
                sisaWaktu -= Time.deltaTime;
                UpdateUIWaktu();
                if (sisaWaktu <= 0) MisiGagal();
            }
        }

        UpdateKecepatanUI();
    }

    private void LateUpdate()
    {
        UpdatePosisiMinimap();
    }

    private void InisialisasiPosPosition()
    {
        Vector3 posisiStart = Vector3.zero;
        Quaternion rotasiStart = Quaternion.identity;
        bool titikValid = false;

        if (misiAktifSaatIni == 99)
        {
            int indexGunakan = indeksLevelEksplorasiBebas; 
            if (indexGunakan < daftarMisi.Length && daftarMisi[indexGunakan] != null && daftarMisi[indexGunakan].titikStartMisi != null)
            {
                posisiStart = daftarMisi[indexGunakan].titikStartMisi.position;
                rotasiStart = daftarMisi[indexGunakan].titikStartMisi.rotation;
                titikValid = true;
            }
            else if (daftarMisi != null && daftarMisi.Length > 0 && daftarMisi[0].titikStartMisi != null)
            {
                posisiStart = daftarMisi[0].titikStartMisi.position;
                rotasiStart = daftarMisi[0].titikStartMisi.rotation;
                titikValid = true;
            }
        }
        else if (daftarMisi != null && misiAktifSaatIni < daftarMisi.Length)
        {
            DataMisi misi = daftarMisi[misiAktifSaatIni];
            if (misi != null && misi.titikStartMisi != null)
            {
                posisiStart = misi.titikStartMisi.position;
                rotasiStart = misi.titikStartMisi.rotation;
                titikValid = true;
            }
        }

        if (!titikValid && awalGarisStartTercatat)
        {
            posisiStart = posisiAwalGarisStart;
            rotasiStart = rotasiAwalGarisStart;
            titikValid = true;
        }

        if (!titikValid) return;

        for (int i = 0; i < daftarMobil.Length; i++)
        {
            if (daftarMobil[i] != null)
            {
                GameObject mobil = daftarMobil[i];
                Rigidbody rb = mobil.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.isKinematic = true; 
                    mobil.transform.position = posisiStart;
                    mobil.transform.rotation = rotasiStart;
                    rb.isKinematic = false; 
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                else
                {
                    mobil.transform.position = posisiStart;
                    mobil.transform.rotation = rotasiStart;
                }

                CarControllerPro controller = mobil.GetComponentInChildren<CarControllerPro>(true);
                if (controller != null) controller.AturPosisiAwal(posisiStart, rotasiStart);
            }
        }
        Physics.SyncTransforms();
    }

    public void MulaiTancapGas()
    {
        if (gameSudahMulai) return;

        gameSudahMulai = true;
        if (teksMulai != null) teksMulai.gameObject.SetActive(false);
        if (labelTombolUtilitas != null) labelTombolUtilitas.text = "Reset";
        if (panelKontrolMobile != null) panelKontrolMobile.SetActive(true);

        if (misiAktifSaatIni != 99 && daftarMisi.Length > 0)
        {
            if (daftarMisi[misiAktifSaatIni].gunakanWaktu && teksWaktu != null) teksWaktu.gameObject.SetActive(true);
            if (daftarMisi[misiAktifSaatIni].targetJumlahBarang > 0 && teksBarang != null) teksBarang.gameObject.SetActive(true);
        }
    }

    public void MuatMisi(int indeksMisi)
    {
        DataMisi misi = daftarMisi[indeksMisi];
        barangTerkumpul = 0;
        sisaWaktu = misi.waktuBatasDetik;
        UpdateUIBarang();

        for (int i = 0; i < daftarMisi.Length; i++)
        {
            if (daftarMisi[i].folderMisi != null) daftarMisi[i].folderMisi.SetActive(i == indeksMisi);
        }

        if (misi.titikStartMisi != null)
        {
            for (int i = 0; i < daftarMobil.Length; i++)
            {
                if (daftarMobil[i] != null)
                {
                    GameObject mobil = daftarMobil[i];
                    Rigidbody rb = mobil.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        mobil.transform.position = misi.titikStartMisi.position;
                        mobil.transform.rotation = misi.titikStartMisi.rotation;
                        rb.isKinematic = false; 
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }
                    else
                    {
                        mobil.transform.position = misi.titikStartMisi.position;
                        mobil.transform.rotation = misi.titikStartMisi.rotation;
                    }

                    CarControllerPro controller = mobil.GetComponentInChildren<CarControllerPro>(true);
                    if (controller != null) controller.AturPosisiAwal(misi.titikStartMisi.position, misi.titikStartMisi.rotation);
                }
            }
            Physics.SyncTransforms();
        }

        if (misi.targetPanahAwal != null) UpdatePanahNavigasi(misi.targetPanahAwal);
        else
        {
            Transform koinTerdekat = CariKoinTerdekat();
            if (koinTerdekat != null) UpdatePanahNavigasi(koinTerdekat);
        }
    }

    public void TambahBarang(Transform targetPanahBerikutnya)
    {
        if (misiAktifSaatIni == 99) return;

        barangTerkumpul++;
        UpdateUIBarang();
        MainkanSFX(sfxKoin, 1f);

        if (CekApakahBarangCukup() && daftarMisi[misiAktifSaatIni].selesaiInstanSaatTargetTercapai)
        {
            MisiSelesai();
            return;
        }
        
        if (targetPanahBerikutnya != null) UpdatePanahNavigasi(targetPanahBerikutnya);
        else
        {
            Transform koinTerdekat = CariKoinTerdekat();
            if (koinTerdekat != null) UpdatePanahNavigasi(koinTerdekat);
        }
    }

    public bool CekApakahBarangCukup()
    {
        if (daftarMisi.Length == 0 || misiAktifSaatIni == 99) return true;
        return barangTerkumpul >= daftarMisi[misiAktifSaatIni].targetJumlahBarang;
    }

    public void MisiSelesai()
    {
        if (gameSelesai || misiAktifSaatIni == 99) return; 
        
        gameSelesai = true;
        MatikanUIBermain();

        int levelSelanjutnya = misiAktifSaatIni + 1;

        if (levelSelanjutnya >= daftarMisi.Length)
        {
            PlayerPrefs.SetInt("LevelAktif", levelSelanjutnya);
            PlayerPrefs.Save();
            BuatPanelTamatProsedural();
        }
        else
        {
            PlayerPrefs.SetInt("LevelAktif", levelSelanjutnya);
            PlayerPrefs.Save();

            if (teksSelesai != null)
            {
                teksSelesai.text = "<b><color=#FFD700><size=150%>MISSION COMPLETE!</size></color></b>\n" + 
                                 "<size=80%><color=#FFFFFF>Misi Berhasil Diselesaikan</color></size>\n" +
                                 "<size=50%><color=#AAAAAA>Memuat level berikutnya...</color></size>";
                teksSelesai.alignment = TextAlignmentOptions.Center;
                teksSelesai.gameObject.SetActive(true);
            }

            StartCoroutine(ProsesMuatUlang(4f));
        }
    }

    public void MisiGagal()
    {
        if (gameSelesai) return;
        gameSelesai = true;
        MatikanUIBermain();

        if (teksSelesai != null)
        {
            teksSelesai.text = "<b><color=#FF0000><size=150%>MISSION FAILED!</size></color></b>\n" + 
                               "<size=80%><color=#FFFFFF>Waktu Telah Habis</color></size>\n" +
                               "<size=50%><color=#AAAAAA>Mengulang misi...</color></size>";
            teksSelesai.alignment = TextAlignmentOptions.Center;
            teksSelesai.gameObject.SetActive(true);
        }
        StartCoroutine(ProsesMuatUlang(3f));
    }

    public void MisiGagalKarenaBarangKurang()
    {
        if (gameSelesai) return;
        gameSelesai = true;
        MatikanUIBermain();

        if (teksSelesai != null)
        {
            teksSelesai.text = "<b><color=#FF0000><size=150%>MISSION FAILED!</size></color></b>\n" + 
                               "<size=80%><color=#FFFFFF>Barang misi tidak cukup</color></size>\n" +
                               "<size=50%><color=#AAAAAA>Mengulang misi...</color></size>";
            teksSelesai.alignment = TextAlignmentOptions.Center;
            teksSelesai.gameObject.SetActive(true);
        }
        StartCoroutine(ProsesMuatUlang(3f));
    }

    private void MatikanUIBermain()
    {
        if (musicSource != null) musicSource.Stop();
        if (teksKecepatan != null) teksKecepatan.gameObject.SetActive(false);
        if (teksBarang != null) teksBarang.gameObject.SetActive(false);
        if (teksWaktu != null) teksWaktu.gameObject.SetActive(false);
        if (kameraMinimap != null) kameraMinimap.gameObject.SetActive(false);
        if (layarMinimap != null) layarMinimap.SetActive(false);
        if (tombolUtilitasUI != null) tombolUtilitasUI.SetActive(false);
    }

    private System.Collections.IEnumerator ProsesMuatUlang(float jeda)
    {
        yield return new WaitForSeconds(jeda);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    private System.Collections.IEnumerator ProsesKembaliKeMainMenu(float jeda)
    {
        yield return new WaitForSeconds(jeda);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }

    public void UpdatePanahNavigasi(Transform targetBaru)
    {
        for (int i = 0; i < daftarMobil.Length; i++)
        {
            if (daftarMobil[i] != null)
            {
                PenunjukArah panah = daftarMobil[i].GetComponentInChildren<PenunjukArah>();
                if (panah != null) 
                {
                    panah.targetObjektif = (misiAktifSaatIni == 99) ? null : targetBaru;
                }
            }
        }
    }

    private void UpdateUIWaktu()
    {
        if (teksWaktu != null)
        {
            int totalDetik = Mathf.CeilToInt(sisaWaktu);
            if (totalDetik != sisaWaktuDetikSebelumnya && totalDetik >= 0)
            {
                sisaWaktuDetikSebelumnya = totalDetik;
                int menit = totalDetik / 60;
                int detik = totalDetik % 60;
                teksWaktu.text = $"Sisa Waktu: {menit:00}:{detik:00}";
            }
        }
    }

    private void UpdateUIBarang()
    {
        if (misiAktifSaatIni == 99) return; 

        if (teksBarang != null && daftarMisi.Length > 0)
        {
            int target = daftarMisi[misiAktifSaatIni].targetJumlahBarang;
            string namaBarang = daftarMisi[misiAktifSaatIni].namaSatuanBarang;
            if (string.IsNullOrEmpty(namaBarang))
            {
                namaBarang = "Coins";
            }
            teksBarang.text = $"{namaBarang}: {barangTerkumpul} / {target}";
        }
    }

    private void UpdateKecepatanUI()
    {
        if (teksKecepatan != null && daftarMobil.Length > 0 && daftarMobil[indeksMobilAktif] != null)
        {
            Rigidbody rbMobil = daftarMobil[indeksMobilAktif].GetComponent<Rigidbody>();
            if (rbMobil != null)
            {
                int kecepatanAktual = Mathf.RoundToInt(rbMobil.linearVelocity.magnitude * 3.6f);
                if (kecepatanAktual != kecepatanSebelumnya)
                {
                    kecepatanSebelumnya = kecepatanAktual;
                    teksKecepatan.text = kecepatanAktual.ToString() + " KM/H";
                }
            }
        }
    }

    public void GantiMobil()
    {
        if (daftarMobil.Length <= 1) return; 

        int levelTerbuka = PlayerPrefs.GetInt("LevelAktif", 0);
        int indeksBaru = (indeksMobilAktif + 1) % daftarMobil.Length;

        while (indeksBaru < levelMinimalBukaMobil.Length && levelMinimalBukaMobil[indeksBaru] > levelTerbuka)
        {
            indeksBaru = (indeksBaru + 1) % daftarMobil.Length;
        }

        if (indeksBaru == indeksMobilAktif) return;

        GameObject mobilLama = daftarMobil[indeksMobilAktif];
        Vector3 posisiLama = mobilLama.transform.position;
        Quaternion rotasiLama = mobilLama.transform.rotation;
        
        Rigidbody rbLama = mobilLama.GetComponent<Rigidbody>();
        Vector3 kecepatanLama = rbLama != null ? rbLama.linearVelocity : Vector3.zero;

        GameObject mobilBaru = daftarMobil[indeksBaru];

        Rigidbody rbBaru = mobilBaru.GetComponent<Rigidbody>();
        if (rbBaru != null)
        {
            rbBaru.isKinematic = true;
            mobilBaru.transform.position = posisiLama;
            mobilBaru.transform.rotation = rotasiLama;
            rbBaru.isKinematic = false; 
            rbBaru.linearVelocity = kecepatanLama;
            rbBaru.angularVelocity = Vector3.zero;
        }
        else
        {
            mobilBaru.transform.position = posisiLama;
            mobilBaru.transform.rotation = rotasiLama;
        }

        Physics.SyncTransforms();

        indeksMobilAktif = indeksBaru;
        PlayerPrefs.SetInt("MobilAktif", indeksMobilAktif);
        PlayerPrefs.Save();
        PasangKameraKeMobilAktif();

        CarControllerPro controllerBaru = mobilBaru.GetComponentInChildren<CarControllerPro>(true);
        if (controllerBaru != null) controllerBaru.AturPosisiAwal(posisiLama, rotasiLama);

        if (daftarMisi.Length > 0)
        {
            Transform targetTerdekat = CariKoinTerdekat();
            if (targetTerdekat != null) UpdateNavigasi(targetTerdekat);
            else if (misiAktifSaatIni != 99 && daftarMisi[misiAktifSaatIni].targetPanahAwal != null) 
            {
                UpdatePanahNavigasi(daftarMisi[misiAktifSaatIni].targetPanahAwal);
            }
            else
            {
                UpdatePanahNavigasi(null);
            }
        }

        mobilBaru.SetActive(true);
        mobilLama.SetActive(false);
    }

    private void UpdateNavigasi(Transform targetBaru)
    {
        for (int i = 0; i < daftarMobil.Length; i++)
        {
            if (daftarMobil[i] != null)
            {
                PenunjukArah panah = daftarMobil[i].GetComponentInChildren<PenunjukArah>();
                if (panah != null) panah.targetObjektif = targetBaru;
            }
        }
    }

    private void PasangKameraKeMobilAktif()
    {
        if (objekFollowKamera != null)
        {
            Transform titikKameraSpesifik = daftarMobil[indeksMobilAktif].transform.Find("Follow");
            if (titikKameraSpesifik != null)
            {
                objekFollowKamera.SetParent(titikKameraSpesifik);
                objekFollowKamera.localPosition = Vector3.zero;
                objekFollowKamera.localRotation = Quaternion.identity;
            }
        }
    }

    private void UpdatePosisiMinimap()
    {
        if (kameraMinimap != null && daftarMobil.Length > 0 && daftarMobil[indeksMobilAktif] != null)
        {
            Transform mobilAktif = daftarMobil[indeksMobilAktif].transform;
            kameraMinimap.position = new Vector3(mobilAktif.position.x, mobilAktif.position.y + tinggiMinimap, mobilAktif.position.z);
            kameraMinimap.rotation = Quaternion.Euler(90f, mobilAktif.eulerAngles.y, 0f);
        }
    }

    public Transform CariKoinTerdekat()
    {
        if (daftarMisi.Length == 0 || misiAktifSaatIni >= daftarMisi.Length) return null;
        GameObject folderMisiAktif = daftarMisi[misiAktifSaatIni].folderMisi;
        if (folderMisiAktif == null) return null;

        BarangMisi[] semuaBarang = folderMisiAktif.GetComponentsInChildren<BarangMisi>(false);
        Transform targetTerdekat = null;
        float jarakPalingDekat = float.MaxValue;
        Vector3 posisiMobil = GetPosisiMobilAktif();

        foreach (BarangMisi barang in semuaBarang)
        {
            float jarak = Vector3.Distance(posisiMobil, barang.transform.position);
            if (jarak < jarakPalingDekat)
            {
                jarakPalingDekat = jarak;
                targetTerdekat = barang.transform;
            }
        }
        return targetTerdekat;
    }

    public Vector3 GetPosisiMobilAktif()
    {
        if (daftarMobil != null && indeksMobilAktif < daftarMobil.Length && daftarMobil[indeksMobilAktif] != null)
        {
            return daftarMobil[indeksMobilAktif].transform.position;
        }
        return Vector3.zero;
    }

    public void OnTombolUtilitasClicked()
    {
        if (!gameSudahMulai) GantiMobil();
        else ResetMobilAktif();
    }

    private void ResetMobilAktif()
    {
        if (CarControllerPro.ActiveInstance != null) CarControllerPro.ActiveInstance.ResetKeJalan();
    }

    private bool IsPointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    public void UpdateVolumeMusik(float volumeBaru)
    {
        if (musicSource != null) musicSource.volume = volumeBaru;
    }

    public void UpdateVolumeSFX(float volumeBaru)
    {
        if (sfxSource != null) sfxSource.volume = volumeBaru;
    }

    public void GantiLagu(int indeksLagu)
    {
        if (musicSource == null || daftarLaguTheme == null || daftarLaguTheme.Length == 0) return;
        
        if (indeksLagu >= 0 && indeksLagu < daftarLaguTheme.Length)
        {
            musicSource.clip = daftarLaguTheme[indeksLagu];
            musicSource.Play();
        }
    }

    public void MainkanSFXKlik()
    {
        MainkanSFX(sfxKlikUI, 1f);
    }

    public void MainkanSFX(AudioClip klip, float skalaEkstra = 1f)
    {
        if (klip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(klip, skalaEkstra);
        }
    }

    private void BuatPanelTamatProsedural()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        panelTamatProsedural = new GameObject("PanelTamatProsedural");
        panelTamatProsedural.transform.SetParent(canvas.transform, false);
        
        RectTransform rectPanel = panelTamatProsedural.AddComponent<RectTransform>();
        rectPanel.anchorMin = Vector2.zero;
        rectPanel.anchorMax = Vector2.one;
        rectPanel.sizeDelta = Vector2.zero;

        Image imgPanel = panelTamatProsedural.AddComponent<Image>();
        imgPanel.color = new Color(0f, 0f, 0f, 0.9f);

        GameObject titleObj = new GameObject("TeksJudul");
        titleObj.transform.SetParent(panelTamatProsedural.transform, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "<b><color=#FFD700><size=150%>CONGRATULATIONS!</size></color></b>\n<size=80%>Anda Telah Menamatkan Seluruh Level!</size>";
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 24;

        RectTransform rectTitle = titleObj.GetComponent<RectTransform>();
        rectTitle.anchoredPosition = new Vector2(0, 100);
        rectTitle.sizeDelta = new Vector2(600, 150);

        GameObject btnMenuObj = BuatTombolProsedural(panelTamatProsedural.transform, "KEMBALI KE MENU", new Vector2(-150, -100));
        Button btnMenu = btnMenuObj.GetComponent<Button>();
        btnMenu.onClick.AddListener(() => {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
        });

        GameObject btnJelajahObj = BuatTombolProsedural(panelTamatProsedural.transform, "LANJUT JELAJAH", new Vector2(150, -100));
        Button btnJelajah = btnJelajahObj.GetComponent<Button>();
        btnJelajah.onClick.AddListener(() => {
            panelTamatProsedural.SetActive(false);
            LanjutJelajahBebas();
        });
    }

    private GameObject BuatTombolProsedural(Transform parent, string label, Vector2 posisi)
    {
        GameObject btnObj = new GameObject("Tombol_" + label);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(250, 60);
        rect.anchoredPosition = posisi;

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.85f, 0.35f, 0f, 1f);

        btnObj.AddComponent<Button>();
        
        GameObject txtObj = new GameObject("Teks");
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform rectTxt = txtObj.AddComponent<RectTransform>();
        rectTxt.anchorMin = Vector2.zero;
        rectTxt.anchorMax = Vector2.one;
        rectTxt.sizeDelta = Vector2.zero;

        TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 20;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;

        return btnObj;
    }

    private void LanjutJelajahBebas()
    {
        gameSelesai = false;
        gameSudahMulai = true;
        sisaWaktu = 9999f;
        if (teksSelesai != null) teksSelesai.gameObject.SetActive(false);
        if (teksWaktu != null) teksWaktu.gameObject.SetActive(false);
        if (panelKontrolMobile != null) panelKontrolMobile.SetActive(true);
        if (musikLatar != null && !musikLatar.isPlaying)
        {
            musikLatar.Play();
        }
    }
}