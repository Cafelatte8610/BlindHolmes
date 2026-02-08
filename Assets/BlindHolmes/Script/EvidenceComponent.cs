using UnityEngine;
using R3;
using VContainer;

namespace BlindHolmes
{
    public class EvidenceComponent : MonoBehaviour,IInteractable
    {
        [SerializeField] EvidenceData m_evidenceData;
        Outline _outline;
        
        private readonly Subject<EvidenceData> _interactSubject = new Subject<EvidenceData>();

        [Inject] private MVP.HolmesPresenter _presenter;
        private void Awake()
        {
            _outline = GetComponent<Outline>();
        }

        // LTSにInjectionする
        private void Start()
        {
            if (_presenter != null)
            {
                _presenter.RegisterEvidenceSource(_interactSubject);
            }
        }
        
        public EvidenceData GetEvidenceData()
        {
            return m_evidenceData;
        }
        
        public void OnHoverEnter()
        {
            _outline.OutlineColor = Color.orange;
        }

        public void OnHoverExit()
        {
            _outline.OutlineColor = Color.white;
        }

        public void OnInteract()
        {
            _interactSubject.OnNext(m_evidenceData);
        }
        
        private void OnDestroy()
        {
            _interactSubject.OnCompleted();
            _interactSubject.Dispose();
        }
    }
}
