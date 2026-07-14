using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarControllerPro : MonoBehaviour
{
    public static CarControllerPro ActiveInstance { get; private set; }

    [Header("1. Hubungkan Roda Fisik")]
    public WheelCollider wcDepanKanan;
    public WheelCollider wcDepanKiri;
    public WheelCollider wcBelakangKanan;
    public WheelCollider wcBelakangKiri;

    [Header("2. Hubungkan Roda Visual")]
    public Transform visualDepanKanan;
    public Transform visualDepanKiri;
    public Transform visualBelakangKanan;
    public Transform visualBelakangKiri;

    [Header("3. Tuning Mesin Utama")]
    public float maxSpeedKmH = 200f; 
    public float tenagaMesin = 5000f; 
    public float tenagaRem = 6000f; 
    public float engineBrake = 500f;

    [Header("4. Tuning Setir Pintar")]
    public float sudutBelokMaksimal = 35f; 
    public float sudutBelokSaatNgebut = 5f; 
    public float kecepatanPutarSetir = 3f; 
    public float kecepatanLurusOtomatis = 12f; 

    [Header("5. Tuning Drift Arcade")]
    [Range(0.1f, 1f)] public float driftStiffness = 0.35f;
    public float remInersiaDrift = 1.5f; 
    public float cengkramanMukaDrift = 2f;

    [Header("6. Efek Visual & Audio")]
    public TrailRenderer jejakBanKanan;
    public TrailRenderer jejakBanKiri;
    public AudioSource audioMesin;
    public AudioSource audioBan;
    public AudioClip suaraIdle;
    public AudioClip suaraNgebut;
    public AudioClip suaraDrift;
    public AudioClip suaraRem;
    public AudioClip suaraTabrakan;

    [Header("7. Sistem Drift Assist Arcade")]
    public bool gunakanDriftAssist = true;
    public float tenagaDorongDriftAssist = 3500f;
    public float kecepatanRotasiDriftAssist = 2.5f;
    public float stabilitasDriftAssist = 1.5f;

    [Header("8. Pengaturan Fisika")]
    public Transform pusatMassaObjek;

    private float normalStiffness = 1f;
    private float inputGas;
    private float sumbuSetirVirtual = 0f; 
    private float setirAkhirDiterapkan = 0f;
    private bool isHandbrake;
    private bool isBraking; 

    private bool btnMajuPressed = false;
    private bool btnMundurPressed = false;
    private bool btnKiriPressed = false;
    private bool btnKananPressed = false;
    private bool btnRemPressed = false;

    private Rigidbody rb;
    public static float volumeSFX = 1f;
    
    private Vector3 lastNodePosition;
    private Quaternion lastSafeRotation;
    private Transform lastRoadHit;
    
    private Vector3 startPosition;
    private Quaternion startRotation;

    private float stuckTimer = 0f;
    private const float STUCK_THRESHOLD = 1.5f; 
    private float lastRPressTime = 0f;
    private float doubleTapWaktu = 0.4f; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        ActiveInstance = this;
        volumeSFX = PlayerPrefs.GetFloat("Set_SFX", 1f);
        
        if (audioMesin != null) audioMesin.UnPause();
        if (audioBan != null) audioBan.UnPause();
    }

    private void OnDisable()
    {
        if (audioMesin != null) audioMesin.Pause();
        if (audioBan != null) audioBan.Pause();
    }

    private void Start()
    {
        if (rb != null)
        {
            if (pusatMassaObjek != null) rb.centerOfMass = pusatMassaObjek.localPosition;
            else rb.centerOfMass = new Vector3(0, -0.15f, 0); 
        }
        normalStiffness = wcBelakangKanan.sidewaysFriction.stiffness;

        startPosition = transform.position;
        startRotation = transform.rotation;
        
        lastNodePosition = startPosition;
        lastSafeRotation = startRotation;

        kecepatanPutarSetir = PlayerPrefs.GetFloat("Set_Sensitivitas", 3f);
        gunakanDriftAssist = PlayerPrefs.GetInt("Set_DriftAssist", 1) == 1;
        volumeSFX = PlayerPrefs.GetFloat("Set_SFX", 1f);

        if (audioMesin != null)
        {
            audioMesin.clip = suaraIdle;
            audioMesin.loop = true;
            audioMesin.volume = volumeSFX * 0.8f;
            audioMesin.Play();
        }

        #if UNITY_STANDALONE || UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        #endif
    }

    private void Update()
    {
        if (ActiveInstance != this) ActiveInstance = this;

        AmbilInputSistemBaru();
        CekMekanismeResetStuck();
        UpdateSuaraMobil();
    }

    private void FixedUpdate()
    {
        JalankanMesin();
        BelokkanSetir();
        FisikaDriftArcade(); 
        UpdateAnimasiPutaranRoda();
        DeteksiTitikResetJalan(); 
    }

    public void AturPosisiAwal(Vector3 posisiBaru, Quaternion rotasiBaru)
    {
        startPosition = posisiBaru;
        startRotation = rotasiBaru;
        lastNodePosition = posisiBaru;
        lastSafeRotation = rotasiBaru;
    }

    public void SetVolumeSFX(float vol)
    {
        volumeSFX = vol;
    }

    private void AmbilInputSistemBaru()
    {
        inputGas = 0f;
        isHandbrake = false;
        isBraking = false; 

        if (GameManager.Instance != null && !GameManager.Instance.gameSudahMulai)
        {
            isHandbrake = true; 
            return; 
        }

        float inputGasRaw = 0f;
        bool sedangBelok = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                sumbuSetirVirtual += Time.deltaTime * kecepatanPutarSetir;
                sedangBelok = true;
            }
            else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                sumbuSetirVirtual -= Time.deltaTime * kecepatanPutarSetir;
                sedangBelok = true;
            }
        }

        if (btnKananPressed)
        {
            sumbuSetirVirtual += Time.deltaTime * kecepatanPutarSetir;
            sedangBelok = true;
        }
        else if (btnKiriPressed)
        {
            sumbuSetirVirtual -= Time.deltaTime * kecepatanPutarSetir;
            sedangBelok = true;
        }

        if (!sedangBelok) sumbuSetirVirtual = Mathf.MoveTowards(sumbuSetirVirtual, 0f, Time.deltaTime * kecepatanLurusOtomatis);
        sumbuSetirVirtual = Mathf.Clamp(sumbuSetirVirtual, -1f, 1f);

        float rasioKecepatan = 0f;
        if (rb != null) rasioKecepatan = Mathf.Clamp01(rb.linearVelocity.magnitude / (maxSpeedKmH / 3.6f));
        float batasSudutSaatIni = Mathf.Lerp(sudutBelokMaksimal, sudutBelokSaatNgebut, rasioKecepatan);

        setirAkhirDiterapkan = sumbuSetirVirtual * batasSudutSaatIni;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputGasRaw = 1f;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputGasRaw = -1f;

            if (Keyboard.current.spaceKey.isPressed) isHandbrake = true;
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                if (Time.time - lastRPressTime <= doubleTapWaktu) ResetKeAwal(); 
                else ResetKeJalan(); 
                lastRPressTime = Time.time;
            }
        }

        if (btnMajuPressed) inputGasRaw = 1f;
        else if (btnMundurPressed) inputGasRaw = -1f;
        if (btnRemPressed) isHandbrake = true; 

        if (inputGasRaw > 0.05f) inputGas = inputGasRaw * tenagaMesin;
        else if (inputGasRaw < -0.05f)
        {
            float arahMaju = 0f;
            if (rb != null) arahMaju = Vector3.Dot(transform.forward, rb.linearVelocity);
            
            if (arahMaju > 1f) isBraking = true; 
            else inputGas = inputGasRaw * tenagaMesin; 
        }
    }

    private void JalankanMesin()
    {
        if (rb == null) return;

        float kecepatanSaatIni = rb.linearVelocity.magnitude * 3.6f;
        float tenagaGasBeneran = inputGas;

        if (isHandbrake || isBraking) tenagaGasBeneran = 0f;
        if (kecepatanSaatIni > maxSpeedKmH && tenagaGasBeneran > 0) tenagaGasBeneran = 0f; 

        wcDepanKanan.motorTorque = tenagaGasBeneran;
        wcDepanKiri.motorTorque = tenagaGasBeneran;
        wcBelakangKanan.motorTorque = tenagaGasBeneran;
        wcBelakangKiri.motorTorque = tenagaGasBeneran;

        float remDepan = 0f;
        float remBelakang = 0f;

        if (isHandbrake)
        {
            remDepan = 0f; 
            remBelakang = tenagaRem;     
            SetDriftFriction(true);      
        }
        else if (isBraking)
        {
            remDepan = tenagaRem;
            remBelakang = tenagaRem;
            SetDriftFriction(false);     
        }
        else if (inputGas == 0)
        {
            remDepan = engineBrake; 
            remBelakang = engineBrake; 
            SetDriftFriction(false);
        }
        else SetDriftFriction(false);

        wcDepanKanan.brakeTorque = remDepan; 
        wcDepanKiri.brakeTorque = remDepan;
        wcBelakangKanan.brakeTorque = remBelakang;
        wcBelakangKiri.brakeTorque = remBelakang;
    }

    private void BelokkanSetir()
    {
        wcDepanKanan.steerAngle = setirAkhirDiterapkan;
        wcDepanKiri.steerAngle = setirAkhirDiterapkan;
    }

    private void FisikaDriftArcade()
    {
        if (rb == null) return;

        if (isHandbrake)
        {
            Vector3 kecepatanAktual = rb.linearVelocity;
            float lajuMaju = Vector3.Dot(kecepatanAktual, transform.forward);
            
            Vector3 gayaRemDrift = -transform.forward * (lajuMaju * remInersiaDrift);
            rb.AddForce(gayaRemDrift, ForceMode.Acceleration);

            rb.AddTorque(transform.up * (sumbuSetirVirtual * cengkramanMukaDrift), ForceMode.Acceleration);

            if (gunakanDriftAssist && Mathf.Abs(sumbuSetirVirtual) > 0.05f)
            {
                float arahMajuDot = Vector3.Dot(rb.linearVelocity.normalized, transform.forward);

                if (arahMajuDot > 0.2f && rb.linearVelocity.magnitude > 3f)
                {
                    Vector3 gayaDorong = transform.forward * tenagaDorongDriftAssist;
                    rb.AddForce(gayaDorong, ForceMode.Force);
                }

                float lajuRotasiY = rb.angularVelocity.y;
                float targetLajuRotasiY = sumbuSetirVirtual * kecepatanRotasiDriftAssist;
                float koreksiRotasiY = (targetLajuRotasiY - lajuRotasiY) * stabilitasDriftAssist;

                rb.AddTorque(transform.up * koreksiRotasiY, ForceMode.VelocityChange);
            }
        }
    }

    private void SetDriftFriction(bool isDrifting)
    {
        WheelFrictionCurve frictionRight = wcBelakangKanan.sidewaysFriction;
        WheelFrictionCurve frictionLeft = wcBelakangKiri.sidewaysFriction;

        frictionRight.stiffness = isDrifting ? driftStiffness : normalStiffness;
        frictionLeft.stiffness = isDrifting ? driftStiffness : normalStiffness;

        wcBelakangKanan.sidewaysFriction = frictionRight;
        wcBelakangKiri.sidewaysFriction = frictionLeft;

        if (rb == null) return;

        bool gesekAktif = isDrifting && rb.linearVelocity.magnitude > 5f; 
        if (jejakBanKanan != null) jejakBanKanan.emitting = gesekAktif;
        if (jejakBanKiri != null) jejakBanKiri.emitting = gesekAktif;
    }

    private void UpdateAnimasiPutaranRoda()
    {
        UpdateVisualSatuRoda(wcDepanKanan, visualDepanKanan);
        UpdateVisualSatuRoda(wcDepanKiri, visualDepanKiri);
        UpdateVisualSatuRoda(wcBelakangKanan, visualBelakangKanan);
        UpdateVisualSatuRoda(wcBelakangKiri, visualBelakangKiri);
    }

    private void UpdateVisualSatuRoda(WheelCollider wc, Transform visual)
    {
        wc.GetWorldPose(out Vector3 posisi, out Quaternion rotasi);
        visual.position = posisi;
        visual.rotation = rotasi;
    }

    private void DeteksiTitikResetJalan()
    {
        if (rb == null) return;

        bool isHit = wcBelakangKanan.GetGroundHit(out WheelHit hit);
        if (!isHit) isHit = wcBelakangKiri.GetGroundHit(out hit);

        if (isHit && hit.collider.CompareTag("Road"))
        {
            Transform jalanYangDiinjak = hit.collider.transform;

            if (jalanYangDiinjak != lastRoadHit)
            {
                Transform titik = DapatkanTitikReset(jalanYangDiinjak);
                if (titik != null)
                {
                    lastNodePosition = titik.position;
                    lastRoadHit = jalanYangDiinjak; 
                }
            }

            if (Vector3.Dot(transform.up, Vector3.up) > 0.8f && rb.linearVelocity.magnitude > 2f && !isHandbrake)
            {
                lastSafeRotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            }
        }
    }

    private Transform DapatkanTitikReset(Transform asalHit)
    {
        Transform current = asalHit;
        while (current != null)
        {
            Transform titik = current.Find("TitikReset");
            if (titik != null) return titik;
            current = current.parent; 
        }
        return null; 
    }

    private void CekMekanismeResetStuck()
    {
        if (rb == null) return;

        if (Mathf.Abs(inputGas) > 0.1f && rb.linearVelocity.magnitude < 0.5f && !isHandbrake && !isBraking)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= STUCK_THRESHOLD) ResetKeJalan();
        }
        else stuckTimer = 0f;
    }

    public void ResetKeJalan()
    {
        TeleportMobil(lastNodePosition + Vector3.up * 1.5f, lastSafeRotation);
    }

    public void ResetKeAwal()
    {
        TeleportMobil(startPosition + Vector3.up * 1.5f, startRotation);
    }

    private void TeleportMobil(Vector3 posisiTarget, Quaternion rotasiTarget)
    {
        transform.position = posisiTarget;
        transform.rotation = rotasiTarget;
        
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        stuckTimer = 0f;
    }

    private void UpdateSuaraMobil()
    {
        if (audioMesin == null || rb == null) return;

        if (Time.timeScale == 0f)
        {
            if (audioMesin.isPlaying) audioMesin.Pause();
            if (audioBan.isPlaying) audioBan.Pause();
            return;
        }
        else
        {
            if (!audioMesin.isPlaying)
            {
                audioMesin.UnPause();
            }
        }

        audioMesin.volume = volumeSFX * 0.8f;
        float laju = rb.linearVelocity.magnitude * 3.6f;
        
        AudioClip targetKlipMesin = suaraIdle;
        float targetPitch = 1f;

        if (Mathf.Abs(inputGas) > 0.1f)
        {
            targetKlipMesin = suaraNgebut;
            targetPitch = 1f + (laju / maxSpeedKmH) * 1.5f;
        }
        else if (laju > 15f)
        {
            targetKlipMesin = suaraRem; 
            targetPitch = 0.8f + (laju / maxSpeedKmH) * 0.7f;
        }
        else
        {
            targetKlipMesin = suaraIdle;
            targetPitch = 1f + (laju / 20f) * 0.2f;
        }

        if (audioMesin.clip != targetKlipMesin)
        {
            audioMesin.clip = targetKlipMesin;
            audioMesin.Play();
        }

        audioMesin.pitch = Mathf.Lerp(audioMesin.pitch, targetPitch, Time.deltaTime * 3f);

        if (audioBan != null)
        {
            bool lagiDrift = (isHandbrake || (isBraking && laju > 30f)) && laju > 10f;
            
            if (lagiDrift)
            {
                audioBan.volume = volumeSFX * 0.8f;
                if (!audioBan.isPlaying)
                {
                    audioBan.clip = suaraDrift;
                    audioBan.pitch = Random.Range(0.9f, 1.1f);
                    audioBan.Play();
                }
            }
            else
            {
                if (audioBan.isPlaying)
                {
                    audioBan.volume = Mathf.Lerp(audioBan.volume, 0f, Time.deltaTime * 5f);
                    if (audioBan.volume < 0.05f) audioBan.Stop();
                }
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (suaraTabrakan != null && collision.relativeVelocity.magnitude > 10f)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MainkanSFX(suaraTabrakan, 0.8f);
            }
        }
    }

    public void SetInputMaju(bool isPressed) => btnMajuPressed = isPressed;
    public void SetInputMundur(bool isPressed) => btnMundurPressed = isPressed;
    public void SetInputKiri(bool isPressed) => btnKiriPressed = isPressed;
    public void SetInputKanan(bool isPressed) => btnKananPressed = isPressed;
    public void SetInputRem(bool isPressed) => btnRemPressed = isPressed;
}