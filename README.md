# Metro Shift: Urban Driving 🚗💨 [Version 0.1.0 - progress #1]

[![Platform](https://img.shields.io/badge/Platform-Android-green.svg)](#)
[![Engine](https://img.shields.io/badge/Engine-Unity_6-blue.svg)](#)
[![Render-Pipeline](https://img.shields.io/badge/Render--Pipeline-URP-orange.svg)](#)
[![License](https://img.shields.io/badge/License-MIT-yellow.svg)](#)

**Metro Shift: Urban Driving** adalah game *3D Arcade Driving* kasual untuk platform Android yang dikembangkan menggunakan **Unity 6** dan **Universal Render Pipeline (URP)**. Game ini menawarkan pengalaman berkendara di area perkotaan yang responsif dengan mekanika drifting ala arcade, sistem navigasi cerdas, dan siklus level dinamis.

---

## 🌟 Fitur Utama (Core Features)

### 1. Sistem Kontrol Mobile Ergonomis (Custom Mobile UI Controller)
* Mengganti sistem joystick tradisional dengan tombol kemudi (*digital steering buttons*) dan pedal vertikal responsif di layar sentuh untuk kenyamanan manuver yang presisi.
* Efek umpan balik visual (*color-dimming*) dinamis pada tombol saat ditekan oleh jari pemain.

### 2. Asisten Drift Arcade Cerdas (Smart Arcade Drift Assist)
* **Momentum Thrust Assist:** Menambahkan gaya dorong konstan searah moncong mobil saat drift menyamping agar mobil tidak kehilangan momentum kecepatan.
* **Dynamic Yaw Control:** Sistem closed-loop feedback yang menstabilkan laju rotasi poros Y mobil secara real-time untuk mencegah mobil terputar arah (*spin-out*).

### 3. Tombol Utilitas Adaptif Dua Status (Dual-State Utility Button)
* **Sebelum Mulai:** Tombol berfungsi sebagai **"Ganti Mobil"** untuk berotasi memilih kendaraan di garasi.
* **Setelah Mulai:** Tombol secara otomatis bertransformasi menjadi **"Reset Mobil"** untuk mengembalikan posisi mobil ke jalan aman jika terjebak, lengkap dengan perubahan teks label dinamis.

### 4. Sistem Misi Sekuensial & Panah Navigasi Dinamis
* **Smart Compass Arrow:** Panah 3D di atas mobil yang secara dinamis menghitung dan menunjuk ke koin terdekat yang aktif.
* **Strict Order Validation:** Pemain wajib mengambil koin secara berurutan sesuai arah panah. Menabrak koin yang salah akan memicu penolakan dan memunculkan teks peringatan merah kustom secara otomatis dari kode.
* **Finish Line Validator:** Garis finish akan memicu keberhasilan misi jika koin lengkap, atau langsung memicu kegagalan misi (*Mission Failed*) dengan info khusus jika koin belum lengkap.

### 5. Optimasi Performa Mobile Android
* Optimasi bayangan real-time URP (*shadow distance & resolution reduction*) dan presisi depth buffer kamera (*clipping planes*) untuk menghilangkan gangguan kedipan visual (*Z-fighting / shadow acne*).

---

## 🛠️ Spesifikasi Teknis (Tech Stack)

* **Game Engine:** Unity 6 (6000.3.14f1 LTS)
* **Render Pipeline:** Universal Render Pipeline (URP)
* **Bahasa Pemrograman:** C#
* **Sistem Input:** Unity New Input System
* **UI Framework:** TextMesh Pro (TMP) & Unity UI
* **Scripting Backend:** IL2CPP (Target arsitektur ARM64 untuk Android modern)

---

## 📂 Struktur Folder Proyek

* `Assets/Scripts/`: Seluruh logika C# (CarControllerPro, GameManager, MobileInputButton, BarangMisi, GarisFinishMisi, PenunjukArah).
* `Assets/UI/BUTTON/`: File aset gambar Sprite PNG transparan untuk tombol kustom mobile.
* `Assets/Scenes/`: Scene utama permainan perkotaan *low-poly*.

---

## 🚀 Cara Menjalankan & Build Proyek

### Persyaratan Sistem (Prerequisites):
1. **Unity Hub** dengan instalasi **Unity 6**.
2. Modul **Android Build Support** terinstal (OpenJDK, Android SDK & NDK Tools).
