using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiChat : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private string apiKey = "AIzaSyAjhbtEL5U7arwLauV9Tqg9j8rY48buFe0";
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";    void Start()
    {
        // テスト送信（ゲーム開始時に挨拶してみる）
        StartCoroutine(PostRequest("こんにちは！あなたは誰ですか？", (response) =>
        {
            Debug.Log("Geminiからの返答: " + response);
        }));
    }

    // 外部から呼び出すための関数
    public void SendMessageToGemini(string userMessage, System.Action<string> callback)
    {
        StartCoroutine(PostRequest(userMessage, callback));
    }

    IEnumerator PostRequest(string text, System.Action<string> callback)
    {
        string url = $"{apiUrl}?key={apiKey}";

        // 1. リクエストデータの作成
        GeminiRequest requestData = new GeminiRequest
        {
            contents = new Content[]
            {
                new Content
                {
                    role = "user",
                    parts = new Part[] { new Part { text = text } }
                }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        // 2. HTTPリクエストの設定
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // 3. 送信と待機
            yield return request.SendWebRequest();

            // 4. 結果の処理
            if (request.result == UnityWebRequest.Result.Success)
            {
                // JSONをパースしてテキスト部分だけ取り出す
                GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);
                
                if (response.candidates != null && response.candidates.Length > 0)
                {
                    string reply = response.candidates[0].content.parts[0].text;
                    callback?.Invoke(reply);
                }
            }
            else
            {
                Debug.LogError($"Error: {request.error}\nResponse: {request.downloadHandler.text}");
            }
        }
    }
}