using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CheckAvailableModels : MonoBehaviour
{
    [SerializeField] private string apiKey = "ここにAPIキーを貼り付け";

    void Start()
    {
        StartCoroutine(GetModels());
    }

    IEnumerator GetModels()
    {
        // モデル一覧を取得するURL
        string url = $"https://generativelanguage.googleapis.com/v1beta/models?key={apiKey}";

        using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("=== 利用可能なモデル一覧 ===");
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("モデル一覧の取得に失敗: " + request.error);
            }
        }
    }
}