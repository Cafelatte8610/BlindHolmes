using UnityEngine;

namespace BlindHolmes
{
    [CreateAssetMenu(fileName = "NewEvidence", menuName = "DetectiveGame/EvidenceData")]
    public class EvidenceData : ScriptableObject
    {
        [Header("Infomation")] 
        public string evidencceID;
        public string displayName;
        [SerializeField, Multiline(3)] public string description;

        [Header("Visual")] public Sprite evidenceImage;
        public GameObject prefabObj;

    }
}