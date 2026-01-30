using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// ManagerUI = central runtime controller that drives which UI Pages are visible.
// - Pages are referenced by an id (string) -> GameObject (usually the root of a UI page/panel).
// - There are 2 different stacks:
//   1) MainStack    : history of main pages opened via OpenTo()
//   2) OverlayStack : overlays / popups currently opened on top of the main page via OpenOverlay()
// - Back() rule: close overlays first; if no overlay is open, go back in the main page history.
public class ManagerUI : MonoBehaviour
{
     // Main dictionary: id -> GameObject page instance.
     // Assumption: these GameObjects are already instantiated in the scene (not prefabs),
     // and can be shown/hidden using SetActive.
     private Dictionary<string, Page> Pages = new Dictionary<string, Page>();

     // History stack of main pages opened with OpenTo().
     // Serialized mainly for debugging in the inspector.
     [SerializeField]
     private List<string> MainStack = new List<string>();

     // Stack of currently opened overlays (popups).
     // Last element = topmost overlay.
     private List<string> OverlayStack = new List<string>();

     // Id of the default (root) page, opened at Start().
     [SerializeField]
     private string defaultID; 
     
     #region SINGLETON PATTERN
     // Simple singleton:
     // - Only one instance is allowed.
     // - The instance survives scene changes (DontDestroyOnLoad).
     public static ManagerUI Instance;

     private void Awake()
     {
          // If another instance already exists, destroy this one.
          if (Instance != null && Instance != this)
          {
               Destroy(gameObject);
               return;
          }

          // Otherwise, become the global instance.
          Instance = this;

          // Make this manager persistent across scenes.
          DontDestroyOnLoad(gameObject);
     }
     #endregion

     private void Start()
     {
          // On startup, open the default (root) page.
          // OpenTo() closes all overlays and shows only one main page.
     }

     private bool tg;
     private void Update()
     {
          if (!tg)
          {
               OpenTo(defaultID);
               tg = true;
          }
     }

     // Registers a page in the dictionary (id -> GameObject).
     // If the id already exists, TryAdd fails and we log a warning instead of silently overwriting it.
     public void AddElement(string id, Page _page)
     {
          if (!Pages.TryAdd(id, _page))
               Debug.LogWarning($"ManagerUI: id '{id}' already registered.");
     }

     // Unregisters a page.
     // - If the id is not found, do nothing.
     // - Also removes this id from both stacks to avoid keeping dead references in history.
     public void RemoveElement(string id)
     {
          if (!Pages.Remove(id)) return;
          OverlayStack.RemoveAll(x => x == id);
          MainStack.RemoveAll(x => x == id);
     }

     // Opens a "main" page (exclusive mode):
     // - Checks that the id exists.
     // - Closes all overlays (change of context).
     // - Disables all pages, then enables only the target page.
     // - Pushes the id to MainStack if it is not already the current top page.
     public void OpenTo(string id)
     {
          // Safety check: the id must exist.
          if (!CheckIdExists(id)) return;

          // Close all overlays when changing main page.
          CloseAllOverlaysInternal();

          // Exclusive display: turn everything off, then turn the target page on.
          foreach (var kv in Pages) kv.Value.OnExit() ;
          Pages[id].OnEnter();

          // Update main history stack, avoid duplicate on top.
          if (MainStack.Count == 0 || MainStack[^1] != id)
               MainStack.Add(id);
     }

     // Opens an overlay page (popup) on top of the current main page:
     // - Checks that the id exists.
     // - Prevents opening the same overlay multiple times (no duplicates policy).
     // - Activates the GameObject, moves it to the top of the hierarchy (render order),
     //   and pushes the id to OverlayStack.
     public void OpenOverlay(string id)
     {
          // Safety check.
          if (!CheckIdExists(id)) return;

          // Policy: do not allow the same overlay to be stacked multiple times.
          if (OverlayStack.Contains(id)) return;

          // Show the overlay.
          Pages[id].OnEnter();

          // Ensure it is rendered above the others (last sibling in the Canvas hierarchy).
          Pages[id].transform.SetAsLastSibling();

          // Push to overlay stack so Back() will close it first.
          OverlayStack.Add(id);
     }

     // Back action (Escape / Android back / gamepad B):
     // Rule:
     // 1) If there is at least one overlay open -> close the topmost overlay and return.
     // 2) Otherwise, if there is more than one main page in history ->
     //    close the current main page and show the previous one.
     // 3) Otherwise -> we are at root, do nothing (or later trigger an event).
     public void Back()
     {
          // 1) Overlays have priority.
          if (OverlayStack.Count > 0)
          {
               // Get the top overlay.
               var id = OverlayStack[^1];

               // Pop it from the stack.
               OverlayStack.RemoveAt(OverlayStack.Count - 1);

               // Safely disable it if it still exists.
               if (Pages.TryGetValue(id, out var go)) go.OnExit();

               return;
          }

          // 2) No overlays: handle main page history.
          // If we have 0 or 1 main page, we are at root: nothing to go back to.
          if (MainStack.Count <= 1)
               return; // root

          // Current main page.
          var current = MainStack[^1];

          // Pop it.
          MainStack.RemoveAt(MainStack.Count - 1);

          // Disable the current page.
          if (Pages.TryGetValue(current, out var curGo)) curGo.OnExit();

          // Previous main page becomes the new top.
          var prev = MainStack[^1];

          // Enable the previous page.
          if (Pages.TryGetValue(prev, out var prevGo)) prevGo.OnEnter();
     }

     // Closes all currently opened overlays:
     // - Iterates from top to bottom (LIFO order).
     // - Disables each overlay if it exists.
     // - Clears the OverlayStack.
     private void CloseAllOverlaysInternal()
     {
          for (int i = OverlayStack.Count - 1; i >= 0; i--)
          {
               var id = OverlayStack[i];
               if (Pages.TryGetValue(id, out var go)) go.OnExit();
          }
          OverlayStack.Clear();
     }

     // Closes absolutely everything:
     // - Disables all registered pages.
     // - Clears both stacks (full navigation reset).
     public void CloseAll()
     {
          foreach (var screen in Pages)
               screen.Value.OnExit();

          OverlayStack.Clear();
          MainStack.Clear();
     }

     // Shortcut to go back to the default (root) page.
     public void OpenDefault()
     {
          OpenTo(defaultID);
     }

     // Checks whether an id exists in the Pages dictionary.
     // - Logs an error if it does not.
     // - Returns true/false so callers can early-out safely.
     private bool CheckIdExists(string id)
     {
          if (!Pages.TryGetValue(id, out var go))
          {
               Debug.LogError($"ManagerUI: Unknown id '{id}'");
               return false;
          }
          
          return true;
     }
}
