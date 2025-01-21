using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GetData : MonoBehaviour
{
    public Slider loadingSlider; 

    void Start()
    {
        Connection.Packet.RequestPlayerData();
        StartCoroutine(RequestPlayerDataCoroutine());
    }

    IEnumerator RequestPlayerDataCoroutine()
    {
        loadingSlider.gameObject.SetActive(true);
        loadingSlider.value = 0f;

        float simulatedProgress = 0f;
        while (simulatedProgress < 1f)
        {
            simulatedProgress += Time.deltaTime * 0.5f;
            loadingSlider.value = simulatedProgress;
            yield return null;
        }

        loadingSlider.value = 1f;

        loadingSlider.gameObject.SetActive(false);

        OnDataLoaded();
    }

    void OnDataLoaded()
    {
        SceneManager.LoadScene("PlayerHub");
        Debug.Log("Player data loaded successfully.");
    }
}
