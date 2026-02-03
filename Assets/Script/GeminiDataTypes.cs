using System;

// --- リクエスト用 (Unity -> Gemini) ---
[Serializable]
public class GeminiRequest
{
    public Content[] contents;
}

[Serializable]
public class Content
{
    public string role; // "user" または "model"
    public Part[] parts;
}

[Serializable]
public class Part
{
    public string text;
}

// --- レスポンス用 (Gemini -> Unity) ---
[Serializable]
public class GeminiResponse
{
    public Candidate[] candidates;
}

[Serializable]
public class Candidate
{
    public Content content;
    public string finishReason;
    public int index;
}