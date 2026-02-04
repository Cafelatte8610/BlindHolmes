using System;
using R3;
using UnityEngine;
using VContainer;

namespace BlindHolmes
{
    public class HolmesManager : MonoBehaviour,IInteractable
    {
        Outline _outline;
        public readonly Subject<Unit> _interactHolmesSubject = new Subject<Unit>();
        [Inject] private MVP.HolmesPresenter _presenter;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
        }
        private void Start()
        {
            if (_presenter != null)
            {
                _presenter.RegisterHolmesSource(_interactHolmesSubject);
            }
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
            _interactHolmesSubject.OnNext(Unit.Default);
        }
    }
}
