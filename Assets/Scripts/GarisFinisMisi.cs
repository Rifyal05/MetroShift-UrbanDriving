using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GarisFinishMisi : MonoBehaviour
{
    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponentInParent<CarControllerPro>() != null)
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CekApakahBarangCukup())
                {
                    GameManager.Instance.MisiSelesai();
                }
                else
                {
                    GameManager.Instance.MisiGagalKarenaBarangKurang();
                }
            }
        }
    }
}