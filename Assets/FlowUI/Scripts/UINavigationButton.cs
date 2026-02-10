
namespace FlowUI
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public class UINavigationButton : MonoBehaviour
    {
        public enum ActionType
        {
            OpenTo,
            OpenOverlay,
            Back,
            CloseAll,
            OpenDefault,
            None
        }

        [Header("Navigation")]
        [Tooltip("Defines which navigation action will be triggered when the button is clicked.")]
        [SerializeField]
        private ActionType action = ActionType.OpenTo;


        [Tooltip("Target Page ID.\nUsed only for OpenTo and OpenOverlay actions.")] [SerializeField]
        private string targetId;


        [Header("Interaction")]
        [Tooltip("If enabled, the Button becomes non-interactable while ManagerUI is busy.\n" +
                 "Prevents rapid clicking during transitions.")]
        [SerializeField]
        private bool disableButtonWhileBusy = true;


// Cached reference (runtime only)
        private Button btn;

        private void Awake()
        {
            btn = GetComponent<Button>();
            if (btn == null)
            {
                Debug.LogWarning($"NavBtn: aucun Button sur '{name}'.", this);
                enabled = false;
                return;
            }

            btn.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (btn != null)
                btn.onClick.RemoveListener(OnClick);
        }

        private void OnEnable()
        {
            // Petit anti-spam local optionnel : si ManagerUI busy, on désactive le bouton
            // (ManagerUI bloque déjà, mais ça améliore la sensation utilisateur).
            if (disableButtonWhileBusy)
                InvokeRepeating(nameof(RefreshInteractable), 0f, 0.05f);
        }

        private void OnDisable()
        {
            if (disableButtonWhileBusy)
                CancelInvoke(nameof(RefreshInteractable));
        }

        private void RefreshInteractable()
        {
            if (btn == null) return;
            var mgr = UIManager.Instance;
            if (mgr == null) return;

            // Expose isBusy via une propriété si tu veux (voir plus bas),
            // sinon enlève ce bloc et garde juste ManagerUI comme garde-fou.
            btn.interactable = !mgr.IsBusy;
        }

        private void OnClick()
        {
            var mgr = UIManager.Instance;
            if (mgr == null) return;

            switch (action)
            {
                case ActionType.OpenTo:
                    if (!string.IsNullOrEmpty(targetId))
                        mgr.OpenTo(targetId);
                    else
                        Debug.LogWarning("NavBtn: targetId vide pour OpenTo.", this);
                    break;

                case ActionType.OpenOverlay:
                    if (!string.IsNullOrEmpty(targetId))
                        mgr.OpenOverlay(targetId);
                    else
                        Debug.LogWarning("NavBtn: targetId vide pour OpenOverlay.", this);
                    break;

                case ActionType.Back:
                    mgr.Back();
                    break;

                case ActionType.CloseAll:
                    mgr.CloseAll();
                    break;

                case ActionType.OpenDefault:
                    mgr.OpenDefault();
                    break;
            }
        }

        public string GetTargetIdForValidation()
        {
            return targetId;
        }

        public bool RequiresTargetIdForValidation()
        {
            if (action == ActionType.OpenTo || action == ActionType.OpenOverlay)
            {
                return true;
            }

            return false;
        }
    }
}
