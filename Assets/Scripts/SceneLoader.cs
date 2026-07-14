using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Tooltip("Memuat Scene secara sinkron (Layar mungkin freeze sesaat)")]
    public void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    [Tooltip("Memuat Scene di latar belakang (Direkomendasikan untuk Mobile)")]
    public void LoadSceneAsync(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadSceneAsync(sceneName);
        }
    }
}