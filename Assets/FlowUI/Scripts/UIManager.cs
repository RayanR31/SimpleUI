using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UIManager = central runtime controller that drives which UI Pages are visible.
// - Pages are referenced by an id (string) -> Page (root of UI page/panel).
// - There are 2 stacks:
//   1) MainStack    : history of main pages opened via OpenTo()
//   2) OverlayStack : overlays / popups opened via OpenOverlay()
// - Back() rule: close overlays first; if no overlay is open, go back in main history.
[DefaultExecutionOrder(-1000)]
public class UIManager : MonoBehaviour
{
    // Runtime registry (not shown in Inspector)
    // Main dictionary: pageId -> Page instance.
    private readonly Dictionary<string, UIPage> Pages = new Dictionary<string, UIPage>();

    [Header("Startup")]

    [Tooltip("Page ID opened automatically on Start(). Acts as the root page.")]
    [SerializeField] private string defaultID;
    [Tooltip("If enabled, ManagerUI automatically opens the Default Page on Start().\n" +
             "Disable this if you want to control the first page manually (for example after a loading screen).")]
    [SerializeField] private bool ActiveDefaultPage = true;

    [Header("Debug (Runtime)")]

    [Tooltip("Main navigation history (top = last). Serialized for debugging only.")]
    [SerializeField] private List<string> MainStack = new List<string>();

    // Not serialized by default (not needed in Inspector).
    [Tooltip("Overlay stack (top = last). Runtime only.")]
    private readonly List<string> OverlayStack = new List<string>();

    [Header("Input")]

    [Tooltip("Optional full-screen CanvasGroup used to block UI clicks during transitions.\n" +
             "Assign a UI object stretched to screen with CanvasGroup.blocksRaycasts = true.")]
    [SerializeField] private CanvasGroup inputBlocker;

    [Header("State (Runtime)")]

    [Tooltip("True while a navigation transaction is running (transitions playing). Navigation calls are ignored.")]
    [SerializeField] private bool isBusy;


    /// <summary>
    /// True while transitions/navigation are running. Useful for UI/inputs to disable interactions.
    /// </summary>
    public bool IsBusy => isBusy;

    #region SINGLETON
    public static UIManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    private void Start()
    {
        // Opens the default page automatically at startup.
        // Can be disabled if the first navigation is triggered manually.
        if (ActiveDefaultPage && !string.IsNullOrEmpty(defaultID))
            OpenTo(defaultID);
    }

    // Registers a page (id -> Page).
    public void AddElement(string id, UIPage uiPage)
    {
        if (string.IsNullOrEmpty(id) || uiPage == null)
        {
            LogW("ManagerUI: AddElement called with invalid arguments.");
            return;
        }

        if (!Pages.TryAdd(id, uiPage))
            LogW($"ManagerUI: id '{id}' already registered (keeping first).");
    }

    // Unregisters a page and removes it from stacks.
    public void RemoveElement(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!Pages.Remove(id)) return;

        OverlayStack.RemoveAll(x => x == id);
        MainStack.RemoveAll(x => x == id);
    }

    // === PUBLIC API ===

    public void OpenTo(string id)
    {
        if (isBusy) return;
        if (!CheckIdExists(id)) return;

        StartCoroutine(IE_OpenTo(id));
    }

    public void OpenOverlay(string id)
    {
        if (isBusy) return;
        if (!CheckIdExists(id)) return;

        // No duplicate on top policy (you can change to Contains if you prefer)
        if (OverlayStack.Count > 0 && OverlayStack[^1] == id) return;

        StartCoroutine(IE_OpenOverlay(id));
    }

    public void Back()
    {
        if (isBusy) return;
        StartCoroutine(IE_Back());
    }

    public void CloseAll()
    {
        if (isBusy) return;
        StartCoroutine(IE_CloseAll());
    }

    public void OpenDefault()
    {
        if (isBusy) return;
        if (string.IsNullOrEmpty(defaultID)) return;
        OpenTo(defaultID);
    }

    // === COROUTINES (transaction-style) ===

    private IEnumerator IE_OpenTo(string id)
    {
        SetBusy(true);

        // 1) Close overlays first
        yield return CloseAllOverlaysInternal(id);

        // 2) Exit current main (top of MainStack), if any and different
        string currentMain = MainStack.Count > 0 ? MainStack[^1] : null;
        if (!string.IsNullOrEmpty(currentMain) && currentMain != id && Pages.TryGetValue(currentMain, out var curPage))
        {
            yield return curPage.Hide(); // exit + disable
        }

        // 3) Ensure all non-target MAIN pages are disabled (robust exclusive mode)
        // Note: We only hard-disable pages that are NOT the target and NOT currently in overlay stack.
        // This prevents weird leftover active pages.
        foreach (var kv in Pages)
        {
            var pid = kv.Key;
            var p = kv.Value;

            if (pid == id) continue;
            if (OverlayStack.Contains(pid)) continue; // should be empty here, but safe

            // Hard disable (no transition) if it’s active for any reason.
            // You can also choose to call Hide() if you want exit transitions everywhere.
            if (p.gameObject.activeSelf)
                p.ForceDisable();
        }

        // 4) Show target
        if (Pages.TryGetValue(id, out var target))
        {
            yield return target.Show(); // enable + enter
        }

        // 5) Update history (no duplicate on top; also remove existing occurrences)
        MainStack.RemoveAll(x => x == id);
        MainStack.Add(id);

        SetBusy(false);
    }

    private IEnumerator IE_OpenOverlay(string id)
    {
        SetBusy(true);

        if (Pages.TryGetValue(id, out var overlay))
        {
            // Show and bring to front
            overlay.transform.SetAsLastSibling();
            yield return overlay.Show();
        }

        // Push overlay
        OverlayStack.Add(id);

        SetBusy(false);
    }

    private IEnumerator IE_Back()
    {
        SetBusy(true);

        // 1) Close top overlay if any
        if (OverlayStack.Count > 0)
        {
            var id = OverlayStack[^1];
            OverlayStack.RemoveAt(OverlayStack.Count - 1);

            if (Pages.TryGetValue(id, out var overlay))
                yield return overlay.Hide();

            SetBusy(false);
            yield break;
        }

        // 2) Main history
        if (MainStack.Count <= 1)
        {
            // Root: do nothing (later: event)
            SetBusy(false);
            yield break;
        }

        var current = MainStack[^1];
        MainStack.RemoveAt(MainStack.Count - 1);

        if (Pages.TryGetValue(current, out var curPage))
            yield return curPage.Hide();

        var prev = MainStack[^1];
        if (Pages.TryGetValue(prev, out var prevPage))
            yield return prevPage.Show();

        SetBusy(false);
    }

    private IEnumerator IE_CloseAll()
    {
        SetBusy(true);

        // Close overlays (top to bottom)
        yield return CloseAllOverlaysInternal();

        // Close mains (optionally transition only the current one)
        // Here: hide everything with transitions if active
        foreach (var kv in Pages)
        {
            if (kv.Value.gameObject.activeSelf)
                yield return kv.Value.Hide();
        }

        OverlayStack.Clear();
        MainStack.Clear();

        SetBusy(false);
    }

    private IEnumerator CloseAllOverlaysInternal(string targetId = null)
    {
        for (int i = OverlayStack.Count - 1; i >= 0; i--)
        {
            var id = OverlayStack[i];
            if (Pages.TryGetValue(id, out var page) && id != targetId)
                yield return page.Hide();
        }

        OverlayStack.Clear();
    }

    // === HELPERS ===

    private void SetBusy(bool value)
    {
        isBusy = value;

        if (inputBlocker != null)
        {
            inputBlocker.blocksRaycasts = value;
            inputBlocker.interactable = value;
            // Optional: show/hide blocker visually if you want:
            // inputBlocker.alpha = value ? 1f : 0f;
        }
    }

    private bool CheckIdExists(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            LogE("ManagerUI: Empty id.");
            return false;
        }

        if (!Pages.ContainsKey(id))
        {
            LogE($"ManagerUI: Unknown id '{id}'");
            return false;
        }

        return true;
    }
    
    private void LogW(string msg, Object ctx = null)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning(msg, ctx);
#endif
    }

    private void LogE(string msg, Object ctx = null)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogError(msg, ctx);
#endif
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(defaultID))
        {
            Debug.LogWarning("ManagerUI: Default ID is empty.", this);
        }
    }
#endif


}
