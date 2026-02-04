using System;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace BlindHolmes.MVP
{
    public class HolmesPresenter:IInitializable, IDisposable
    {
        private HolmesModel _model;
        private HolmesView _holmesView;
        private EvidenceView _evidenceView;
        private GameManager _gameManager;

        private CompositeDisposable _disposables = new CompositeDisposable();
        
        
        public HolmesPresenter(HolmesView holmesView,EvidenceView evidenceView, HolmesModel model, GameManager gameManager)
        {
            _holmesView = holmesView;
            _evidenceView = evidenceView;
            _model = model;
            _gameManager = gameManager;
        }
        
        public void RegisterEvidenceSource(Observable<EvidenceData> evidenceStream)
        {
            evidenceStream
                .Subscribe(data => 
                {
                    _gameManager.OpenUI();
                    _evidenceView.SetEvidenceData(data);
                })
                .AddTo(_disposables);
        }
        
        public void RegisterHolmesSource(Observable<Unit> holmesStream)
        {
            holmesStream
                .Subscribe(_ => 
                {
                    _gameManager.OpenUI();
                    _holmesView.Open();
                })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void Initialize()
        {
            Bind();
        }

        private void Bind()
        {

            _holmesView.CloseHolmesSubject
                .Subscribe(_ =>
                {
                    _gameManager.CloseUI();
                })
                .AddTo(_disposables); 
            
            _holmesView.CloseButtonAsObservable
                .Subscribe(_ =>
                {
                    _holmesView.Close();
                })
                .AddTo(_disposables); 
            
            _evidenceView.CloseButtonAsObservable
                .Subscribe(_ =>
                {
                    _gameManager.CloseUI();
                    _evidenceView.Close();
                })
                .AddTo(_disposables);

        }
    } 
}