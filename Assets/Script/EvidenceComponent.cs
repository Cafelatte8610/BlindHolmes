using UnityEngine;

public class EvidenceComponent : MonoBehaviour
{
    [SerializeField] EvidenceData m_evidenceData;
    Outline _outline;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
    }

    public EvidenceData GetEvidenceData()
    {
        return m_evidenceData;
    }

    public void Focused()
    {
        _outline.OutlineColor = Color.orange;
    }

    public void Unfocused()
    {
        _outline.OutlineColor = Color.white;
    }

}
