using R3;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace BlindHolmes
{
    [DisallowMultipleComponent]
    public class BaseCustomButton : OnScreenControl, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Observable<Unit> ClickAsObservable => _clickObservable ??= _clickSubject.AsObservable();

        [SerializeField]
        protected bool m_interactable = true;

        private readonly Subject<Unit> _clickSubject = new Subject<Unit>();
        private Observable<Unit> _clickObservable;
        private TweenerCore<Color, Color ,ColorOptions> _tweener;

        [InputControl(layout = "Button")]
        [SerializeField]
        private string m_ControlPath;
        
        private UnityEngine.UI.Image _image;
        private Color _baseColor;

        protected override string controlPathInternal
        {
            get => m_ControlPath;
            set => m_ControlPath = value;
        }


        private void OnEnable()
        {
            _image = this.GetComponent<UnityEngine.UI.Image>();
            _baseColor = _image.color;
            Observable.EveryValueChanged(this,x=>m_interactable)
                .Subscribe(x=> ActionAsInteractive(x))
                .AddTo(this);
                
        }

        /// <summary>
        /// ボタンを有効にするかどうか
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            m_interactable = interactable;
        }

        /// <summary>
        /// クリックされるまで待つ
        /// </summary>
        public Task AwaitClickAsync(CancellationToken ct = default)
        {
            return _clickSubject.FirstAsync(ct);
        }

        /// <summary>
        /// ポインターがホバーしたときの処理
        /// </summary>
        protected virtual void OnPointerEntered()
        {
            this.transform.DOScale(1.1f, 0.25f).SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// ポインターが外れたときの処理
        /// </summary>
        protected virtual void OnPointerExited()
        {
            this.transform.DOScale(1.0f, 0.25f).SetEase(Ease.OutCubic);
        }

        /// <summary>
        /// ポインターが押されたときの処理
        /// </summary>
        protected virtual void OnPointerDowned() { }

        /// <summary>
        /// ポインターが離されたときの処理
        /// </summary>
        protected virtual void OnPointerUpped() { }

        /// <summary>
        /// クリックされたときの処理
        /// </summary>
        protected virtual void OnClicked()
        {
            _tweener = _image.DOColor(Color.black, 0.2f).SetEase(Ease.OutCubic)
                .OnComplete(() => _image.DOColor(_baseColor, 0.2f).SetEase(Ease.OutCubic));
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            if (!m_interactable) return;
            OnPointerEntered();
        }


        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            if (!m_interactable) return;
            OnPointerExited();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (!m_interactable) return;
            _clickSubject.OnNext(default);
            OnClicked();
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (!m_interactable) return;
            OnPointerDowned();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (!m_interactable) return;
            OnPointerUpped();
        }

        protected void OnDestroy()
        {
            _clickSubject?.Dispose();
            _clickObservable = null;
        }
        
        
        protected virtual void ActionAsInteractive(bool isInteractive)
        {
            _tweener?.Complete();
            if (isInteractive)
            {
                _tweener = _image.DOColor(_baseColor, 0.1f).SetEase(Ease.OutCubic);
            }
            else
            {
                _tweener = _image.DOColor(new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.5f), 0.1f)
                    .SetEase(Ease.OutCubic);
            }
        }
    }
}