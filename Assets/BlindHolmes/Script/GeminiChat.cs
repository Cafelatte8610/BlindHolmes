using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiChat : MonoBehaviour
{
    private string apiKey = "";
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent";
    
    [System.Serializable]
    private class SecretConfig
    {
        public string apiKey;
    }
    
    void Start()
    {
        LoadApiKey();

        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError("APIキーが見つかりません！ secrets.json を確認してください。");
            return;
        }

        // テスト送信
        SendMessageToGemini("おはよう。事件の状況はどうなってる？", (reply) => 
        {
            Debug.Log("探偵: " + reply);
        });
    }
    
    void LoadApiKey()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "APIKey.json");

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SecretConfig config = JsonUtility.FromJson<SecretConfig>(json);
            apiKey = config.apiKey;
            Debug.Log("APIキーを読み込みました");
        }
        else
        {
            Debug.LogError("secrets.json が見つかりません: " + path);
        }
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