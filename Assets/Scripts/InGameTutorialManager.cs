using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InGameTutorialManager : MonoBehaviour
{
    [Header("Referensi UI Panel")]
    public GameObject panelTutorialInduk;
    public TextMeshProUGUI teksJudulMisi;
    public TextMeshProUGUI teksDeskripsiMisi;
    public Image gambarPanduanMisi;
    public Button tombolMulaiMisi;

    [Header("Aset Gambar Panduan (Opsional)")]
    public Sprite gambarKoinDanDrift;
    public Sprite gambarKoinWaktu;
    public Sprite gambarBoxCargo;
    public Sprite gambarBoxWaktu;

    private void Start()
    {
        if (GameManager.Instance == null) return;

        int levelSekarang = GameManager.Instance.misiAktifSaatIni;

        if (levelSekarang == 99)
        {
            if (panelTutorialInduk != null) panelTutorialInduk.SetActive(false);
            return;
        }

        SetupTutorialBerdasarkanLevel(levelSekarang);
    }

    private void SetupTutorialBerdasarkanLevel(int levelIndex)
    {
        if (panelTutorialInduk == null) return;

        panelTutorialInduk.SetActive(true);

        if (GameManager.Instance.panelKontrolMobile != null)
        {
            GameManager.Instance.panelKontrolMobile.SetActive(false);
        }

        switch (levelIndex)
        {
            case 0: 
                teksJudulMisi.text = "<b>MISI LEVEL 1: PENGANTARAN KOIN</b>";
                teksDeskripsiMisi.text = "<b>TUGAS UTAMA:</b>\nKumpulkan koin di sepanjang jalan sirkuit menuju garis FINISH.\n" +
                                         "<i>Tidak ada batas waktu pada sirkuit ini. Mengemudilah dengan santai.</i>\n\n" +
                                         "<b>TUTORIAL DRIFT MOBILE:</b>\n" +
                                         "1. Tahan tombol <b>MAJU</b> untuk melaju.\n" +
                                         "2. Saat menikung tajam, tahan tombol <b>BELOK</b> (Kiri/Kanan) bersamaan dengan mengetuk cepat tombol <b>REM TANGAN/DRIFT</b> untuk mulai meluncur!\n" +
                                         "3. Lepaskan tombol Rem untuk mengembalikan posisi cengkeraman mobil.";
                if (gambarPanduanMisi != null && gambarKoinDanDrift != null) gambarPanduanMisi.sprite = gambarKoinDanDrift;
                break;

            case 1: 
                teksJudulMisi.text = "<b>MISI LEVEL 2: PACUAN WAKTU</b>";
                teksDeskripsiMisi.text = "<b>TUGAS UTAMA:</b>\nKumpulkan koin dan capai garis FINISH sebelum <b>SISA WAKTU</b> di layar Anda habis!\n\n" +
                                         "<b>TIPS MOBILE:</b>\n" +
                                         "Gunakan teknik Drift di setiap tikungan tajam untuk menjaga momentum kecepatan mobil Anda agar waktu tidak terbuang sia-sia.";
                if (gambarPanduanMisi != null && gambarKoinWaktu != null) gambarPanduanMisi.sprite = gambarKoinWaktu;
                break;

            case 2: 
                teksJudulMisi.text = "<b>MISI LEVEL 3: CARGO BOX</b>";
                teksDeskripsiMisi.text = "<b>TUGAS UTAMA:</b>\nObjektif Anda berubah! Sekarang cari dan kumpulkan <b>KOTAK KARGO (BOX)</b> yang tersebar di sepanjang sirkuit.\n\n" +
                                         "<b>INFORMASI:</b>\n" +
                                         "Tidak ada batas waktu pada level ini. Pelajari kontrol kendaraan Anda dengan baik untuk mengangkut seluruh kargo dengan aman.";
                if (gambarPanduanMisi != null && gambarBoxCargo != null) gambarPanduanMisi.sprite = gambarBoxCargo;
                break;

            case 3: 
                teksJudulMisi.text = "<b>MISI LEVEL 4: TANTANGAN AKHIR</b>";
                teksDeskripsiMisi.text = "<b>TUGAS UTAMA:</b>\nUjian akhir kemampuan mengemudi Anda! Kumpulkan seluruh <b>KOTAK KARGO (BOX)</b> dan meluncurlah ke garis FINISH secepat mungkin sebelum waktu habis.\n\n" +
                                         "<b>HADIAH:</b>\n" +
                                         "Selesaikan level ini untuk membuka: <b>Mode Jelajah Bebas</b> sirkuit!";
                if (gambarPanduanMisi != null && gambarBoxWaktu != null) gambarPanduanMisi.sprite = gambarBoxWaktu;
                break;
        }

        if (tombolMulaiMisi != null)
        {
            tombolMulaiMisi.onClick.RemoveAllListeners();
            tombolMulaiMisi.onClick.AddListener(TutupTutorialDanMulaiMisi);
        }
    }

    public void TutupTutorialDanMulaiMisi()
    {
        if (panelTutorialInduk != null) panelTutorialInduk.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.MulaiTancapGas();
        }
    }
}