using System;
using R3;
using UnityEngine;
using VContainer;

namespace BlindHolmes
{
    public class HolmesManager : MonoBehaviour,IInteractable
    {
        Outline _outline;
        private readonly Subject<Unit> _interactHolmesSubject = new Subject<Unit>();
        [Inject] private MVP.HolmesPresenter _presenter;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
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
