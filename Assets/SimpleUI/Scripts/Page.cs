using System.Collections;
using UnityEngine;

// Page = component that marks a GameObject as a UI Page managed by ManagerUI.
// Responsibilities:
// - Holds a unique id
// - Auto register/unregister
// - Provides Show/Hide methods that play transitions explicitly
public class Page : MonoBehaviour
{
    [SerializeField] private string id;

    [Header("Optional")]
    public PageTransition pageTransition = new PageTransition();

    private Coroutine enterCo;
    private Coroutine exitCo;

    private void Start()
    {
        if (string.IsNullOrEmpty(id))
            id = gameObject.name;

        ManagerUI.Instance.AddElement(id, this);

        // Optional: start disabled so ManagerUI controls everything
        // (You can remove this if you want designers to keep some pages active by default.)
        // gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (ManagerUI.Instance != null)
            ManagerUI.Instance.RemoveElement(id);
    }

    // Enable + enter transition
    public IEnumerator Show()
    {
        // Stop only transition coroutines (not StopAllCoroutines)
        if (exitCo != null) { StopCoroutine(exitCo); exitCo = null; }
        if (enterCo != null) { StopCoroutine(enterCo); enterCo = null; }

        gameObject.SetActive(true);

        if (pageTransition != null)
            yield return pageTransition.PlayEnter();
    }

    // Exit transition + disable
    public IEnumerator Hide()
    {
        if (!gameObject.activeSelf)
            yield break;

        if (enterCo != null) { StopCoroutine(enterCo); enterCo = null; }
        if (exitCo != null) { StopCoroutine(exitCo); exitCo = null; }

        if (pageTransition != null)
            yield return pageTransition.PlayExit();

        // Disable after exit
        gameObject.SetActive(false);
    }

    // Hard disable without transition (robust cleanup)
    public void ForceDisable()
    {
        if (enterCo != null) { StopCoroutine(enterCo); enterCo = null; }
        if (exitCo != null) { StopCoroutine(exitCo); exitCo = null; }
        gameObject.SetActive(false);
    }
}
