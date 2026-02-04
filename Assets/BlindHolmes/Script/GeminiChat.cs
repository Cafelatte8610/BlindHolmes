using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiChat : MonoBehaviour
{
    [Header("Settings")]
    // コード上のキーは空にし、JSONから読み込みます
    private string apiKey = ""; 
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    [Header("Game Data")]
    [TextArea(5, 10)] public string detectivePersona = "あなたは名探偵です。証拠が不十分な段階では断定を避けてください。";
    [TextArea(5, 10)] public string suspectList = "容疑者リスト:\n1. A (動機あり)\n2. B (アリバイなし)";

    // 会話履歴を保存するリスト
    private List<Content> chatHistory = new List<Content>();

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
            Debug.LogError("APIキーが見つかりません！ APIKey.json を確認してください。");
            return;
        }

        // 起動確認
        Debug.Log("Gemini探偵システム起動完了。外部からの情報入力を待機中...");
        // SendMessageToGemini("あなたについて簡潔におしえてください", (reply) =>
        // {
        //     Debug.Log(reply);
        // });    
    }

    // --- 1. 外部から情報を送信する関数 ---
    public void SendMessageToGemini(string userMessage, System.Action<string> callback)
    {
        // ユーザーの入力を履歴に追加
        chatHistory.Add(new Content
        {
            role = "user",
            parts = new Part[] { new Part { text = userMessage } }
        });

        StartCoroutine(PostRequest(callback));
    }

    // 犯人を特定させる
    public void AccuseCulprit(System.Action<string, int> callback)
    {
        // 最後に必ずIDだけの行を作るよう指示
        string forcingPrompt = @"
これまでの報告に基づき、犯人を特定して推理を披露したまえ。
推理のプロセスを短く述べた後、改行して、最後に犯人のID番号（1, 2, 3, -1）だけを書きなさい。
他の文字や記号は不要だ。

例：
～という理由で彼が犯人だ。
3
";
        SendMessageToGemini(forcingPrompt, (reply) => 
        {
            int culpritId = -1;
            string finalExplanation = reply;
            try
            {
                string[] lines = reply.Trim().Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                {
                    string lastLine = lines[lines.Length - 1].Trim();

                    if (int.TryParse(lastLine, out int parsedId))
                    {
                        culpritId = parsedId;
                        // finalExplanation = reply.Substring(0, reply.LastIndexOf(lastLine)).Trim();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("ID抽出に失敗しました: " + e.Message);
            }
            Debug.Log($"抽出されたID: {culpritId}");
            // 推理テキストとIDの両方を返す
            callback?.Invoke(finalExplanation, culpritId);
        });
    }

    // --- 通信処理 ---
    IEnumerator PostRequest(System.Action<string> callback)
    {
        string url = $"{apiUrl}?key={apiKey}";

        // システムプロンプト（人格 + 容疑者情報）を結合
        string fullSystemPrompt = $"{detectivePersona}\n\n【事件データ】\n{suspectList}";

        GeminiRequest requestData = new GeminiRequest
        {
            // System Instructionで「役割」と「初期情報」をセット
            system_instruction = new SystemInstruction
            {
                parts = new Part[] { new Part { text = fullSystemPrompt } }
            },
            // ここまでの会話履歴をすべて送信
            contents = chatHistory.ToArray()
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(request.downloadHandler.text);

                if (response.candidates != null && response.candidates.Length > 0)
                {
                    string reply = response.candidates[0].content.parts[0].text;

                    // AIの返答も履歴に追加（文脈を維持するため）
                    chatHistory.Add(new Content
                    {
                        role = "model",
                        parts = new Part[] { new Part { text = reply } }
                    });

                    callback?.Invoke(reply);
                }
            }
            else
            {
                Debug.LogError($"Error: {request.error}\nResponse: {request.downloadHandler.text}");
            }
        }
    }

    void LoadApiKey()
    {
        // ※ファイル名を secrets.json から APIKey.json に統一しました（ご提示コード準拠）
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
            Debug.LogError("APIキーファイルが見つかりません: " + path);
        }
    }
}