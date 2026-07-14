# Metro Shift: Urban Driving

[![Platform](https://img.shields.io/badge/Platform-Android-green.svg)](#)
[![Engine](https://img.shields.io/badge/Engine-Unity_6-blue.svg)](#)
[![Render-Pipeline](https://img.shields.io/badge/Render--Pipeline-URP-orange.svg)](#)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](#)

**Metro Shift: Urban Driving** adalah sebuah proyek perangkat lunak permainan simulasi mengemudi arcade 3D yang dikembangkan untuk platform Android menggunakan **Unity 6** dan **Universal Render Pipeline (URP)**. Proyek ini memprioritaskan efisiensi kinerja komputasi pada perangkat mobile, fleksibilitas pengaturan performa secara dinamis, serta mekanika kendali kemudi yang responsif melalui integrasi sistem asisten manuver (*Drift Assist*) dan pelacakan rute terstruktur.

---

## 🌟 Fitur Utama (Core Features)

### 1. Sistem Kontrol Mobile Ergonomis
* **Custom Touch Input:** Mengganti kontrol analog tradisional dengan tombol arah (*digital steering*) dan pedal akselerasi/mundur terpisah untuk meningkatkan presisi kendali pada layar sentuh.
* **Visual Press Feedback:** Mengimplementasikan mekanika penyesuaian intensitas warna (*color-dimming*) berbasis komponen `Image` pada UI yang meredup saat ditekan (`OnPointerDown`) dan kembali normal saat dilepas (`OnPointerUp`), guna memberikan umpan balik taktil instan kepada pemain.

### 2. Arcade Drift Assist & Stabilitas Fisika
* **Momentum Thrust Assist:** Memberikan gaya dorong terarah konstan searah orientasi kendaraan (*forward vector*) saat mendeteksi status manuver menyamping (*handbrake drifting*) agar kendaraan tidak kehilangan kecepatan.
* **Dynamic Yaw Control:** Memanfaatkan kalkulasi umpan balik real-time (*closed-loop feedback*) pada poros Y kendaraan untuk menyelaraskan laju rotasi sudut (*angular velocity*) guna mencegah kondisi kehilangan kendali atau berputar balik (*spin-out*).

### 3. Modul Utilitas Adaptif Dua Status (Dual-State Utility)
* **Status Pra-Permainan:** Tombol utilitas berfungsi sebagai pemilih kendaraan (*Ganti Mobil*) yang memutar pilihan display mobil pada baris sirkuit sebelum akselerasi awal dimulai.
* **Status Pasca-Mulai:** Tombol secara dinamis bertransformasi menjadi fungsi pemulihan (*Reset Mobil*). Sistem ini mendukung pemulihan otomatis jika kendaraan diam selama 1,5 detik dalam kondisi input gas aktif, serta pemulihan manual ganda (satu ketukan untuk aspal terdekat, dan ketuk cepat ganda untuk kembali ke garis awal).

### 4. Manajemen Misi Sekuensial & Validasi Pengumpulan
* **Panah Navigasi Dinamis:** Objek penunjuk arah 3D di atas kendaraan yang secara real-time menghitung rotasi sudut (*LookRotation*) untuk menunjuk koordinat target terdekat yang aktif.
* **Strict Order Validation:** Menerapkan sistem penolakan pengumpulan jika pemain melanggar urutan pengambilan objek yang ditunjuk panah navigasi, disertai pemanggilan teks peringatan (*warning panel*) kustom secara prosedural.
* **Gate Validator:** Komponen garis finish yang memvalidasi kecukupan jumlah objek terkumpul sebelum mengonfirmasi keberhasilan level (*Mission Complete*) atau kegagalan akibat kekurangan kargo (*Mission Failed*).

### 5. Jendela Panduan Interaktif (In-Game Tutorial)
* Mengintegrasikan panel instruksi sebelum inisialisasi laju kendaraan di setiap tingkat level. Jendela ini menampilkan deskripsi misi, batasan waktu, dan gambar panduan aset objek yang relevan (koin pada level ganjil, kotak kargo pada level genap).

### 6. Pengaturan Grafis, Audio, dan Manajemen Data Dinamis
* **Optimasi Grafis:** Fitur penguncian batas kecepatan bingkai (30 FPS dan 60 FPS) serta opsi penonaktifan bayangan (*Shadow Casting Disable*) secara real-time untuk mengurangi beban kerja GPU pada perangkat mobile spesifikasi menengah ke bawah.
* **Manajemen Audio:** Slider penyesuaian volume terpisah untuk Music dan SFX, serta opsi pemilihan daftar lagu tema (*Playlist Selector*).
* **Reset Data:** Menyediakan fungsi penghapusan seluruh data progres level dan kendaraan yang tersimpan di memori perangkat dengan aman menggunakan validasi panel konfirmasi ganda.

---

## 🛠️ Arsitektur Teknis & Alur Kerja (Architecture & Flow)

Permainan ini dibangun menggunakan pemrograman berbasis objek (*OOP*) dan arsitektur modular di Unity. Alur interaksi antar-skrip didefinisikan sebagai berikut:

```text
[Touch Input] ──> [MobileInputButton] ──> [CarControllerPro] (Fisika Roda & Drift)
                                                 │
                                                 ▼
[GarisFinishMisi] <── [GameManager] <────────────┘ (Menghitung Kecepatan,
        │                   │                      Waktu, & Progres Misi)
        ▼                   ▼
[Auto-Save Data]    [InGameTutorialManager]
```

* **Manajemen Data Progres:** Memanfaatkan sistem penyimpanan lokal `PlayerPrefs` untuk merekam progres level aktif (`LevelAktif`) dan jenis mobil yang sedang dipilih oleh pemain (`MobilAktif`).
* **Optimasi Render Pipeline:** Menggunakan Universal Render Pipeline (URP) dengan penyesuaian parameter jarak bayangan (*shadow distance reduction*) dan presisi penajaman kamera (*clipping planes*) untuk meminimalkan gangguan visual kedipan bayangan (*z-fighting / shadow acne*).

---

## 💻 Spesifikasi Teknologi (Tech Stack)

* **Game Engine:** Unity 6 (Versi Minimum: 6000.3.14f1 LTS)
* **Render Pipeline:** Universal Render Pipeline (URP)
* **Bahasa Pemrograman:** C# (.NET Standard 2.1)
* **Sistem Input:** Unity New Input System API
* **Penyimpanan Lokal:** PlayerPrefs API
* **UI Framework:** TextMesh Pro (TMP) & Unity UI Core
* **Scripting Backend:** IL2CPP (Arsitektur target ARM64 untuk platform Android)

---

## 📂 Struktur Direktori Proyek

```text
Assets/
├── Scenes/               # Berisi scene utama sirkuit perkotaan low-poly.
├── Scripts/              # Direktori seluruh file logika C#.
│   ├── CarControllerPro.cs       # Sistem fisika mobil, drift assist, dan audio mesin.
│   ├── GameManager.cs            # Logika utama level, waktu, validasi koin, dan auto-save.
│   ├── MobileInputButton.cs      # Handler event sentuh tombol UI dan color-dimming.
│   ├── InGameTutorialManager.cs  # Handler instruksi pop-up statis dan dinamis setiap level.
│   ├── MainMenuManager.cs        # Handler menu utama, pilih level, dan panel garasi.
│   ├── SettingManager.cs         # Kontrol grafis (FPS/Shadows), volume audio, dan hapus data.
│   ├── PauseManager.cs           # Handler penjedaan sistem fisika dan pemanggilan menu jeda.
│   ├── BarangMisi.cs             # Deteksi benturan objek, urutan panah, dan animasi melayang.
│   ├── GarisFinishMisi.cs        # Verifikator akhir kecukupan objek kargo di garis finish.
│   └── PenunjukArah.cs           # Kalkulasi rotasi panah penunjuk rute kargo.
└── UI/
    └── BUTTON/           # Kumpulan aset sprite gambar PNG transparan untuk tombol UI.
```


## 🚀 Panduan Build Proyek ke Android

### Persyaratan Lingkungan (Prerequisites)
1. Instalasi **Unity Hub** dengan versi editor **Unity 6**.
2. Modul **Android Build Support** terinstal (OpenJDK, Android SDK & NDK Tools).

### Langkah-langkah Build:
1. Buka folder proyek menggunakan Unity Editor versi Unity 6.
2. Akses menu **File > Build Profiles** pada bilah menu atas.
3. Ubah platform aktif menjadi **Android**, kemudian ketuk tombol **Switch Platform**.
4. Pastikan file scene utama (`Assets/Scenes/Car Game.scene`) berada di dalam daftar *Scenes in Build*.
5. Akses **Player Settings**, pastikan opsi *Scripting Backend* diatur ke **IL2CPP**, dan centang target arsitektur **ARM64**.
6. Ketuk tombol **Build** untuk menghasilkan berkas keluaran berformat **APK** yang siap dipasang pada perangkat Android target.
