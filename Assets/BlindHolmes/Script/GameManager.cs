using UnityEngine;
using UnityEngine.InputSystem;

namespace BlindHolmes
{
    public class GameManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float interactionRange = 3.0f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private Transform cameraRoot; 
        [Header("Input References")]
        [SerializeField] private PlayerInput m_playerInput;
        [SerializeField] private string actionName = "Attack";
        private InputAction m_interactAction; 
        private IInteractable _currentInteractable;
        [SerializeField]
        private PlayerController m_playerController;

        void Start()
        {
            Debug.Log ("GameManager: Start");
            m_interactAction = m_playerInput.actions[actionName];
        }

        void Update()
        {
            // if (Cursor.lockState != CursorLockMode.Locked) return;
            if (m_interactAction == null) return;

            CheckForInteractable();
            CheckInput();
        }

        private void CheckForInteractable()
        {
            Ray ray = new Ray(cameraRoot.position, cameraRoot.forward);
            RaycastHit hit;
            
            IInteractable newInteractable = null;

            if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
            {
                newInteractable = hit.transform.GetComponent<IInteractable>();
            }

            if (_currentInteractable != newInteractable)
            {
                if (_currentInteractable != null)
                {
                    _currentInteractable.OnHoverExit();
                }

                if (newInteractable != null)
                {
                    newInteractable.OnHoverEnter();
                }

                _currentInteractable = newInteractable;
            }
        }

        private void CheckInput()
        {
            if (_currentInteractable != null && m_interactAction.WasPressedThisFrame())
            {
                _currentInteractable.OnInteract();
                Debug.Log("Interact");
            }
        }

        private void OnDrawGizmos()
        {
            if (cameraRoot != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(cameraRoot.position, cameraRoot.forward * interactionRange);
            }
        }

        public void OpenUI()
        {
            m_playerController.OperationUI();
        }

        public void CloseUI()
        {
            m_playerController.ClosedUI();
        }
    }
}