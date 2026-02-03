using System;
using UnityEngine;
using R3;
using UnityEngine.UI;
using TMPro;

namespace BlindHolmes.MVP
{
    public class EvidenceView : MonoBehaviour
    {
        [SerializeField] private Image m_evidenceImage;
        [SerializeField] private TMP_Text m_evidenceDescription;
        [SerializeField] private GameObject m_nonePanel;
        [SerializeField] private CustomButton m_evidenceButton;
        [SerializeField] private GameObject m_viewObject;
        private CompositeDisposable _disposables = new CompositeDisposable();

        public Observable<Unit> CloseButtonAsObservable
        {
            get { return m_evidenceButton.ClickAsObservable; }
        }

        private void Start()
        {
            Close();
        }

        public void Close()
        {
            m_viewObject.SetActive(false);
        }

        public void Open()
        {
            m_viewObject.SetActive(true);
        }

        public void SetEvidenceData(EvidenceData evidenceData)
        {
            Open();
            if (evidenceData.evidenceImage != null)
            {
                m_evidenceImage.gameObject.SetActive(true);
                m_evidenceImage.sprite = evidenceData.evidenceImage;
                m_nonePanel.SetActive(false);
            }
            else
            {
                m_evidenceImage.gameObject.SetActive(false);
                m_nonePanel.SetActive(true);
            }

            m_evidenceDescription.text = evidenceData.description;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _disposables.Clear();
        }
    }

}