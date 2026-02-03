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
            // //カウントアップボタンが押されたらModelに通知する
            // _view.CountupButtonAsObservable
            //     .Subscribe(_ => _model.CountUp())
            //     .AddTo(_view);
            //
            // //カウントダウンボタンが押されたらModelに通知する
            // _view.CountDownButtonAsObservable
            //     .Subscribe(_ => _model.CountDown())
            //     .AddTo(_view);
            //
            // //Modelの値が変化したらViewに通知する
            // _model.CountAsObservable
            //     .Subscribe(num => _view.SetNumText(num))
            //     .AddTo(_disposables);
            
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