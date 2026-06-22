using UnityEngine;
using UnityEngine.InputSystem;
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
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public DataMisi[] daftarMisi;
    public int misiAktifSaatIni = 0;

    public GameObject[] daftarMobil;
    private int indeksMobilAktif = 0;
    public Transform objekFollowKamera;

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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        if (daftarMobil == null || daftarMobil.Length == 0) return;

        if (daftarMobil[0] != null)
        {
            posisiAwalGarisStart = daftarMobil[0].transform.position;
            rotasiAwalGarisStart = daftarMobil[0].transform.rotation;
            awalGarisStartTercatat = true;
        }

        misiAktifSaatIni = PlayerPrefs.GetInt("LevelAktif", 0);
        if (misiAktifSaatIni >= daftarMisi.Length && daftarMisi.Length > 0)
        {
            misiAktifSaatIni = 0; 
            PlayerPrefs.SetInt("LevelAktif", 0);
        }

        indeksMobilAktif = PlayerPrefs.GetInt("MobilAktif", 0);
        if (indeksMobilAktif >= daftarMobil.Length)
        {
            indeksMobilAktif = 0;
            PlayerPrefs.SetInt("MobilAktif", 0);
        }

        InisialisasiPosPosition();

        if (daftarMisi.Length > 0) MuatMisi(misiAktifSaatIni);

        for (int i = 0; i < daftarMobil.Length; i++)
        {
            if (daftarMobil[i] != null)
            {
                daftarMobil[i].SetActive(i == indeksMobilAktif);
            }
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
                if (!IsPointerOverUI())
                {
                    MulaiTancapGas();
                }
            }
        }
        else
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetMobilAktif();
            }

            if (daftarMisi.Length > 0 && daftarMisi[misiAktifSaatIni].gunakanWaktu)
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

        if (daftarMisi != null && misiAktifSaatIni < daftarMisi.Length)
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
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = false; 
                }
                else
                {
                    mobil.transform.position = posisiStart;
                    mobil.transform.rotation = rotasiStart;
                }

                CarControllerPro controller = mobil.GetComponentInChildren<CarControllerPro>(true);
                if (controller != null)
                {
                    SINKRONISASI_SPAWN_CONTROLLER(controller, posisiStart, rotasiStart);
                }
            }
        }

        Physics.SyncTransforms();
    }

    public void MulaiTancapGas()
    {
        if (gameSudahMulai) return;

        gameSudahMulai = true;
        if (teksMulai != null) teksMulai.gameObject.SetActive(false);

        if (labelTombolUtilitas != null)
        {
            labelTombolUtilitas.text = "Reset";
        }

        if (panelKontrolMobile != null) panelKontrolMobile.SetActive(true);

        if (daftarMisi.Length > 0)
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
            if (daftarMisi[i].folderMisi != null)
                daftarMisi[i].folderMisi.SetActive(i == indeksMisi);
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
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        rb.isKinematic = false;
                    }
                    else
                    {
                        mobil.transform.position = misi.titikStartMisi.position;
                        mobil.transform.rotation = misi.titikStartMisi.rotation;
                    }

                    CarControllerPro controller = mobil.GetComponentInChildren<CarControllerPro>(true);
                    if (controller != null)
                    {
                        SINKRONISASI_SPAWN_CONTROLLER(controller, misi.titikStartMisi.position, misi.titikStartMisi.rotation);
                    }
                }
            }
            Physics.SyncTransforms();
        }

        if (misi.targetPanahAwal != null)
        {
            UpdatePanahNavigasi(misi.targetPanahAwal);
        }
        else
        {
            Transform koinTerdekat = CariKoinTerdekat();
            if (koinTerdekat != null) UpdatePanahNavigasi(koinTerdekat);
        }
    }

    public void TambahBarang(Transform targetPanahBerikutnya)
    {
        barangTerkumpul++;
        UpdateUIBarang();
        
        if (targetPanahBerikutnya != null)
        {
            UpdatePanahNavigasi(targetPanahBerikutnya);
        }
        else
        {
            Transform koinTerdekat = CariKoinTerdekat();
            if (koinTerdekat != null) UpdatePanahNavigasi(koinTerdekat);
        }
    }

    public bool CekApakahBarangCukup()
    {
        if (daftarMisi.Length == 0) return true;
        return barangTerkumpul >= daftarMisi[misiAktifSaatIni].targetJumlahBarang;
    }

    public void MisiSelesai()
    {
        if (gameSelesai) return;
        gameSelesai = true;

        if (teksKecepatan != null) teksKecepatan.gameObject.SetActive(false);
        if (teksBarang != null) teksBarang.gameObject.SetActive(false);
        if (teksWaktu != null) teksWaktu.gameObject.SetActive(false);
        if (kameraMinimap != null) kameraMinimap.gameObject.SetActive(false);
        
        if (layarMinimap != null) layarMinimap.SetActive(false);
        if (tombolUtilitasUI != null) tombolUtilitasUI.SetActive(false);

        if (teksSelesai != null)
        {
            teksSelesai.text = "<b><color=#FFD700><size=150%>MISSION COMPLETE!</size></color></b>\n" + 
                             "<size=80%><color=#FFFFFF>Misi Berhasil Diselesaikan</color></size>\n" +
                             "<size=50%><color=#AAAAAA>Memuat level berikutnya...</color></size>";
            
            teksSelesai.alignment = TextAlignmentOptions.Center;
            teksSelesai.gameObject.SetActive(true);
        }

        int levelSelanjutnya = misiAktifSaatIni + 1;
        PlayerPrefs.SetInt("LevelAktif", levelSelanjutnya);
        PlayerPrefs.Save();

        Invoke("MuatUlangMapUntukMisiBaru", 4f);
    }

    public void MisiGagal()
    {
        if (gameSelesai) return;
        gameSelesai = true;

        if (teksKecepatan != null) teksKecepatan.gameObject.SetActive(false);
        if (teksBarang != null) teksBarang.gameObject.SetActive(false);
        if (teksWaktu != null) teksWaktu.gameObject.SetActive(false);
        if (kameraMinimap != null) kameraMinimap.gameObject.SetActive(false);

        if (layarMinimap != null) layarMinimap.SetActive(false);
        if (tombolUtilitasUI != null) tombolUtilitasUI.SetActive(false);

        if (teksSelesai != null)
        {
            teksSelesai.text = "<b><color=#FF0000><size=150%>MISSION FAILED!</size></color></b>\n" + 
                               "<size=80%><color=#FFFFFF>Waktu Telah Habis</color></size>\n" +
                               "<size=50%><color=#AAAAAA>Mengulang misi...</color></size>";
            
            teksSelesai.alignment = TextAlignmentOptions.Center;
            teksSelesai.gameObject.SetActive(true);
        }

        Invoke("MuatUlangMapUntukMisiBaru", 3f);
    }

    public void MisiGagalKarenaBarangKurang()
    {
        if (gameSelesai) return;
        gameSelesai = true;

        if (teksKecepatan != null) teksKecepatan.gameObject.SetActive(false);
        if (teksBarang != null) teksBarang.gameObject.SetActive(false);
        if (teksWaktu != null) teksWaktu.gameObject.SetActive(false);
        if (kameraMinimap != null) kameraMinimap.gameObject.SetActive(false);

        if (layarMinimap != null) layarMinimap.SetActive(false);
        if (tombolUtilitasUI != null) tombolUtilitasUI.SetActive(false);

        if (teksSelesai != null)
        {
            teksSelesai.text = "<b><color=#FF0000><size=150%>MISSION FAILED!</size></color></b>\n" + 
                               "<size=80%><color=#FFFFFF>Barang misi tidak cukup</color></size>\n" +
                               "<size=50%><color=#AAAAAA>Mengulang misi...</color></size>";
            
            teksSelesai.alignment = TextAlignmentOptions.Center;
            teksSelesai.gameObject.SetActive(true);
        }

        Invoke("MuatUlangMapUntukMisiBaru", 3f);
    }

    private void MuatUlangMapUntukMisiBaru()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void UpdatePanahNavigasi(Transform targetBaru)
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

    private void UpdateUIWaktu()
    {
        if (teksWaktu != null)
        {
            int menit = Mathf.FloorToInt(sisaWaktu / 60);
            int detik = Mathf.FloorToInt(sisaWaktu % 60);
            teksWaktu.text = string.Format("Sisa Waktu: {0:00}:{1:00}", menit, detik);
        }
    }

    private void UpdateUIBarang()
    {
        if (teksBarang != null && daftarMisi.Length > 0)
        {
            int target = daftarMisi[misiAktifSaatIni].targetJumlahBarang;
            teksBarang.text = $"Coins: {barangTerkumpul} / {target}";
        }
    }

    public void GantiMobil()
    {
        if (daftarMobil.Length <= 1) return; 

        GameObject mobilLama = daftarMobil[indeksMobilAktif];
        Vector3 posisiLama = mobilLama.transform.position;
        Quaternion rotasiLama = mobilLama.transform.rotation;
        
        Rigidbody rbLama = mobilLama.GetComponent<Rigidbody>();
        Vector3 kecepatanLama = rbLama != null ? rbLama.linearVelocity : Vector3.zero;

        int indeksBaru = (indeksMobilAktif + 1) % daftarMobil.Length;
        GameObject mobilBaru = daftarMobil[indeksBaru];

        Rigidbody rbBaru = mobilBaru.GetComponent<Rigidbody>();
        if (rbBaru != null)
        {
            rbBaru.isKinematic = true;
            mobilBaru.transform.position = posisiLama;
            mobilBaru.transform.rotation = rotasiLama;
            rbBaru.linearVelocity = kecepatanLama;
            rbBaru.angularVelocity = Vector3.zero;
            rbBaru.isKinematic = false;
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
        if (controllerBaru != null)
        {
            SINKRONISASI_SPAWN_CONTROLLER(controllerBaru, posisiLama, rotasiLama);
        }

        if (daftarMisi.Length > 0)
        {
            Transform targetTerdekat = CariKoinTerdekat();
            if (targetTerdekat != null) UpdatePanahNavigasi(targetTerdekat);
            else if (daftarMisi[misiAktifSaatIni].targetPanahAwal != null) UpdatePanahNavigasi(daftarMisi[misiAktifSaatIni].targetPanahAwal);
        }

        mobilBaru.SetActive(true);
        mobilLama.SetActive(false);
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

    private void UpdateKecepatanUI()
    {
        if (teksKecepatan != null && daftarMobil.Length > 0 && daftarMobil[indeksMobilAktif] != null)
        {
            Rigidbody rbMobil = daftarMobil[indeksMobilAktif].GetComponent<Rigidbody>();
            if (rbMobil != null)
            {
                float kecepatan = rbMobil.linearVelocity.magnitude * 3.6f;
                teksKecepatan.text = Mathf.RoundToInt(kecepatan).ToString() + " KM/H";
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
        if (!gameSudahMulai)
        {
            GantiMobil();
        }
        else
        {
            ResetMobilAktif();
        }
    }

    private void ResetMobilAktif()
    {
        if (CarControllerPro.ActiveInstance != null)
        {
            CarControllerPro.ActiveInstance.ResetKeJalan();
        }
    }

    private void SINKRONISASI_SPAWN_CONTROLLER(Component controller, Vector3 targetPos, Quaternion targetRot)
    {
        if (controller == null) return;
        System.Type tipe = controller.GetType();

        System.Reflection.FieldInfo[] fields = tipe.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            string namaKecil = field.Name.ToLower();
            if (namaKecil.Contains("spawn") || namaKecil.Contains("start") || namaKecil.Contains("initial") || namaKecil.Contains("reset") || namaKecil.Contains("origin"))
            {
                try
                {
                    if (field.FieldType == typeof(Vector3))
                    {
                        field.SetValue(controller, targetPos);
                    }
                    else if (field.FieldType == typeof(Transform))
                    {
                        Transform t = (Transform)field.GetValue(controller);
                        if (t != null)
                        {
                            t.position = targetPos;
                            t.rotation = targetRot;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning("[Safe-Research] Gagal menyelaraskan field: " + ex.Message);
                }
            }
        }
    }

    private bool IsPointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
}