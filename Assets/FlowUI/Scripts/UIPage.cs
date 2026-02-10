namespace FlowUI
{
    using System.Collections;
    using UnityEngine;
    using UnityEngine.Serialization;

    /// <summary>
    /// Marks a GameObject as a navigable UI Page.
    /// Pages automatically register themselves to ManagerUI
    /// and can be opened via their unique ID.
    /// </summary>
// UIPage = component that marks a GameObject as a UI Page managed by UIManager.
// Responsibilities:
// - Holds a unique id
// - Auto register/unregister
// - Provides Show/Hide methods that play transitions explicitly
    [DisallowMultipleComponent]
    public class UIPage : MonoBehaviour
    {
        [Header("Identification")]
        [Tooltip("Unique identifier for this page.\n" +
                 "Used by ManagerUI to open or close the page.\n" +
                 "If left empty, the GameObject name will be used automatically.")]
        [SerializeField]
        private string id;


        [FormerlySerializedAs("pageTransition")]
        [Header("Transitions (Optional)")]
        [Tooltip("Optional enter and exit transitions for this page.\n" +
                 "Uses UnityEvents, allowing you to trigger Animators, CanvasGroups, Timelines, etc.")]
        [SerializeField]
        private UIPageTransition uiPageTransition = new UIPageTransition();


// Runtime only
        private Coroutine enterCo;
        private Coroutine exitCo;

        private void Awake()
        {
            if (string.IsNullOrEmpty(id))
                id = gameObject.name;

            if (UIManager.Instance == null)
            {
                Debug.LogError($"UIPage '{name}': ManagerUI instance not found in the scene.", this);
                return;
            }

            UIManager.Instance.AddElement(id, this);

            // Optional: start disabled so ManagerUI controls everything
            // (You can remove this if you want designers to keep some pages active by default.)
            // gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (UIManager.Instance != null)
                UIManager.Instance.RemoveElement(id);
        }

        // Enable + enter transition
        public IEnumerator Show()
        {
            // Stop only transition coroutines (not StopAllCoroutines)
            if (exitCo != null)
            {
                StopCoroutine(exitCo);
                exitCo = null;
            }

            if (enterCo != null)
            {
                StopCoroutine(enterCo);
                enterCo = null;
            }

            gameObject.SetActive(true);

            if (uiPageTransition != null)
                yield return uiPageTransition.PlayEnter();
        }

        // Exit transition + disable
        public IEnumerator Hide()
        {
            if (!gameObject.activeSelf)
                yield break;

            if (enterCo != null)
            {
                StopCoroutine(enterCo);
                enterCo = null;
            }

            if (exitCo != null)
            {
                StopCoroutine(exitCo);
                exitCo = null;
            }

            if (uiPageTransition != null)
                yield return uiPageTransition.PlayExit();

            // Disable after exit
            gameObject.SetActive(false);
        }

        // Hard disable without transition (robust cleanup)
        public void ForceDisable()
        {
            if (enterCo != null)
            {
                StopCoroutine(enterCo);
                enterCo = null;
            }

            if (exitCo != null)
            {
                StopCoroutine(exitCo);
                exitCo = null;
            }

            gameObject.SetActive(false);
        }

        public string GetIdForValidation()
        {
            return id;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                id = gameObject.name;
        }
#endif

    }
}
