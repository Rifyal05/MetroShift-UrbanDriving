using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class SplashAnimator : MonoBehaviour
{
    public float durasiFade = 1.5f;
    public TextMeshProUGUI teksLoading;
    public float kecepatanTeks = 0.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        StartCoroutine(MulaiFadeIn());
    }

    private IEnumerator MulaiFadeIn()
    {
        float elapsed = 0f;
        canvasGroup.alpha = 0f;

        if (teksLoading != null)
        {
            StartCoroutine(AnimasiTeksLoading());
        }

        while (elapsed < durasiFade)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / durasiFade);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator AnimasiTeksLoading()
    {
        int jumlahTitik = 0;
        while (true)
        {
            string titik = "";
            for (int i = 0; i < jumlahTitik; i++)
            {
                titik += " .";
            }
            teksLoading.text = "LOADING" + titik;
            jumlahTitik = (jumlahTitik + 1) % 4;
            yield return new WaitForSecondsRealtime(kecepatanTeks);
        }
    }
}