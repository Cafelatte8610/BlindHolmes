using System;

[Serializable]
public class GeminiRequest
{
    public SystemInstruction system_instruction; // キャラ設定・容疑者情報
    public Content[] contents;
}

[Serializable]
public class SystemInstruction
{
    public Part[] parts;
}

[Serializable]
public class Content
{
    public string role;
    public Part[] parts;
}

[Serializable]
public class Part
{
    public string text;
}

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