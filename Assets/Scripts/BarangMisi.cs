using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class BarangMisi : MonoBehaviour
{
    public Transform targetPanahBerikutnya; 
    
    [Header("Animasi Koin")]
    public float kecepatanPutar = 100f;
    public float tinggiMelayang = 0.5f;
    public float kecepatanMelayang = 2f;

    private float posisiAwalY;

    private static GameObject objekPeringatanAktif;

    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        posisiAwalY = transform.position.y;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * kecepatanPutar * Time.deltaTime, Space.World);

        float yBaru = posisiAwalY + Mathf.Sin(Time.time * kecepatanMelayang) * tinggiMelayang;
        transform.position = new Vector3(transform.position.x, yBaru, transform.position.z);
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
                    BuatTeksPeringatanLokal("Ambil koin yang ditunjuk arah panah!");
                    return; 
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TambahBarang(targetPanahBerikutnya);
            }
            gameObject.SetActive(false); 
        }
    }

    private void BuatTeksPeringatanLokal(string pesan)
    {
        if (objekPeringatanAktif != null) return;

        Canvas canvasAktif = FindAnyObjectByType<Canvas>();
        if (canvasAktif != null)
        {
            objekPeringatanAktif = new GameObject("PeringatanUrutanMisi");
            objekPeringatanAktif.transform.SetParent(canvasAktif.transform, false);

            TextMeshProUGUI tmpText = objekPeringatanAktif.AddComponent<TextMeshProUGUI>();
            tmpText.text = $"<b>{pesan}</b>";
            tmpText.fontSize = 24;
            tmpText.color = Color.red;
            tmpText.alignment = TextAlignmentOptions.Center;

            tmpText.fontMaterial.EnableKeyword("UNDERLAY_ON");
            tmpText.fontMaterial.SetColor("_UnderlayColor", Color.black);
            tmpText.fontMaterial.SetFloat("_UnderlayOffsetX", 1f);
            tmpText.fontMaterial.SetFloat("_UnderlayOffsetY", -1f);

            RectTransform rect = objekPeringatanAktif.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -100);

            Destroy(objekPeringatanAktif, 1.5f);
        }
    }
}