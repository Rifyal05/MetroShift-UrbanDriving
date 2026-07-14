using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class BarangMisi : MonoBehaviour
{
    public Transform targetPanahBerikutnya; 
    
    [Header("Animasi Model Utama")]
    public Transform modelVisual;
    public float kecepatanPutar = 100f;
    public float tinggiMelayang = 0.5f;
    public float kecepatanMelayang = 2f;

    [Header("Animasi Area Lingkaran")]
    public Transform areaLingkaran;
    public float kecepatanDenyut = 5f;
    public float skalaDenyut = 0.05f;

    [Header("Pengaturan Interaksi")]
    public bool gunakanWaktu = false;
    public float durasiInteraksi = 2f;
    public string teksInteraksi = "LOADING";

    private float posisiAwalY;
    private Transform objTransform;
    private float timerInteraksi = 0f;
    private bool sedangInteraksi = false;

    private static GameObject objekPeringatanGlobal;
    private static TextMeshProUGUI teksPeringatanGlobal;
    private float timerPeringatan = 0f;

    private static GameObject objekProgressGlobal;
    private static TextMeshProUGUI teksProgressGlobal;

    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        objTransform = transform;
        
        if (modelVisual == null)
        {
            modelVisual = transform;
        }
        
        posisiAwalY = modelVisual.localPosition.y;
    }

    private void Update()
    {
        if (modelVisual != null)
        {
            modelVisual.Rotate(Vector3.up * kecepatanPutar * Time.deltaTime, Space.World);
            float yBaru = posisiAwalY + Mathf.Sin(Time.time * kecepatanMelayang) * tinggiMelayang;
            modelVisual.localPosition = new Vector3(modelVisual.localPosition.x, yBaru, modelVisual.localPosition.z);
        }

        if (areaLingkaran != null)
        {
            float denyut = 1f + Mathf.Sin(Time.time * kecepatanDenyut) * skalaDenyut;
            areaLingkaran.localScale = new Vector3(denyut, denyut, 1f);
        }

        if (timerPeringatan > 0f && objekPeringatanGlobal != null && objekPeringatanGlobal.activeSelf)
        {
            timerPeringatan -= Time.deltaTime;
            if (timerPeringatan <= 0f)
            {
                objekPeringatanGlobal.SetActive(false);
            }
        }

        if (gunakanWaktu && sedangInteraksi)
        {
            timerInteraksi += Time.deltaTime;
            float progress = Mathf.Clamp01(timerInteraksi / durasiInteraksi);
            
            UpdateProgressUI($"{teksInteraksi} {Mathf.RoundToInt(progress * 100f)}%");

            if (timerInteraksi >= durasiInteraksi)
            {
                SelesaikanPengambilan();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<CarControllerPro>() != null)
        {
            if (CarControllerPro.ActiveInstance != null)
            {
                PenunjukArah penunjuk = CarControllerPro.ActiveInstance.GetComponentInChildren<PenunjukArah>();
                if (penunjuk != null && penunjuk.targetObjektif != this.transform)
                {
                    MunculkanPeringatanLokal("Ambil koin yang ditunjuk arah panah!");
                    return; 
                }
            }

            if (!gunakanWaktu)
            {
                SelesaikanPengambilan();
            }
            else
            {
                sedangInteraksi = true;
                timerInteraksi = 0f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<CarControllerPro>() != null)
        {
            BatalInteraksi();
        }
    }

    private void SelesaikanPengambilan()
    {
        sedangInteraksi = false;
        if (objekProgressGlobal != null) objekProgressGlobal.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TambahBarang(targetPanahBerikutnya);
        }
        gameObject.SetActive(false);
    }

    private void BatalInteraksi()
    {
        if (!sedangInteraksi) return;
        sedangInteraksi = false;
        timerInteraksi = 0f;
        if (objekProgressGlobal != null) objekProgressGlobal.SetActive(false);
    }

    private void MunculkanPeringatanLokal(string pesan)
    {
        if (objekPeringatanGlobal == null)
        {
            Canvas canvasAktif = FindAnyObjectByType<Canvas>();
            if (canvasAktif == null) return;

            objekPeringatanGlobal = new GameObject("PeringatanUrutanMisi_Pool");
            objekPeringatanGlobal.transform.SetParent(canvasAktif.transform, false);

            teksPeringatanGlobal = objekPeringatanGlobal.AddComponent<TextMeshProUGUI>();
            teksPeringatanGlobal.fontSize = 24;
            teksPeringatanGlobal.color = Color.red;
            teksPeringatanGlobal.alignment = TextAlignmentOptions.Center;

            teksPeringatanGlobal.fontMaterial.EnableKeyword("UNDERLAY_ON");
            teksPeringatanGlobal.fontMaterial.SetColor("_UnderlayColor", Color.black);
            teksPeringatanGlobal.fontMaterial.SetFloat("_UnderlayOffsetX", 1f);
            teksPeringatanGlobal.fontMaterial.SetFloat("_UnderlayOffsetY", -1f);

            RectTransform rect = objekPeringatanGlobal.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -100);
        }

        teksPeringatanGlobal.text = $"<b>{pesan}</b>";
        objekPeringatanGlobal.SetActive(true);
        timerPeringatan = 1.5f;
    }

    private void UpdateProgressUI(string teks)
    {
        if (objekProgressGlobal == null)
        {
            Canvas canvasAktif = FindAnyObjectByType<Canvas>();
            if (canvasAktif == null) return;

            objekProgressGlobal = new GameObject("ProgressInteraksi_Pool");
            objekProgressGlobal.transform.SetParent(canvasAktif.transform, false);

            teksProgressGlobal = objekProgressGlobal.AddComponent<TextMeshProUGUI>();
            teksProgressGlobal.fontSize = 28;
            teksProgressGlobal.color = Color.yellow;
            teksProgressGlobal.alignment = TextAlignmentOptions.Center;

            teksProgressGlobal.fontMaterial.EnableKeyword("UNDERLAY_ON");
            teksProgressGlobal.fontMaterial.SetColor("_UnderlayColor", Color.black);
            teksProgressGlobal.fontMaterial.SetFloat("_UnderlayOffsetX", 1f);
            teksProgressGlobal.fontMaterial.SetFloat("_UnderlayOffsetY", -1f);

            RectTransform rect = objekProgressGlobal.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, 50);
        }

        teksProgressGlobal.text = $"<b>{teks}</b>";
        objekProgressGlobal.SetActive(true);
    }
}